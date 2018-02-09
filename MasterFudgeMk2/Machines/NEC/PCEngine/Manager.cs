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
using MasterFudgeMk2.Devices.NEC;

namespace MasterFudgeMk2.Machines.NEC.PCEngine
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
        [Description("Button I")]
        Button1,
        [Description("Button II")]
        Button2,
        [Description("Run")]
        Run,
        [Description("Select")]
        Select
    }

    class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "NEC PC-Engine"; } }
        public override string FriendlyShortName { get { return "PC-Engine"; } }
        public override string FileFilter { get { return "PC-Engine ROMs (*.pce)|*.pce"; } }
        public override string DatFileName { get { return "NEC - PC Engine - TurboGrafx 16.dat"; } }

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
                throw new NotImplementedException();
            }
        }

        /* Constants */
        const double masterClock = 21477272;
        const double refreshRate = 60.000000;   // TODO: exact?
        const int wramSize = 1 * 8192;
        const int bramSize = 1 * 8192;

        /* Devices on bus */
        IMedia cartridge;
        byte[] wram;
        byte[] bram;
        HuC6280 cpu;
        HuC6260 vce;
        object vdc;
        object psg;
        byte[] mpr;

        [Flags]
        enum PortIoButtons : byte
        {
            Up = 0x01,
            Right = 0x02,
            Down = 0x04,
            Left = 0x08,

            Button1 = 0x01,
            Button2 = 0x02,
            Select = 0x04,
            Run = 0x08
        }

        double cpuClock, timerClock, psgClock;

        byte portIo, ioBuffer;
        bool ioSelect, ioClear;

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        Configuration configuration;

        public Manager()
        {
            configuration = ConfigFile.Load<Configuration>();

            cartridge = null;

            wram = new byte[wramSize];
            bram = new byte[bramSize];
            cpu = new HuC6280(cpuClock, refreshRate, ReadMemory, WriteMemory, ReadPagingRegister, WritePagingRegister, ChangeClockSpeed);
            vce = new HuC6260();
            vdc = null;//new xxx(timerClock, refreshRate, false);
            psg = null;//new xxx(psgClock, refreshRate, 44100, 2, (s, e) => { OnAddSampleData(e); });

            mpr = new byte[0x08];
        }

        public override void Startup()
        {
            cpu.Startup();
            vce.Startup();
            //psg.Startup();
            //vdc.Startup();

            Reset();
        }

        public override void Reset()
        {
            int random = Environment.TickCount;
            for (byte i = 0x00; i < 0x07; i++)
                mpr[i] = (byte)(random = ((random * 1103515245) + 12345) & 0x7FFFFFFF); /* https://stackoverflow.com/a/1537970 */
            mpr[0x07] = 0x00;

            cartridge?.Reset();

            cpu.Reset();
            vce.Reset();
            //psg.Reset();
            //vdc.Reset();

            ChangeClockSpeed(false);

            portIo = 0xFF;
            ioSelect = ioClear = false;

            base.Reset();
        }

        public override bool CanLoadMedia(FileInfo mediaFile)
        {
            // TODO: detection
            return true;
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

            //psg?.Shutdown();
        }

        private void ChangeClockSpeed(bool fast)
        {
            cpuClock = (masterClock / (fast ? 3.0 : 12.0));
            timerClock = (masterClock / 3.0);
            psgClock = (masterClock / 6.0);
        }

        public override void RunStep()
        {
            //
        }

        protected override void ParseInput(PollInputEventArgs input)
        {
            //
        }

        private byte ReadMemory(uint address)
        {
            byte bank = (byte)(mpr[(address >> 13) & 0x07] & 0xFF);
            byte retVal = 0xFF;

            if (bank <= 0x7F)
            {
                /* ROM */
                if (cartridge != null)
                    retVal = cartridge.Read(((uint)(bank << 13) | (address & 0x1FFF)));
            }
            else if (bank == 0xF7)
            {
                /* BRAM */
                retVal = bram[(address & (bram.Length - 1))];
            }
            else if (bank == 0xF8)
            {
                /* WRAM */
                retVal = wram[(address & (wram.Length - 1))];
            }
            else if (bank == 0xFF)
            {
                /* I/O */
                switch (address & 0x1C00)
                {
                    case 0x0000:    /* VDC */
                        //retVal = VDC.ReadIO(address);
                        break;
                    case 0x0400:    /* VCE */
                        retVal = vce.Read(address);
                        break;
                    case 0x0800:    /* PSG */
                        break;
                    case 0x0C00:    /* Timer */
                        //retVal = CPU.ReadTimer(address);
                        ioBuffer = retVal;
                        break;
                    case 0x1000:    /* Joypad */
                        //retVal = ReadJoyIO();
                        ioBuffer = retVal;
                        break;
                    case 0x1400:    /* IRQ */
                        //retVal = CPU.ReadIRQStatus(address);
                        ioBuffer = retVal;
                        break;
                }
            }

            return retVal;
        }

        private void WriteMemory(uint address, byte value)
        {
            byte bank = (byte)(mpr[(address >> 13) & 0x07] & 0xFF);

            if (bank == 0xF7)
            {
                /* BRAM */
                bram[(address & (bram.Length - 1))] = value;
            }
            else if (bank == 0xF8)
            {
                /* WRAM */
                wram[(address & (wram.Length - 1))] = value;
            }
            else if (bank == 0xFF)
            {
                /* I/O */
                switch (address & 0x1C00)
                {
                    case 0x0000:
                        /* VDC */
                        //vdc.Write(address, value);
                        break;
                    case 0x0400:
                        /* VCE */
                        vce.Write(address, value);
                        break;
                    case 0x0800:
                        /* PSG */
                        //psg.Write(address, value);
                        ioBuffer = value;
                        break;
                    case 0x0C00:
                        /* Timer */
                        //cpu.WriteTimer(address, value);
                        ioBuffer = value;
                        break;
                    case 0x1000:
                        /* Joypad */
                        //WriteJoyIO(value);
                        ioBuffer = value;
                        break;
                    case 0x1400:
                        /* IRQ */
                        //cpu.WriteIRQStatus(address, value);
                        ioBuffer = value;
                        break;
                }
            }
        }

        private byte ReadPagingRegister(byte register)
        {
            return mpr[register & 0x07];
        }

        private void WritePagingRegister(byte register, byte value)
        {
            mpr[register & 0x07] = value;
        }
    }
}
