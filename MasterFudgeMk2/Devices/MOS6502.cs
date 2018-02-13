using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using MasterFudgeMk2.Common.Enumerations;

namespace MasterFudgeMk2.Devices
{
    // TODO: verify decimal mode calculations, add unofficial opcodes...

    public partial class MOS6502
    {
        static readonly bool DEBUG_NESTEST = false;

        [Flags]
        public enum Flags : byte
        {
            Carry = (1 << 0),               /* C */
            Zero = (1 << 1),                /* Z */
            InterruptDisable = (1 << 2),    /* I */
            DecimalMode = (1 << 3),         /* D */
            Brk = (1 << 4),                 /* B */
            UnusedBit5 = (1 << 5),          /* - */
            Overflow = (1 << 6),            /* V */
            Sign = (1 << 7)                 /* S */
        };

        private static readonly int[] cycleCounts6502 = new int[]
        {
            /* 0x00 */  7, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6,
            /* 0x10 */  2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            /* 0x20 */  6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 4, 4, 6, 6,
            /* 0x30 */  2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            /* 0x40 */  6, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 3, 4, 6, 6,
            /* 0x50 */  2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            /* 0x60 */  6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 5, 4, 6, 6,
            /* 0x70 */  2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            /* 0x80 */  2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
            /* 0x90 */  2, 6, 2, 6, 4, 4, 4, 4, 2, 5, 2, 5, 5, 5, 5, 5,
            /* 0xA0 */  2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
            /* 0xB0 */  2, 5, 2, 5, 4, 4, 4, 4, 2, 4, 2, 4, 4, 4, 4, 4,
            /* 0xC0 */  2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
            /* 0xD0 */  2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
            /* 0xE0 */  2, 6, 3, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
            /* 0xF0 */  2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7
        };

        static readonly int[] opcodeLengths6502 = new int[]
        {
            /* 0x00 */  1, 2, 0, 0, 0, 2, 2, 0, 1, 2, 1, 0, 0, 3, 3, 0,
            /* 0x10 */  2, 2, 0, 0, 0, 2, 2, 0, 1, 3, 0, 0, 0, 3, 3, 0,
            /* 0x20 */  3, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            /* 0x30 */  2, 2, 0, 0, 0, 2, 2, 0, 1, 3, 0, 0, 0, 3, 3, 0,
            /* 0x40 */  1, 2, 0, 0, 0, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            /* 0x50 */  2, 2, 0, 0, 0, 2, 2, 0, 1, 3, 0, 0, 0, 3, 3, 0,
            /* 0x60 */  1, 2, 0, 0, 0, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            /* 0x70 */  2, 2, 0, 0, 0, 2, 2, 0, 1, 3, 0, 0, 0, 3, 3, 0,
            /* 0x80 */  0, 2, 0, 0, 2, 2, 2, 0, 1, 0, 1, 0, 3, 3, 3, 0,
            /* 0x90 */  2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 0, 3, 0, 0,
            /* 0xA0 */  2, 2, 2, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            /* 0xB0 */  2, 2, 0, 0, 2, 2, 2, 0, 1, 3, 1, 0, 3, 3, 3, 0,
            /* 0xC0 */  2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            /* 0xD0 */  2, 2, 0, 0, 0, 2, 2, 0, 1, 3, 0, 0, 0, 3, 3, 0,
            /* 0xE0 */  2, 2, 0, 0, 2, 2, 2, 0, 1, 2, 1, 0, 3, 3, 3, 0,
            /* 0xF0 */  2, 2, 0, 0, 0, 2, 2, 0, 1, 3, 0, 0, 0, 3, 3, 0
        };

        public enum AddressingModes
        {
            Invalid = -1,
            Implied,
            Accumulator,
            Immediate,
            ZeroPage,
            ZeroPageX,
            ZeroPageY,
            Relative,
            Absolute,
            AbsoluteX,
            AbsoluteY,
            Indirect,
            IndirectX,
            IndirectY
        }

        public virtual int[] CycleCounts { get { return cycleCounts6502; } }
        public virtual int[] OpcodeLengths { get { return opcodeLengths6502; } }

        public delegate byte MemoryReadDelegate(uint address);
        public delegate void MemoryWriteDelegate(uint address, byte value);

        MemoryReadDelegate memoryReadDelegate;
        MemoryWriteDelegate memoryWriteDelegate;

        protected ushort pc;
        protected byte sp, a, x, y;
        protected Flags p;

        protected byte op;

        InterruptState intState, nmiState;

        protected int currentCycles;
        double clockRate, refreshRate;

        protected MOS6502(double clockRate, double refreshRate)
        {
            this.refreshRate = refreshRate;
            this.clockRate = clockRate;
        }

