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
using MasterFudgeMk2.Media.Sega;
using MasterFudgeMk2.Devices;
using MasterFudgeMk2.Devices.Sega;

namespace MasterFudgeMk2.Machines.Sega.MasterSystem
{
    [TypeConverter(typeof(DescriptionTypeConverter))]
    public enum MachineInputs
    {
        [Description("Player 1: Up")]
        P1Up,
        [Description("Player 1: Down")]
        P1Down,
        [Description("Player 1: Left")]
        P1Left,
        [Description("Player 1: Right")]
        P1Right,
        [Description("Player 1: Button 1")]
        P1Button1,
        [Description("Player 1: Button 2")]
        P1Button2,

        [Description("Player 2: Up")]
        P2Up,
        [Description("Player 2: Down")]
        P2Down,
        [Description("Player 2: Left")]
        P2Left,
        [Description("Player 2: Right")]
        P2Right,
        [Description("Player 2: Button 1")]
        P2Button1,
        [Description("Player 2: Button 2")]
        P2Button2,

        [Description("Reset Button")]
        Reset,
        [Description("Pause Button")]
        Pause
    }

    class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Sega Master System"; } }
        public override string FriendlyShortName { get { return "Master System"; } }
        public override string FileFilter { get { return "Master System ROMs (*.sms)|*.sms"; } }

        public override double RefreshRate { get { return refreshRate; } }
        public override float AspectRatio { get { return (!configuration.IsPalSystem ? (576.0f / 486.0f) : (720.0f / 486.0f)); } }
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
        public override bool CanCurrentlyBootWithoutMedia { get { return (File.Exists(configuration.BootstrapPath) && configuration.UseBootstrap); } }
        public override string[] MediaSlots { get { return new string[] { "Cartridge Slot", "Card Slot" }; } }

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
        const double masterClockNtsc = 10738635;
        const double masterClockPal = 10640684;
        const double refreshRateNtsc = 59.922743;
        const double refreshRatePal = 49.701459;
        const int ramSize = 1 * 8192;

        /* Clocks & refresh rate */
        double masterClock, refreshRate, cpuClock, vdpClock, psgClock;

        /* Devices on bus */
        IMedia bootstrap, cartridge, card;
        byte[] wram;
        Z80A cpu;
        SegaSMS2VDP vdp;
        SegaSMS2PSG psg;

        [Flags]
        enum PortIoABButtons : byte
        {
            P1Up = (1 << 0),
            P1Down = (1 << 1),
            P1Left = (1 << 2),
            P1Right = (1 << 3),
            P1Button1 = (1 << 4),
            P1Button2 = (1 << 5),
            P2Up = (1 << 6),
            P2Down = (1 << 7),
            Mask = (((P2Down << 1) - 1) - (P1Up - 1))
        }
        [Flags]
        enum PortIoBMiscButtons : byte
        {
            P2Left = (1 << 0),
            P2Right = (1 << 1),
            P2Button1 = (1 << 2),
            P2Button2 = (1 << 3),
            Reset = (1 << 4),
            Mask = (((Reset << 1) - 1) - (P2Left - 1))
        }

        byte portMemoryControl, portIoControl, portIoAB, portIoBMisc;
        byte lastHCounter;

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        bool pauseButtonPressed, pauseButtonToggle;

        public bool isExpansionSlotEnabled { get { return !BitUtilities.IsBitSet(portMemoryControl, 7); } }
        public bool isCartridgeSlotEnabled { get { return !BitUtilities.IsBitSet(portMemoryControl, 6); } }
        public bool isCardSlotEnabled { get { return !BitUtilities.IsBitSet(portMemoryControl, 5); } }
        public bool isWorkRamEnabled { get { return !BitUtilities.IsBitSet(portMemoryControl, 4); } }
        public bool isBootstrapRomEnabled { get { return !BitUtilities.IsBitSet(portMemoryControl, 3); } }
        public bool isIoChipEnabled { get { return !BitUtilities.IsBitSet(portMemoryControl, 2); } }

        Configuration configuration;

