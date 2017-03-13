using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using MasterFudgeMk2.Common;

namespace MasterFudgeMk2.Devices
{
    // TODO: add decimal mode (currently we're just a Ricoh 2A03!), afterwards derive decimal-less 2A03 from this

    public class MOS6502
    {
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

        public enum AddressingModes
        {
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

        public static readonly int[] CycleCounts = new int[]
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

        public delegate byte MemoryReadDelegate(ushort address);
        public delegate void MemoryWriteDelegate(ushort address, byte value);

        MemoryReadDelegate memoryReadDelegate;
        MemoryWriteDelegate memoryWriteDelegate;

        protected ushort pc;
        protected byte sp, a, x, y;
        protected Flags p;

        protected byte op;

        InterruptState intState, nmiState;

        int currentCycles;
        double clockRate, refreshRate;

        public MOS6502(double clockRate, double refreshRate, MemoryReadDelegate memoryRead, MemoryWriteDelegate memoryWrite)
        {
            this.refreshRate = refreshRate;
            this.clockRate = clockRate;

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
            pc = ReadMemory8(0xFFFC);
            sp = 0xFF;
            a = x = y = 0;
            p = 0;

            intState = nmiState = InterruptState.Clear;

            currentCycles = 0;
        }

        public int Step()
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

            /* Fetch and execute opcode */
            op = ReadMemory8(pc++);
            switch (op)
            {
                case 0x00: OpBRK(); break;
                case 0x01: OpORA(AddressingModes.IndirectX); break;
                case 0x05: OpORA(AddressingModes.ZeroPage); break;
                case 0x06: OpASL(AddressingModes.ZeroPage); break;
                case 0x08: PushP(); pc++; break;
                case 0x09: OpORA(AddressingModes.Immediate); break;
                case 0x0A: OpASL(AddressingModes.Accumulator); break;
                case 0x0D: OpORA(AddressingModes.Absolute); break;
                case 0x0E: OpASL(AddressingModes.Absolute); break;

                case 0x10: OpBPL(); break;
                case 0x11: OpORA(AddressingModes.IndirectY); break;
                case 0x15: OpORA(AddressingModes.ZeroPageX); break;
                case 0x16: OpASL(AddressingModes.ZeroPageX); break;
                case 0x18: ClearFlag(Flags.Carry); pc++; break;
                case 0x19: OpORA(AddressingModes.AbsoluteY); break;
                case 0x1D: OpORA(AddressingModes.AbsoluteX); break;
                case 0x1E: OpASL(AddressingModes.AbsoluteX); break;

                case 0x20: OpJSR(); break;
                case 0x21: OpAND(AddressingModes.IndirectX); break;
                case 0x24: OpBIT(AddressingModes.ZeroPage); break;
                case 0x25: OpAND(AddressingModes.ZeroPage); break;
                case 0x26: OpROL(AddressingModes.ZeroPage); break;
                case 0x28: PullP(); pc++; break;
                case 0x29: OpAND(AddressingModes.Immediate); break;
                case 0x2A: OpROL(AddressingModes.Accumulator); break;
                case 0x2C: OpBIT(AddressingModes.Absolute); break;
                case 0x2D: OpAND(AddressingModes.Absolute); break;
                case 0x2E: OpROL(AddressingModes.Absolute); break;

                case 0x30: OpBMI(); break;
                case 0x31: OpAND(AddressingModes.IndirectY); break;
                case 0x35: OpAND(AddressingModes.ZeroPageX); break;
                case 0x36: OpROL(AddressingModes.ZeroPageX); break;
                case 0x38: SetFlag(Flags.Carry); pc++; break;
                case 0x39: OpAND(AddressingModes.AbsoluteY); break;
                case 0x3D: OpAND(AddressingModes.AbsoluteX); break;
                case 0x3E: OpROL(AddressingModes.AbsoluteX); break;

                case 0x40: PullP(); pc = Pull16(); break;
                case 0x41: OpEOR(AddressingModes.IndirectX); break;
                case 0x45: OpEOR(AddressingModes.ZeroPage); break;
                case 0x46: OpLSR(AddressingModes.ZeroPage); break;
                case 0x48: Push(a); pc++; break;
                case 0x49: OpEOR(AddressingModes.Immediate); break;
                case 0x4A: OpLSR(AddressingModes.Accumulator); break;
                case 0x4C: OpJMP(AddressingModes.Absolute); break;
                case 0x4D: OpEOR(AddressingModes.Absolute); break;
                case 0x4E: OpLSR(AddressingModes.Absolute); break;

                case 0x50: OpBVC(); break;
                case 0x51: OpEOR(AddressingModes.IndirectY); break;
                case 0x55: OpEOR(AddressingModes.ZeroPageX); break;
                case 0x56: OpLSR(AddressingModes.ZeroPageX); break;
                case 0x58: ClearFlag(Flags.InterruptDisable); pc++; break;
                case 0x59: OpEOR(AddressingModes.AbsoluteY); break;
                case 0x5D: OpEOR(AddressingModes.AbsoluteX); break;
                case 0x5E: OpLSR(AddressingModes.AbsoluteX); break;

                case 0x60: pc = Pull16(); pc++; break;
                case 0x61: OpADC(AddressingModes.IndirectX); break;
                case 0x65: OpADC(AddressingModes.ZeroPage); break;
                case 0x66: OpROR(AddressingModes.ZeroPage); break;
                case 0x68: a = Pull(); SetClearFlagConditional(Flags.Zero, (a == 0x00)); SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80)); pc++; break;
                case 0x69: OpADC(AddressingModes.Immediate); break;
                case 0x6A: OpROR(AddressingModes.Accumulator); break;
                case 0x6C: OpJMP(AddressingModes.Indirect); break;
                case 0x6D: OpADC(AddressingModes.Absolute); break;
                case 0x6E: OpROR(AddressingModes.Absolute); break;

