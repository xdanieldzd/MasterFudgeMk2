using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Drawing;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.Enumerations;
using MasterFudgeMk2.Common.EventArguments;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Machines.Coleco.ColecoVision
{
    // http://www.atarihq.com/danb/files/CV-Tech.txt
    // http://www.colecoboxart.com/faq/FAQ05.htm

    // TODO: PAL system stuff

    [TypeConverter(typeof(DescriptionTypeConverter))]
    public enum MachineInputs
    {
        [Description("Up")]
        Up,
        [Description("Down")]
        Down,
        [Description("Left")]
        Left,
        [Description("Right")]
        Right,
        [Description("Left Button")]
        LeftButton,
        [Description("Right Button")]
        RightButton,

        [Description("Keypad 1")]
        Keypad1,
        [Description("Keypad 2")]
        Keypad2,
        [Description("Keypad 3")]
        Keypad3,
        [Description("Keypad 4")]
        Keypad4,
        [Description("Keypad 5")]
        Keypad5,
        [Description("Keypad 6")]
        Keypad6,
        [Description("Keypad 7")]
        Keypad7,
        [Description("Keypad 8")]
        Keypad8,
        [Description("Keypad 9")]
        Keypad9,
        [Description("Keypad 0")]
        Keypad0,
        [Description("Keypad *")]
        KeypadStar,
        [Description("Keypad #")]
        KeypadNumberSign
    }

    class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Coleco ColecoVision"; } }
        public override string FriendlyShortName { get { return "ColecoVision"; } }
        public override string FileFilter { get { return "ColecoVision ROMs (*.col)|*.col"; } }
        public override string DatFileName { get { return "Coleco - ColecoVision.dat"; } }

        public override double RefreshRate { get { return refreshRate; } }
        public override float AspectRatio { get { return (576.0f / 486.0f); } }
        public override Rectangle ScreenViewport
        {
            get
            {
                int screenHeight = (int)((vdp.NumTotalScanlines / 100.0f) * 93.0f);
                int borderHeight = (vdp.NumTotalScanlines - screenHeight);
                return new Rectangle(0, borderHeight, TMS9918A.NumActivePixelsPerScanline, (screenHeight - borderHeight));
            }
        }

        public override bool SupportsBootingWithoutMedia { get { return true; } }
        public override bool CanCurrentlyBootWithoutMedia { get { return File.Exists(configuration.BiosPath); } }
        public override string[] MediaSlots { get { return new string[] { "Cartridge Slot" }; } }

        public override MachineConfiguration Configuration { get { return configuration; } set { configuration = (value as Configuration); } }

        public override List<Tuple<string, Type, double>> DebugChipInformation
        {
            get
            {
                return new List<Tuple<string, Type, double>>
                {
                    { "CPU", cpu.GetType(), cpuClock },
                    { "VDP", vdp.GetType(), vdpClock },
                    { "PSG", psg.GetType(), psgClock }
                };
            }
        }

        /* Constants */
        const double masterClock = 10738635;
        const double refreshRate = 59.922743;
        const int ramSize = 1 * 1024;

        const double cpuClock = (masterClock / 3.0);
        const double vdpClock = (masterClock / 1.0);
        const double psgClock = cpuClock;

        /* Devices on bus */
        IMedia cartridge;
        byte[] wram;
        Z80A cpu;
        TMS9918A vdp;
        SN76489 psg;

        [Flags]
        enum KeyJoyButtons : ushort
        {
            None = 0x0000,
            KeyNumber6 = 0x0001,
            KeyNumber1 = 0x0002,
            KeyNumber3 = 0x0003,
            KeyNumber9 = 0x0004,
            KeyNumber0 = 0x0005,
            KeyStarKey = 0x0006,
            KeyInvalid8 = 0x0007,
            KeyNumber2 = 0x0008,
            KeyNumberSignKey = 0x0009,
            KeyNumber7 = 0x000A,
            KeyInvalid4 = 0x000B,
            KeyNumber5 = 0x000C,
            KeyNumber4 = 0x000D,
            KeyNumber8 = 0x000E,
            KeyMask = 0x000F,
            JoyRightButton = 0x0040,
            JoyUp = 0x0100,
            JoyRight = 0x0200,
            JoyDown = 0x0400,
            JoyLeft = 0x0800,
            JoyLeftButton = 0x4000,
            JoyMask = 0x0F40,
        }

        ushort portControls1, portControls2;
        byte controlsReadMode;
        byte[] bios;

        bool isNmi, isNmiPending;

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        Configuration configuration;

        public Manager()
        {
            // TODO: better bios handling?

            configuration = new Configuration();

            cartridge = null;

            cpu = new Z80A(cpuClock, refreshRate, ReadMemory, WriteMemory, ReadPort, WritePort);
            wram = new byte[ramSize];
            vdp = new TMS9918A(vdpClock, refreshRate, false);
            psg = new SN76489(psgClock, refreshRate, (s, e) => { OnAddSampleData(e); });

            if (CanCurrentlyBootWithoutMedia)
                bios = File.ReadAllBytes(configuration.BiosPath);
        }

        public override void Startup()
        {
            cpu.Startup();
            psg.Startup();
            vdp.Startup();

            Reset();
        }

        public override void Reset()
        {
            cartridge?.Reset();

            cpu.Reset();
            psg.Reset();
            vdp.Reset();

            portControls1 = portControls2 = 0xFFFF;
            controlsReadMode = 0x00;

            isNmi = isNmiPending = false;

            base.Reset();
        }

        public override bool CanLoadMedia(FileInfo mediaFile)
        {
            byte[] romStart = new byte[2];
            using (FileStream file = new FileStream(mediaFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(romStart, 0, romStart.Length);
            }
            return ((romStart[0x00] == 0xAA && romStart[0x01] == 0x55) || (romStart[0x00] == 0x55 && romStart[0x01] == 0xAA));
        }

        public override void LoadMedia(int slotNumber, IMedia media)
        {
            switch (slotNumber)
            {
                case 0: cartridge = media; break;
                default: throw new ArgumentException("Invalid slot number");
            }
        }

        public override void SaveMedia()
        {
            //
        }

        public override void Shutdown()
        {
            cartridge?.Unload();

            psg?.Shutdown();
        }

        public override void RunStep()
        {
            double currentCpuClockCycles = 0.0;
            currentCpuClockCycles += cpu.Step();

            double currentMasterClockCycles = (currentCpuClockCycles * 3.0);

            if (vdp.Step((int)Math.Round(currentMasterClockCycles)))
                OnRenderScreen(new RenderScreenEventArgs(TMS9918A.NumActivePixelsPerScanline, vdp.NumTotalScanlines, vdp.OutputFramebuffer));

            /* The IMO oddball NMI-Vblank handling, see ex. Cogwheel */
            if (vdp.InterruptLine == InterruptState.Assert && !isNmi) isNmiPending = true;
            isNmi = (vdp.InterruptLine == InterruptState.Assert);

            if (isNmiPending)
            {
                isNmiPending = false;
                cpu.SetNonMaskableInterruptLine(InterruptState.Assert);
            }

            psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        protected override void ParseInput(PollInputEventArgs input)
        {
            // TODO: controller 2

            ushort dataCtrl1 = 0x0000;

            if (input.Pressed.Contains(configuration.Keypad1)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber1;
            if (input.Pressed.Contains(configuration.Keypad2)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber2;
            if (input.Pressed.Contains(configuration.Keypad3)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber3;
            if (input.Pressed.Contains(configuration.Keypad4)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber4;
            if (input.Pressed.Contains(configuration.Keypad5)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber5;
            if (input.Pressed.Contains(configuration.Keypad6)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber6;
            if (input.Pressed.Contains(configuration.Keypad7)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber7;
            if (input.Pressed.Contains(configuration.Keypad8)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber8;
            if (input.Pressed.Contains(configuration.Keypad9)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber9;
            if (input.Pressed.Contains(configuration.Keypad0)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumber0;
            if (input.Pressed.Contains(configuration.KeypadStar)) dataCtrl1 = (ushort)KeyJoyButtons.KeyStarKey;
            if (input.Pressed.Contains(configuration.KeypadNumberSign)) dataCtrl1 = (ushort)KeyJoyButtons.KeyNumberSignKey;

            if (input.Pressed.Contains(configuration.RightButton)) dataCtrl1 |= (ushort)KeyJoyButtons.JoyRightButton;

            if (input.Pressed.Contains(configuration.Up)) dataCtrl1 |= (ushort)KeyJoyButtons.JoyUp;
            if (input.Pressed.Contains(configuration.Down)) dataCtrl1 |= (ushort)KeyJoyButtons.JoyDown;
            if (input.Pressed.Contains(configuration.Left)) dataCtrl1 |= (ushort)KeyJoyButtons.JoyLeft;
            if (input.Pressed.Contains(configuration.Right)) dataCtrl1 |= (ushort)KeyJoyButtons.JoyRight;
            if (input.Pressed.Contains(configuration.LeftButton)) dataCtrl1 |= (ushort)KeyJoyButtons.JoyLeftButton;

            portControls1 = (ushort)~dataCtrl1;
        }

        private byte ReadMemory(ushort address)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                return bios[address & (bios.Length - 1)];
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                /* Expansion port */
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                /* Expansion port */
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                return wram[address & (ramSize - 1)];
            }
            else if (address >= 0x8000 && address <= 0xFFFF)
            {
                return (cartridge != null ? cartridge.Read(address) : (byte)0x00);
            }

            /* Cannot read from address, return 0 */
            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                /* Can't write to BIOS */
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                /* Expansion port */
            }
            else if (address >= 0x4000 && address <= 0x5FFF)
            {
                /* Expansion port */
            }
            else if (address >= 0x6000 && address <= 0x7FFF)
            {
                wram[address & (ramSize - 1)] = value;
            }
            else if (address >= 0x8000 && address <= 0xFFFF)
            {
                cartridge?.Write(address, value);
            }
        }

        private byte ReadPort(byte port)
        {
            switch (port & 0xE0)
            {
                case 0xA0:
                    /* VDP */
                    if ((port & 0x01) == 0)
                        return vdp.ReadDataPort();      /* Data port */
                    else
                        return vdp.ReadControlPort();   /* Status flags */

                case 0xE0:
                    /* Controls */
                    if ((port & 0x01) == 0)
                        return (controlsReadMode == 0x00 ? (byte)(portControls1 & 0xFF) : (byte)((portControls1 >> 8) & 0xFF));
                    else
                        return (controlsReadMode == 0x00 ? (byte)(portControls2 & 0xFF) : (byte)((portControls2 >> 8) & 0xFF));

                default:
                    /* Not connected */
                    return 0xFF;
            }
        }

        public void WritePort(byte port, byte value)
        {
            switch (port & 0xE0)
            {
                case 0x80:
                    /* Control mode (keypad/right buttons) */
                    controlsReadMode = 0x00;
                    break;

                case 0xA0:
                    /* VDP */
                    if ((port & 0x01) == 0)
                        vdp.WriteDataPort(value);       /* Data port */
                    else
                        vdp.WriteControlPort(value);    /* Control port */
                    break;

                case 0xC0:
                    /* Control mode (joystick/left buttons) */
                    controlsReadMode = 0x01;
                    break;

                case 0xE0:
                    /* PSG */
                    psg.WriteData(value);
                    break;
            }
        }
    }
}