        public Manager()
        {
            configuration = new Configuration();

            bootstrap = null;
            cartridge = null;
            card = null;

            if (!configuration.IsPalSystem)
            {
                masterClock = masterClockNtsc;
                refreshRate = refreshRateNtsc;
            }
            else
            {
                masterClock = masterClockPal;
                refreshRate = refreshRatePal;
            }

            cpuClock = (masterClock / 3.0);
            vdpClock = (masterClock / 1.0);
            psgClock = cpuClock;

            cpu = new Z80A(cpuClock, refreshRate, ReadMemory, WriteMemory, ReadPort, WritePort);
            wram = new byte[ramSize];
            vdp = new SegaSMS2VDP(vdpClock, refreshRate, configuration.IsPalSystem);
            psg = new SegaSMS2PSG(psgClock, refreshRate, (s, e) => { OnAddSampleData(e); });
        }

        public override void Startup()
        {
            if (configuration.UseBootstrap && File.Exists(configuration.BootstrapPath))
                bootstrap = MediaLoader.LoadMedia(this, new FileInfo(configuration.BootstrapPath));

            cpu.Startup();
            psg.Startup();
            vdp.Startup();

            Reset();
        }

        public override void Reset()
        {
            bootstrap?.Reset();
            cartridge?.Reset();
            card?.Reset();

            cpu.Reset();
            cpu.SetStackPointer(0xDFF0);
            psg.Reset();
            vdp.Reset();

            portMemoryControl = (byte)(bootstrap != null ? 0xE3 : 0x00);
            portIoControl = portIoAB = portIoBMisc = 0xFF;
            lastHCounter = 0x00;

            pauseButtonPressed = pauseButtonToggle = false;

            base.Reset();
        }

        public override bool CanLoadMedia(FileInfo mediaFile)
        {
            RomHeader romHeader = new RomHeader(File.ReadAllBytes(mediaFile.FullName));
            return (romHeader.IsSEGAStringCorrect && !romHeader.IsGameGear);
        }

        public override void LoadMedia(int slotNumber, IMedia media)
        {
            switch (slotNumber)
            {
                case 0: cartridge = media; break;
                case 1: card = media; break;
                default: throw new ArgumentException("Invalid slot number");
            }
        }

        public override void SaveMedia()
        {
            //
        }

        public override void Shutdown()
        {
            bootstrap?.Unload();
            cartridge?.Unload();
            card?.Unload();

            psg?.Shutdown();
        }

