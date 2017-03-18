using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.VideoBackend;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Machines.Sega.SC3000
{
    /* Keyboard stuff:
    /* - http://www.smspower.org/uploads/Development/sc3000h-20040729.txt
     * - https://sites.google.com/site/mavati56/sega_sf7000 
     */

    // TODO: make joypad work, too

    [TypeConverter(typeof(DescriptionTypeConverter))]
    public enum MachineInputs
    {
        //
    }

    public class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Sega SC-3000"; } }
        public override string FriendlyShortName { get { return "SC-3000"; } }
        public override string FileFilter { get { return "SC-3000 ROMs (*.sc)|*.sc"; } }
        public override double RefreshRate { get { return refreshRate; } }
        public override bool SupportsBootingWithoutMedia { get { return false; } }
        public override bool CanCurrentlyBootWithoutMedia { get { return false; } }
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
        const int ramSize = 1 * 2048;

        /* Clocks & refresh rate */
        double masterClock, refreshRate, cpuClock, vdpClock, psgClock;

        /* Devices on bus */
        IMedia cartridge;
        byte[] wram;
        Z80A cpu;
        TMS9918A vdp;
        SN76489 psg;
        i8255 ppi;
        bool[,] keyMatrix;

        enum KeyboardKeys
        {
            None = -1,

            D1 = (0 * 8), D2, D3, D4, D5, D6, D7, P1Up,
            Q = (8 * 1), W, E, R, T, Y, U, P1Down,
            A = (8 * 2), S, D, F, G, H, J, P1Left,
            Z = (8 * 3), X, C, V, B, N, M, P1Right,
            EngDiers = (8 * 4), Space, HomeClr, InsDel, Unmapped36, Unmapped37, Unmapped38, P1Button1,
            Comma = (8 * 5), Period, Slash, Pi, Down, Left, Right, P1Button2,
            K = (8 * 6), L, Semicolon, Colon, BracketClose, CR, Up, P2Up,
            I = (8 * 7), O, P, At, BracketOpen, Unmapped61, Unmapped62, P2Down,
            D8 = (8 * 8), D9, D0, Minus, Caret, Yen, Break, P2Left,
            Unmapped72 = (8 * 9), Unmapped73, Unmapped74, Unmapped75, Unmapped76, Unmapped77, Graph, P2Right,
            Unmapped80 = (8 * 10), Unmapped81, Unmapped82, Unmapped83, Unmapped84, Unmapped85, Ctrl, P2Button1,
            Unmapped88 = (8 * 11), Unmapped89, Unmapped90, Unmapped91, Unmapped92, Func, Shift, P2Button2
        }

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        Configuration configuration;

        public Manager()
        {
            configuration = new Configuration();

            cartridge = null;

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
            vdp = new TMS9918A(vdpClock, refreshRate, false);
            psg = new SN76489(psgClock, refreshRate, (s, e) => { OnAddSampleData(e); });
            ppi = new i8255();
            keyMatrix = new bool[12, 8];
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
            cpu.SetStackPointer(0xDFF0);
            psg.Reset();
            vdp.Reset();
            ppi.Reset();

            for (int i = 0; i < keyMatrix.GetLength(0); i++)
                for (int j = 0; j < keyMatrix.GetLength(1); j++)
                    keyMatrix[i, j] = false;

            OnScreenResize(new ScreenResizeEventArgs(TMS9918A.NumPixelsPerLine, vdp.NumScanlines));
            OnScreenViewportChange(new ScreenViewportChangeEventArgs(0, 0, TMS9918A.NumPixelsPerLine, vdp.NumScanlines));

            base.Reset();
        }

        public override void LoadMedia(IMedia media)
        {
            cartridge = media;
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
                OnRenderScreen(new RenderScreenEventArgs(TMS9918A.NumPixelsPerLine, vdp.NumScanlines, vdp.OutputFramebuffer));

            cpu.SetInterruptLine(vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        protected override void SetButtonData(PollInputEventArgs input)
        {
            //
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
            // TODO: simplify more

            switch (port & 0x60)
            {
                case 0x00:
                    /* PPI+VDP (unreliable, because VDP corrupts bits from PPI) */
                    return ppi.ReadPort((byte)(port & 0x03));

                case 0x20:
                    /* VDP */
                    if ((port & 0x01) == 0)
                        return vdp.ReadDataPort();      /* Data port */
                    else
                        return vdp.ReadControlPort();   /* Status flags */

                case 0x40:
                    /* PPI */
                    return ppi.ReadPort((byte)(port & 0x03));

                case 0x60:
                    // TODO: "Instruction referenced by R" ??
                    return 0x00;

                default:
                    // TODO: handle properly
                    return 0x00;
            }
        }

        public void WritePort(byte port, byte value)
        {
            // TODO: simplify more

            if ((port & 0x20) == 0)
            {
                ppi.WritePort((byte)(port & 0x03), value);
                UpdateKeyboard();
            }

            if ((port & 0x40) == 0)
                if ((port & 0x01) == 0)
                    vdp.WriteDataPort(value);
                else
                    vdp.WriteControlPort(value);

            if ((port & 0x80) == 0)
                psg.WriteData(value);
        }

        private void UpdateKeyboard()
        {
            int matrixRow = (ppi.PortCOutput & 0x07);
            if (matrixRow != 0x07)
            {
                byte rowStateA = 0xFF, rowStateB = 0xFF;

                for (int i = 0; i < 8; i++)
                    if (keyMatrix[i, matrixRow]) rowStateA &= (byte)~(1 << i);
                for (int i = 0; i < 4; i++)
                    if (keyMatrix[8 + i, matrixRow]) rowStateB &= (byte)~(1 << i);

                ppi.PortAInput = rowStateA;
                ppi.PortBInput = (byte)((ppi.PortBInput & 0xF0) | (rowStateB & 0x0F));
            }
        }
    }
}
