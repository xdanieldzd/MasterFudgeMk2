using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
using System.Drawing;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;
using MasterFudgeMk2.Media;
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Machines.Various.MSX1
{
    class Manager : BaseMachine
    {
        public override string FriendlyName { get { return "Generic MSX"; } }
        public override string FriendlyShortName { get { return "MSX"; } }
        public override string FileFilter { get { return "MSX ROMs (*.rom)|*.rom"; } }

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
        const int ramSize = 1 * 65536;  // TODO: 64k atm, make selectable in settings (8/16/32/64k)

        const double cpuClock = (masterClock / 3.0);
        const double vdpClock = (masterClock / 1.0);
        const double psgClock = cpuClock;

        /* Devices on bus */
        IMedia cartridge;
        byte[] wram;
        Z80A cpu;
        TMS9918A vdp;
        object psg; // TODO: General Instrument AY-3-8910, huh...
        i8255 ppi;

        byte[] bios;

        enum KeyboardKeys
        {
            D0, D1, D2, D3, D4, D5, D6, D7,
            D8, D9, Minus, Equals, Backslash, BracketOpen, BracketClose, Semicolon,
            Grave, Apostrophe, Comma, Period, Slash, DeadKey, A, B,
            C, D, E, F, G, H, I, J,
            K, L, M, N, O, P, Q, R,
            S, T, U, V, W, X, Y, Z,
            Shift, Ctrl, Graph, Cap, Code, F1, F2, F3,
            F4, F5, Esc, Tab, Stop, BS, Select, Return,
            Space, Home, Ins, Del, Left, Up, Down, Right,

            NumMultiply, NumPlus, NumDivide, Num0, Num1, Num2, Num3, Num4,
            Num5, Num6, Num7, Num8, Num9, NumMinus, NumComma, NumPeriod
        }
        bool[,] keyMatrix;

        byte portSystemControl, portAVControl;

        protected override int totalMasterClockCyclesInFrame { get { return (int)Math.Round(masterClock / refreshRate); } }

        Configuration configuration;

        public Manager()
        {
            configuration = new Configuration();

            cartridge = null;

            cpu = new Z80A(cpuClock, refreshRate, ReadMemory, WriteMemory, ReadPort, WritePort);
            wram = new byte[ramSize];
            vdp = new TMS9918A(vdpClock, refreshRate, false);
            psg = null;
            ppi = new i8255();

            keyMatrix = new bool[11, 8];

            if (CanCurrentlyBootWithoutMedia)
                bios = File.ReadAllBytes(configuration.BiosPath);
        }

        public override void Startup()
        {
            cpu.Startup();
            //psg.Startup();
            vdp.Startup();

            Reset();
        }

        public override void Reset()
        {
            cartridge?.Reset();

            cpu.Reset();
            //psg.Reset();
            vdp.Reset();
            ppi.Reset();

            for (int i = 0; i < keyMatrix.GetLength(0); i++)
                for (int j = 0; j < keyMatrix.GetLength(1); j++)
                    keyMatrix[i, j] = false;

            portSystemControl = portAVControl = 0x00;

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

            //psg?.Shutdown();
        }

        public override void RunStep()
        {
            double currentCpuClockCycles = 0.0;
            currentCpuClockCycles += cpu.Step();

            double currentMasterClockCycles = (currentCpuClockCycles * 3.0);

            if (vdp.Step((int)Math.Round(currentMasterClockCycles)))
                OnRenderScreen(new RenderScreenEventArgs(TMS9918A.NumActivePixelsPerScanline, vdp.NumTotalScanlines, vdp.OutputFramebuffer));

            cpu.SetInterruptLine(vdp.InterruptLine);

            //psg.Step((int)Math.Round(currentCpuClockCycles));

            currentMasterClockCyclesInFrame += (int)Math.Round(currentMasterClockCycles);
        }

        protected override void ParseInput(PollInputEventArgs input)
        {
            SetKeyboardState(KeyboardKeys.D0, (input.Pressed.Contains(System.Windows.Forms.Keys.D0)));
            SetKeyboardState(KeyboardKeys.D1, (input.Pressed.Contains(System.Windows.Forms.Keys.D1)));
            SetKeyboardState(KeyboardKeys.D2, (input.Pressed.Contains(System.Windows.Forms.Keys.D2)));
            SetKeyboardState(KeyboardKeys.D3, (input.Pressed.Contains(System.Windows.Forms.Keys.D3)));
            SetKeyboardState(KeyboardKeys.A, (input.Pressed.Contains(System.Windows.Forms.Keys.A)));

            //
        }

        private void SetKeyboardState(KeyboardKeys key, bool state)
        {
            keyMatrix[((int)key / keyMatrix.GetLength(0)), ((int)key % keyMatrix.GetLength(1))] = state;
        }

        private void UpdateKeyboard()
        {
            int matrixLine = (ppi.PortCOutput & 0x0F);

            byte rowState = 0xFF;
            for (int i = 0; i < keyMatrix.GetLength(1); i++)
                if (keyMatrix[matrixLine, i]) rowState &= (byte)~(1 << i);

            ppi.PortBInput = rowState;
        }

        private byte ReadMemory(ushort address)
        {
            /*            0-3   4-7   8-B   C-F   */
            /* PSLOT=0 -> BIOS  BASIC N/A   N/A   */
            /* PSLOT=1 -> CartA CartA CartA CartA */
            /* PSLOT=2 -> CartB CartB CartB CartB */
            /* PSLOT=3 -> RAM3  RAM2  RAM1  RAM0  */

            // TODO: ram mirroring? or lack thereof?

            byte primarySlot;

            if (address >= 0x0000 && address <= 0x3FFF)
            {
                primarySlot = (byte)((ppi.ReadPort(0xA8) >> 0) & 0x03);
                if (primarySlot == 0x00)
                {
                    /* BIOS */
                    return bios[address & (bios.Length - 1)];
                }
                else if (primarySlot == 0x01)
                {
                    /* Cartridge A */
                    if (cartridge != null) return cartridge.Read(address);
                }
                else if (primarySlot == 0x02)
                {
                    /* Cartridge B */
                    return 0x00;
                }
                else if (primarySlot == 0x03)
                {
                    /* RAM3 */
                    return wram[address & (ramSize - 1)];
                }
            }
            else if (address >= 0x4000 && address <= 0x7FFF)
            {
                primarySlot = (byte)((ppi.ReadPort(0xA8) >> 2) & 0x03);
                if (primarySlot == 0x00)
                {
                    /* BASIC */
                    return bios[address & (bios.Length - 1)];
                }
                else if (primarySlot == 0x01)
                {
                    /* Cartridge A */
                    if (cartridge != null) return cartridge.Read(address);
                }
                else if (primarySlot == 0x02)
                {
                    /* Cartridge B */
                    return 0x00;
                }
                else if (primarySlot == 0x03)
                {
                    /* RAM2 */
                    return wram[address & (ramSize - 1)];
                }
            }
            else if (address >= 0x8000 && address <= 0xBFFF)
            {
                primarySlot = (byte)((ppi.ReadPort(0xA8) >> 4) & 0x03);
                if (primarySlot == 0x00)
                {
                    /* N/A */
                    return 0x00;
                }
                else if (primarySlot == 0x01)
                {
                    /* Cartridge A */
                    if (cartridge != null) return cartridge.Read(address);
                }
                else if (primarySlot == 0x02)
                {
                    /* Cartridge B */
                    return 0x00;
                }
                else if (primarySlot == 0x03)
                {
                    /* RAM1 */
                    return wram[address & (ramSize - 1)];
                }
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                primarySlot = (byte)((ppi.ReadPort(0xA8) >> 6) & 0x03);
                if (primarySlot == 0x00)
                {
                    /* N/A */
                    return 0x00;
                }
                else if (primarySlot == 0x01)
                {
                    /* Cartridge A */
                    if (cartridge != null) return cartridge.Read(address);
                }
                else if (primarySlot == 0x02)
                {
                    /* Cartridge B */
                    return 0x00;
                }
                else if (primarySlot == 0x03)
                {
                    /* RAM0 */
                    return wram[address & (ramSize - 1)];
                }
            }

            return 0x00;
        }

        private void WriteMemory(ushort address, byte value)
        {
            byte primarySlot;

            if (address >= 0x0000 && address <= 0x3FFF)
            {
                primarySlot = (byte)((ppi.ReadPort(0xA8) >> 0) & 0x03);
                if (primarySlot == 0x00)
                {
                    /* BIOS -- can't write */
                    return;
                }
                else if (primarySlot == 0x01)
                {
                    /* Cartridge A */
                    cartridge?.Write(address, value);
                }
                else if (primarySlot == 0x02)
                {
                    /* Cartridge B */
                    return;
                }
                else if (primarySlot == 0x03)
                {
                    /* RAM3 */
                    wram[address & (ramSize - 1)] = value; ;
                }
            }
            else if (address >= 0x4000 && address <= 0x7FFF)
            {
                primarySlot = (byte)((ppi.ReadPort(0xA8) >> 2) & 0x03);
                if (primarySlot == 0x00)
                {
                    /* BASIC -- can't write */
                    return;
                }
                else if (primarySlot == 0x01)
                {
                    /* Cartridge A */
                    cartridge?.Write(address, value);
                }
                else if (primarySlot == 0x02)
                {
                    /* Cartridge B */
                    return;
                }
                else if (primarySlot == 0x03)
                {
                    /* RAM2 */
                    wram[address & (ramSize - 1)] = value; ;
                }
            }
            else if (address >= 0x8000 && address <= 0xBFFF)
            {
                primarySlot = (byte)((ppi.ReadPort(0xA8) >> 4) & 0x03);
                if (primarySlot == 0x00)
                {
                    /* N/A */
                    return;
                }
                else if (primarySlot == 0x01)
                {
                    /* Cartridge A */
                    cartridge?.Write(address, value);
                }
                else if (primarySlot == 0x02)
                {
                    /* Cartridge B */
                    return;
                }
                else if (primarySlot == 0x03)
                {
                    /* RAM1 */
                    wram[address & (ramSize - 1)] = value; ;
                }
            }
            else if (address >= 0xC000 && address <= 0xFFFF)
            {
                primarySlot = (byte)((ppi.ReadPort(0xA8) >> 6) & 0x03);
                if (primarySlot == 0x00)
                {
                    /* N/A */
                    return;
                }
                else if (primarySlot == 0x01)
                {
                    /* Cartridge A */
                    cartridge?.Write(address, value);
                }
                else if (primarySlot == 0x02)
                {
                    /* Cartridge B */
                    return;
                }
                else if (primarySlot == 0x03)
                {
                    /* RAM0 */
                    wram[address & (ramSize - 1)] = value;
                }
            }
        }

        private byte ReadPort(byte port)
        {
            switch (port)
            {
                /* VDP */
                case 0x98: return vdp.ReadDataPort();               /* Data port */
                case 0x99: return vdp.ReadControlPort();            /* Status flags */

                /* PPI */
                case 0xA8:                                              /* Port A (PSLOT register) */
                case 0xA9:                                              /* Port B (Keyboard matrix line status) */
                case 0xAA: UpdateKeyboard(); return ppi.ReadPort(port); /* Port C (Keyboard, cassette, etc.) */

                /* Special ports */
                case 0xF7: return portAVControl;                    /* A/V control */

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

                /* PPI */
                case 0xA8:                                          /* Port A (PSLOT register) */
                case 0xAA:                                          /* Port C (Keyboard, cassette, etc.) */
                case 0xAB: ppi.WritePort(port, value); break;       /* Control port */

                /* Special ports */
                case 0xF5: portSystemControl = value; break;        /* System control */
                case 0xF7: portAVControl = value; break;            /* A/V control */
            }
        }
    }
}
