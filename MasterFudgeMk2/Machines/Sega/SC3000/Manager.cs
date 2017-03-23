using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using System.IO;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Machines.Sega.SC3000
{
    /* Keyboard stuff:
    /* - http://www.smspower.org/uploads/Development/sc3000h-20040729.txt
     * - https://sites.google.com/site/mavati56/sega_sf7000 
     */

    [TypeConverter(typeof(DescriptionTypeConverter))]
    public enum MachineInputs
    {
        [Description("Reset Key")]
        Reset,

        [Description("Number 1")]
        D1,
        [Description("Number 2")]
        D2,
        [Description("Number 3")]
        D3,
        [Description("Number 4")]
        D4,
        [Description("Number 5")]
        D5,
        [Description("Number 6")]
        D6,
        [Description("Number 7")]
        D7,
        [Description("Player 1: Up")]
        P1Up,
        [Description("Q Key")]
        Q,
        [Description("W Key")]
        W,
        [Description("E Key")]
        E,
        [Description("R Key")]
        R,
        [Description("T Key")]
        T,
        [Description("Y Key")]
        Y,
        [Description("U Key")]
        U,
        [Description("Player 1: Down")]
        P1Down,
        [Description("A Key")]
        A,
        [Description("S Key")]
        S,
        [Description("D Key")]
        D,
        [Description("F Key")]
        F,
        [Description("G Key")]
        G,
        [Description("H Key")]
        H,
        [Description("J Key")]
        J,
        [Description("Player 1: Left")]
        P1Left,
        [Description("Z Key")]
        Z,
        [Description("X Key")]
        X,
        [Description("C Key")]
        C,
        [Description("V Key")]
        V,
        [Description("B Key")]
        B,
        [Description("N Key")]
        N,
        [Description("M Key")]
        M,
        [Description("Player 1: Right")]
        P1Right,
        [Description("Eng/Dier's or Kana Key")]
        EngDiers,
        [Description("Spacebar")]
        Space,
        [Description("Home/Clr Key")]
        HomeClr,
        [Description("Ins/Del Key")]
        InsDel,
        [Description("Player 1: Button Left")]
        P1Button1,
        [Description("Comma Key")]
        Comma,
        [Description("Period Key")]
        Period,
        [Description("Slash Key")]
        Slash,
        [Description("Pi Key")]
        Pi,
        [Description("Cursor Down")]
        Down,
        [Description("Cursor Left")]
        Left,
        [Description("Cursor Right")]
        Right,
        [Description("Player 1: Button Right")]
        P1Button2,
        [Description("K Key")]
        K,
        [Description("L Key")]
        L,
        [Description("Semicolon Key")]
        Semicolon,
        [Description("Colon Key")]
        Colon,
        [Description("\"}\" Key")]
        BracketClose,
        [Description("CR (Return)")]
        CR,
        [Description("Cursor Up")]
        Up,
        [Description("Player 2: Up")]
        P2Up,
        [Description("I Key")]
        I,
        [Description("O Key")]
        O,
        [Description("P Key")]
        P,
        [Description("\"@\" Key")]
        At,
        [Description("\"[\" Key")]
        BracketOpen,
        [Description("Player 2: Down")]
        P2Down,
        [Description("Number 8")]
        D8,
        [Description("Number 9")]
        D9,
        [Description("Number 0")]
        D0,
        [Description("Minus Key")]
        Minus,
        [Description("Caret Key")]
        Caret,
        [Description("Yen Key")]
        Yen,
        [Description("Break Key")]
        Break,
        [Description("Player 2: Left")]
        P2Left,
        [Description("Graph Key")]
        Graph,
        [Description("Player 2: Right")]
        P2Right,
        [Description("Ctrl Key")]
        Ctrl,
        [Description("Player 2: Button Left")]
        P2Button1,
        [Description("Func Key")]
        Func,
        [Description("Shift Key")]
        Shift,
        [Description("Player 2: Button Right")]
        P2Button2
    }

    [TypeConverter(typeof(DescriptionTypeConverter))]
    public enum ExternalRamSizes
    {
        [Description("None")]
        ExtNone,
        [Description("2 Kilobyte")]
        Ext2Kilobyte,
        [Description("16 Kilobyte")]
        Ext16Kilobyte,
        [Description("32 Kilobyte")]
        Ext32Kilobyte
    }

    public class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Sega SC-3000"; } }
        public override string FriendlyShortName { get { return "SC-3000"; } }
        public override string FileFilter { get { return "SC-3000 ROMs (*.sc)|*.sc"; } }

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

        byte[] extRam;          /* Is *technically* provided by (Basic II/III) cartridges, but fudging it like this might be easier...? */

        enum KeyboardKeys
        {
            None = -1,

            D1 = (8 * 0), D2, D3, D4, D5, D6, D7, P1Up,
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
        bool[,] keyMatrix;

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        bool resetButtonPressed;

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

            int extRamSize;
            switch (configuration.ExternalRam)
            {
                case ExternalRamSizes.ExtNone: extRamSize = 0x0000; break;
                case ExternalRamSizes.Ext2Kilobyte: extRamSize = 0x0800; break;
                case ExternalRamSizes.Ext16Kilobyte: extRamSize = 0x4000; break;
                case ExternalRamSizes.Ext32Kilobyte: extRamSize = 0x8000; break;
                default: throw new Exception("Invalid external RAM size");
            }
            extRam = new byte[extRamSize];

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

            resetButtonPressed = false;

            base.Reset();
        }

        public override bool CanLoadMedia(FileInfo mediaFile)
        {
            return (mediaFile.Extension == ".sc");
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
                OnRenderScreen(new RenderScreenEventArgs(TMS9918A.NumActivePixelsPerScanline, vdp.NumTotalScanlines, vdp.OutputFramebuffer));

            if (resetButtonPressed)
            {
                resetButtonPressed = false;
                cpu.SetNonMaskableInterruptLine(InterruptState.Assert);
            }

            cpu.SetInterruptLine(vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        protected override void ParseInput(PollInputEventArgs input)
        {
            if (input.Pressed.Contains(configuration.Reset)) resetButtonPressed = true;

            // TODO: uhh
            SetKeyboardState(KeyboardKeys.D1, (input.Pressed.Contains(configuration.D1)));
            SetKeyboardState(KeyboardKeys.D2, (input.Pressed.Contains(configuration.D2)));
            SetKeyboardState(KeyboardKeys.D3, (input.Pressed.Contains(configuration.D3)));
            SetKeyboardState(KeyboardKeys.D4, (input.Pressed.Contains(configuration.D4)));
            SetKeyboardState(KeyboardKeys.D5, (input.Pressed.Contains(configuration.D5)));
            SetKeyboardState(KeyboardKeys.D6, (input.Pressed.Contains(configuration.D6)));
            SetKeyboardState(KeyboardKeys.D7, (input.Pressed.Contains(configuration.D7)));
            SetKeyboardState(KeyboardKeys.P1Up, (input.Pressed.Contains(configuration.P1Up)));
            SetKeyboardState(KeyboardKeys.Q, (input.Pressed.Contains(configuration.Q)));
            SetKeyboardState(KeyboardKeys.W, (input.Pressed.Contains(configuration.W)));
            SetKeyboardState(KeyboardKeys.E, (input.Pressed.Contains(configuration.E)));
            SetKeyboardState(KeyboardKeys.R, (input.Pressed.Contains(configuration.R)));
            SetKeyboardState(KeyboardKeys.T, (input.Pressed.Contains(configuration.T)));
            SetKeyboardState(KeyboardKeys.Y, (input.Pressed.Contains(configuration.Y)));
            SetKeyboardState(KeyboardKeys.U, (input.Pressed.Contains(configuration.U)));
            SetKeyboardState(KeyboardKeys.P1Down, (input.Pressed.Contains(configuration.P1Down)));
            SetKeyboardState(KeyboardKeys.A, (input.Pressed.Contains(configuration.A)));
            SetKeyboardState(KeyboardKeys.S, (input.Pressed.Contains(configuration.S)));
            SetKeyboardState(KeyboardKeys.D, (input.Pressed.Contains(configuration.D)));
            SetKeyboardState(KeyboardKeys.F, (input.Pressed.Contains(configuration.F)));
            SetKeyboardState(KeyboardKeys.G, (input.Pressed.Contains(configuration.G)));
            SetKeyboardState(KeyboardKeys.H, (input.Pressed.Contains(configuration.H)));
            SetKeyboardState(KeyboardKeys.J, (input.Pressed.Contains(configuration.J)));
            SetKeyboardState(KeyboardKeys.P1Left, (input.Pressed.Contains(configuration.P1Left)));
            SetKeyboardState(KeyboardKeys.Z, (input.Pressed.Contains(configuration.Z)));
            SetKeyboardState(KeyboardKeys.X, (input.Pressed.Contains(configuration.X)));
            SetKeyboardState(KeyboardKeys.C, (input.Pressed.Contains(configuration.C)));
            SetKeyboardState(KeyboardKeys.V, (input.Pressed.Contains(configuration.V)));
            SetKeyboardState(KeyboardKeys.B, (input.Pressed.Contains(configuration.B)));
            SetKeyboardState(KeyboardKeys.N, (input.Pressed.Contains(configuration.N)));
            SetKeyboardState(KeyboardKeys.M, (input.Pressed.Contains(configuration.M)));
            SetKeyboardState(KeyboardKeys.P1Right, (input.Pressed.Contains(configuration.P1Right)));
            SetKeyboardState(KeyboardKeys.EngDiers, (input.Pressed.Contains(configuration.EngDiers)));
            SetKeyboardState(KeyboardKeys.Space, (input.Pressed.Contains(configuration.Space)));
            SetKeyboardState(KeyboardKeys.HomeClr, (input.Pressed.Contains(configuration.HomeClr)));
            SetKeyboardState(KeyboardKeys.InsDel, (input.Pressed.Contains(configuration.InsDel)));
            SetKeyboardState(KeyboardKeys.P1Button1, (input.Pressed.Contains(configuration.P1Button1)));
            SetKeyboardState(KeyboardKeys.Comma, (input.Pressed.Contains(configuration.Comma)));
            SetKeyboardState(KeyboardKeys.Period, (input.Pressed.Contains(configuration.Period)));
            SetKeyboardState(KeyboardKeys.Slash, (input.Pressed.Contains(configuration.Slash)));
            SetKeyboardState(KeyboardKeys.Pi, (input.Pressed.Contains(configuration.Pi)));
            SetKeyboardState(KeyboardKeys.Down, (input.Pressed.Contains(configuration.Down)));
            SetKeyboardState(KeyboardKeys.Left, (input.Pressed.Contains(configuration.Left)));
            SetKeyboardState(KeyboardKeys.Right, (input.Pressed.Contains(configuration.Right)));
            SetKeyboardState(KeyboardKeys.P1Button2, (input.Pressed.Contains(configuration.P1Button2)));
            SetKeyboardState(KeyboardKeys.K, (input.Pressed.Contains(configuration.K)));
            SetKeyboardState(KeyboardKeys.L, (input.Pressed.Contains(configuration.L)));
            SetKeyboardState(KeyboardKeys.Semicolon, (input.Pressed.Contains(configuration.Semicolon)));
            SetKeyboardState(KeyboardKeys.Colon, (input.Pressed.Contains(configuration.Colon)));
            SetKeyboardState(KeyboardKeys.BracketClose, (input.Pressed.Contains(configuration.BracketClose)));
            SetKeyboardState(KeyboardKeys.CR, (input.Pressed.Contains(configuration.CR)));
            SetKeyboardState(KeyboardKeys.Up, (input.Pressed.Contains(configuration.Up)));
            SetKeyboardState(KeyboardKeys.P2Up, (input.Pressed.Contains(configuration.P2Up)));
            SetKeyboardState(KeyboardKeys.I, (input.Pressed.Contains(configuration.I)));
            SetKeyboardState(KeyboardKeys.O, (input.Pressed.Contains(configuration.O)));
            SetKeyboardState(KeyboardKeys.P, (input.Pressed.Contains(configuration.P)));
            SetKeyboardState(KeyboardKeys.At, (input.Pressed.Contains(configuration.At)));
            SetKeyboardState(KeyboardKeys.BracketOpen, (input.Pressed.Contains(configuration.BracketOpen)));
            SetKeyboardState(KeyboardKeys.P2Down, (input.Pressed.Contains(configuration.P2Down)));
            SetKeyboardState(KeyboardKeys.D8, (input.Pressed.Contains(configuration.D8)));
            SetKeyboardState(KeyboardKeys.D9, (input.Pressed.Contains(configuration.D9)));
            SetKeyboardState(KeyboardKeys.D0, (input.Pressed.Contains(configuration.D0)));
            SetKeyboardState(KeyboardKeys.Minus, (input.Pressed.Contains(configuration.Minus)));
            SetKeyboardState(KeyboardKeys.Caret, (input.Pressed.Contains(configuration.Caret)));
            SetKeyboardState(KeyboardKeys.Yen, (input.Pressed.Contains(configuration.Yen)));
            SetKeyboardState(KeyboardKeys.Break, (input.Pressed.Contains(configuration.Break)));
            SetKeyboardState(KeyboardKeys.P2Left, (input.Pressed.Contains(configuration.P2Left)));
            SetKeyboardState(KeyboardKeys.Graph, (input.Pressed.Contains(configuration.Graph)));
            SetKeyboardState(KeyboardKeys.P2Right, (input.Pressed.Contains(configuration.P2Right)));
            SetKeyboardState(KeyboardKeys.Ctrl, (input.Pressed.Contains(configuration.Ctrl)));
            SetKeyboardState(KeyboardKeys.P2Button1, (input.Pressed.Contains(configuration.P2Button1)));
            SetKeyboardState(KeyboardKeys.Func, (input.Pressed.Contains(configuration.Func)));
            SetKeyboardState(KeyboardKeys.Shift, (input.Pressed.Contains(configuration.Shift)));
            SetKeyboardState(KeyboardKeys.P2Button2, (input.Pressed.Contains(configuration.P2Button2)));
        }

        private void SetKeyboardState(KeyboardKeys key, bool state)
        {
            if (key == KeyboardKeys.None) return;
            keyMatrix[(int)key / 8, (int)key % 8] = state;
        }

        private void UpdateKeyboard()
        {
            int matrixRow = (ppi.PortCOutput & 0x07);
            byte rowStateA = 0xFF, rowStateB = 0xFF;

            for (int i = 0; i < 8; i++)
                if (keyMatrix[i, matrixRow]) rowStateA &= (byte)~(1 << i);
            for (int i = 0; i < 4; i++)
                if (keyMatrix[8 + i, matrixRow]) rowStateB &= (byte)~(1 << i);

            ppi.PortAInput = rowStateA;
            ppi.PortBInput = (byte)((ppi.PortBInput & 0xF0) | (rowStateB & 0x0F));
        }

        private byte ReadMemory(ushort address)
        {
            if (address >= 0x0000 && address <= 0x7FFF)
            {
                return (cartridge != null ? cartridge.Read(address) : (byte)0x00);
            }
            else
            {
                /* External RAM, huh? Smaller than 32k? */
                if (extRam.Length < 0x8000)
                {
                    if (extRam.Length > 0 && address >= 0x8000 && address <= 0xBFFF)
                    {
                        /* Inside external RAM area */
                        return extRam[address & (extRam.Length - 1)];
                    }
                    else if (address >= 0xC000 && address <= 0xFFFF)
                    {
                        /* Inside internal RAM area */
                        return wram[address & (ramSize - 1)];
                    }
                }
                else
                {
                    /* 32k external RAM means internal RAM isn't used */
                    return extRam[address & (extRam.Length - 1)];
                }
            }

            /* Cannot read from address*/
            return (byte)(address >> 8);
        }

        private void WriteMemory(ushort address, byte value)
        {
            if (address >= 0x0000 && address <= 0x7FFF)
            {
                cartridge?.Write(address, value);
            }
            else
            {
                /* External RAM again, check size */
                if (extRam.Length < 0x8000)
                {
                    if (extRam.Length > 0 && address >= 0x8000 && address <= 0xBFFF)
                    {
                        /* External RAM */
                        extRam[address & (extRam.Length - 1)] = value;
                    }
                    else if (address >= 0xC000 && address <= 0xFFFF)
                    {
                        /* Internal RAM */
                        wram[address & (ramSize - 1)] = value;
                    }
                }
                else
                {
                    /* No internal RAM visible */
                    extRam[address & (extRam.Length - 1)] = value;
                }
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
    }
}