        public MOS6502(double clockRate, double refreshRate, MemoryReadDelegate memoryRead, MemoryWriteDelegate memoryWrite) : this(clockRate, refreshRate)
        {
            memoryReadDelegate = memoryRead;
            memoryWriteDelegate = memoryWrite;
        }

        public virtual void Startup()
        {
            Reset();

            Debug.Assert(memoryReadDelegate != null, "Memory read method is null", "{0} has invalid memory read method", GetType().FullName);
            Debug.Assert(memoryWriteDelegate != null, "Memory write method is null", "{0} has invalid memory write method", GetType().FullName);
        }

        public virtual void Reset()
        {
            pc = ReadMemory16(0xFFFC);
            sp = 0xFD;
            a = x = y = 0;
            p = (Flags.InterruptDisable | Flags.Brk | Flags.UnusedBit5);

            intState = nmiState = InterruptState.Clear;

            currentCycles = 0;

            if (DEBUG_NESTEST) pc = 0xC000;
        }

        public virtual int Step()
        {
            currentCycles = 0;

            /* Check INT line */
            if (intState == InterruptState.Assert)
            {
                intState = InterruptState.Clear;
                ServiceInterrupt();
            }

            /* Check NMI line */
            if (nmiState == InterruptState.Assert)
            {
                nmiState = InterruptState.Clear;
                ServiceNonMaskableInterrupt();
            }

            if (DEBUG_NESTEST)
            {
                string disasm = $"{DisassembleOpcode(this, pc).PadRight(48)} | {PrintRegisters(this)} | {PrintFlags(this)} | {PrintInterrupt(this)}\n";
                System.IO.File.AppendAllText(@"E:\temp\sms\nes\log.txt", disasm);
            }

            /* Fetch and execute opcode */
            op = ReadMemory8(pc);
            switch (op)
            {
                case 0x00: OpBRK(); break;
                case 0x01: OpORA(AddressingModes.IndirectX); break;
                case 0x05: OpORA(AddressingModes.ZeroPage); break;
                case 0x06: OpASL(AddressingModes.ZeroPage); break;
                case 0x08: SetFlag(Flags.Brk); PushP(); IncrementPC(); IncrementCycles(); break;
                case 0x09: OpORA(AddressingModes.Immediate); break;
                case 0x0A: OpASL(AddressingModes.Accumulator); break;
                case 0x0D: OpORA(AddressingModes.Absolute); break;
                case 0x0E: OpASL(AddressingModes.Absolute); break;

                case 0x10: OpBPL(); break;
                case 0x11: OpORA(AddressingModes.IndirectY); break;
                case 0x15: OpORA(AddressingModes.ZeroPageX); break;
                case 0x16: OpASL(AddressingModes.ZeroPageX); break;
                case 0x18: ClearFlag(Flags.Carry); IncrementPC(); IncrementCycles(); break;
                case 0x19: OpORA(AddressingModes.AbsoluteY); break;
                case 0x1D: OpORA(AddressingModes.AbsoluteX); break;
                case 0x1E: OpASL(AddressingModes.AbsoluteX); break;

                case 0x20: OpJSR(); break;
                case 0x21: OpAND(AddressingModes.IndirectX); break;
                case 0x24: OpBIT(AddressingModes.ZeroPage); break;
                case 0x25: OpAND(AddressingModes.ZeroPage); break;
                case 0x26: OpROL(AddressingModes.ZeroPage); break;
                case 0x28: PullP(); IncrementPC(); IncrementCycles(); break;
                case 0x29: OpAND(AddressingModes.Immediate); break;
                case 0x2A: OpROL(AddressingModes.Accumulator); break;
                case 0x2C: OpBIT(AddressingModes.Absolute); break;
                case 0x2D: OpAND(AddressingModes.Absolute); break;
                case 0x2E: OpROL(AddressingModes.Absolute); break;

                case 0x30: OpBMI(); break;
                case 0x31: OpAND(AddressingModes.IndirectY); break;
                case 0x35: OpAND(AddressingModes.ZeroPageX); break;
                case 0x36: OpROL(AddressingModes.ZeroPageX); break;
                case 0x38: SetFlag(Flags.Carry); IncrementPC(); IncrementCycles(); break;
                case 0x39: OpAND(AddressingModes.AbsoluteY); break;
                case 0x3D: OpAND(AddressingModes.AbsoluteX); break;
                case 0x3E: OpROL(AddressingModes.AbsoluteX); break;

                case 0x40: PullP(); pc = Pull16(); break;
                case 0x41: OpEOR(AddressingModes.IndirectX); break;
                case 0x45: OpEOR(AddressingModes.ZeroPage); break;
                case 0x46: OpLSR(AddressingModes.ZeroPage); break;
                case 0x48: Push(a); IncrementPC(); IncrementCycles(); break;
                case 0x49: OpEOR(AddressingModes.Immediate); break;
                case 0x4A: OpLSR(AddressingModes.Accumulator); break;
                case 0x4C: OpJMP(AddressingModes.Absolute); break;
                case 0x4D: OpEOR(AddressingModes.Absolute); break;
                case 0x4E: OpLSR(AddressingModes.Absolute); break;

                case 0x50: OpBVC(); break;
                case 0x51: OpEOR(AddressingModes.IndirectY); break;
                case 0x55: OpEOR(AddressingModes.ZeroPageX); break;
                case 0x56: OpLSR(AddressingModes.ZeroPageX); break;
                case 0x58: ClearFlag(Flags.InterruptDisable); IncrementPC(); IncrementCycles(); break;
                case 0x59: OpEOR(AddressingModes.AbsoluteY); break;
                case 0x5D: OpEOR(AddressingModes.AbsoluteX); break;
                case 0x5E: OpLSR(AddressingModes.AbsoluteX); break;

                case 0x60: pc = Pull16(); IncrementPC(); IncrementCycles(); break;
                case 0x61: OpADC(AddressingModes.IndirectX); break;
                case 0x65: OpADC(AddressingModes.ZeroPage); break;
                case 0x66: OpROR(AddressingModes.ZeroPage); break;
                case 0x68: a = Pull(); SetClearFlagConditional(Flags.Zero, (a == 0x00)); SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80)); IncrementPC(); IncrementCycles(); break;
                case 0x69: OpADC(AddressingModes.Immediate); break;
                case 0x6A: OpROR(AddressingModes.Accumulator); break;
                case 0x6C: OpJMP(AddressingModes.Indirect); break;
                case 0x6D: OpADC(AddressingModes.Absolute); break;
                case 0x6E: OpROR(AddressingModes.Absolute); break;

