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
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Machines.Nintendo.SuperNintendo
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
        [Description("Player 1: L")]
        P1L,
        [Description("Player 1: R")]
        P1R,
        [Description("Player 1: B")]
        P1B,
        [Description("Player 1: A")]
        P1A,
        [Description("Player 1: Y")]
        P1Y,
        [Description("Player 1: X")]
        P1X,

        [Description("Reset Button")]
        Reset
    }

    class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Super Nintendo"; } }
        public override string FriendlyShortName { get { return "SNES"; } }
        public override string FileFilter { get { return "Super Nintendo ROMs (*.smc)|*.smc"; } }
        public override string DatFileName { get { return "Nintendo - Super Nintendo Entertainment System.dat"; } }

        public override double RefreshRate { get { return refreshRate[clockIndex]; } }
        public override float AspectRatio { get { return (!configuration.IsPalSystem ? (576.0f / 486.0f) : (720.0f / 486.0f)); } }
        public override Rectangle ScreenViewport
        {
            get
            {
                // viewport
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
                    //chips
                };
            }
        }

        /* Constants */
        static readonly double[] crystal = { 21477270.0, 17734475.0 };
        static readonly double[] colorClock = { (crystal[0] / 6.0), (crystal[1] / 4.0) };
        static readonly double[] masterClock = { (crystal[0] / 1.0), (crystal[1] * 6.0 / 5.0) };
        static readonly double[] dotClock = { (crystal[0] / 4.0), (masterClock[1] / 4.0) };
        static readonly double[] cpuClockNoWait = { (crystal[0] / 6.0), (masterClock[1] / 6.0) };
        static readonly double[] cpuClockShortWait = { (crystal[0] / 8.0), (masterClock[1] / 8.0) };
        static readonly double[] cpuClockJoyWait = { (crystal[0] / 8.0), (masterClock[1] / 8.0) };
        static readonly double[] refreshRate = { (crystal[0] / (262.0 * 1364.0 - 4.0 / 2.0)), (masterClock[1] / (312.0 * 1364.0)) };
        static readonly double[] interlace = { (crystal[0] / (525.0 * 1364.0)), (masterClock[1] / (625.0 * 1364.0 + 4.0 / 2.0)) };

        const int ramSize = 1 * 8192;

        /* Devices on bus */
        // cpu, ppu, etc

        // ioport flags or w/e

        // ports

        int clockIndex { get { return (configuration.IsPalSystem ? 1 : 0); } }

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock[clockIndex] / refreshRate[clockIndex]); } }

        // port enable/disable checks, if applicable...?

        Configuration configuration;

        public Manager()
        {
            configuration = ConfigFile.Load<Configuration>();

            // null cart etc

            // create devices
        }

        public override void Startup()
        {
            // startup devices

            Reset();
        }

        public override void Reset()
        {
            // reset devices

            base.Reset();
        }

        public override bool CanLoadMedia(FileInfo mediaFile)
        {
            // check file

            return true;
        }

        public override void LoadMedia(int slotNumber, IMedia media)
        {
            // load media
        }

        public override void SaveMedia()
        {
            //
        }

        public override void Shutdown()
        {
            // unload/shutdown
        }

        public override void RunStep()
        {
            double currentCpuClockCycles = 0.0;
            // step cpu
            double currentMasterClockCycles = 0.0;

            // step other devices
            // check reset, interrupts, etc

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        protected override void ParseInput(PollInputEventArgs input)
        {
            // do input
        }

        private byte ReadMemory(ushort address)
        {
            // read memory
            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            // write memory
        }

        private byte ReadPort(byte port)
        {
            // read ports
            return 0x00;
        }

        public void WritePort(byte port, byte value)
        {
            // write ports
        }
    }
}
