using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.AudioBackend;
using MasterFudgeMk2.Common.VideoBackend;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Media.Sega;
using MasterFudgeMk2.Devices;
using MasterFudgeMk2.Devices.Sega;

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

    class Manager : IMachineManager
    {
        public string FriendlyName { get { return "Sega Game Gear"; } }
        public string FriendlyShortName { get { return "Game Gear"; } }
        public string FileFilter { get { return "Game Gear ROMs (*.gg)|*.gg"; } }
        public double RefreshRate { get { return refreshRate; } }
        public bool SupportsBootingWithoutMedia { get { return false; } }
        public bool CanCurrentlyBootWithoutMedia { get { return false; } }
        public MachineConfiguration Configuration { get { return configuration; } set { configuration = (value as Configuration); } }

        public List<Tuple<string, Type, double>> DebugChipInformation
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

        public event EventHandler<ScreenResizeEventArgs> OnScreenResize;
        public event EventHandler<RenderScreenEventArgs> OnRenderScreen;
        public event EventHandler<ScreenViewportChangeEventArgs> OnScreenViewportChange;
        public event EventHandler<PollInputEventArgs> OnPollInput;
        public event EventHandler<AddSampleDataEventArgs> OnAddSampleData;

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
            Mask = ((1 << 8) - 1)
        }
        [Flags]
        enum PortIoBMiscButtons : byte
        {
            P2Left = (1 << 0),
            P2Right = (1 << 1),
            P2Button1 = (1 << 2),
            P2Button2 = (1 << 3),
            Reset = (1 << 4),
            Mask = ((1 << 5) - 1)
        }
        [Flags]
        enum PortIoCButtons : byte
        {
            Start = (1 << 7),
            Mask = ((1 << 8) - 1)
        }

        byte portMemoryControl, portIoControl, portIoAB, portIoBMisc;
        byte lastHCounter;

        byte portIoC, portParallelData, portDataDirNMI, portTxBuffer, portRxBuffer, portSerialControl, portStereoControl;

        bool emulationPaused;
        int currentCyclesInLine, currentMasterClockCyclesInFrame;

        public bool isWorkRamEnabled { get { return !Utilities.IsBitSet(portMemoryControl, 4); } }
        public bool isBootstrapRomEnabled { get { return !Utilities.IsBitSet(portMemoryControl, 3); } }

        Configuration configuration;

        public Manager()
        {
            configuration = new Configuration();
            //configuration.BootstrapPath = @"D:\ROMs\GG\majbios.gg";
            //configuration.UseBootstrap = true;
            //configuration.IsExportSystem = true;
            /*configuration.Start = Common.XInput.Buttons.Start;
            configuration.Up = Common.XInput.Buttons.DPadUp;
            configuration.Down = Common.XInput.Buttons.DPadDown;
            configuration.Left = Common.XInput.Buttons.DPadLeft;
            configuration.Right = Common.XInput.Buttons.DPadRight;
            configuration.Button1 = Common.XInput.Buttons.A;
            configuration.Button2 = Common.XInput.Buttons.X;
            */
            bootstrap = null;
            cartridge = null;

            cpu = new Z80A(cpuClock, refreshRate, ReadMemory, WriteMemory, ReadPort, WritePort);
            wram = new byte[ramSize];
            vdp = new SegaGGVDP(vdpClock, refreshRate);
            psg = new SegaSMS2PSG(psgClock, refreshRate, (s, e) => { OnAddSampleData?.Invoke(s, e); });
        }

        public bool CanLoadMedia(FileInfo mediaFile)
        {
            RomHeaderSMSGG romHeader = new RomHeaderSMSGG(File.ReadAllBytes(mediaFile.FullName));
            return (romHeader.IsSEGAStringCorrect && romHeader.IsGameGear);
        }

        public void Startup()
        {
            if (configuration.UseBootstrap && File.Exists(configuration.BootstrapPath))
                bootstrap = MediaLoader.LoadMedia(this, new FileInfo(configuration.BootstrapPath));

            bootstrap?.Startup();
            cartridge?.Startup();

            cpu.Startup();
            psg.Startup();
            vdp.Startup();

            Reset();
        }

        public void Reset()
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

            emulationPaused = false;
            currentCyclesInLine = currentMasterClockCyclesInFrame = 0;

            OnScreenResize?.Invoke(this, new ScreenResizeEventArgs(TMS9918A.NumPixelsPerLine, SegaSMS2VDP.NumVisibleLinesHigh));
            OnScreenViewportChange?.Invoke(this, new ScreenViewportChangeEventArgs(SegaGGVDP.ScreenViewportX, ((SegaSMS2VDP.NumVisibleLinesHigh / 2) - (SegaGGVDP.ScreenViewportHeight) / 2), SegaGGVDP.ScreenViewportWidth, SegaGGVDP.ScreenViewportHeight));
        }

        public void LoadMedia(IMedia media)
        {
            cartridge = media;
        }

        public void SaveMedia()
        {
            //
        }

        public void Shutdown()
        {
            bootstrap?.Unload();
            cartridge?.Unload();

            psg?.Shutdown();
        }

        public void Run()
        {
            if (!emulationPaused)
            {
                PollInputEventArgs pollInputEventArgs = new PollInputEventArgs();
                OnPollInput?.Invoke(this, pollInputEventArgs);
                SetButtonData(pollInputEventArgs);

                while (currentMasterClockCyclesInFrame < (int)Math.Round(masterClock / refreshRate))
                    Step();

                currentMasterClockCyclesInFrame -= (int)Math.Round(masterClock / refreshRate);
            }
        }

        public void Pause()
        {
            emulationPaused = true;
        }

        public void Unpause()
        {
            emulationPaused = false;
        }

        private void Step()
        {
            double currentCpuClockCycles = 0.0;
            currentCpuClockCycles += cpu.Step();

            double currentMasterClockCycles = (currentCpuClockCycles * 3.0);

            if (vdp.Step((int)Math.Round(currentMasterClockCycles)))
                OnRenderScreen?.Invoke(this, new RenderScreenEventArgs(TMS9918A.NumPixelsPerLine, SegaSMS2VDP.NumVisibleLinesHigh, vdp.OutputFramebuffer));

            cpu.SetInterruptLine(vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        private void SetButtonData(PollInputEventArgs input)
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