        public override void RunStep()
        {
            double currentCpuClockCycles = 0.0;
            currentCpuClockCycles += cpu.Step();

            double currentMasterClockCycles = (currentCpuClockCycles * 3.0);

            if (vdp.Step((int)Math.Round(currentMasterClockCycles)))
                OnRenderScreen(new RenderScreenEventArgs(TMS9918A.NumActivePixelsPerScanline, vdp.NumTotalScanlines, vdp.OutputFramebuffer));

            if (pauseButtonPressed)
            {
                pauseButtonPressed = false;
                cpu.SetNonMaskableInterruptLine(InterruptState.Assert);
            }

            cpu.SetInterruptLine(vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        protected override void ParseInput(PollInputEventArgs input)
        {
            portIoAB |= (byte)PortIoABButtons.Mask;
            portIoBMisc |= (byte)PortIoBMiscButtons.Mask;

            byte maskAB = 0, maskBMisc = 0;

            /* General */
            HandlePauseButton(input.Pressed.Contains(configuration.Pause));
            if (input.Pressed.Contains(configuration.Reset)) maskBMisc |= (byte)PortIoBMiscButtons.Reset;

            /* Player 1 */
            if (input.Pressed.Contains(configuration.P1Up)) maskAB |= (byte)PortIoABButtons.P1Up;
            if (input.Pressed.Contains(configuration.P1Down)) maskAB |= (byte)PortIoABButtons.P1Down;
            if (input.Pressed.Contains(configuration.P1Left)) maskAB |= (byte)PortIoABButtons.P1Left;
            if (input.Pressed.Contains(configuration.P1Right)) maskAB |= (byte)PortIoABButtons.P1Right;
            if (input.Pressed.Contains(configuration.P1Button1)) maskAB |= (byte)PortIoABButtons.P1Button1;
            if (input.Pressed.Contains(configuration.P1Button2)) maskAB |= (byte)PortIoABButtons.P1Button2;

            /* Player 2 */
            if (input.Pressed.Contains(configuration.P2Up)) maskAB |= (byte)PortIoABButtons.P2Up;
            if (input.Pressed.Contains(configuration.P2Down)) maskAB |= (byte)PortIoABButtons.P2Down;
            if (input.Pressed.Contains(configuration.P2Left)) maskBMisc |= (byte)PortIoBMiscButtons.P2Left;
            if (input.Pressed.Contains(configuration.P2Right)) maskBMisc |= (byte)PortIoBMiscButtons.P2Right;
            if (input.Pressed.Contains(configuration.P2Button1)) maskBMisc |= (byte)PortIoBMiscButtons.P2Button1;
            if (input.Pressed.Contains(configuration.P2Button2)) maskBMisc |= (byte)PortIoBMiscButtons.P2Button2;

            portIoAB &= (byte)~maskAB;
            portIoBMisc &= (byte)~maskBMisc;
        }

        private void HandlePauseButton(bool wasPressed)
        {
            bool pauseButtonHeld = (pauseButtonToggle && wasPressed);
            if (wasPressed)
            {
                if (!pauseButtonHeld) pauseButtonPressed = true;
                pauseButtonToggle = true;
            }
            else if (pauseButtonToggle)
                pauseButtonToggle = false;
        }

        private byte ReadMemory(ushort address)
        {
            if (address >= 0x0000 && address <= 0xBFFF)
            {
                if (isBootstrapRomEnabled && bootstrap != null)
                    return bootstrap.Read(address);

                else if (isCartridgeSlotEnabled && cartridge != null)
                    return cartridge.Read(address);

                else if (isCardSlotEnabled && card != null)
                    return card.Read(address);
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                if (isWorkRamEnabled)
                    return wram[address & (ramSize - 1)];
            }

            /* Cannot read from address (ex. for bootstrap, no usable media mapped), return 0 */
            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            if (isBootstrapRomEnabled) bootstrap?.Write(address, value);
            if (isCartridgeSlotEnabled) cartridge?.Write(address, value);
            if (isCardSlotEnabled) card?.Write(address, value);

            if (isWorkRamEnabled && address >= 0xC000 && address <= 0xFFFF)
                wram[address & (ramSize - 1)] = value;
        }

        private byte ReadPort(byte port)
        {
            port = (byte)(port & 0xC1);

            switch (port & 0xF0)
            {
                case 0x00:
                    /* Behave like SMS2 */
                    return 0xFF;

                case 0x40:
                    /* Counters */
                    if ((port & 0x01) == 0)
                        return vdp.ReadVCounter();      /* V counter */
                    else
                        return lastHCounter;            /* H counter */

                case 0x80:
                    /* VDP */
                    if ((port & 0x01) == 0)
                        return vdp.ReadDataPort();      /* Data port */
                    else
                        return vdp.ReadControlPort();   /* Status flags */

                case 0xC0:
                    if ((port & 0x01) == 0)
                        return portIoAB;                /* IO port A/B register */
                    else
                    {
                        /* IO port B/misc register */
                        if (configuration.IsExportSystem)
                        {
                            if (portIoControl == 0xF5)
                                return (byte)(portIoBMisc | 0xC0);
                            else
                                return (byte)(portIoBMisc & 0x3F);
                        }
                        else
                            return portIoBMisc;
                    }

                default:
                    throw new Exception(string.Format("SMS: Unsupported read from port 0x{0:X2}", port));
            }
        }

        public void WritePort(byte port, byte value)
        {
            port = (byte)(port & 0xC1);

            switch (port & 0xF0)
            {
                case 0x00:
                    /* System stuff */
                    if ((port & 0x01) == 0)
                        portMemoryControl = value;      /* Memory control */
                    else
                    {
                        /* I/O control */
                        if ((portIoControl & 0x0A) == 0x00 && ((value & 0x02) == 0x02 || (value & 0x08) == 0x08))
                            lastHCounter = vdp.ReadHCounter();
                        portIoControl = value;
                    }
                    break;

                case 0x40:
                    /* PSG */
                    psg.WriteData(value);
                    break;

                case 0x80:
                    /* VDP */
                    if ((port & 0x01) == 0)
                        vdp.WriteDataPort(value);       /* Data port */
                    else
                        vdp.WriteControlPort(value);    /* Control port */
                    break;

                case 0xC0:
                    /* No effect */
                    // TODO: proper implementation of SDSC debug console - http://www.smspower.org/Development/SDSCDebugConsoleSpecification
                    break;

                default:
                    throw new Exception(string.Format("SMS: Unsupported write to port 0x{0:X2}, value 0x{1:X2}", port, value));
            }
        }
    }
}
