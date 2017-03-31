﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Drawing;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Media.MSX;
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Machines.Various.MSX2
{
    [TypeConverter(typeof(DescriptionTypeConverter))]
    public enum MachineInputs
    {
        [Description("Joystick 1: Up")]
        J1Up,
        [Description("Joystick 1: Down")]
        J1Down,
        [Description("Joystick 1: Left")]
        J1Left,
        [Description("Joystick 1: Right")]
        J1Right,
        [Description("Joystick 1: Trigger A")]
        J1TriggerA,
        [Description("Joystick 1: Trigger B")]
        J1TriggerB,

        [Description("Number 0")]
        D0,
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

        [Description("Number 8")]
        D8,
        [Description("Number 9")]
        D9,

        [Description("Minus Key")]
        Minus,
        [Description("Equals Key")]
        EqualSign,
        [Description("Backslash Key")]
        Backslash,
        [Description("\"[\" Key")]
        BracketOpen,
        [Description("\"]\" Key")]
        BracketClose,
        [Description("Semicolon Key")]
        Semicolon,

        [Description("Grave Key")]
        Grave,
        [Description("Apostrophe Key")]
        Apostrophe,
        [Description("Comma Key")]
        Comma,
        [Description("Period Key")]
        Period,
        [Description("Slash Key")]
        Slash,
        [Description("Dead Key/Modifier")]
        DeadKey,
        [Description("A Key")]
        A,
        [Description("B Key")]
        B,

        [Description("C Key")]
        C,
        [Description("D Key")]
        D,
        [Description("E Key")]
        E,
        [Description("F Key")]
        F,
        [Description("G Key")]
        G,
        [Description("H Key")]
        H,
        [Description("I Key")]
        I,
        [Description("J Key")]
        J,

        [Description("K Key")]
        K,
        [Description("L Key")]
        L,
        [Description("M Key")]
        M,
        [Description("N Key")]
        N,
        [Description("O Key")]
        O,
        [Description("P Key")]
        P,
        [Description("Q Key")]
        Q,
        [Description("R Key")]
        R,

        [Description("S Key")]
        S,
        [Description("T Key")]
        T,
        [Description("U Key")]
        U,
        [Description("V Key")]
        V,
        [Description("W Key")]
        W,
        [Description("X Key")]
        X,
        [Description("Y Key")]
        Y,
        [Description("Z Key")]
        Z,

        [Description("Shift Key")]
        Shift,
        [Description("Ctrl Key")]
        Ctrl,
        [Description("Graph Key")]
        Graph,
        [Description("Cap Key")]
        Cap,
        [Description("Code Key")]
        Code,
        [Description("F1-6 Key")]
        F1,
        [Description("F2-7 Key")]
        F2,
        [Description("F3-8 Key")]
        F3,

        [Description("F4-9 Key")]
        F4,
        [Description("F5-10 Key")]
        F5,
        [Description("Esc Key")]
        Esc,
        [Description("Tab Key")]
        Tab,
        [Description("Stop Key")]
        Stop,
        [Description("Backspace Key")]
        BS,
        [Description("Select Key")]
        Select,
        [Description("Return Key")]
        Return,

        [Description("Spacebar")]
        Space,
        [Description("Home Key")]
        Home,
        [Description("Ins Key")]
        Ins,
        [Description("Del Key")]
        Del,
        [Description("Cursor Left")]
        Left,
        [Description("Cursor Up")]
        Up,
        [Description("Cursor Down")]
        Down,
        [Description("Cursor Right")]
        Right,

        [Description("Numpad: Multiply")]
        NumMultiply,
        [Description("Numpad: Plus")]
        NumPlus,
        [Description("Numpad: Divide")]
        NumDivide,
        [Description("Numpad: 0")]
        Num0,
        [Description("Numpad: 1")]
        Num1,
        [Description("Numpad: 2")]
        Num2,
        [Description("Numpad: 3")]
        Num3,
        [Description("Numpad: 4")]
        Num4,

        [Description("Numpad: 5")]
        Num5,
        [Description("Numpad: 6")]
        Num6,
        [Description("Numpad: 7")]
        Num7,
        [Description("Numpad: 8")]
        Num8,
        [Description("Numpad: 9")]
        Num9,
        [Description("Numpad: Minus")]
        NumMinus,
        [Description("Numpad: Comma")]
        NumComma,
        [Description("Numpad: Period")]
        NumPeriod
    }

    [TypeConverter(typeof(DescriptionTypeConverter))]
    public enum InternalRamSizes
    {
        [Description("64 Kilobyte")]
        Int64Kilobyte,
        [Description("128 Kilobyte")]
        Int128Kilobyte,
        [Description("256 Kilobyte")]
        Int256Kilobyte
    }

    class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Generic MSX2"; } }
        public override string FriendlyShortName { get { return "MSX2"; } }
        public override string FileFilter { get { return "MSX2 ROMs (*.mx2;*.rom)|*.mx2;*.rom"; } }

        public override double RefreshRate { get { return refreshRate; } }
        public override float AspectRatio { get { return (576.0f / 486.0f); } }
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
        public override bool CanCurrentlyBootWithoutMedia { get { return File.Exists(configuration.BiosPath); } }
        public override string[] MediaSlots { get { return new string[] { "Cartridge Slot A", "Cartridge Slot B" }; } }

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

        const double cpuClock = (masterClock / 3.0);
        const double vdpClock = (masterClock / 1.0);
        const double psgClock = cpuClock;

        /* Devices on bus */
        IMedia cartridgeA, cartridgeB, subRom, diskRom;
        int ramSize;
        byte[] wram;
        Z80A cpu;
        V9938 vdp;
        AY38910 psg;
        i8255 ppi;

        byte[] bios;

        byte[] secondarySlots, ramBankNumbers;
        delegate byte ReadMemoryPageDelegate(ushort address);
        delegate void WriteMemoryPageDelegate(ushort address, byte value);

        ReadMemoryPageDelegate[] readMemoryPages;
        WriteMemoryPageDelegate[] writeMemoryPages;

        enum JoystickButtons
        {
            Up = (1 << 0),
            Down = (1 << 1),
            Left = (1 << 2),
            Right = (1 << 3),
            TriggerA = (1 << 4),
            TriggerB = (1 << 5),
            Mask = (((TriggerB << 1) - 1) - (Up - 1))
        }

        enum KeyboardKeys
        {
            /* Column 0-7 */
            D0 = (8 * 0), D1, D2, D3, D4, D5, D6, D7,                                               /* Line 0 */
            D8 = (8 * 1), D9, Minus, EqualSign, Backslash, BracketOpen, BracketClose, Semicolon,    /* Line 1 */
            Grave = (8 * 2), Apostrophe, Comma, Period, Slash, DeadKey, A, B,                       /* Line 2 */
            C = (8 * 3), D, E, F, G, H, I, J,                                                       /* Line 3 */
            K = (8 * 4), L, M, N, O, P, Q, R,                                                       /* Line 4 */
            S = (8 * 5), T, U, V, W, X, Y, Z,                                                       /* Line 5 */
            Shift = (8 * 6), Ctrl, Graph, Cap, Code, F1, F2, F3,                                    /* Line 6 */
            F4 = (8 * 7), F5, Esc, Tab, Stop, BS, Select, Return,                                   /* Line 7 */
            Space = (8 * 8), Home, Ins, Del, Left, Up, Down, Right,                                 /* Line 8 */
            NumMultiply = (8 * 9), NumPlus, NumDivide, Num0, Num1, Num2, Num3, Num4,                /* Line 9 */
            Num5 = (8 * 10), Num6, Num7, Num8, Num9, NumMinus, NumComma, NumPeriod                  /* Line 10 */
        }
        bool[,] keyMatrix;

        byte portSystemControl, portAVControl;

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        Configuration configuration;

        public Manager()
        {
            configuration = new Configuration();

            switch (configuration.InternalRam)
            {
                case InternalRamSizes.Int64Kilobyte: ramSize = 1 * 65536; break;
                case InternalRamSizes.Int128Kilobyte: ramSize = 1 * 131070; break;
                case InternalRamSizes.Int256Kilobyte: ramSize = 1 * 262140; break;
                default: throw new Exception("Invalid internal RAM size");
            }

            cartridgeA = null;
            cartridgeB = null;
            subRom = null;
            diskRom = null;

            cpu = new Z80A(cpuClock, refreshRate, ReadMemory, WriteMemory, ReadPort, WritePort);
            wram = new byte[ramSize];
            vdp = new V9938(vdpClock, refreshRate, false);
            psg = new AY38910(psgClock, refreshRate, (s, e) => { OnAddSampleData(e); });
            ppi = new i8255();

            keyMatrix = new bool[11, 8];

            if (CanCurrentlyBootWithoutMedia)
                bios = File.ReadAllBytes(configuration.BiosPath);

            if (File.Exists(configuration.SubRomPath))
                subRom = MediaLoader.LoadMedia(this, new FileInfo(configuration.SubRomPath));

            if (File.Exists(configuration.DiskRomPath))
                diskRom = MediaLoader.LoadMedia(this, new FileInfo(configuration.DiskRomPath));

            secondarySlots = new byte[4];
            ramBankNumbers = new byte[4];

            readMemoryPages = new ReadMemoryPageDelegate[4];
            readMemoryPages[0] = ReadMemoryPage0;
            readMemoryPages[1] = ReadMemoryPage1;
            readMemoryPages[2] = ReadMemoryPage2;
            readMemoryPages[3] = ReadMemoryPage3;

            writeMemoryPages = new WriteMemoryPageDelegate[4];
            writeMemoryPages[0] = WriteMemoryPage0;
            writeMemoryPages[1] = WriteMemoryPage1;
            writeMemoryPages[2] = WriteMemoryPage2;
            writeMemoryPages[3] = WriteMemoryPage3;
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
            cartridgeA?.Reset();
            cartridgeB?.Reset();
            diskRom?.Reset();
            subRom?.Reset();

            cpu.Reset();
            psg.Reset();
            vdp.Reset();
            ppi.Reset();

            for (int i = 0; i < keyMatrix.GetLength(0); i++)
                for (int j = 0; j < keyMatrix.GetLength(1); j++)
                    keyMatrix[i, j] = false;

            portSystemControl = portAVControl = 0x00;

            base.Reset();
        }

        public override bool CanLoadMedia(FileInfo mediaFile)
        {
            RomHeader romHeader = new RomHeader(File.ReadAllBytes(mediaFile.FullName));
            return (mediaFile.Extension == ".mx2" && romHeader.IsValidCartridge);
        }

        public override void LoadMedia(int slotNumber, IMedia media)
        {
            switch (slotNumber)
            {
                case 0: cartridgeA = media; break;
                case 1: cartridgeB = media; break;
                default: throw new ArgumentException("Invalid slot number");
            }
        }

        public override void SaveMedia()
        {
            //
        }

        public override void Shutdown()
        {
            cartridgeA?.Unload();
            cartridgeB?.Unload();
            diskRom?.Unload();
            subRom?.Unload();

            psg?.Shutdown();
        }

        public override void RunStep()
        {
            double currentCpuClockCycles = 0.0;
            currentCpuClockCycles += cpu.Step();

            double currentMasterClockCycles = (currentCpuClockCycles * 3.0);

            if (vdp.Step((int)Math.Round(currentMasterClockCycles)))
                OnRenderScreen(new RenderScreenEventArgs(TMS9918A.NumActivePixelsPerScanline, vdp.NumTotalScanlines, vdp.OutputFramebuffer));

            cpu.SetInterruptLine(vdp.InterruptLine);

            psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        protected override void ParseInput(PollInputEventArgs input)
        {
            /* Joystick */
            // TODO: joystick select in IO port B
            byte joyData = 0x00;
            if (input.Pressed.Contains(configuration.J1Up)) joyData |= (byte)JoystickButtons.Up;
            if (input.Pressed.Contains(configuration.J1Down)) joyData |= (byte)JoystickButtons.Down;
            if (input.Pressed.Contains(configuration.J1Left)) joyData |= (byte)JoystickButtons.Left;
            if (input.Pressed.Contains(configuration.J1Right)) joyData |= (byte)JoystickButtons.Right;
            if (input.Pressed.Contains(configuration.J1TriggerA)) joyData |= (byte)JoystickButtons.TriggerA;
            if (input.Pressed.Contains(configuration.J1TriggerB)) joyData |= (byte)JoystickButtons.TriggerB;
            joyData = (byte)(~joyData & 0x7F);
            psg.WriteDataDirect(0x0E, joyData);

            /* Keyboard */
            SetKeyboardState(KeyboardKeys.D0, (input.Pressed.Contains(configuration.D0)));
            SetKeyboardState(KeyboardKeys.D1, (input.Pressed.Contains(configuration.D1)));
            SetKeyboardState(KeyboardKeys.D2, (input.Pressed.Contains(configuration.D2)));
            SetKeyboardState(KeyboardKeys.D3, (input.Pressed.Contains(configuration.D3)));
            SetKeyboardState(KeyboardKeys.D4, (input.Pressed.Contains(configuration.D4)));
            SetKeyboardState(KeyboardKeys.D5, (input.Pressed.Contains(configuration.D5)));
            SetKeyboardState(KeyboardKeys.D6, (input.Pressed.Contains(configuration.D6)));
            SetKeyboardState(KeyboardKeys.D7, (input.Pressed.Contains(configuration.D7)));
            SetKeyboardState(KeyboardKeys.D8, (input.Pressed.Contains(configuration.D8)));
            SetKeyboardState(KeyboardKeys.D9, (input.Pressed.Contains(configuration.D9)));
            SetKeyboardState(KeyboardKeys.Minus, (input.Pressed.Contains(configuration.Minus)));
            SetKeyboardState(KeyboardKeys.EqualSign, (input.Pressed.Contains(configuration.EqualSign)));
            SetKeyboardState(KeyboardKeys.Backslash, (input.Pressed.Contains(configuration.Backslash)));
            SetKeyboardState(KeyboardKeys.BracketOpen, (input.Pressed.Contains(configuration.BracketOpen)));
            SetKeyboardState(KeyboardKeys.BracketClose, (input.Pressed.Contains(configuration.BracketClose)));
            SetKeyboardState(KeyboardKeys.Semicolon, (input.Pressed.Contains(configuration.Semicolon)));
            SetKeyboardState(KeyboardKeys.Grave, (input.Pressed.Contains(configuration.Grave)));
            SetKeyboardState(KeyboardKeys.Apostrophe, (input.Pressed.Contains(configuration.Apostrophe)));
            SetKeyboardState(KeyboardKeys.Comma, (input.Pressed.Contains(configuration.Comma)));
            SetKeyboardState(KeyboardKeys.Period, (input.Pressed.Contains(configuration.Period)));
            SetKeyboardState(KeyboardKeys.Slash, (input.Pressed.Contains(configuration.Slash)));
            SetKeyboardState(KeyboardKeys.DeadKey, (input.Pressed.Contains(configuration.DeadKey)));
            SetKeyboardState(KeyboardKeys.A, (input.Pressed.Contains(configuration.A)));
            SetKeyboardState(KeyboardKeys.B, (input.Pressed.Contains(configuration.B)));
            SetKeyboardState(KeyboardKeys.C, (input.Pressed.Contains(configuration.C)));
            SetKeyboardState(KeyboardKeys.D, (input.Pressed.Contains(configuration.D)));
            SetKeyboardState(KeyboardKeys.E, (input.Pressed.Contains(configuration.E)));
            SetKeyboardState(KeyboardKeys.F, (input.Pressed.Contains(configuration.F)));
            SetKeyboardState(KeyboardKeys.G, (input.Pressed.Contains(configuration.G)));
            SetKeyboardState(KeyboardKeys.H, (input.Pressed.Contains(configuration.H)));
            SetKeyboardState(KeyboardKeys.I, (input.Pressed.Contains(configuration.I)));
            SetKeyboardState(KeyboardKeys.J, (input.Pressed.Contains(configuration.J)));
            SetKeyboardState(KeyboardKeys.K, (input.Pressed.Contains(configuration.K)));
            SetKeyboardState(KeyboardKeys.L, (input.Pressed.Contains(configuration.L)));
            SetKeyboardState(KeyboardKeys.M, (input.Pressed.Contains(configuration.M)));
            SetKeyboardState(KeyboardKeys.N, (input.Pressed.Contains(configuration.N)));
            SetKeyboardState(KeyboardKeys.O, (input.Pressed.Contains(configuration.O)));
            SetKeyboardState(KeyboardKeys.P, (input.Pressed.Contains(configuration.P)));
            SetKeyboardState(KeyboardKeys.Q, (input.Pressed.Contains(configuration.Q)));
            SetKeyboardState(KeyboardKeys.R, (input.Pressed.Contains(configuration.R)));
            SetKeyboardState(KeyboardKeys.S, (input.Pressed.Contains(configuration.S)));
            SetKeyboardState(KeyboardKeys.T, (input.Pressed.Contains(configuration.T)));
            SetKeyboardState(KeyboardKeys.U, (input.Pressed.Contains(configuration.U)));
            SetKeyboardState(KeyboardKeys.V, (input.Pressed.Contains(configuration.V)));
            SetKeyboardState(KeyboardKeys.W, (input.Pressed.Contains(configuration.W)));
            SetKeyboardState(KeyboardKeys.X, (input.Pressed.Contains(configuration.X)));
            SetKeyboardState(KeyboardKeys.Y, (input.Pressed.Contains(configuration.Y)));
            SetKeyboardState(KeyboardKeys.Z, (input.Pressed.Contains(configuration.Z)));
            SetKeyboardState(KeyboardKeys.Shift, (input.Pressed.Contains(configuration.Shift)));
            SetKeyboardState(KeyboardKeys.Ctrl, (input.Pressed.Contains(configuration.Ctrl)));
            SetKeyboardState(KeyboardKeys.Graph, (input.Pressed.Contains(configuration.Graph)));
            SetKeyboardState(KeyboardKeys.Cap, (input.Pressed.Contains(configuration.Cap)));
            SetKeyboardState(KeyboardKeys.Code, (input.Pressed.Contains(configuration.Code)));
            SetKeyboardState(KeyboardKeys.F1, (input.Pressed.Contains(configuration.F1)));
            SetKeyboardState(KeyboardKeys.F2, (input.Pressed.Contains(configuration.F2)));
            SetKeyboardState(KeyboardKeys.F3, (input.Pressed.Contains(configuration.F3)));
            SetKeyboardState(KeyboardKeys.F4, (input.Pressed.Contains(configuration.F4)));
            SetKeyboardState(KeyboardKeys.F5, (input.Pressed.Contains(configuration.F5)));
            SetKeyboardState(KeyboardKeys.Esc, (input.Pressed.Contains(configuration.Esc)));
            SetKeyboardState(KeyboardKeys.Tab, (input.Pressed.Contains(configuration.Tab)));
            SetKeyboardState(KeyboardKeys.Stop, (input.Pressed.Contains(configuration.Stop)));
            SetKeyboardState(KeyboardKeys.BS, (input.Pressed.Contains(configuration.BS)));
            SetKeyboardState(KeyboardKeys.Select, (input.Pressed.Contains(configuration.Select)));
            SetKeyboardState(KeyboardKeys.Return, (input.Pressed.Contains(configuration.Return)));
            SetKeyboardState(KeyboardKeys.Space, (input.Pressed.Contains(configuration.Space)));
            SetKeyboardState(KeyboardKeys.Home, (input.Pressed.Contains(configuration.Home)));
            SetKeyboardState(KeyboardKeys.Ins, (input.Pressed.Contains(configuration.Ins)));
            SetKeyboardState(KeyboardKeys.Del, (input.Pressed.Contains(configuration.Del)));
            SetKeyboardState(KeyboardKeys.Left, (input.Pressed.Contains(configuration.Left)));
            SetKeyboardState(KeyboardKeys.Up, (input.Pressed.Contains(configuration.Up)));
            SetKeyboardState(KeyboardKeys.Down, (input.Pressed.Contains(configuration.Down)));
            SetKeyboardState(KeyboardKeys.Right, (input.Pressed.Contains(configuration.Right)));
            SetKeyboardState(KeyboardKeys.NumMultiply, (input.Pressed.Contains(configuration.NumMultiply)));
            SetKeyboardState(KeyboardKeys.NumPlus, (input.Pressed.Contains(configuration.NumPlus)));
            SetKeyboardState(KeyboardKeys.NumDivide, (input.Pressed.Contains(configuration.NumDivide)));
            SetKeyboardState(KeyboardKeys.Num0, (input.Pressed.Contains(configuration.Num0)));
            SetKeyboardState(KeyboardKeys.Num1, (input.Pressed.Contains(configuration.Num1)));
            SetKeyboardState(KeyboardKeys.Num2, (input.Pressed.Contains(configuration.Num2)));
            SetKeyboardState(KeyboardKeys.Num3, (input.Pressed.Contains(configuration.Num3)));
            SetKeyboardState(KeyboardKeys.Num4, (input.Pressed.Contains(configuration.Num4)));
            SetKeyboardState(KeyboardKeys.Num5, (input.Pressed.Contains(configuration.Num5)));
            SetKeyboardState(KeyboardKeys.Num6, (input.Pressed.Contains(configuration.Num6)));
            SetKeyboardState(KeyboardKeys.Num7, (input.Pressed.Contains(configuration.Num7)));
            SetKeyboardState(KeyboardKeys.Num8, (input.Pressed.Contains(configuration.Num8)));
            SetKeyboardState(KeyboardKeys.Num9, (input.Pressed.Contains(configuration.Num9)));
            SetKeyboardState(KeyboardKeys.NumMinus, (input.Pressed.Contains(configuration.NumMinus)));
            SetKeyboardState(KeyboardKeys.NumComma, (input.Pressed.Contains(configuration.NumComma)));
            SetKeyboardState(KeyboardKeys.NumPeriod, (input.Pressed.Contains(configuration.NumPeriod)));
        }

        private void SetKeyboardState(KeyboardKeys key, bool state)
        {
            keyMatrix[(int)key / 8, (int)key % 8] = state;
        }

        private void UpdateKeyboard()
        {
            int matrixLine = (ppi.PortCOutput & 0x0F);

            byte rowState = 0xFF;
            for (int i = 0; i < keyMatrix.GetLength(1); i++)
                if (keyMatrix[matrixLine, i]) rowState &= (byte)~(1 << i);

            ppi.PortBInput = rowState;
        }

        /*              0-3   4-7   8-B   C-F   */
        /* PSLOT=0 ->   BIOS  BASIC N/A   N/A   */
        /* PSLOT=1 ->   CartA CartA CartA CartA */
        /* PSLOT=2 ->   CartB CartB CartB CartB */

        /* PSLOT=3 ->   ...                     */
        /*   SSLOT=0 -> N/A   N/A   N/A   N/A   */
        /*   SSLOT=1 -> SUB   DISK  N/A   N/A   */
        /*   SSLOT=2 -> RAM3  RAM2  RAM1  RAM0  */
        /*   SSLOT=3 -> N/A   N/A   N/A   N/A   */

        private byte ReadMemory(ushort address)
        {
            return readMemoryPages[address >> 14](address);
        }

        private byte ReadMemoryPage0(ushort address)
        {
            byte primarySlot = (byte)((ppi.ReadPort(0xA8) >> 0) & 0x03);
            byte secondarySlot = secondarySlots[0];

            if (primarySlot == 0x00)
            {
                /* BIOS */
                return (bios != null ? bios[address & (bios.Length - 1)] : (byte)0x00);
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                if (cartridgeA != null) return cartridgeA.Read(address);
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
                if (cartridgeB != null) return cartridgeB.Read(address);
            }
            else if (primarySlot == 0x03)
            {
                if (secondarySlot == 0x00)
                {
                    /* N/A */
                    return 0x00;
                }
                else if (secondarySlot == 0x01)
                {
                    /* SUB ROM */
                    if (subRom != null) return subRom.Read(address);
                }
                else if (secondarySlot == 0x02)
                {
                    /* RAM3 */
                    int ramAddress = ((ramBankNumbers[0] << 15) | (address & 0x3FFF));
                    return wram[ramAddress & (ramSize - 1)];
                }
                else if (secondarySlot == 0x03)
                {
                    /* N/A */
                    return 0x00;
                }
            }
            return 0x00;
        }

        private byte ReadMemoryPage1(ushort address)
        {
            byte primarySlot = (byte)((ppi.ReadPort(0xA8) >> 2) & 0x03);
            byte secondarySlot = secondarySlots[1];

            if (primarySlot == 0x00)
            {
                /* BASIC */
                return (bios != null ? bios[address & (bios.Length - 1)] : (byte)0x00);
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                if (cartridgeA != null) return cartridgeA.Read(address);
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
                if (cartridgeB != null) return cartridgeB.Read(address);
            }
            else if (primarySlot == 0x03)
            {
                if (secondarySlot == 0x00)
                {
                    /* N/A */
                    return 0x00;
                }
                else if (secondarySlot == 0x01)
                {
                    /* DISK ROM */
                    if (diskRom != null) return diskRom.Read(address);
                }
                else if (secondarySlot == 0x02)
                {
                    /* RAM2 */
                    int ramAddress = ((ramBankNumbers[1] << 15) | (address & 0x3FFF));
                    return wram[ramAddress & (ramSize - 1)];
                }
                else if (secondarySlot == 0x03)
                {
                    /* N/A */
                    return 0x00;
                }
            }
            return 0x00;
        }

        private byte ReadMemoryPage2(ushort address)
        {
            byte primarySlot = (byte)((ppi.ReadPort(0xA8) >> 4) & 0x03);
            byte secondarySlot = secondarySlots[2];

            if (primarySlot == 0x00)
            {
                /* N/A */
                return 0x00;
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                if (cartridgeA != null) return cartridgeA.Read(address);
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
                if (cartridgeB != null) return cartridgeB.Read(address);
            }
            else if (primarySlot == 0x03)
            {
                if (secondarySlot == 0x00)
                {
                    /* N/A */
                    return 0x00;
                }
                else if (secondarySlot == 0x01)
                {
                    /* N/A */
                    return 0x00;
                }
                else if (secondarySlot == 0x02)
                {
                    /* RAM1 */
                    int ramAddress = ((ramBankNumbers[2] << 15) | (address & 0x3FFF));
                    return wram[ramAddress & (ramSize - 1)];
                }
                else if (secondarySlot == 0x03)
                {
                    /* N/A */
                    return 0x00;
                }
            }
            return 0x00;
        }

        private byte ReadMemoryPage3(ushort address)
        {
            byte primarySlot = (byte)((ppi.ReadPort(0xA8) >> 6) & 0x03);
            byte secondarySlot = secondarySlots[3];

            if (primarySlot == 0x00)
            {
                /* N/A */
                return 0x00;
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                if (cartridgeA != null) return cartridgeA.Read(address);
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
                if (cartridgeB != null) return cartridgeB.Read(address);
            }
            else if (primarySlot == 0x03)
            {
                if (secondarySlot == 0x00)
                {
                    /* N/A */
                    return 0x00;
                }
                else if (secondarySlot == 0x01)
                {
                    /* N/A */
                    return 0x00;
                }
                else if (secondarySlot == 0x02)
                {
                    /* RAM0 */
                    int ramAddress = ((ramBankNumbers[3] << 15) | (address & 0x3FFF));
                    return wram[ramAddress & (ramSize - 1)];
                }
                else if (secondarySlot == 0x03)
                {
                    /* N/A */
                    return 0x00;
                }
            }
            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            writeMemoryPages[address >> 14](address, value);
        }

        private void WriteMemoryPage0(ushort address, byte value)
        {
            byte primarySlot = (byte)((ppi.ReadPort(0xA8) >> 0) & 0x03);
            byte secondarySlot = secondarySlots[0];

            if (primarySlot == 0x00)
            {
                /* BIOS -- can't write */
                return;
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                cartridgeA?.Write(address, value);
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
                cartridgeB?.Write(address, value);
            }
            else if (primarySlot == 0x03)
            {
                if (secondarySlot == 0x00)
                {
                    /* N/A */
                    return;
                }
                else if (secondarySlot == 0x01)
                {
                    /* SUB ROM */
                    subRom?.Write(address, value);
                    return;
                }
                else if (secondarySlot == 0x02)
                {
                    /* RAM3 */
                    int ramAddress = ((ramBankNumbers[0] << 15) | (address & 0x3FFF));
                    wram[ramAddress & (ramSize - 1)] = value;
                }
                else if (secondarySlot == 0x03)
                {
                    /* N/A */
                    return;
                }
            }
        }

        private void WriteMemoryPage1(ushort address, byte value)
        {
            byte primarySlot = (byte)((ppi.ReadPort(0xA8) >> 2) & 0x03);
            byte secondarySlot = secondarySlots[1];

            if (primarySlot == 0x00)
            {
                /* BASIC -- can't write */
                return;
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                cartridgeA?.Write(address, value);
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
                cartridgeB?.Write(address, value);
            }
            else if (primarySlot == 0x03)
            {
                if (secondarySlot == 0x00)
                {
                    /* N/A */
                    return;
                }
                else if (secondarySlot == 0x01)
                {
                    /* DISK ROM */
                    diskRom?.Write(address, value);
                    return;
                }
                else if (secondarySlot == 0x02)
                {
                    /* RAM2 */
                    int ramAddress = ((ramBankNumbers[1] << 15) | (address & 0x3FFF));
                    wram[ramAddress & (ramSize - 1)] = value;
                }
                else if (secondarySlot == 0x03)
                {
                    /* N/A */
                    return;
                }
            }
        }

        private void WriteMemoryPage2(ushort address, byte value)
        {
            byte primarySlot = (byte)((ppi.ReadPort(0xA8) >> 4) & 0x03);
            byte secondarySlot = secondarySlots[2];

            if (primarySlot == 0x00)
            {
                /* N/A -- can't write */
                return;
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                cartridgeA?.Write(address, value);
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
                cartridgeB?.Write(address, value);
            }
            else if (primarySlot == 0x03)
            {
                if (secondarySlot == 0x00)
                {
                    /* N/A */
                    return;
                }
                else if (secondarySlot == 0x01)
                {
                    /* N/A */
                    return;
                }
                else if (secondarySlot == 0x02)
                {
                    /* RAM1 */
                    int ramAddress = ((ramBankNumbers[2] << 15) | (address & 0x3FFF));
                    wram[ramAddress & (ramSize - 1)] = value;
                }
                else if (secondarySlot == 0x03)
                {
                    /* N/A */
                    return;
                }
            }
        }

        private void WriteMemoryPage3(ushort address, byte value)
        {
            byte primarySlot = (byte)((ppi.ReadPort(0xA8) >> 6) & 0x03);
            byte secondarySlot = secondarySlots[3];

            if (primarySlot == 0x00)
            {
                /* N/A */
                return;
            }
            else if (primarySlot == 0x01)
            {
                /* Cartridge A */
                cartridgeA?.Write(address, value);
            }
            else if (primarySlot == 0x02)
            {
                /* Cartridge B */
                cartridgeB?.Write(address, value);
            }
            else if (primarySlot == 0x03)
            {
                if (secondarySlot == 0x00)
                {
                    /* N/A */
                    return;
                }
                else if (secondarySlot == 0x01)
                {
                    /* N/A */
                    return;
                }
                else if (secondarySlot == 0x02)
                {
                    /* RAM0 */
                    int ramAddress = ((ramBankNumbers[0] << 15) | (address & 0x3FFF));
                    wram[ramAddress & (ramSize - 1)] = value;
                }
                else if (secondarySlot == 0x03)
                {
                    /* N/A */
                    return;
                }
            }

            /* Secondary slot */
            if (address == 0xFFFF)
            {
                secondarySlots[0] = (byte)(((value >> 0) ^ 0xFF) & 0x03);
                secondarySlots[1] = (byte)(((value >> 2) ^ 0xFF) & 0x03);
                secondarySlots[2] = (byte)(((value >> 4) ^ 0xFF) & 0x03);
                secondarySlots[3] = (byte)(((value >> 6) ^ 0xFF) & 0x03);
            }
        }

        private byte ReadPort(byte port)
        {
            switch (port)
            {
                /* VDP */
                case 0x98: return vdp.ReadDataPort();               /* Data port */
                case 0x99: return vdp.ReadControlPort();            /* Status flags */

                /* PSG */
                case 0xA2: return psg.ReadData();                   /* Data */

                /* PPI */
                case 0xA8:                                              /* Port A (PSLOT register) */
                case 0xA9:                                              /* Port B (Keyboard matrix line status) */
                case 0xAA: UpdateKeyboard(); return ppi.ReadPort(port); /* Port C (Keyboard, cassette, etc.) */

                /* Special ports */
                case 0xF7: return portAVControl;                    /* A/V control */

                /* Memory mapper */
                case 0xFC: return ramBankNumbers[0];                /* RAM bank for 0000-3FFF */
                case 0xFD: return ramBankNumbers[1];                /* RAM bank for 4000-7FFF */
                case 0xFE: return ramBankNumbers[2];                /* RAM bank for 8000-BFFF */
                case 0xFF: return ramBankNumbers[3];                /* RAM bank for C000-FFFF */

                /* Unmapped or unimplemented... */
                default: return 0x00;
            }
        }

        public void WritePort(byte port, byte value)
        {
            switch (port)
            {
                /* VDP */
                case 0x98: vdp.WriteDataPort(value); break;         /* Data port */
                case 0x99: vdp.WriteControlPort(value); break;      /* Control port */

                /* PSG */
                case 0xA0: psg.WriteIndex(value); break;            /* Index */
                case 0xA1: psg.WriteData(value); break;             /* Data */

                /* PPI */
                case 0xA8:                                          /* Port A (PSLOT register) */
                case 0xAA:                                          /* Port C (Keyboard, cassette, etc.) */
                case 0xAB: ppi.WritePort(port, value); break;       /* Control port */

                /* Special ports */
                case 0xF5: portSystemControl = value; break;        /* System control */
                case 0xF7: portAVControl = value; break;            /* A/V control */

                /* Memory mapper */
                case 0xFC: ramBankNumbers[0] = value; break;        /* RAM bank for 0000-3FFF */
                case 0xFD: ramBankNumbers[1] = value; break;        /* RAM bank for 4000-7FFF */
                case 0xFE: ramBankNumbers[2] = value; break;        /* RAM bank for 8000-BFFF */
                case 0xFF: ramBankNumbers[3] = value; break;        /* RAM bank for C000-FFFF */
            }
        }
    }
}
