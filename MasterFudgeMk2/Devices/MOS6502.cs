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

        protected ushort PC;
        protected byte SP, A, X, Y;
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
            PC = ReadMemory8(0xFFFC);
            SP = 0xFF;
            A = X = Y = 0;
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
                ServiceInterrupt();
            }

            /* Check NMI line */
            if (nmiState == InterruptState.Assert)
            {
                nmiState = InterruptState.Clear;
                ServiceNonMaskableInterrupt();
            }

            /* Fetch and execute opcode */
            op = ReadMemory8(PC++);
            switch (op)
            {
                case 0x00: OpBRK(); break;
                case 0x01: OpORA(AddressingModes.IndirectX); break;
                case 0x05: OpORA(AddressingModes.ZeroPage); break;
                case 0x06: OpASL(AddressingModes.ZeroPage); break;
                case 0x08: PushP(); PC++; break;
                case 0x09: OpORA(AddressingModes.Immediate); break;
                case 0x0A: OpASL(AddressingModes.Accumulator); break;
                case 0x0D: OpORA(AddressingModes.Absolute); break;
                case 0x0E: OpASL(AddressingModes.Absolute); break;

                case 0x10: OpBPL(); break;
                case 0x11: OpORA(AddressingModes.IndirectY); break;
                case 0x15: OpORA(AddressingModes.ZeroPageX); break;
                case 0x16: OpASL(AddressingModes.ZeroPageX); break;
                case 0x18: ClearFlag(Flags.Carry); PC++; break;
                case 0x19: OpORA(AddressingModes.AbsoluteY); break;
                case 0x1D: OpORA(AddressingModes.AbsoluteX); break;
                case 0x1E: OpASL(AddressingModes.AbsoluteX); break;

                case 0x20: OpJSR(); break;
                case 0x21: OpAND(AddressingModes.IndirectX); break;
                case 0x24: OpBIT(AddressingModes.ZeroPage); break;
                case 0x25: OpAND(AddressingModes.ZeroPage); break;
                case 0x26: OpROL(AddressingModes.ZeroPage); break;
                case 0x28: PullP(); PC++; break;
                case 0x29: OpAND(AddressingModes.Immediate); break;
                case 0x2A: OpROL(AddressingModes.Accumulator); break;
                case 0x2C: OpBIT(AddressingModes.Absolute); break;
                case 0x2D: OpAND(AddressingModes.Absolute); break;
                case 0x2E: OpROL(AddressingModes.Absolute); break;

                case 0x30: OpBMI(); break;
                case 0x31: OpAND(AddressingModes.IndirectY); break;
                case 0x35: OpAND(AddressingModes.ZeroPageX); break;
                case 0x36: OpROL(AddressingModes.ZeroPageX); break;
                case 0x38: SetFlag(Flags.Carry); PC++; break;
                case 0x39: OpAND(AddressingModes.AbsoluteY); break;
                case 0x3D: OpAND(AddressingModes.AbsoluteX); break;
                case 0x3E: OpROL(AddressingModes.AbsoluteX); break;

                case 0x40: PullP(); PC = Pull16(); break;
                case 0x41: OpEOR(AddressingModes.IndirectX); break;
                case 0x45: OpEOR(AddressingModes.ZeroPage); break;
                case 0x46: OpLSR(AddressingModes.ZeroPage); break;
                case 0x48: Push(A); PC++; break;
                case 0x49: OpEOR(AddressingModes.Immediate); break;
                case 0x4A: OpLSR(AddressingModes.Accumulator); break;
                case 0x4C: OpJMP(AddressingModes.Absolute); break;
                case 0x4D: OpEOR(AddressingModes.Absolute); break;
                case 0x4E: OpLSR(AddressingModes.Absolute); break;

                case 0x50: OpBVC(); break;
                case 0x51: OpEOR(AddressingModes.IndirectY); break;
                case 0x55: OpEOR(AddressingModes.ZeroPageX); break;
                case 0x56: OpLSR(AddressingModes.ZeroPageX); break;
                case 0x58: ClearFlag(Flags.InterruptDisable); PC++; break;
                case 0x59: OpEOR(AddressingModes.AbsoluteY); break;
                case 0x5D: OpEOR(AddressingModes.AbsoluteX); break;
                case 0x5E: OpLSR(AddressingModes.AbsoluteX); break;

                case 0x60: PC = Pull16(); PC++; break;
                case 0x61: OpADC(AddressingModes.IndirectX); break;
                case 0x65: OpADC(AddressingModes.ZeroPage); break;
                case 0x66: OpROR(AddressingModes.ZeroPage); break;
                case 0x68: A = Pull(); SetClearFlagConditional(Flags.Zero, (A == 0x00)); SetClearFlagConditional(Flags.Sign, ((A & 0x80) == 0x80)); PC++; break;
                case 0x69: OpADC(AddressingModes.Immediate); break;
                case 0x6A: OpROR(AddressingModes.Accumulator); break;
                case 0x6C: OpJMP(AddressingModes.Indirect); break;
                case 0x6D: OpADC(AddressingModes.Absolute); break;
                case 0x6E: OpROR(AddressingModes.Absolute); break;

                case 0x70: OpBVS(); break;
                case 0x71: OpADC(AddressingModes.IndirectY); break;
                case 0x75: OpADC(AddressingModes.ZeroPageX); break;
                case 0x76: OpROR(AddressingModes.ZeroPageX); break;
                case 0x78: SetFlag(Flags.InterruptDisable); PC++; break;
                case 0x79: OpADC(AddressingModes.AbsoluteY); break;
                case 0x7D: OpADC(AddressingModes.AbsoluteX); break;
                case 0x7E: OpROR(AddressingModes.AbsoluteX); break;

                case 0x81: OpSTA(AddressingModes.IndirectX); break;
                case 0x84: OpSTY(AddressingModes.ZeroPage); break;
                case 0x85: OpSTA(AddressingModes.ZeroPage); break;
                case 0x86: OpSTX(AddressingModes.ZeroPage); break;
                case 0x88: OpDEY(); break;
                case 0x8A: A = X; SetClearFlagConditional(Flags.Zero, (A == 0x00)); SetClearFlagConditional(Flags.Sign, ((A & 0x80) == 0x80)); PC++; break;
                case 0x8C: OpSTY(AddressingModes.Absolute); break;
                case 0x8D: OpSTA(AddressingModes.Absolute); break;
                case 0x8E: OpSTX(AddressingModes.Absolute); break;

                case 0x90: OpBCC(); break;
                case 0x91: OpSTA(AddressingModes.IndirectY); break;
                case 0x94: OpSTY(AddressingModes.ZeroPageX); break;
                case 0x95: OpSTA(AddressingModes.ZeroPageX); break;
                case 0x96: OpSTX(AddressingModes.ZeroPageY); break;
                case 0x98: A = Y; SetClearFlagConditional(Flags.Zero, (A == 0x00)); SetClearFlagConditional(Flags.Sign, ((A & 0x80) == 0x80)); PC++; break;
                case 0x99: OpSTA(AddressingModes.AbsoluteY); break;
                case 0x9A: SP = X; PC++; break;
                case 0x9D: OpSTA(AddressingModes.AbsoluteX); break;

                case 0xA0: OpLDY(AddressingModes.Immediate); break;
                case 0xA1: OpLDA(AddressingModes.IndirectX); break;
                case 0xA2: OpLDX(AddressingModes.Immediate); break;
                case 0xA4: OpLDY(AddressingModes.ZeroPage); break;
                case 0xA5: OpLDA(AddressingModes.ZeroPage); break;
                case 0xA6: OpLDX(AddressingModes.ZeroPage); break;
                case 0xA8: Y = A; SetClearFlagConditional(Flags.Zero, (Y == 0x00)); SetClearFlagConditional(Flags.Sign, ((Y & 0x80) == 0x80)); PC++; break;
                case 0xA9: OpLDA(AddressingModes.Immediate); break;
                case 0xAA: X = A; SetClearFlagConditional(Flags.Zero, (X == 0x00)); SetClearFlagConditional(Flags.Sign, ((X & 0x80) == 0x80)); PC++; break;
                case 0xAC: OpLDY(AddressingModes.Absolute); break;
                case 0xAD: OpLDA(AddressingModes.Absolute); break;
                case 0xAE: OpLDX(AddressingModes.Absolute); break;

                case 0xB0: OpBCS(); break;
                case 0xB1: OpLDA(AddressingModes.IndirectY); break;
                case 0xB4: OpLDY(AddressingModes.ZeroPageX); break;
                case 0xB5: OpLDA(AddressingModes.ZeroPageX); break;
                case 0xB6: OpLDX(AddressingModes.ZeroPageY); break;
                case 0xB8: ClearFlag(Flags.Overflow); PC++; break;
                case 0xB9: OpLDA(AddressingModes.AbsoluteY); break;
                case 0xBA: X = SP; SetClearFlagConditional(Flags.Zero, (X == 0x00)); SetClearFlagConditional(Flags.Sign, ((X & 0x80) == 0x80)); PC++; break;
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
                case 0xD8: ClearFlag(Flags.DecimalMode); PC++; break;
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
                case 0xF8: SetFlag(Flags.DecimalMode); PC++; break;
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
            //
        }

        private void ServiceNonMaskableInterrupt()
        {
            //
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

        private byte ReadZeroPage(ushort A)
        {
            return ReadMemory8(A);
        }

        private byte ReadZeroPageX(ushort A)
        {
            return ReadMemory8((ushort)((A + X) & 0xFF));
        }

        private byte ReadZeroPageY(ushort A)
        {
            return ReadMemory8((ushort)((A + Y) & 0xFF));
        }

        private byte ReadAbsolute(byte A, byte B)
        {
            return ReadMemory8(CalculateAddress(A, B));
        }

        private byte ReadAbsoluteX(byte A, byte B)
        {
            if ((CalculateAddress(A, B) & 0xFF00) != ((CalculateAddress(A, B) + X) & 0xFF00))
                currentCycles += 1;

            return ReadMemory8((ushort)(CalculateAddress(A, B) + X));
        }

        private byte ReadAbsoluteY(byte A, byte B)
        {
            if ((CalculateAddress(A, B) & 0xFF00) != ((CalculateAddress(A, B) + Y) & 0xFF00))
                currentCycles += 1;

            return ReadMemory8((ushort)(CalculateAddress(A, B) + Y));
        }

        private byte ReadIndirectX(byte A)
        {
            return ReadMemory8(ReadMemory16((ushort)((A + X) & 0xFF)));
        }

        private byte ReadIndirectY(byte A)
        {
            if ((ReadMemory16(A) & 0xFF00) != ((ReadMemory16(A) + Y) & 0xFF00))
                currentCycles += 1;

            return ReadMemory8((ushort)(ReadMemory16(A) + Y));
        }

        private void WriteZeroPage(ushort A, byte Value)
        {
            WriteMemory8(A, Value);
        }

        private void WriteZeroPageX(ushort A, byte Value)
        {
            WriteMemory8((ushort)((A + X) & 0xFF), Value);
        }

        private void WriteZeroPageY(ushort A, byte Value)
        {
            WriteMemory8((ushort)((A + Y) & 0xFF), Value);
        }

        private void WriteAbsolute(byte A, byte B, byte Value)
        {
            WriteMemory8(CalculateAddress(A, B), Value);
        }

        private void WriteAbsoluteX(byte A, byte B, byte Value)
        {
            WriteMemory8((ushort)(CalculateAddress(A, B) + X), Value);
        }

        private void WriteAbsoluteY(byte A, byte B, byte Value)
        {
            WriteMemory8((ushort)(CalculateAddress(A, B) + Y), Value);
        }

        private void WriteIndirectX(byte A, byte Value)
        {
            WriteMemory8(ReadMemory16((ushort)((A + X) & 0xFF)), Value);
        }

        private void WriteIndirectY(byte A, byte Value)
        {
            WriteMemory8((ushort)(ReadMemory16(A) + Y), Value);
        }

        private byte GetOperand(AddressingModes Mode)
        {
            byte Arg1 = ReadMemory8((ushort)(PC + 1));
            byte Arg2 = ReadMemory8((ushort)(PC + 2));
            byte Value = 0xFF;

            switch (Mode)
            {
                case AddressingModes.Implied: break;
                case AddressingModes.Accumulator: Value = A; break;
                case AddressingModes.Immediate: Value = Arg1; PC++; break;
                case AddressingModes.ZeroPage: Value = ReadZeroPage(Arg1); PC++; break;
                case AddressingModes.ZeroPageX: Value = ReadZeroPageX(Arg1); PC++; break;
                case AddressingModes.ZeroPageY: Value = ReadZeroPageY(Arg1); PC++; break;
                case AddressingModes.Absolute: Value = ReadAbsolute(Arg1, Arg2); PC += 2; break;
                case AddressingModes.AbsoluteX: Value = ReadAbsoluteX(Arg1, Arg2); PC += 2; break;
                case AddressingModes.AbsoluteY: Value = ReadAbsoluteY(Arg1, Arg2); PC += 2; break;
                case AddressingModes.IndirectX: Value = ReadIndirectX(Arg1); PC++; break;
                case AddressingModes.IndirectY: Value = ReadIndirectY(Arg1); PC++; break;
                default: throw new Exception("6502 addressing mode error on read");
            }

            return Value;
        }

        private void WriteValue(AddressingModes Mode, byte Value)
        {
            byte Arg1 = ReadMemory8((ushort)(PC + 1));
            byte Arg2 = ReadMemory8((ushort)(PC + 2));

            switch (Mode)
            {
                case AddressingModes.Implied: break;
                case AddressingModes.Accumulator: A = Value; break;
                case AddressingModes.ZeroPage: WriteZeroPage(Arg1, Value); PC++; break;
                case AddressingModes.ZeroPageX: WriteZeroPageX(Arg1, Value); PC++; break;
                case AddressingModes.ZeroPageY: WriteZeroPageY(Arg1, Value); PC++; break;
                case AddressingModes.Absolute: WriteAbsolute(Arg1, Arg2, Value); PC += 2; break;
                case AddressingModes.AbsoluteX: WriteAbsoluteX(Arg1, Arg2, Value); PC += 2; break;
                case AddressingModes.AbsoluteY: WriteAbsoluteY(Arg1, Arg2, Value); PC += 2; break;
                case AddressingModes.IndirectX: WriteIndirectX(Arg1, Value); PC++; break;
                case AddressingModes.IndirectY: WriteIndirectY(Arg1, Value); PC++; break;
                default: throw new Exception("6502 addressing mode error on write");
            }
        }

        public void Push(byte Value)
        {
            WriteMemory8((ushort)(0x100 + SP), (byte)(Value & 0xff));
            SP = (byte)(SP - 1);
        }

        public void Push16(ushort Value)
        {
            Push((byte)(Value >> 8));
            Push((byte)(Value & 0xff));
        }

        public void PushP()
        {
            Push((byte)p);
        }

        public byte Pull()
        {
            SP = (byte)(SP + 1);
            return ReadMemory8((ushort)(0x100 + SP));
        }

        public ushort Pull16()
        {
            return CalculateAddress(Pull(), Pull());
        }

        public void PullP()
        {
            p = (Flags)Pull();
        }

        private void OpADC(AddressingModes Mode)
        {
            byte Data = GetOperand(Mode);

            uint w;

            SetClearFlagConditional(Flags.Overflow, ((A ^ Data) & 0x80) != 0);

            w = (uint)(A + Data + (IsFlagSet(Flags.Carry) ? 1 : 0));
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

            A = (byte)(w & 0xFF);
            SetClearFlagConditional(Flags.Zero, (A == 0x00));
            SetClearFlagConditional(Flags.Sign, ((A & 0x80) == 0x80));
        }

        private void OpAND(AddressingModes Mode)
        {
            A = (byte)(A & GetOperand(Mode));

            SetClearFlagConditional(Flags.Zero, (A == 0x00));
            SetClearFlagConditional(Flags.Sign, ((A & 0x80) == 0x80));
        }

        private void OpASL(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);
            SetClearFlagConditional(Flags.Carry, ((Value & 0x80) == 0x80));

            Value = (byte)(Value << 1);
            SetClearFlagConditional(Flags.Zero, (Value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));

            WriteValue(Mode, (byte)Value);
        }

        private void OpBCC()
        {
            byte Value = GetOperand(AddressingModes.Immediate);

            if (!IsFlagSet(Flags.Carry))
            {
                if ((PC & 0xFF00) != ((PC + (sbyte)Value + 2) & 0xFF00))
                    currentCycles += 1;

                PC = (ushort)(PC + (sbyte)Value);
            }
        }

        private void OpBCS()
        {
            byte Value = GetOperand(AddressingModes.Immediate);

            if (IsFlagSet(Flags.Carry))
            {
                if ((PC & 0xFF00) != ((PC + (sbyte)Value + 2) & 0xFF00))
                    currentCycles += 1;

                PC = (ushort)(PC + (sbyte)Value);
            }
        }

        private void OpBEQ()
        {
            byte Value = GetOperand(AddressingModes.Immediate);

            if (IsFlagSet(Flags.Zero))
            {
                if ((PC & 0xFF00) != ((PC + (sbyte)Value + 2) & 0xFF00))
                    currentCycles += 1;

                PC = (ushort)(PC + (sbyte)Value);
            }
        }

        private void OpBIT(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Zero, ((A & Value) == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));
            SetClearFlagConditional(Flags.Overflow, ((Value & 0x40) == 0x40));
        }

        private void OpBMI()
        {
            byte Value = GetOperand(AddressingModes.Immediate);

            if (IsFlagSet(Flags.Sign))
            {
                if ((PC & 0xFF00) != ((PC + (sbyte)Value + 2) & 0xFF00))
                    currentCycles += 1;

                PC = (ushort)(PC + (sbyte)Value);
            }
        }

        private void OpBNE()
        {
            byte Value = GetOperand(AddressingModes.Immediate);

            if (!IsFlagSet(Flags.Zero))
            {
                if ((PC & 0xFF00) != ((PC + (sbyte)Value + 2) & 0xFF00))
                    currentCycles += 1;

                PC = (ushort)(PC + (sbyte)Value);
            }
        }

        private void OpBPL()
        {
            byte Value = GetOperand(AddressingModes.Immediate);

            if (!IsFlagSet(Flags.Sign))
            {
                if ((PC & 0xFF00) != ((PC + (sbyte)Value + 2) & 0xFF00))
                    currentCycles += 1;

                PC = (ushort)(PC + (sbyte)Value);
            }
        }

        private void OpBRK()
        {
            PC += 2;
            Push16(PC);
            SetFlag(Flags.Brk);
            PushP();
            SetFlag(Flags.InterruptDisable);
            PC = ReadMemory16(0xFFFE);
        }

        private void OpBVC()
        {
            byte Value = GetOperand(AddressingModes.Immediate);

            if (!IsFlagSet(Flags.Overflow))
            {
                if ((PC & 0xFF00) != ((PC + (sbyte)Value + 2) & 0xFF00))
                    currentCycles += 1;

                PC = (ushort)(PC + (sbyte)Value);
            }
        }

        private void OpBVS()
        {
            byte Value = GetOperand(AddressingModes.Immediate);

            if (IsFlagSet(Flags.Overflow))
            {
                if ((PC & 0xFF00) != ((PC + (sbyte)Value + 2) & 0xFF00))
                    currentCycles += 1;

                PC = (ushort)(PC + (sbyte)Value);
            }
        }

        private void OpCMP(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Carry, A >= (byte)Value);

            Value = (byte)(A - Value);

            SetClearFlagConditional(Flags.Zero, (Value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));
        }

        private void OpCPX(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Carry, X >= (byte)Value);

            Value = (byte)(X - Value);

            SetClearFlagConditional(Flags.Zero, (Value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));
        }

        private void OpCPY(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Carry, Y >= (byte)Value);

            Value = (byte)(Y - Value);

            SetClearFlagConditional(Flags.Zero, (Value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));
        }

        private void OpDEC(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);

            Value--;

            SetClearFlagConditional(Flags.Zero, (Value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));

            WriteValue(Mode, (byte)(Value & 0xFF));
        }

        private void OpDEX()
        {
            X--;

            SetClearFlagConditional(Flags.Zero, (X == 0x00));
            SetClearFlagConditional(Flags.Sign, ((X & 0x80) == 0x80));
        }

        private void OpDEY()
        {
            Y--;

            SetClearFlagConditional(Flags.Zero, (Y == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Y & 0x80) == 0x80));
        }

        private void OpEOR(AddressingModes Mode)
        {
            A = (byte)(A ^ GetOperand(Mode));

            SetClearFlagConditional(Flags.Zero, (A == 0x00));
            SetClearFlagConditional(Flags.Sign, ((A & 0x80) == 0x80));
        }

        private void OpINC(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);

            Value++;

            SetClearFlagConditional(Flags.Zero, (Value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));

            WriteValue(Mode, (byte)(Value & 0xFF));
        }

        private void OpINX()
        {
            X++;

            SetClearFlagConditional(Flags.Zero, (X == 0x00));
            SetClearFlagConditional(Flags.Sign, ((X & 0x80) == 0x80));
        }

        private void OpINY()
        {
            Y++;

            SetClearFlagConditional(Flags.Zero, (Y == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Y & 0x80) == 0x80));
        }

        private void OpJMP(AddressingModes Mode)
        {
            ushort Address = ReadMemory16((ushort)(PC + 1));

            switch (Mode)
            {
                case AddressingModes.Absolute: PC = Address; break;
                case AddressingModes.Indirect: PC = ReadMemory16(Address); break;
                default: throw new Exception("6502 addressing mode error on jump");
            }
        }

        private void OpJSR()
        {
            byte Arg1 = ReadMemory8((ushort)(PC + 1));
            byte Arg2 = ReadMemory8((ushort)(PC + 2));
            Push16((ushort)(PC + 2));
            PC = CalculateAddress(Arg1, Arg2);
        }

        private void OpLDA(AddressingModes Mode)
        {
            A = (byte)(GetOperand(Mode) & 0xFF);

            SetClearFlagConditional(Flags.Zero, (A == 0x00));
            SetClearFlagConditional(Flags.Sign, ((A & 0x80) == 0x80));
        }

        private void OpLDX(AddressingModes Mode)
        {
            X = (byte)(GetOperand(Mode) & 0xFF);

            SetClearFlagConditional(Flags.Zero, (X == 0x00));
            SetClearFlagConditional(Flags.Sign, ((X & 0x80) == 0x80));
        }

        private void OpLDY(AddressingModes Mode)
        {
            Y = (byte)(GetOperand(Mode) & 0xFF);

            SetClearFlagConditional(Flags.Zero, (Y == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Y & 0x80) == 0x80));
        }

        private void OpLSR(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);

            SetClearFlagConditional(Flags.Sign, ((Value & 0x01) == 0x01));

            Value = (byte)(Value >> 1);

            SetClearFlagConditional(Flags.Zero, (Value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));

            WriteValue(Mode, (byte)Value);
        }

        private void OpNOP()
        {
            PC++;
        }

        private void OpORA(AddressingModes Mode)
        {
            A = (byte)(A | GetOperand(Mode));

            SetClearFlagConditional(Flags.Zero, (A == 0x00));
            SetClearFlagConditional(Flags.Sign, ((A & 0x80) == 0x80));
        }

        private void OpROL(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);
            bool TempBit = false;

            if ((Value & 0x80) == 0x80)
                TempBit = true;
            else
                TempBit = false;

            Value = (byte)(Value << 1);
            Value = (byte)(Value | (byte)(IsFlagSet(Flags.Carry) ? 0x01 : 0x00));

            SetClearFlagConditional(Flags.Carry, TempBit);

            SetClearFlagConditional(Flags.Zero, (Value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));

            WriteValue(Mode, (byte)Value);
        }

        private void OpROR(AddressingModes Mode)
        {
            uint Value = GetOperand(Mode);
            bool TempBit = false;

            if ((Value & 0x01) == 0x01)
                TempBit = true;
            else
                TempBit = false;

            Value = (byte)(Value >> 1);

            if (IsFlagSet(Flags.Carry))
                Value = (byte)(Value | 0x80);

            SetClearFlagConditional(Flags.Carry, TempBit);

            SetClearFlagConditional(Flags.Zero, (Value == 0x00));
            SetClearFlagConditional(Flags.Sign, ((Value & 0x80) == 0x80));

            WriteValue(Mode, (byte)Value);
        }

        private void OpSBC(AddressingModes Mode)
        {
            byte Data = GetOperand(Mode);
            uint w;

            SetClearFlagConditional(Flags.Overflow, ((A ^ Data) & 0x80) != 0);

            w = (uint)(0xff + A - Data + (IsFlagSet(Flags.Carry) ? 1 : 0));
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

            A = (byte)(w & 0xFF);

            SetClearFlagConditional(Flags.Zero, (A == 0x00));
            SetClearFlagConditional(Flags.Sign, ((A & 0x80) == 0x80));
        }

        private void OpSTA(AddressingModes Mode)
        {
            WriteValue(Mode, A);
        }

        private void OpSTX(AddressingModes Mode)
        {
            WriteValue(Mode, X);
        }

        private void OpSTY(AddressingModes Mode)
        {
            WriteValue(Mode, Y);
        }
    }
}
