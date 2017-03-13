using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.AudioBackend;
using MasterFudgeMk2.Common.VideoBackend;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Machines.Nintendo.NES
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
        [Description("B Button")]
        B,
        [Description("A Button")]
        A,
        [Description("Select Button")]
        Select,
        [Description("Start Button")]
        Start
    }

    class Manager : IMachineManager
    {
        public string FriendlyName { get { return "Nintendo Entertainment System"; } }
        public string FriendlyShortName { get { return "NES"; } }
        public string FileFilter { get { return "NES ROMs (*.nes)|*.nes"; } }
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
                    { "CPU", cpu?.GetType(), cpuClock },
                    { "PPU", ppu?.GetType(), ppuClock },
                    { "APU", apu?.GetType(), apuClock }
                };
            }
        }

        public event EventHandler<ScreenResizeEventArgs> OnScreenResize;
        public event EventHandler<RenderScreenEventArgs> OnRenderScreen;
        public event EventHandler<ScreenViewportChangeEventArgs> OnScreenViewportChange;
        public event EventHandler<PollInputEventArgs> OnPollInput;
        public event EventHandler<AddSampleDataEventArgs> OnAddSampleData;

        /* Constants */
        const double masterClock = 21477272;
        const double refreshRate = 60.0988;
        const int ramSize = 1 * 2048;

        const double cpuClock = (masterClock / 12.0);
        const double ppuClock = (masterClock / 4.0);
        const double apuClock = cpuClock;

        IMedia cartridge;
        byte[] wram;
        MOS6502 cpu;
        object ppu;
        object apu;

        [Flags]
        enum JoyButtons : byte
        {
            A = (1 << 0),
            B = (1 << 1),
            Select = (1 << 2),
            Start = (1 << 3),
            Up = (1 << 4),
            Down = (1 << 5),
            Left = (1 << 6),
            Right = (1 << 7),
            Mask = ((1 << 8) - 1)
        }

        JoyButtons joyInput;
        byte joy1Data, joy2Data, joyStrobe;

        bool emulationPaused;
        int currentCyclesInLine, currentMasterClockCyclesInFrame;

        Configuration configuration;

        public Manager()
        {
            configuration = new Configuration();

            cartridge = null;

            cpu = new MOS6502(cpuClock, refreshRate, ReadMemory, WriteMemory);
            wram = new byte[ramSize];
            ppu = null;
            apu = null;
        }

        public void Startup()
        {
            cartridge?.Startup();

            cpu.Startup();
            //apu.Startup();
            //ppu.Startup();

            Reset();
        }

        public void Reset()
        {
            cartridge?.Reset();

            cpu.Reset();
            //apu.Reset();
            //ppu.Reset();

            joyInput = 0;
            joy1Data = joy2Data = joyStrobe = 0;

            emulationPaused = false;
            currentCyclesInLine = currentMasterClockCyclesInFrame = 0;

            OnScreenResize?.Invoke(this, new ScreenResizeEventArgs(256, 240));
            OnScreenViewportChange?.Invoke(this, new ScreenViewportChangeEventArgs(0, 0, 256, 240));
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

            //apu?.Shutdown();
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

            double currentMasterClockCycles = (currentCpuClockCycles * 12.0);
            /*
            if (vdp.Step((int)Math.Round(currentMasterClockCycles)))
                OnRenderScreen?.Invoke(this, new RenderScreenEventArgs(TMS9918A.NumPixelsPerLine, SegaSMS2VDP.NumVisibleLinesHigh, vdp.OutputFramebuffer));

            cpu.SetInterruptLine(vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));
            */
            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        private void SetButtonData(PollInputEventArgs input)
        {
            joyInput = 0;

            if (input.Pressed.Contains(configuration.Start)) joyInput |= JoyButtons.Start;
            if (input.Pressed.Contains(configuration.Select)) joyInput |= JoyButtons.Select;
            if (input.Pressed.Contains(configuration.Up)) joyInput |= JoyButtons.Up;
            if (input.Pressed.Contains(configuration.Down)) joyInput |= JoyButtons.Down;
            if (input.Pressed.Contains(configuration.Left)) joyInput |= JoyButtons.Left;
            if (input.Pressed.Contains(configuration.Right)) joyInput |= JoyButtons.Right;
            if (input.Pressed.Contains(configuration.B)) joyInput |= JoyButtons.B;
            if (input.Pressed.Contains(configuration.A)) joyInput |= JoyButtons.A;
        }

        private byte ReadMemory(ushort address)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                return wram[address & (ramSize - 1)];
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                // PPU read
            }
            else if (address >= 0x4000 && address <= 0x401F)
            {
                // APU & IO read
            }
            else if (address >= 0x4020 && address <= 0xFFFF)
            {
                if (cartridge != null)
                    return cartridge.Read(address);
            }

            /* Cannot read from address, return 0 */
            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                wram[address & (ramSize - 1)] = value;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                // PPU write
            }
            else if (address >= 0x4000 && address <= 0x401F)
            {
                // APU & IO write
            }
            else if (address >= 0x4020 && address <= 0xFFFF)
            {
                cartridge?.Write(address, value);
            }
        }
    }
}