                case 0x70: OpBVS(); break;
                case 0x71: OpADC(AddressingModes.IndirectY); break;
                case 0x75: OpADC(AddressingModes.ZeroPageX); break;
                case 0x76: OpROR(AddressingModes.ZeroPageX); break;
                case 0x78: SetFlag(Flags.InterruptDisable); pc++; break;
                case 0x79: OpADC(AddressingModes.AbsoluteY); break;
                case 0x7D: OpADC(AddressingModes.AbsoluteX); break;
                case 0x7E: OpROR(AddressingModes.AbsoluteX); break;

                case 0x81: OpSTA(AddressingModes.IndirectX); break;
                case 0x84: OpSTY(AddressingModes.ZeroPage); break;
                case 0x85: OpSTA(AddressingModes.ZeroPage); break;
                case 0x86: OpSTX(AddressingModes.ZeroPage); break;
                case 0x88: OpDEY(); break;
                case 0x8A: a = x; SetClearFlagConditional(Flags.Zero, (a == 0x00)); SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80)); pc++; break;
                case 0x8C: OpSTY(AddressingModes.Absolute); break;
                case 0x8D: OpSTA(AddressingModes.Absolute); break;
                case 0x8E: OpSTX(AddressingModes.Absolute); break;

                case 0x90: OpBCC(); break;
                case 0x91: OpSTA(AddressingModes.IndirectY); break;
                case 0x94: OpSTY(AddressingModes.ZeroPageX); break;
                case 0x95: OpSTA(AddressingModes.ZeroPageX); break;
                case 0x96: OpSTX(AddressingModes.ZeroPageY); break;
                case 0x98: a = y; SetClearFlagConditional(Flags.Zero, (a == 0x00)); SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80)); pc++; break;
                case 0x99: OpSTA(AddressingModes.AbsoluteY); break;
                case 0x9A: sp = x; pc++; break;
                case 0x9D: OpSTA(AddressingModes.AbsoluteX); break;

                case 0xA0: OpLDY(AddressingModes.Immediate); break;
                case 0xA1: OpLDA(AddressingModes.IndirectX); break;
                case 0xA2: OpLDX(AddressingModes.Immediate); break;
                case 0xA4: OpLDY(AddressingModes.ZeroPage); break;
                case 0xA5: OpLDA(AddressingModes.ZeroPage); break;
                case 0xA6: OpLDX(AddressingModes.ZeroPage); break;
                case 0xA8: y = a; SetClearFlagConditional(Flags.Zero, (y == 0x00)); SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80)); pc++; break;
                case 0xA9: OpLDA(AddressingModes.Immediate); break;
                case 0xAA: x = a; SetClearFlagConditional(Flags.Zero, (x == 0x00)); SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80)); pc++; break;
                case 0xAC: OpLDY(AddressingModes.Absolute); break;
                case 0xAD: OpLDA(AddressingModes.Absolute); break;
                case 0xAE: OpLDX(AddressingModes.Absolute); break;

                case 0xB0: OpBCS(); break;
                case 0xB1: OpLDA(AddressingModes.IndirectY); break;
                case 0xB4: OpLDY(AddressingModes.ZeroPageX); break;
                case 0xB5: OpLDA(AddressingModes.ZeroPageX); break;
                case 0xB6: OpLDX(AddressingModes.ZeroPageY); break;
                case 0xB8: ClearFlag(Flags.Overflow); pc++; break;
                case 0xB9: OpLDA(AddressingModes.AbsoluteY); break;
                case 0xBA: x = sp; SetClearFlagConditional(Flags.Zero, (x == 0x00)); SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80)); pc++; break;
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
                case 0xD8: ClearFlag(Flags.DecimalMode); pc++; break;
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
                case 0xF8: SetFlag(Flags.DecimalMode); pc++; break;
                case 0xF9: OpSBC(AddressingModes.AbsoluteY); break;
                case 0xFD: OpSBC(AddressingModes.AbsoluteX); break;
                case 0xFE: OpINC(AddressingModes.AbsoluteX); break;
            }

            currentCycles += CycleCounts[op];

            return currentCycles;
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

        private void ServiceInterrupt()
        {
            if (!IsFlagSet(Flags.InterruptDisable))
            {
                ClearFlag(Flags.Brk);
                Push16(pc);
                PushP();
                SetFlag(Flags.InterruptDisable);
                pc = ReadMemory16(0xFFFE);

                currentCycles += 7;
            }
        }

        private void ServiceNonMaskableInterrupt()
        {
            ClearFlag(Flags.Brk);
            Push16(pc);
            PushP();
            SetFlag(Flags.InterruptDisable);
            pc = ReadMemory16(0xFFFA);

            currentCycles += 7;
        }

        private byte ReadMemory8(ushort address)
        {
            return memoryReadDelegate(address);
        }

        private void WriteMemory8(ushort address, byte value)
        {
            memoryWriteDelegate(address, value);
        }

        private ushort ReadMemory16(ushort address)
        {
            byte low = ReadMemory8(address);
            byte high = ReadMemory8((ushort)(address + 1));
            return (ushort)((high << 8) | low);
        }

        private void WriteMemory16(ushort address, ushort value)
        {
            WriteMemory8(address, (byte)(value & 0xFF));
            WriteMemory8((ushort)(address + 1), (byte)(value >> 8));
        }

        protected ushort CalculateAddress(byte a, byte b)
        {
            return (ushort)((b << 8) | a);
        }

        private byte ReadZeroPage(ushort address)
        {
            return ReadMemory8(address);
        }

        private byte ReadZeroPageX(ushort address)
        {
            return ReadMemory8((ushort)((address + x) & 0xFF));
        }

        private byte ReadZeroPageY(ushort address)
        {
            return ReadMemory8((ushort)((address + y) & 0xFF));
        }

        private byte ReadAbsolute(byte address1, byte address2)
        {
            return ReadMemory8(CalculateAddress(address1, address2));
        }

        private byte ReadAbsoluteX(byte address1, byte address2)
        {
            if ((CalculateAddress(address1, address2) & 0xFF00) != ((CalculateAddress(address1, address2) + x) & 0xFF00))
                currentCycles += 1;

            return ReadMemory8((ushort)(CalculateAddress(address1, address2) + x));
        }

        private byte ReadAbsoluteY(byte address1, byte address2)
        {
            if ((CalculateAddress(address1, address2) & 0xFF00) != ((CalculateAddress(address1, address2) + y) & 0xFF00))
                currentCycles += 1;

            return ReadMemory8((ushort)(CalculateAddress(address1, address2) + y));
        }

        private byte ReadIndirectX(byte address)
        {
            return ReadMemory8(ReadMemory16((ushort)((address + x) & 0xFF)));
        }

        private byte ReadIndirectY(byte address)
        {
            if ((ReadMemory16(address) & 0xFF00) != ((ReadMemory16(address) + y) & 0xFF00))
                currentCycles += 1;

            return ReadMemory8((ushort)(ReadMemory16(address) + y));
        }

        private void WriteZeroPage(ushort address, byte Value)
        {
            WriteMemory8(address, Value);
        }

        private void WriteZeroPageX(ushort address, byte value)
        {
            WriteMemory8((ushort)((address + x) & 0xFF), value);
        }

        private void WriteZeroPageY(ushort address, byte value)
        {
            WriteMemory8((ushort)((address + y) & 0xFF), value);
        }

        private void WriteAbsolute(byte address1, byte address2, byte value)
        {
            WriteMemory8(CalculateAddress(address1, address2), value);
        }

        private void WriteAbsoluteX(byte address1, byte address2, byte value)
        {
            WriteMemory8((ushort)(CalculateAddress(address1, address2) + x), value);
        }

        private void WriteAbsoluteY(byte address1, byte address2, byte value)
        {
            WriteMemory8((ushort)(CalculateAddress(address1, address2) + y), value);
        }

        private void WriteIndirectX(byte address, byte value)
        {
            WriteMemory8(ReadMemory16((ushort)((address + x) & 0xFF)), value);
        }

        private void WriteIndirectY(byte address, byte value)
        {
            WriteMemory8((ushort)(ReadMemory16(address) + y), value);
        }

        private byte GetOperand(AddressingModes mode)
        {
            byte arg1 = ReadMemory8((ushort)(pc + 1));
            byte arg2 = ReadMemory8((ushort)(pc + 2));
            byte value = 0xFF;

            switch (mode)
            {
                case AddressingModes.Implied: break;
                case AddressingModes.Accumulator: value = a; break;
                case AddressingModes.Immediate: value = arg1; pc++; break;
                case AddressingModes.ZeroPage: value = ReadZeroPage(arg1); pc++; break;
                case AddressingModes.ZeroPageX: value = ReadZeroPageX(arg1); pc++; break;
                case AddressingModes.ZeroPageY: value = ReadZeroPageY(arg1); pc++; break;
                case AddressingModes.Absolute: value = ReadAbsolute(arg1, arg2); pc += 2; break;
                case AddressingModes.AbsoluteX: value = ReadAbsoluteX(arg1, arg2); pc += 2; break;
                case AddressingModes.AbsoluteY: value = ReadAbsoluteY(arg1, arg2); pc += 2; break;
                case AddressingModes.IndirectX: value = ReadIndirectX(arg1); pc++; break;
                case AddressingModes.IndirectY: value = ReadIndirectY(arg1); pc++; break;
                default: throw new Exception("6502 addressing mode error on read");
            }

            return value;
        }

        private void WriteValue(AddressingModes mode, byte value)
        {
            byte arg1 = ReadMemory8((ushort)(pc + 1));
            byte arg2 = ReadMemory8((ushort)(pc + 2));

            switch (mode)
            {
                case AddressingModes.Implied: break;
                case AddressingModes.Accumulator: a = value; break;
                case AddressingModes.ZeroPage: WriteZeroPage(arg1, value); pc++; break;
                case AddressingModes.ZeroPageX: WriteZeroPageX(arg1, value); pc++; break;
                case AddressingModes.ZeroPageY: WriteZeroPageY(arg1, value); pc++; break;
                case AddressingModes.Absolute: WriteAbsolute(arg1, arg2, value); pc += 2; break;
                case AddressingModes.AbsoluteX: WriteAbsoluteX(arg1, arg2, value); pc += 2; break;
                case AddressingModes.AbsoluteY: WriteAbsoluteY(arg1, arg2, value); pc += 2; break;
                case AddressingModes.IndirectX: WriteIndirectX(arg1, value); pc++; break;
                case AddressingModes.IndirectY: WriteIndirectY(arg1, value); pc++; break;
                default: throw new Exception("6502 addressing mode error on write");
            }
        }

        public void Push(byte value)
        {
            WriteMemory8((ushort)(0x100 + sp), (byte)(value & 0xff));
            sp = (byte)(sp - 1);
        }

        public void Push16(ushort value)
        {
            Push((byte)(value >> 8));
            Push((byte)(value & 0xff));
        }

        public void PushP()
        {
            Push((byte)p);
        }

        public byte Pull()
        {
            sp = (byte)(sp + 1);
            return ReadMemory8((ushort)(0x100 + sp));
        }

        public ushort Pull16()
        {
            return CalculateAddress(Pull(), Pull());
        }

        public void PullP()
        {
            p = (Flags)Pull();
        }

        private void OpADC(AddressingModes mode)
        {
            byte data = GetOperand(mode);

            uint w;

            SetClearFlagConditional(Flags.Overflow, ((a ^ data) & 0x80) != 0);

            w = (uint)(a + data + (IsFlagSet(Flags.Carry) ? 1 : 0));
            if (w >= 0x100)
            {
                SetFlag(Flags.Carry);
                SetClearFlagConditional(Flags.Overflow, !(IsFlagSet(Flags.Overflow) && w >= 0x180));
            }
            else
            {
                ClearFlag(Flags.Carry);
                SetClearFlagConditional(Flags.Overflow, !(IsFlagSet(Flags.Overflow) && w < 0x80));
            }

            a = (byte)(w & 0xFF);
            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));
        }

        private void OpAND(AddressingModes mode)
        {
            a = (byte)(a & GetOperand(mode));

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));
        }

        private void OpASL(AddressingModes mode)
        {
            uint value = GetOperand(mode);
            SetClearFlagConditional(Flags.Carry, ((value & 0x80) == 0x80));

            value = (byte)(value << 1);
            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(mode, (byte)value);
        }

        private void OpBCC()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            if (!IsFlagSet(Flags.Carry))
            {
                if ((pc & 0xFF00) != ((pc + (sbyte)value + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        private void OpBCS()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            if (IsFlagSet(Flags.Carry))
            {
                if ((pc & 0xFF00) != ((pc + (sbyte)value + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        private void OpBEQ()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            if (IsFlagSet(Flags.Zero))
            {
                if ((pc & 0xFF00) != ((pc + (sbyte)value + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        private void OpBIT(AddressingModes Mode)
        {
            uint value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Zero, ((a & value) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));
            SetClearFlagConditional(Flags.Overflow, ((value & 0x40) == 0x40));
        }

        private void OpBMI()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            if (IsFlagSet(Flags.Sign))
            {
                if ((pc & 0xFF00) != ((pc + (sbyte)value + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        private void OpBNE()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            if (!IsFlagSet(Flags.Zero))
            {
                if ((pc & 0xFF00) != ((pc + (sbyte)value + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        private void OpBPL()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            if (!IsFlagSet(Flags.Sign))
            {
                if ((pc & 0xFF00) != ((pc + (sbyte)value + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        private void OpBRK()
        {
            pc += 2;
            Push16(pc);
            SetFlag(Flags.Brk);
            PushP();
            SetFlag(Flags.InterruptDisable);
            pc = ReadMemory16(0xFFFE);
        }

        private void OpBVC()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            if (!IsFlagSet(Flags.Overflow))
            {
                if ((pc & 0xFF00) != ((pc + (sbyte)value + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        private void OpBVS()
        {
            byte value = GetOperand(AddressingModes.Immediate);

            if (IsFlagSet(Flags.Overflow))
            {
                if ((pc & 0xFF00) != ((pc + (sbyte)value + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + (sbyte)value);
            }
        }

        private void OpCMP(AddressingModes Mode)
        {
            uint value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Carry, a >= (byte)value);

            value = (byte)(a - value);

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));
        }

        private void OpCPX(AddressingModes Mode)
        {
            uint value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Carry, x >= (byte)value);

            value = (byte)(x - value);

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));
        }

        private void OpCPY(AddressingModes Mode)
        {
            uint value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Carry, y >= (byte)value);

            value = (byte)(y - value);

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));
        }

        private void OpDEC(AddressingModes Mode)
        {
            uint value = GetOperand(Mode);

            value--;

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(Mode, (byte)(value & 0xFF));
        }

        private void OpDEX()
        {
            x--;

            SetClearFlagConditional(Flags.Zero, (x == 0x00));
            SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80));
        }

        private void OpDEY()
        {
            y--;

            SetClearFlagConditional(Flags.Zero, (y == 0x00));
            SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80));
        }

        private void OpEOR(AddressingModes Mode)
        {
            a = (byte)(a ^ GetOperand(Mode));

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));
        }

        private void OpINC(AddressingModes Mode)
        {
            uint value = GetOperand(Mode);

            value++;

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(Mode, (byte)(value & 0xFF));
        }

        private void OpINX()
        {
            x++;

            SetClearFlagConditional(Flags.Zero, (x == 0x00));
            SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80));
        }

        private void OpINY()
        {
            y++;

            SetClearFlagConditional(Flags.Zero, (y == 0x00));
            SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80));
        }

        private void OpJMP(AddressingModes Mode)
        {
            ushort address = ReadMemory16((ushort)(pc + 1));

            switch (Mode)
            {
                case AddressingModes.Absolute: pc = address; break;
                case AddressingModes.Indirect: pc = ReadMemory16(address); break;
                default: throw new Exception("6502 addressing mode error on jump");
            }
        }

        private void OpJSR()
        {
            byte arg1 = ReadMemory8((ushort)(pc + 1));
            byte arg2 = ReadMemory8((ushort)(pc + 2));
            Push16((ushort)(pc + 2));
            pc = CalculateAddress(arg1, arg2);
        }

        private void OpLDA(AddressingModes Mode)
        {
            a = (byte)(GetOperand(Mode) & 0xFF);

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));
        }

        private void OpLDX(AddressingModes Mode)
        {
            x = (byte)(GetOperand(Mode) & 0xFF);

            SetClearFlagConditional(Flags.Zero, (x == 0x00));
            SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80));
        }

        private void OpLDY(AddressingModes Mode)
        {
            y = (byte)(GetOperand(Mode) & 0xFF);

            SetClearFlagConditional(Flags.Zero, (y == 0x00));
            SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80));
        }

        private void OpLSR(AddressingModes Mode)
        {
            uint value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Sign, ((value & 0x01) == 0x01));

            value = (byte)(value >> 1);

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(Mode, (byte)value);
        }

        private void OpNOP()
        {
            pc++;
        }

        private void OpORA(AddressingModes Mode)
        {
            a = (byte)(a | GetOperand(Mode));

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));
        }

        private void OpROL(AddressingModes Mode)
        {
            uint value = GetOperand(Mode);
            bool tempBit = ((value & 0x80) == 0x80);

            value = (byte)(value << 1);
            value = (byte)(value | (byte)(IsFlagSet(Flags.Carry) ? 0x01 : 0x00));

            SetClearFlagConditional(Flags.Carry, tempBit);

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(Mode, (byte)value);
        }

        private void OpROR(AddressingModes Mode)
        {
            uint value = GetOperand(Mode);
            bool tempBit = ((value & 0x01) == 0x01);

            value = (byte)(value >> 1);

            if (IsFlagSet(Flags.Carry))
                value = (byte)(value | 0x80);

            SetClearFlagConditional(Flags.Carry, tempBit);

            SetClearFlagConditional(Flags.Zero, (value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((value & 0x80) == 0x80));

            WriteValue(Mode, (byte)value);
        }

        private void OpSBC(AddressingModes Mode)
        {
            byte data = GetOperand(Mode);
            uint w;

            SetClearFlagConditional(Flags.Overflow, ((a ^ data) & 0x80) != 0);

            w = (uint)(0xff + a - data + (IsFlagSet(Flags.Carry) ? 1 : 0));
            if (w < 0x100)
            {
                ClearFlag(Flags.Carry);
                SetClearFlagConditional(Flags.Overflow, !(IsFlagSet(Flags.Overflow) && w < 0x80));
            }
            else
            {
                SetFlag(Flags.Carry);
                SetClearFlagConditional(Flags.Overflow, !(IsFlagSet(Flags.Overflow) && w >= 0x180));
            }

            a = (byte)(w & 0xFF);

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));
        }

        private void OpSTA(AddressingModes Mode)
        {
            WriteValue(Mode, a);
        }

        private void OpSTX(AddressingModes Mode)
        {
            WriteValue(Mode, x);
        }

        private void OpSTY(AddressingModes Mode)
        {
            WriteValue(Mode, y);
        }
    }
}
