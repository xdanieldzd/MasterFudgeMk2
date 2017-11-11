using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Media.Sega;
using MasterFudgeMk2.Devices;
using MasterFudgeMk2.Devices.Sega;
using System.Drawing;

namespace MasterFudgeMk2.Machines.Sega.GameGear
{
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
        [Description("Button 1")]
        Button1,
        [Description("Button 2")]
        Button2,

        [Description("Reset Button")]
        Reset,
        [Description("Start Button")]
        Start
    }

    class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Sega Game Gear"; } }
        public override string FriendlyShortName { get { return "Game Gear"; } }
        public override string FileFilter { get { return "Game Gear ROMs (*.gg)|*.gg"; } }
        public override string DatFileName { get { return "Sega - Game Gear.dat"; } }

        public override double RefreshRate { get { return refreshRate; } }
        public override float AspectRatio { get { return (SegaGGVDP.ScreenViewportWidth / (float)SegaGGVDP.ScreenViewportHeight); } }
        public override Rectangle ScreenViewport { get { return new Rectangle(SegaGGVDP.ScreenViewportX, ((vdp.ScreenHeight / 2) - (SegaGGVDP.ScreenViewportHeight) / 2), SegaGGVDP.ScreenViewportWidth, SegaGGVDP.ScreenViewportHeight); } }

        public override bool SupportsBootingWithoutMedia { get { return false; } }
        public override bool CanCurrentlyBootWithoutMedia { get { return false; } }
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
        const int ramSize = 1 * 8192;

        const double cpuClock = (masterClock / 3.0);
        const double vdpClock = (masterClock / 1.0);
        const double psgClock = cpuClock;

        /* Devices on bus */
        IMedia bootstrap, cartridge;
        byte[] wram;
        Z80A cpu;
        SegaGGVDP vdp;
        SegaSMS2PSG psg;    // TODO: derive stereo-capable GG PSG when/if we get more accurate

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
        [Flags]
        enum PortIoCButtons : byte
        {
            Start = (1 << 7),
            Mask = (((Start << 1) - 1) - (Start - 1))
        }

        byte portMemoryControl, portIoControl, portIoAB, portIoBMisc;
        byte lastHCounter;

        byte portIoC, portParallelData, portDataDirNMI, portTxBuffer, portRxBuffer, portSerialControl, portStereoControl;

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        public bool isWorkRamEnabled { get { return !BitUtilities.IsBitSet(portMemoryControl, 4); } }
        public bool isBootstrapRomEnabled { get { return !BitUtilities.IsBitSet(portMemoryControl, 3); } }

        Configuration configuration;

        public Manager()
        {
            configuration = new Configuration();

            bootstrap = null;
            cartridge = null;

            cpu = new Z80A(cpuClock, refreshRate, ReadMemory, WriteMemory, ReadPort, WritePort);
            wram = new byte[ramSize];
            vdp = new SegaGGVDP(vdpClock, refreshRate);
            psg = new SegaSMS2PSG(psgClock, refreshRate, (s, e) => { OnAddSampleData(e); });
        }

        public override void Startup()
        {
            if (configuration.UseBootstrap && System.IO.File.Exists(configuration.BootstrapPath))
                bootstrap = MediaLoader.LoadMedia(this, new System.IO.FileInfo(configuration.BootstrapPath));

            cpu.Startup();
            psg.Startup();
            vdp.Startup();

            Reset();
        }

        public override void Reset()
        {
            bootstrap?.Reset();
            cartridge?.Reset();

            cpu.Reset();
            cpu.SetStackPointer(0xDFF0);
            psg.Reset();
            vdp.Reset();

            portMemoryControl = (byte)(bootstrap != null ? 0xA3 : 0x00);
            portIoControl = portIoAB = portIoBMisc = 0xFF;
            lastHCounter = 0x00;

            portIoC = (byte)(0x80 | (configuration.IsExportSystem ? 0x40 : 0x00));
            portParallelData = 0x00;
            portDataDirNMI = 0xFF;
            portTxBuffer = 0x00;
            portRxBuffer = 0xFF;
            portSerialControl = 0x00;
            portStereoControl = 0xFF;

            base.Reset();
        }

        public override bool CanLoadMedia(FileInfo mediaFile)
        {
            RomHeader romHeader = new RomHeader(File.ReadAllBytes(mediaFile.FullName));
            return (romHeader.IsSEGAStringCorrect && romHeader.IsGameGear);
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
            bootstrap?.Unload();
            cartridge?.Unload();

            psg?.Shutdown();
        }

        public override void RunStep()
        {
            double currentCpuClockCycles = 0.0;
            currentCpuClockCycles += cpu.Step();

            double currentMasterClockCycles = (currentCpuClockCycles * 3.0);

            if (vdp.Step((int)Math.Round(currentMasterClockCycles)))
            {
                OnScreenViewportChange(new ScreenViewportChangeEventArgs(ScreenViewport.X, ScreenViewport.Y, ScreenViewport.Width, ScreenViewport.Height));
                OnRenderScreen(new RenderScreenEventArgs(TMS9918A.NumActivePixelsPerScanline, vdp.NumTotalScanlines, vdp.OutputFramebuffer));
            }

            cpu.SetInterruptLine(vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        protected override void ParseInput(PollInputEventArgs input)
        {
            portIoAB |= (byte)PortIoABButtons.Mask;
            portIoBMisc |= (byte)PortIoBMiscButtons.Mask;
            portIoC |= (byte)PortIoCButtons.Mask;

            byte maskAB = 0, maskBMisc = 0, maskC = 0;

            if (input.Pressed.Contains(configuration.Start)) maskC |= (byte)PortIoCButtons.Start;
            if (input.Pressed.Contains(configuration.Up)) maskAB |= (byte)PortIoABButtons.P1Up;
            if (input.Pressed.Contains(configuration.Down)) maskAB |= (byte)PortIoABButtons.P1Down;
            if (input.Pressed.Contains(configuration.Left)) maskAB |= (byte)PortIoABButtons.P1Left;
            if (input.Pressed.Contains(configuration.Right)) maskAB |= (byte)PortIoABButtons.P1Right;
            if (input.Pressed.Contains(configuration.Button1)) maskAB |= (byte)PortIoABButtons.P1Button1;
            if (input.Pressed.Contains(configuration.Button2)) maskAB |= (byte)PortIoABButtons.P1Button2;

            portIoAB &= (byte)~maskAB;
            portIoBMisc &= (byte)~maskBMisc;
            portIoC &= (byte)~maskC;
        }

        private byte ReadMemory(ushort address)
        {
            if (address >= 0x0000 && address <= 0xBFFF)
            {
                if (address <= 0x0400 && isBootstrapRomEnabled && bootstrap != null)
                    return bootstrap.Read(address);

                else if (cartridge != null)
                    return cartridge.Read(address);
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
            cartridge?.Write(address, value);

            if (isWorkRamEnabled && address >= 0xC000 && address <= 0xFFFF)
                wram[address & (ramSize - 1)] = value;
        }

        private byte ReadPort(byte port)
        {
            byte maskedPort = (byte)(port & 0xC1);

            switch (maskedPort & 0xF0)
            {
                case 0x00:
                    /* GG-specific ports */
                    switch (port)
                    {
                        case 0x00: return (byte)((portIoC & 0xBF) | (configuration.IsExportSystem ? 0x40 : 0x00));
                        case 0x01: return portParallelData;
                        case 0x02: return portDataDirNMI;
                        case 0x03: return portTxBuffer;
                        case 0x04: return portRxBuffer;
                        case 0x05: return portSerialControl;
                        case 0x06: return 0xFF;
                    }
                    return 0xFF;

                case 0x40:
                    /* Counters */
                    if ((maskedPort & 0x01) == 0)
                        return vdp.ReadVCounter();      /* V counter */
                    else
                        return lastHCounter;            /* H counter */

                case 0x80:
                    /* VDP */
                    if ((maskedPort & 0x01) == 0)
                        return vdp.ReadDataPort();      /* Data port */
                    else
                        return vdp.ReadControlPort();   /* Status flags */

                case 0xC0:
                    if (port == 0xC0 || port == 0xDC)
                        return portIoAB;                /* IO port A/B register */
                    else if (port == 0xC1 || port == 0xDD)
                        return portIoBMisc;             /* IO port B/misc register */
                    else
                        return 0xFF;

                default:
                    throw new Exception(string.Format("GG: Unsupported read from port 0x{0:X2}", port));
            }
        }

        public void WritePort(byte port, byte value)
        {
            byte maskedPort = (byte)(port & 0xC1);

            switch (maskedPort & 0xF0)
            {
                case 0x00:
                    switch (port)
                    {
                        case 0x00: /* Read-only */ break;
                        case 0x01: portParallelData = value; break;
                        case 0x02: portDataDirNMI = value; break;
                        case 0x03: portTxBuffer = value; break;
                        case 0x04: /* Read-only? */; break;
                        case 0x05: portSerialControl = (byte)(value & 0xF8); break;
                        case 0x06: portStereoControl = value; break; // TODO: write to PSG
                        default:
                            /* System stuff */
                            if ((maskedPort & 0x01) == 0)
                                portMemoryControl = value;  /* Memory control */
                            else
                            {
                                /* I/O control */
                                if ((portIoControl & 0x0A) == 0x00 && ((value & 0x02) == 0x02 || (value & 0x08) == 0x08))
                                    lastHCounter = vdp.ReadHCounter();
                                portIoControl = value;
                            }
                            break;
                    }
                    break;

                case 0x40:
                    /* PSG */
                    psg.WriteData(value);
                    break;

                case 0x80:
                    /* VDP */
                    if ((maskedPort & 0x01) == 0)
                        vdp.WriteDataPort(value);       /* Data port */
                    else
                        vdp.WriteControlPort(value);    /* Control port */
                    break;

                case 0xC0:
                    /* No effect */
                    break;

                default:
                    throw new Exception(string.Format("SMS: Unsupported write to port 0x{0:X2}, value 0x{1:X2}", port, value));
            }
        }
    }
}