                case 0x70: OpBVS(); break;
                case 0x71: OpADC(AddressingModes.IndirectY); break;
                case 0x75: OpADC(AddressingModes.ZeroPageX); break;
                case 0x76: OpROR(AddressingModes.ZeroPageX); break;
                case 0x78: SetFlag(Flags.InterruptDisable); IncrementPC(); IncrementCycles(); break;
                case 0x79: OpADC(AddressingModes.AbsoluteY); break;
                case 0x7D: OpADC(AddressingModes.AbsoluteX); break;
                case 0x7E: OpROR(AddressingModes.AbsoluteX); break;

                case 0x81: OpSTA(AddressingModes.IndirectX); break;
                case 0x84: OpSTY(AddressingModes.ZeroPage); break;
                case 0x85: OpSTA(AddressingModes.ZeroPage); break;
                case 0x86: OpSTX(AddressingModes.ZeroPage); break;
                case 0x88: OpDEY(); break;
                case 0x8A: a = x; SetClearFlagConditional(Flags.Zero, (a == 0x00)); SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80)); IncrementPC(); IncrementCycles(); break;
                case 0x8C: OpSTY(AddressingModes.Absolute); break;
                case 0x8D: OpSTA(AddressingModes.Absolute); break;
                case 0x8E: OpSTX(AddressingModes.Absolute); break;

                case 0x90: OpBCC(); break;
                case 0x91: OpSTA(AddressingModes.IndirectY); break;
                case 0x94: OpSTY(AddressingModes.ZeroPageX); break;
                case 0x95: OpSTA(AddressingModes.ZeroPageX); break;
                case 0x96: OpSTX(AddressingModes.ZeroPageY); break;
                case 0x98: a = y; SetClearFlagConditional(Flags.Zero, (a == 0x00)); SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80)); IncrementPC(); IncrementCycles(); break;
                case 0x99: OpSTA(AddressingModes.AbsoluteY); break;
                case 0x9A: sp = x; IncrementPC(); IncrementCycles(); break;
                case 0x9D: OpSTA(AddressingModes.AbsoluteX); break;

                case 0xA0: OpLDY(AddressingModes.Immediate); break;
                case 0xA1: OpLDA(AddressingModes.IndirectX); break;
                case 0xA2: OpLDX(AddressingModes.Immediate); break;
                case 0xA4: OpLDY(AddressingModes.ZeroPage); break;
                case 0xA5: OpLDA(AddressingModes.ZeroPage); break;
                case 0xA6: OpLDX(AddressingModes.ZeroPage); break;
                case 0xA8: y = a; SetClearFlagConditional(Flags.Zero, (y == 0x00)); SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80)); IncrementPC(); IncrementCycles(); break;
                case 0xA9: OpLDA(AddressingModes.Immediate); break;
                case 0xAA: x = a; SetClearFlagConditional(Flags.Zero, (x == 0x00)); SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80)); IncrementPC(); IncrementCycles(); break;
                case 0xAC: OpLDY(AddressingModes.Absolute); break;
                case 0xAD: OpLDA(AddressingModes.Absolute); break;
                case 0xAE: OpLDX(AddressingModes.Absolute); break;

                case 0xB0: OpBCS(); break;
                case 0xB1: OpLDA(AddressingModes.IndirectY); break;
                case 0xB4: OpLDY(AddressingModes.ZeroPageX); break;
                case 0xB5: OpLDA(AddressingModes.ZeroPageX); break;
                case 0xB6: OpLDX(AddressingModes.ZeroPageY); break;
                case 0xB8: ClearFlag(Flags.Overflow); IncrementPC(); IncrementCycles(); break;
                case 0xB9: OpLDA(AddressingModes.AbsoluteY); break;
                case 0xBA: x = sp; SetClearFlagConditional(Flags.Zero, (x == 0x00)); SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80)); IncrementPC(); IncrementCycles(); break;
                case 0xBC: OpLDY(AddressingModes.AbsoluteX); break;
                case 0xBD: OpLDA(AddressingModes.AbsoluteX); break;
                case 0xBE: OpLDX(AddressingModes.AbsoluteY); break;

                case 0xC0: OpCPY(AddressingModes.Immediate); break;
                case 0xC1: OpCMP(AddressingModes.IndirectX); break;
                case 0xC4: OpCPY(AddressingModes.ZeroPage); break;
                case 0xC5: OpCMP(AddressingModes.ZeroPage); break;
                case 0xC6: OpDEC(AddressingModes.ZeroPage); break;
                case 0xC8: OpINY(); break;
                case 0xC9: OpCMP(AddressingModes.Immediate); break;
                case 0xCA: OpDEX(); break;
                case 0xCC: OpCPY(AddressingModes.Absolute); break;
                case 0xCD: OpCMP(AddressingModes.Absolute); break;
                case 0xCE: OpDEC(AddressingModes.Absolute); break;

                case 0xD0: OpBNE(); break;
                case 0xD1: OpCMP(AddressingModes.IndirectY); break;
                case 0xD5: OpCMP(AddressingModes.ZeroPageX); break;
                case 0xD6: OpDEC(AddressingModes.ZeroPageX); break;
                case 0xD8: ClearFlag(Flags.DecimalMode); IncrementPC(); IncrementCycles(); break;
                case 0xD9: OpCMP(AddressingModes.AbsoluteY); break;
                case 0xDD: OpCMP(AddressingModes.AbsoluteX); break;
                case 0xDE: OpDEC(AddressingModes.AbsoluteX); break;

                case 0xE0: OpCPX(AddressingModes.Immediate); break;
                case 0xE1: OpSBC(AddressingModes.IndirectX); break;
                case 0xE4: OpCPX(AddressingModes.ZeroPage); break;
                case 0xE5: OpSBC(AddressingModes.ZeroPage); break;
                case 0xE6: OpINC(AddressingModes.ZeroPage); break;
                case 0xE8: OpINX(); break;
                case 0xE9: OpSBC(AddressingModes.Immediate); break;
                case 0xEA: OpNOP(); break;
                case 0xEC: OpCPX(AddressingModes.Absolute); break;
                case 0xED: OpSBC(AddressingModes.Absolute); break;
                case 0xEE: OpINC(AddressingModes.Absolute); break;

                case 0xF0: OpBEQ(); break;
                case 0xF1: OpSBC(AddressingModes.IndirectY); break;
                case 0xF5: OpSBC(AddressingModes.ZeroPageX); break;
                case 0xF6: OpINC(AddressingModes.ZeroPageX); break;
                case 0xF8: SetFlag(Flags.DecimalMode); IncrementPC(); IncrementCycles(); break;
                case 0xF9: OpSBC(AddressingModes.AbsoluteY); break;
                case 0xFD: OpSBC(AddressingModes.AbsoluteX); break;
                case 0xFE: OpINC(AddressingModes.AbsoluteX); break;

                default: throw new NotImplementedException($"Unimplemented opcode 0x{op:X2}");
            }

            return currentCycles;
        }

        protected void IncrementPC()
        {
            pc += (ushort)OpcodeLengths[op];
        }

        protected void IncrementCycles()
        {
            currentCycles += CycleCounts[op];
        }

        protected void IncrementCycles(int cycleCount)
        {
            currentCycles += cycleCount;
        }

        protected void CheckPageBoundary(uint address1, uint address2, int penalty)
        {
            if ((address1 & 0xFF00) != (address2 & 0xFF00))
                currentCycles += penalty;
        }

        protected void SetFlag(Flags flags)
        {
            p |= flags;
        }

        protected void ClearFlag(Flags flags)
        {
            p &= ~flags;
        }

        protected void SetClearFlagConditional(Flags flags, bool condition)
        {
            if (condition)
                p |= flags;
            else
                p &= ~flags;
        }

        protected bool IsFlagSet(Flags flags)
        {
            return ((p & flags) == flags);
        }

        public void SetInterruptLine(InterruptState state)
        {
            intState = state;
        }

        public void SetNonMaskableInterruptLine(InterruptState state)
        {
            nmiState = state;
        }

        protected void ServiceInterrupt()
        {
            if (!IsFlagSet(Flags.InterruptDisable))
            {
                ClearFlag(Flags.Brk);
                Push16(pc);
                PushP();
                SetFlag(Flags.InterruptDisable);
                pc = ReadMemory16(0xFFFE);

                IncrementCycles(7);
            }
        }

        protected void ServiceNonMaskableInterrupt()
        {
            ClearFlag(Flags.Brk);
            Push16(pc);
            PushP();
            SetFlag(Flags.InterruptDisable);
            pc = ReadMemory16(0xFFFA);

            IncrementCycles(7);
        }

        protected byte ReadMemory8(uint address)
        {
            return memoryReadDelegate(address);
        }

        protected void WriteMemory8(uint address, byte value)
        {
            memoryWriteDelegate(address, value);
        }

        protected ushort ReadMemory16(uint address, bool wrap = false)
        {
            byte low = ReadMemory8(address);
            if (wrap && ((address & 0xFF00) != ((address + 1) & 0xFF00)))
                address -= 0x100;

            byte high = ReadMemory8(address + 1);
            return (ushort)((high << 8) | low);
        }

        protected ushort CalculateAddress(byte a, byte b)
        {
            return (ushort)((b << 8) | a);
        }

        protected virtual byte ReadZeroPage(byte address)
        {
            return ReadMemory8((uint)address & 0xFF);
        }

        protected virtual byte ReadZeroPageX(byte address)
        {
            return ReadMemory8(((uint)address + x) & 0xFF);
        }

        protected virtual byte ReadZeroPageY(byte address)
        {
            return ReadMemory8(((uint)address + y) & 0xFF);
        }

        protected virtual byte ReadAbsolute(byte address1, byte address2)
        {
            return ReadMemory8(CalculateAddress(address1, address2));
        }

        protected virtual byte ReadAbsoluteX(byte address1, byte address2)
        {
            CheckPageBoundary(CalculateAddress(address1, address2), (uint)(CalculateAddress(address1, address2) + x), 1);

            return ReadMemory8((uint)(CalculateAddress(address1, address2) + x));
        }

        protected virtual byte ReadAbsoluteY(byte address1, byte address2)
        {
            uint address = CalculateAddress(address1, address2);
            CheckPageBoundary(address, (ushort)(address + y), 1);

            return ReadMemory8((ushort)(address + y));
        }

        protected virtual byte ReadIndirectX(byte address)
        {
            return ReadMemory8(ReadMemory16((uint)((address + x) & 0xFF), true));
        }

        protected virtual byte ReadIndirectY(byte address)
        {
            CheckPageBoundary(ReadMemory16(address, true), (ushort)(ReadMemory16(address, true) + y), 1);

            return ReadMemory8((ushort)(ReadMemory16(address, true) + y));
        }

        protected virtual void WriteZeroPage(uint address, byte Value)
        {
            WriteMemory8((address & 0xFF), Value);
        }

        protected virtual void WriteZeroPageX(uint address, byte value)
        {
            WriteMemory8(((address + x) & 0xFF), value);
        }

        protected virtual void WriteZeroPageY(uint address, byte value)
        {
            WriteMemory8(((address + y) & 0xFF), value);
        }

        protected virtual void WriteAbsolute(byte address1, byte address2, byte value)
        {
            WriteMemory8(CalculateAddress(address1, address2), value);
        }

        protected virtual void WriteAbsoluteX(byte address1, byte address2, byte value)
        {
            WriteMemory8((uint)(CalculateAddress(address1, address2) + x), value);
        }

        protected virtual void WriteAbsoluteY(byte address1, byte address2, byte value)
        {
            WriteMemory8((uint)(CalculateAddress(address1, address2) + y), value);
        }

        protected virtual void WriteIndirectX(byte address, byte value)
        {
            WriteMemory8(ReadMemory16((uint)((address + x) & 0xFF), true), value);
        }

        protected virtual void WriteIndirectY(byte address, byte value)
        {
            WriteMemory8((uint)(ReadMemory16(address, true) + y), value);
        }

        protected virtual byte GetOperand(AddressingModes mode)
        {
            return GetOperand(mode, 0);
        }

        protected virtual byte GetOperand(AddressingModes mode, int offset)
        {
            byte arg1 = ReadMemory8((ushort)(pc + 1 + offset));
            byte arg2 = ReadMemory8((ushort)(pc + 2 + offset));
            byte value = 0xFF;

            switch (mode)
            {
                case AddressingModes.Implied: break;
                case AddressingModes.Accumulator: value = a; break;
                case AddressingModes.Immediate: value = arg1; break;
                case AddressingModes.ZeroPage: value = ReadZeroPage(arg1); break;
                case AddressingModes.ZeroPageX: value = ReadZeroPageX(arg1); break;
                case AddressingModes.ZeroPageY: value = ReadZeroPageY(arg1); break;
                //case AddressingModes.Relative: break;
                case AddressingModes.Absolute: value = ReadAbsolute(arg1, arg2); break;
                case AddressingModes.AbsoluteX: value = ReadAbsoluteX(arg1, arg2); break;
                case AddressingModes.AbsoluteY: value = ReadAbsoluteY(arg1, arg2); break;
                //case AddressingModes.Indirect: break;
                case AddressingModes.IndirectX: value = ReadIndirectX(arg1); break;
                case AddressingModes.IndirectY: value = ReadIndirectY(arg1); break;
                default: throw new Exception("6502 addressing mode error on read");
            }

            return value;
        }

        protected virtual void WriteValue(AddressingModes mode, byte value)
        {
            byte arg1 = ReadMemory8((ushort)(pc + 1));
            byte arg2 = ReadMemory8((ushort)(pc + 2));

            switch (mode)
            {
                case AddressingModes.Implied: break;
                case AddressingModes.Accumulator: a = value; break;
                //case AddressingModes.Immediate: break;
                case AddressingModes.ZeroPage: WriteZeroPage(arg1, value); break;
                case AddressingModes.ZeroPageX: WriteZeroPageX(arg1, value); break;
                case AddressingModes.ZeroPageY: WriteZeroPageY(arg1, value); break;
                //case AddressingModes.Relative: break;
                case AddressingModes.Absolute: WriteAbsolute(arg1, arg2, value); break;
                case AddressingModes.AbsoluteX: WriteAbsoluteX(arg1, arg2, value); break;
                case AddressingModes.AbsoluteY: WriteAbsoluteY(arg1, arg2, value); break;
                //case AddressingModes.Indirect: break;
                case AddressingModes.IndirectX: WriteIndirectX(arg1, value); break;
                case AddressingModes.IndirectY: WriteIndirectY(arg1, value); break;
                default: throw new Exception("6502 addressing mode error on write");
            }
        }

        public void Push(byte value)
        {
            WriteMemory8((ushort)(0x0100 | sp), value);
            sp = (byte)(sp - 1);
        }

        public void Push16(ushort value)
        {
            Push((byte)((value >> 8) & 0xFF));
            Push((byte)((value >> 0) & 0xFF));
        }

        public void PushP()
        {
            Push((byte)p);
        }

        public byte Pull()
        {
            sp = (byte)(sp + 1);
            return ReadMemory8((uint)(0x0100 | sp));
        }

        public ushort Pull16()
        {
            return CalculateAddress(Pull(), Pull());
        }

        public void PullP()
        {
            Flags pulledStateUsed = ((Flags)Pull() & ~(Flags.Brk | Flags.UnusedBit5));
            Flags oldStateUnused = (p & (Flags.Brk | Flags.UnusedBit5));
            p = (pulledStateUsed | oldStateUnused);
        }

        protected virtual void OpADC(AddressingModes mode)
        {
            byte data = GetOperand(mode);
            uint sum = (uint)(a + data + (IsFlagSet(Flags.Carry) ? 1 : 0));

            SetClearFlagConditional(Flags.Overflow, ((a ^ sum) & (data ^ sum) & 0x80) != 0);
            SetClearFlagConditional(Flags.Carry, sum >= 0x100);

            if (IsFlagSet(Flags.DecimalMode))
            {
                ClearFlag(Flags.Carry);

                if ((a & 0x0F) > 0x09) a += 0x06;
                if ((a & 0xF0) > 0x90)
                {
                    a += 0x60;
                    SetFlag(Flags.Carry);
                }
            }

            a = (byte)(sum & 0xFF);

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpAND(AddressingModes mode)
        {
            a = (byte)(a & GetOperand(mode));

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpASL(AddressingModes mode)
        {
            uint value = GetOperand(mode);
            SetClearFlagConditional(Flags.Carry, ((value & 0x80) == 0x80));

            value = (byte)(value << 1);
            SetClearFlagConditional(Flags.Zero, ((value & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(mode, (byte)value);

            IncrementPC();
            IncrementCycles();
        }

        protected void OpBCC()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            IncrementPC();
            IncrementCycles();

            if (!IsFlagSet(Flags.Carry))
            {
                CheckPageBoundary(pc, (uint)(pc + (sbyte)value + 2), 1);

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        protected void OpBCS()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            IncrementPC();
            IncrementCycles();

            if (IsFlagSet(Flags.Carry))
            {
                CheckPageBoundary(pc, (uint)(pc + (sbyte)value + 2), 1);

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        protected void OpBEQ()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            IncrementPC();
            IncrementCycles();

            if (IsFlagSet(Flags.Zero))
            {
                CheckPageBoundary(pc, (uint)(pc + (sbyte)value + 2), 1);

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        protected void OpBIT(AddressingModes mode)
        {
            uint value = GetOperand(mode);

            SetClearFlagConditional(Flags.Zero, ((a & value) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));
            SetClearFlagConditional(Flags.Overflow, ((value & 0x40) == 0x40));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpBMI()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            IncrementPC();
            IncrementCycles();

            if (IsFlagSet(Flags.Sign))
            {
                CheckPageBoundary(pc, (uint)(pc + (sbyte)value + 2), 1);

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        protected void OpBNE()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            IncrementPC();
            IncrementCycles();

            if (!IsFlagSet(Flags.Zero))
            {
                CheckPageBoundary(pc, (uint)(pc + (sbyte)value + 2), 1);

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        protected void OpBPL()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            IncrementPC();
            IncrementCycles();

            if (!IsFlagSet(Flags.Sign))
            {
                CheckPageBoundary(pc, (uint)(pc + (sbyte)value + 2), 1);

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        protected void OpBRK()
        {
            IncrementPC();
            IncrementCycles();

            pc++;
            Push16(pc);
            SetFlag(Flags.Brk);
            PushP();
            SetFlag(Flags.InterruptDisable);
            pc = ReadMemory16(0xFFFE);
        }

        protected void OpBVC()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            IncrementPC();
            IncrementCycles();

            if (!IsFlagSet(Flags.Overflow))
            {
                CheckPageBoundary(pc, (uint)(pc + (sbyte)value + 2), 1);

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        protected void OpBVS()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            IncrementPC();
            IncrementCycles();

            if (IsFlagSet(Flags.Overflow))
            {
                CheckPageBoundary(pc, (uint)(pc + (sbyte)value + 2), 1);

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        protected void OpCMP(AddressingModes mode)
        {
            uint value = GetOperand(mode);

            SetClearFlagConditional(Flags.Carry, a >= (byte)value);

            value = (byte)(a - value);

            SetClearFlagConditional(Flags.Zero, ((value & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpCPX(AddressingModes mode)
        {
            uint value = GetOperand(mode);

            SetClearFlagConditional(Flags.Carry, x >= (byte)value);

            value = (byte)(x - value);

            SetClearFlagConditional(Flags.Zero, ((value & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpCPY(AddressingModes mode)
        {
            uint value = GetOperand(mode);

            SetClearFlagConditional(Flags.Carry, y >= (byte)value);

            value = (byte)(y - value);

            SetClearFlagConditional(Flags.Zero, ((value & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpDEC(AddressingModes mode)
        {
            uint value = GetOperand(mode);

            value--;

            SetClearFlagConditional(Flags.Zero, ((value & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(mode, (byte)(value & 0xFF));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpDEX()
        {
            x--;

            SetClearFlagConditional(Flags.Zero, (x == 0x00));
            SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpDEY()
        {
            y--;

            SetClearFlagConditional(Flags.Zero, (y == 0x00));
            SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpEOR(AddressingModes mode)
        {
            a = (byte)(a ^ GetOperand(mode));

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpINC(AddressingModes mode)
        {
            uint value = GetOperand(mode);

            value++;

            SetClearFlagConditional(Flags.Zero, ((value & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(mode, (byte)(value & 0xFF));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpINX()
        {
            x++;

            SetClearFlagConditional(Flags.Zero, (x == 0x00));
            SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpINY()
        {
            y++;

            SetClearFlagConditional(Flags.Zero, (y == 0x00));
            SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpJMP(AddressingModes mode)
        {
            ushort address = ReadMemory16((ushort)(pc + 1));

            switch (mode)
            {
                case AddressingModes.Absolute: pc = address; break;
                case AddressingModes.Indirect: pc = ReadMemory16(address, true); break;
                default: throw new Exception("6502 addressing mode error on jump");
            }

            IncrementCycles();
        }

        protected void OpJSR()
        {
            byte arg1 = ReadMemory8((ushort)(pc + 1));
            byte arg2 = ReadMemory8((ushort)(pc + 2));
            Push16((ushort)(pc + 2));
            pc = CalculateAddress(arg1, arg2);

            IncrementCycles();
        }

        protected void OpLDA(AddressingModes mode)
        {
            a = (byte)(GetOperand(mode) & 0xFF);

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpLDX(AddressingModes mode)
        {
            x = (byte)(GetOperand(mode) & 0xFF);

            SetClearFlagConditional(Flags.Zero, (x == 0x00));
            SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpLDY(AddressingModes mode)
        {
            y = (byte)(GetOperand(mode) & 0xFF);

            SetClearFlagConditional(Flags.Zero, (y == 0x00));
            SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpLSR(AddressingModes mode)
        {
            uint value = GetOperand(mode);

            SetClearFlagConditional(Flags.Carry, ((value & 0x01) == 0x01));

            value = (byte)(value >> 1);

            SetClearFlagConditional(Flags.Zero, ((value & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(mode, (byte)(value & 0xFF));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpNOP()
        {
            IncrementPC();
            IncrementCycles();
        }

        protected void OpORA(AddressingModes mode)
        {
            a = (byte)(a | GetOperand(mode));

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpROL(AddressingModes mode)
        {
            uint value = GetOperand(mode);
            bool tempBit = ((value & 0x80) == 0x80);

            value = (byte)(value << 1);
            value = (byte)(value | (byte)(IsFlagSet(Flags.Carry) ? 0x01 : 0x00));

            SetClearFlagConditional(Flags.Carry, tempBit);

            SetClearFlagConditional(Flags.Zero, ((value & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(mode, (byte)(value & 0xFF));

            IncrementPC();
            IncrementCycles();
        }

        protected void OpROR(AddressingModes mode)
        {
            uint value = GetOperand(mode);
            bool tempBit = ((value & 0x01) == 0x01);

            value = (byte)(value >> 1);
            value = (byte)(value | (byte)(IsFlagSet(Flags.Carry) ? 0x80 : 0x00));

            SetClearFlagConditional(Flags.Carry, tempBit);

            SetClearFlagConditional(Flags.Zero, ((value & 0xFF) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(mode, (byte)(value & 0xFF));

            IncrementPC();
            IncrementCycles();
        }

        protected virtual void OpSBC(AddressingModes mode)
        {
            byte data = (byte)(GetOperand(mode) ^ 0xFF);
            uint sum = (uint)(a + data + (IsFlagSet(Flags.Carry) ? 1 : 0));

            SetClearFlagConditional(Flags.Overflow, ((a ^ sum) & (data ^ sum) & 0x80) != 0);
            SetClearFlagConditional(Flags.Carry, sum >= 0x100);

            if (IsFlagSet(Flags.DecimalMode))
            {
                ClearFlag(Flags.Carry);

                if ((a & 0x0F) > 0x09) a += 0x06;
                if ((a & 0xF0) > 0x90)
                {
                    a += 0x60;
                    SetFlag(Flags.Carry);
                }
            }

            a = (byte)(sum & 0xFF);

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
            /*
            byte data = GetOperand(mode);
            uint sum = (uint)(0xFF + a - data + (IsFlagSet(Flags.Carry) ? 1 : 0));

            SetClearFlagConditional(Flags.Overflow, ((a ^ sum) & (data ^ sum) & 0x80) != 0);
            SetClearFlagConditional(Flags.Carry, sum >= 0x100);

            if (IsFlagSet(Flags.DecimalMode))
            {
                ClearFlag(Flags.Carry);

                a -= 0x66;
                if ((a & 0x0F) > 0x09) a += 0x06;
                if ((a & 0xF0) > 0x90)
                {
                    a += 0x60;
                    SetFlag(Flags.Carry);
                }
            }

            a = (byte)(sum & 0xFF);

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();*/
        }

        protected void OpSTA(AddressingModes mode)
        {
            WriteValue(mode, a);

            IncrementPC();
            IncrementCycles();
        }

        protected void OpSTX(AddressingModes mode)
        {
            WriteValue(mode, x);

            IncrementPC();
            IncrementCycles();
        }

        protected void OpSTY(AddressingModes mode)
        {
            WriteValue(mode, y);

            IncrementPC();
            IncrementCycles();
        }
    }
}
