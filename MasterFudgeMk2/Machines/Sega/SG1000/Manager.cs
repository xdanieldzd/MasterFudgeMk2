using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using NAudio.Wave;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Machines.Sega.SG1000
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

        [Description("Pause Button")]
        Pause
    }

    class Manager : IMachineManager
    {
        public string FriendlyName { get { return "Sega SG-1000"; } }
        public string FriendlyShortName { get { return "SG-1000"; } }
        public string FileFilter { get { return "SG-1000 ROMs (*.sg)|*.sg"; } }
        public double RefreshRate { get { return refreshRate; } }
        public bool SupportsBootingWithoutMedia { get { return false; } }
        public bool CanCurrentlyBootWithoutMedia { get { return false; } }
        public IWaveProvider WaveProvider { get { return (psg as IWaveProvider); } }
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

        public event ScreenResizeHandler OnScreenResize;
        public event RenderScreenHandler OnRenderScreen;
        public event ScreenViewportChangeHandler OnScreenViewportChange;
        public event PollInputHandler OnPollInput;

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
            Mask = ((1 << 4) - 1)
        }

        byte portIoAB, portIoBMisc;

        bool emulationPaused;
        int currentCyclesInLine, currentMasterClockCyclesInFrame;
        bool pauseButtonPressed, pauseButtonToggle;

        Configuration configuration;

        public Manager()
        {
            configuration = new Configuration();
            /*configuration.Pause = Common.XInput.Buttons.Start;
            configuration.Reset = Common.XInput.Buttons.Back;
            configuration.P1Up = Common.XInput.Buttons.DPadUp;
            configuration.P1Down = Common.XInput.Buttons.DPadDown;
            configuration.P1Left = Common.XInput.Buttons.DPadLeft;
            configuration.P1Right = Common.XInput.Buttons.DPadRight;
            configuration.P1Button1 = Common.XInput.Buttons.X;
            configuration.P1Button2 = Common.XInput.Buttons.A;
            */
            cartridge = null;

            cpu = new Z80A(cpuClock, refreshRate, ReadMemory, WriteMemory, ReadPort, WritePort);
            wram = new byte[ramSize];
            vdp = new TMS9918A(vdpClock, refreshRate, false);
            psg = new SN76489(psgClock, refreshRate, 44100);
        }

        public void Startup()
        {
            cartridge?.Startup();

            cpu.Startup();
            psg.Startup();
            vdp.Startup();

            Reset();
        }

        public void Reset()
        {
            cartridge?.Reset();

            cpu.Reset();
            cpu.SetStackPointer(0xDFF0);
            psg.Reset();
            vdp.Reset();

            portIoAB = portIoBMisc = 0xFF;

            emulationPaused = false;
            currentCyclesInLine = currentMasterClockCyclesInFrame = 0;
            pauseButtonPressed = pauseButtonToggle = false;

            OnScreenResize?.Invoke(this, new ScreenResizeEventArgs(TMS9918A.NumPixelsPerLine, TMS9918A.NumVisibleLines));
            OnScreenViewportChange?.Invoke(this, new ScreenViewportChangeEventArgs(0, 0, TMS9918A.NumPixelsPerLine, TMS9918A.NumVisibleLines));
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

        private void Step()
        {
            double currentCpuClockCycles = 0.0;
            currentCpuClockCycles += cpu.Step();

            double currentMasterClockCycles = (currentCpuClockCycles * 3.0);

            if (vdp.Step((int)Math.Round(currentMasterClockCycles)))
                OnRenderScreen?.Invoke(this, new RenderScreenEventArgs(TMS9918A.NumPixelsPerLine, TMS9918A.NumVisibleLines, vdp.OutputFramebuffer));

            if (pauseButtonPressed)
            {
                pauseButtonPressed = false;
                cpu.SetNonMaskableInterruptLine(InterruptState.Assert);
            }

            cpu.SetInterruptLine(vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        public void Pause()
        {
            emulationPaused = true;
        }

        public void Unpause()
        {
            emulationPaused = false;
        }

        private void SetButtonData(PollInputEventArgs input)
        {
            portIoAB |= (byte)PortIoABButtons.Mask;
            portIoBMisc |= (byte)PortIoBMiscButtons.Mask;

            byte maskAB = 0, maskBMisc = 0;

            /* General */
            HandlePauseButton(input.Pressed.Contains(configuration.Pause));

            /* Player 1 */
            if (input.Pressed.Contains(configuration.P1Up)) maskAB |= (byte)PortIoABButtons.P1Up;
            if (input.Pressed.Contains(configuration.P1Down)) maskAB |= (byte)PortIoABButtons.P1Down;
            if (input.Pressed.Contains(configuration.P1Left)) maskAB |= (byte)PortIoABButtons.P1Left;
            if (input.Pressed.Contains(configuration.P1Right)) maskAB |= (byte)PortIoABButtons.P1Right;
            if (input.Pressed.Contains(configuration.P1Button1)) maskAB |= (byte)PortIoABButtons.P1Button1;
            if (input.Pressed.Contains(configuration.P1Button2)) maskAB |= (byte)PortIoABButtons.P1Button2;

            /* Player 2 */
            if (input.Pressed.Contains(MachineInputs.P2Up)) maskAB |= (byte)PortIoABButtons.P2Up;
            if (input.Pressed.Contains(MachineInputs.P2Down)) maskAB |= (byte)PortIoABButtons.P2Down;
            if (input.Pressed.Contains(MachineInputs.P2Left)) maskBMisc |= (byte)PortIoBMiscButtons.P2Left;
            if (input.Pressed.Contains(MachineInputs.P2Right)) maskBMisc |= (byte)PortIoBMiscButtons.P2Right;
            if (input.Pressed.Contains(MachineInputs.P2Button1)) maskBMisc |= (byte)PortIoBMiscButtons.P2Button1;
            if (input.Pressed.Contains(MachineInputs.P2Button2)) maskBMisc |= (byte)PortIoBMiscButtons.P2Button2;

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

        private int HandleInterrupts()
        {

            return 0;
        }

        private byte ReadMemory(ushort address)
        {
            if (address >= 0x0000 && address <= 0xBFFF)
            {
                return (cartridge != null ? cartridge.Read(address) : (byte)0x00);
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                return wram[address & (ramSize - 1)];
            }

            /* Cannot read from address, return 0 */
            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0xBFFF)
            {
                cartridge?.Write(address, value);
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                wram[address & (ramSize - 1)] = value;
            }
        }

        private byte ReadPort(byte port)
        {
            port = (byte)(port & 0xC1);

            switch (port & 0xF0)
            {
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
                        return portIoBMisc;             /* IO port B/misc register */

                default:
                    // TODO: handle properly
                    return 0x00;
            }
        }

        public void WritePort(byte port, byte value)
        {
            port = (byte)(port & 0xC1);

            switch (port & 0xF0)
            {
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

                default:
                    // TODO: handle properly
                    break;
            }
        }
    }
}
