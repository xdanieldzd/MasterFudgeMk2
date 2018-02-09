using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.IO;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.Enumerations;
using MasterFudgeMk2.Common.EventArguments;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Media.Nintendo.NES;
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

    class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Nintendo Entertainment System"; } }
        public override string FriendlyShortName { get { return "NES"; } }
        public override string FileFilter { get { return "NES ROMs (*.nes)|*.nes"; } }
        public override string DatFileName { get { return "Nintendo - Nintendo Entertainment System.dat"; } }

        public override double RefreshRate { get { return refreshRate; } }
        public override float AspectRatio { get { return (576.0f / 486.0f); } }
        public override Rectangle ScreenViewport
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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
                    { "CPU", cpu?.GetType(), cpuClock },
                    { "PPU", ppu?.GetType(), ppuClock },
                    { "APU", apu?.GetType(), apuClock }
                };
            }
        }

        /* Constants */
        const double masterClock = 21477272;
        const double refreshRate = 60.0988;
        const int ramSize = 1 * 2048;

        const double cpuClock = (masterClock / 12.0);
        const double ppuClock = (masterClock / 4.0);
        const double apuClock = cpuClock;

        /* Devices on bus */
        BaseCartridge cartridge;
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

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        Configuration configuration;

        public Manager()
        {
            configuration = ConfigFile.Load<Configuration>();

            cartridge = null;

            cpu = new MOS6502(cpuClock, refreshRate, ReadMemory, WriteMemory);
            wram = new byte[ramSize];
            ppu = null;
            apu = null;
        }

        public override void Startup()
        {
            cpu.Startup();
            //apu.Startup();
            //ppu.Startup();

            Reset();
        }

        public override void Reset()
        {
            cartridge?.Reset();

            cpu.Reset();
            //apu.Reset();
            //ppu.Reset();

            joyInput = 0;
            joy1Data = joy2Data = joyStrobe = 0;

            base.Reset();
        }

        public override bool CanLoadMedia(FileInfo mediaFile)
        {
            using (FileStream file = mediaFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] header = new byte[0x10];
                file.Read(header, 0, header.Length);
                INesHeader inesHeader = new INesHeader(header);
                return inesHeader.IsSignatureCorrect;
            }
        }

        public override void LoadMedia(int slotNumber, IMedia media)
        {
            switch (slotNumber)
            {
                case 0: cartridge = (media as BaseCartridge); break;
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

            //apu?.Shutdown();
        }

        public override void RunStep()
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

        protected override void ParseInput(PollInputEventArgs input)
        {
            joyInput = 0;

            if (input.Pressed.Contains(configuration.P1Start)) joyInput |= JoyButtons.Start;
            if (input.Pressed.Contains(configuration.P1Select)) joyInput |= JoyButtons.Select;
            if (input.Pressed.Contains(configuration.P1Up)) joyInput |= JoyButtons.Up;
            if (input.Pressed.Contains(configuration.P1Down)) joyInput |= JoyButtons.Down;
            if (input.Pressed.Contains(configuration.P1Left)) joyInput |= JoyButtons.Left;
            if (input.Pressed.Contains(configuration.P1Right)) joyInput |= JoyButtons.Right;
            if (input.Pressed.Contains(configuration.P1B)) joyInput |= JoyButtons.B;
            if (input.Pressed.Contains(configuration.P1A)) joyInput |= JoyButtons.A;
        }

        private byte ReadMemory(uint address)
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

        private void WriteMemory(uint address, byte value)
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
