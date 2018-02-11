using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.Enumerations;

namespace MasterFudgeMk2.Devices.NEC
{
    // HuC6280 (CPU)

    // TODO: might be busted after messing w/ 6502

    public class HuC6280 : MOS6502
    {
        public delegate byte PagingRegisterReadDelegate(byte register);
        public delegate void PagingRegisterWriteDelegate(byte register, byte value);
        public delegate void ChangeClockSpeedDelegate(bool fast);

        PagingRegisterReadDelegate pagingRegisterReadDelegate;
        PagingRegisterWriteDelegate pagingRegisterWriteDelegate;
        ChangeClockSpeedDelegate changeClockSpeedDelegate;

        protected enum InterruptVectors : ushort
        {
            IRQ2 = 0xFFF6,
            IRQ1 = 0xFFF8,
            Timer = 0xFFFA,
            NMI = 0xFFFC,
            Reset = 0xFFFE
        }

        protected enum InterruptMasks : byte
        {
            IRQ2 = 0x01,
            IRQ1 = 0x02,
            Timer = 0x04
        }

        private static readonly int[] cycleCounts6280 = new int[]
        {
            /* 0x00 */  7,  6,  3,  4,  6,  3,  5,  7,  3,  2,  2,  2,  7,  4,  6,  6,
            /* 0x10 */  2,  5,  7,  4,  6,  4,  6,  7,  2,  4,  2,  7,  7,  4,  7,  6,
            /* 0x20 */  6,  6,  3,  4,  3,  3,  5,  7,  4,  2,  2,  2,  4,  4,  6,  6,
            /* 0x30 */  2,  5,  7,  8,  4,  4,  6,  7,  2,  4,  2,  7,  5,  4,  7,  6,
            /* 0x40 */  6,  6,  3,  4,  8,  3,  5,  7,  3,  2,  2,  2,  3,  4,  6,  6,
            /* 0x50 */  2,  5,  7,  5,  3,  4,  6,  7,  2,  4,  3,  7,  4,  4,  7,  6,
            /* 0x60 */  6,  6,  2,  8,  3,  3,  5,  7,  4,  2,  2,  2,  5,  4,  6,  6,
            /* 0x70 */  2,  5,  7, 17,  4,  4,  6,  7,  2,  4,  4,  7,  7,  4,  7,  6,
            /* 0x80 */  4,  6,  2,  7,  3,  3,  3,  7,  2,  2,  2,  2,  4,  4,  4,  6,
            /* 0x90 */  2,  6,  7,  8,  4,  4,  4,  7,  2,  5,  2,  5,  4,  5,  4,  6,
            /* 0xA0 */  2,  6,  2,  7,  3,  3,  3,  7,  2,  2,  2,  2,  4,  4,  4,  6,
            /* 0xB0 */  2,  5,  7,  8,  4,  4,  4,  7,  2,  4,  2,  4,  4,  4,  4,  6,
            /* 0xC0 */  2,  6,  2, 17,  3,  3,  5,  7,  2,  2,  2,  2,  4,  4,  6,  6,
            /* 0xD0 */  2,  5,  7, 17,  3,  4,  6,  7,  2,  4,  3,  7,  4,  4,  7,  6,
            /* 0xE0 */  2,  6,  3, 17,  3,  3,  5,  7,  2,  2,  2,  2,  4,  4,  6,  6,
            /* 0xF0 */  2,  5,  7, 17,  2,  4,  6,  7,  2,  4,  4,  7,  4,  4,  7,  6
        };

        public override int[] CycleCounts { get { return cycleCounts6280; } }

        byte IRQMask;
        bool timerEnabled, timerAck;
        int timerValue, timerReload, timerCycles;

        InterruptState int1State, int2State, timerIntState;

        public HuC6280(double clockRate, double refreshRate,
            MemoryReadDelegate memoryRead, MemoryWriteDelegate memoryWrite,
            PagingRegisterReadDelegate pagingRegisterRead, PagingRegisterWriteDelegate pagingRegisterWrite,
            ChangeClockSpeedDelegate changeClockSpeed)
            : base(clockRate, refreshRate, memoryRead, memoryWrite)
        {
            pagingRegisterReadDelegate = pagingRegisterRead;
            pagingRegisterWriteDelegate = pagingRegisterWrite;
            changeClockSpeedDelegate = changeClockSpeed;
        }

        public override void Reset()
        {
            base.Reset();

            pc = ReadMemory16((ushort)InterruptVectors.Reset);
            p = (Flags.InterruptDisable | Flags.Zero);

            IRQMask = 0;

            timerEnabled = false;
            timerAck = true;
            timerValue = timerReload = 0;
            timerCycles = 1024;

            int1State = InterruptState.Clear;
            int2State = InterruptState.Clear;
            timerIntState = InterruptState.Clear;
        }

        //

        public override int Step()
        {
            currentCycles = 0;

            /* Check interrupts */
            if ((int1State == InterruptState.Assert) && ((IRQMask & (byte)InterruptMasks.IRQ1) == 0))
            {
                int1State = InterruptState.Clear;   //clear here?
                ServiceInterrupt();
            }

            if ((int2State == InterruptState.Assert) && ((IRQMask & (byte)InterruptMasks.IRQ2) == 0))
            {
                int2State = InterruptState.Clear;   //clear here?
                ServiceInterrupt();
            }

            if ((timerIntState == InterruptState.Assert) && ((IRQMask & (byte)InterruptMasks.Timer) == 0))
            {
                timerIntState = InterruptState.Clear;
                ServiceInterrupt();
            }

            /* Fetch and execute opcode */
            op = ReadMemory8(pc++);
            switch (op)
            {
                case 0x00: OpBRK(); break;
                case 0x01: OpORA(AddressingModes.IndirectX); break;
                case 0x02: OpSXY(); break;
                case 0x03: OpSTi(0); break;
                case 0x04: OpTSB(AddressingModes.ZeroPage); break;
                case 0x05: OpORA(AddressingModes.ZeroPage); break;
                case 0x06: OpASL(AddressingModes.ZeroPage); break;
                case 0x07: OpRMBi(0); break;
                case 0x08: PushP(); pc++; break;
                case 0x09: OpORA(AddressingModes.Immediate); break;
                case 0x0A: OpASL(AddressingModes.Accumulator); break;
                case 0x0C: OpTSB(AddressingModes.Absolute); break;
                case 0x0D: OpORA(AddressingModes.Absolute); break;
                case 0x0E: OpASL(AddressingModes.Absolute); break;
                case 0x0F: OpBBRi(0); break;

                case 0x10: OpBPL(); break;
                case 0x11: OpORA(AddressingModes.IndirectY); break;
                case 0x12: OpORA(AddressingModes.Indirect); break;
                case 0x13: OpSTi(2); break;
                case 0x14: OpTRB(AddressingModes.ZeroPage); break;
                case 0x15: OpORA(AddressingModes.ZeroPageX); break;
                case 0x16: OpASL(AddressingModes.ZeroPageX); break;
                case 0x17: OpRMBi(1); break;
                case 0x18: ClearFlag(Flags.Carry); pc++; break;
                case 0x19: OpORA(AddressingModes.AbsoluteY); break;
                case 0x1A: OpINA(); break;
                case 0x1C: OpTRB(AddressingModes.Absolute); break;
                case 0x1D: OpORA(AddressingModes.AbsoluteX); break;
                case 0x1E: OpASL(AddressingModes.AbsoluteX); break;
                case 0x1F: OpBBRi(1); break;

                case 0x20: OpJSR(); break;
                case 0x21: OpAND(AddressingModes.IndirectX); break;
                case 0x22: OpSAX(); break;
                case 0x23: OpSTi(3); break;
                case 0x24: OpBIT(AddressingModes.ZeroPage); break;
                case 0x25: OpAND(AddressingModes.ZeroPage); break;
                case 0x26: OpROL(AddressingModes.ZeroPage); break;
                case 0x27: OpRMBi(2); break;
                case 0x28: PullP(); pc++; break;    //check ints?
                case 0x29: OpAND(AddressingModes.Immediate); break;
                case 0x2A: OpROL(AddressingModes.Accumulator); break;
                case 0x2C: OpBIT(AddressingModes.Absolute); break;
                case 0x2D: OpAND(AddressingModes.Absolute); break;
                case 0x2E: OpROL(AddressingModes.Absolute); break;
                case 0x2F: OpBBRi(2); break;

                case 0x30: OpBMI(); break;
                case 0x31: OpAND(AddressingModes.IndirectY); break;
                case 0x32: OpAND(AddressingModes.Indirect); break;
                case 0x34: OpBIT(AddressingModes.ZeroPageX); break;
                case 0x35: OpAND(AddressingModes.ZeroPageX); break;
                case 0x36: OpROL(AddressingModes.ZeroPageX); break;
                case 0x37: OpRMBi(3); break;
                case 0x38: SetFlag(Flags.Carry); pc++; break;
                case 0x39: OpAND(AddressingModes.AbsoluteY); break;
                case 0x3A: OpDEA(); break;
                case 0x3C: OpBIT(AddressingModes.AbsoluteX); break;
                case 0x3D: OpAND(AddressingModes.AbsoluteX); break;
                case 0x3E: OpROL(AddressingModes.AbsoluteX); break;
                case 0x3F: OpBBRi(3); break;

                case 0x40: PullP(); pc = Pull16(); break;   //check ints?
                case 0x41: OpEOR(AddressingModes.IndirectX); break;
                case 0x42: OpSAY(); break;
                case 0x43: OpTMAi(); break;
                case 0x44: OpBSR(); break;
                case 0x45: OpEOR(AddressingModes.ZeroPage); break;
                case 0x46: OpLSR(AddressingModes.ZeroPage); break;
                case 0x47: OpRMBi(4); break;
                case 0x48: Push(a); pc++; break;
                case 0x49: OpEOR(AddressingModes.Immediate); break;
                case 0x4A: OpLSR(AddressingModes.Accumulator); break;
                case 0x4C: OpJMP(AddressingModes.Absolute); break;
                case 0x4D: OpEOR(AddressingModes.Absolute); break;
                case 0x4E: OpLSR(AddressingModes.Absolute); break;
                case 0x4F: OpBBRi(4); break;

                case 0x50: OpBVC(); break;
                case 0x51: OpEOR(AddressingModes.IndirectY); break;
                case 0x52: OpEOR(AddressingModes.Indirect); break;
                case 0x53: OpTAM(); break;
                case 0x54: changeClockSpeedDelegate(false); pc++; break;
                case 0x55: OpEOR(AddressingModes.ZeroPageX); break;
                case 0x56: OpLSR(AddressingModes.ZeroPageX); break;
                case 0x57: OpRMBi(5); break;
                case 0x58: ClearFlag(Flags.InterruptDisable); pc++; break;  //check ints *first*?
                case 0x59: OpEOR(AddressingModes.AbsoluteY); break;
                case 0x5A: Push(y); pc++; break;
                case 0x5D: OpEOR(AddressingModes.AbsoluteX); break;
                case 0x5E: OpLSR(AddressingModes.AbsoluteX); break;
                case 0x5F: OpBBRi(5); break;

                case 0x60: pc = Pull16(); pc++; break;
                case 0x61: OpADC(AddressingModes.IndirectX); break;
                case 0x62: a = 0; pc++; break;
                case 0x64: OpSTZ(AddressingModes.ZeroPage); break;
                case 0x65: OpADC(AddressingModes.ZeroPage); break;
                case 0x66: OpROR(AddressingModes.ZeroPage); break;
                case 0x67: OpRMBi(6); break;
                case 0x68: a = Pull(); SetClearFlagConditional(Flags.Zero, (a == 0x00)); SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80)); pc++; break;
                case 0x69: OpADC(AddressingModes.Immediate); break;
                case 0x6A: OpROR(AddressingModes.Accumulator); break;
                case 0x6C: OpJMP(AddressingModes.Indirect); break;
                case 0x6D: OpADC(AddressingModes.Absolute); break;
                case 0x6E: OpROR(AddressingModes.Absolute); break;
                case 0x6F: OpBBRi(6); break;

                case 0x70: OpBVS(); break;
                case 0x71: OpADC(AddressingModes.IndirectY); break;
                case 0x72: OpADC(AddressingModes.Indirect); break;
                case 0x73: OpTII(); break;
                case 0x74: OpSTZ(AddressingModes.ZeroPageX); break;
                case 0x75: OpADC(AddressingModes.ZeroPageX); break;
                case 0x76: OpROR(AddressingModes.ZeroPageX); break;
                case 0x77: OpRMBi(7); break;
                case 0x78: SetFlag(Flags.InterruptDisable); pc++; break;
                case 0x79: OpADC(AddressingModes.AbsoluteY); break;
                case 0x7A: y = Pull(); SetClearFlagConditional(Flags.Zero, (y == 0x00)); SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80)); pc++; break;
                case 0x7C: OpJMP(AddressingModes.IndirectX); break;
                case 0x7D: OpADC(AddressingModes.AbsoluteX); break;
                case 0x7E: OpROR(AddressingModes.AbsoluteX); break;
                case 0x7F: OpBBRi(7); break;

                case 0x80: OpBRA(); break;
                case 0x81: OpSTA(AddressingModes.IndirectX); break;
                case 0x82: y = 0; pc++; break;
                case 0x83: OpTST(AddressingModes.ZeroPage); break;
                case 0x84: OpSTY(AddressingModes.ZeroPage); break;
                case 0x85: OpSTA(AddressingModes.ZeroPage); break;
                case 0x86: OpSTX(AddressingModes.ZeroPage); break;
                case 0x87: OpSMBi(0); break;
                case 0x88: OpDEY(); break;
                case 0x89: OpBIT(AddressingModes.Immediate); break;
                case 0x8A: a = x; SetClearFlagConditional(Flags.Zero, (a == 0x00)); SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80)); pc++; break;
                case 0x8C: OpSTY(AddressingModes.Absolute); break;
                case 0x8D: OpSTA(AddressingModes.Absolute); break;
                case 0x8E: OpSTX(AddressingModes.Absolute); break;
                case 0x8F: OpBBSi(0); break;

                case 0x90: OpBCC(); break;
                case 0x91: OpSTA(AddressingModes.IndirectY); break;
                case 0x92: OpSTA(AddressingModes.Indirect); break;
                case 0x93: OpTST(AddressingModes.Absolute); break;
                case 0x94: OpSTY(AddressingModes.ZeroPageX); break;
                case 0x95: OpSTA(AddressingModes.ZeroPageX); break;
                case 0x96: OpSTX(AddressingModes.ZeroPageY); break;
                case 0x97: OpSMBi(1); break;
                case 0x98: a = y; SetClearFlagConditional(Flags.Zero, (a == 0x00)); SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80)); pc++; break;
                case 0x99: OpSTA(AddressingModes.AbsoluteY); break;
                case 0x9A: sp = x; pc++; break;
                case 0x9C: OpSTZ(AddressingModes.Absolute); break;
                case 0x9D: OpSTA(AddressingModes.AbsoluteX); break;
                case 0x9E: OpSTZ(AddressingModes.AbsoluteX); break;
                case 0x9F: OpBBSi(1); break;

                case 0xA0: OpLDY(AddressingModes.Immediate); break;
                case 0xA1: OpLDA(AddressingModes.IndirectX); break;
                case 0xA2: OpLDX(AddressingModes.Immediate); break;
                case 0xA3: OpTST(AddressingModes.ZeroPageX); break;
                case 0xA4: OpLDY(AddressingModes.ZeroPage); break;
                case 0xA5: OpLDA(AddressingModes.ZeroPage); break;
                case 0xA6: OpLDX(AddressingModes.ZeroPage); break;
                case 0xA7: OpSMBi(2); break;
                case 0xA8: y = a; SetClearFlagConditional(Flags.Zero, (y == 0x00)); SetClearFlagConditional(Flags.Sign, ((y & 0x80) == 0x80)); pc++; break;
                case 0xA9: OpLDA(AddressingModes.Immediate); break;
                case 0xAA: x = a; SetClearFlagConditional(Flags.Zero, (x == 0x00)); SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80)); pc++; break;
                case 0xAC: OpLDY(AddressingModes.Absolute); break;
                case 0xAD: OpLDA(AddressingModes.Absolute); break;
                case 0xAE: OpLDX(AddressingModes.Absolute); break;
                case 0xAF: OpBBSi(2); break;

                case 0xB0: OpBCS(); break;
                case 0xB1: OpLDA(AddressingModes.IndirectY); break;
                case 0xB2: OpLDA(AddressingModes.Indirect); break;
                case 0xB3: OpTST(AddressingModes.AbsoluteX); break;
                case 0xB4: OpLDY(AddressingModes.ZeroPageX); break;
                case 0xB5: OpLDA(AddressingModes.ZeroPageX); break;
                case 0xB6: OpLDX(AddressingModes.ZeroPageY); break;
                case 0xB7: OpSMBi(3); break;
                case 0xB8: ClearFlag(Flags.Overflow); pc++; break;
                case 0xB9: OpLDA(AddressingModes.AbsoluteY); break;
                case 0xBA: x = sp; SetClearFlagConditional(Flags.Zero, (x == 0x00)); SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80)); pc++; break;
                case 0xBC: OpLDY(AddressingModes.AbsoluteX); break;
                case 0xBD: OpLDA(AddressingModes.AbsoluteX); break;
                case 0xBE: OpLDX(AddressingModes.AbsoluteY); break;
                case 0xBF: OpBBSi(3); break;

                case 0xC0: OpCPY(AddressingModes.Immediate); break;
                case 0xC1: OpCMP(AddressingModes.IndirectX); break;
                case 0xC2: y = 0; pc++; break;
                case 0xC3: OpTDD(); break;
                case 0xC4: OpCPY(AddressingModes.ZeroPage); break;
                case 0xC5: OpCMP(AddressingModes.ZeroPage); break;
                case 0xC6: OpDEC(AddressingModes.ZeroPage); break;
                case 0xC7: OpSMBi(4); break;
                case 0xC8: OpINY(); break;
                case 0xC9: OpCMP(AddressingModes.Immediate); break;
                case 0xCA: OpDEX(); break;
                case 0xCC: OpCPY(AddressingModes.Absolute); break;
                case 0xCD: OpCMP(AddressingModes.Absolute); break;
                case 0xCE: OpDEC(AddressingModes.Absolute); break;
                case 0xCF: OpBBSi(4); break;

                case 0xD0: OpBNE(); break;
                case 0xD1: OpCMP(AddressingModes.IndirectY); break;
                case 0xD2: OpCMP(AddressingModes.Indirect); break;
                case 0xD3: OpTIN(); break;
                case 0xD4: changeClockSpeedDelegate(true); pc++; break;
                case 0xD5: OpCMP(AddressingModes.ZeroPageX); break;
                case 0xD6: OpDEC(AddressingModes.ZeroPageX); break;
                case 0xD7: OpSMBi(5); break;
                case 0xD8: ClearFlag(Flags.DecimalMode); pc++; break;
                case 0xD9: OpCMP(AddressingModes.AbsoluteY); break;
                case 0xDA: Push(x); pc++; break;
                case 0xDD: OpCMP(AddressingModes.AbsoluteX); break;
                case 0xDE: OpDEC(AddressingModes.AbsoluteX); break;
                case 0xDF: OpBBSi(5); break;

                case 0xE0: OpCPX(AddressingModes.Immediate); break;
                case 0xE1: OpSBC(AddressingModes.IndirectX); break;
                case 0xE3: OpTIA(); break;
                case 0xE4: OpCPX(AddressingModes.ZeroPage); break;
                case 0xE5: OpSBC(AddressingModes.ZeroPage); break;
                case 0xE6: OpINC(AddressingModes.ZeroPage); break;
                case 0xE7: OpSMBi(6); break;
                case 0xE8: OpINX(); break;
                case 0xE9: OpSBC(AddressingModes.Immediate); break;
                case 0xEA: OpNOP(); break;
                case 0xEC: OpCPX(AddressingModes.Absolute); break;
                case 0xED: OpSBC(AddressingModes.Absolute); break;
                case 0xEE: OpINC(AddressingModes.Absolute); break;
                case 0xEF: OpBBSi(6); break;

                case 0xF0: OpBEQ(); break;
                case 0xF1: OpSBC(AddressingModes.IndirectY); break;
                case 0xF2: OpSBC(AddressingModes.Indirect); break;
                case 0xF3: OpTAI(); break;
                case 0xF4: SetFlag(Flags.UnusedBit5); pc++; break;
                case 0xF5: OpSBC(AddressingModes.ZeroPageX); break;
                case 0xF6: OpINC(AddressingModes.ZeroPageX); break;
                case 0xF7: OpSMBi(7); break;
                case 0xF8: SetFlag(Flags.DecimalMode); pc++; break;
                case 0xF9: OpSBC(AddressingModes.AbsoluteY); break;
                case 0xFA: x = Pull(); SetClearFlagConditional(Flags.Zero, (x == 0x00)); SetClearFlagConditional(Flags.Sign, ((x & 0x80) == 0x80)); break;
                case 0xFD: OpSBC(AddressingModes.AbsoluteX); break;
                case 0xFE: OpINC(AddressingModes.AbsoluteX); break;
                case 0xFF: OpBBSi(7); break;
            }

            currentCycles += CycleCounts[op];

            if (timerEnabled)
            {
                timerCycles -= CycleCounts[op];
                if (timerCycles < 0)
                {
                    timerValue--;
                    if (timerValue < 0 && timerAck)
                    {
                        timerAck = timerEnabled = false;
                        SetTimerInterruptLine(InterruptState.Assert);
                    }
                    timerCycles = 1024;
                }
            }

            return currentCycles;
        }

        public void SetInterrupt1Line(InterruptState state)
        {
            int1State = state;
        }

        public void SetInterrupt2Line(InterruptState state)
        {
            int2State = state;
        }

        public void SetTimerInterruptLine(InterruptState state)
        {
            timerIntState = state;
        }

        protected void ServiceInterrupt(InterruptVectors vector)
        {
            if (!IsFlagSet(Flags.InterruptDisable))
            {
                ClearFlag(Flags.Brk);
                Push16(pc);
                PushP();
                SetFlag(Flags.InterruptDisable);
                SetFlag(Flags.DecimalMode);
                pc = ReadMemory16((uint)vector);

                currentCycles += 7;
            }
        }

        protected override byte ReadZeroPage(byte address)
        {
            return ReadMemory8(((uint)address + 0x2000) & 0xFFFF);
        }

        protected override byte ReadZeroPageX(byte address)
        {
            return ReadMemory8(((uint)address + 0x2000 + x) & 0xFFFF);
        }

        protected override byte ReadZeroPageY(byte address)
        {
            return ReadMemory8(((uint)address + 0x2000 + y) & 0xFFFF);
        }

        protected byte ReadIndirect(byte address)
        {
            return ReadMemory8(ReadMemory16((uint)((address + 0x2000) & 0xFF)));
        }

        protected override byte ReadIndirectX(byte address)
        {
            return ReadMemory8(ReadMemory16((uint)((address + 0x2000 + x) & 0xFF)));
        }

        protected override byte ReadIndirectY(byte address)
        {
            if ((ReadMemory16(address) & 0xFF00) != ((ReadMemory16((uint)address + 0x2000) + y) & 0xFF00))
                currentCycles += 1;

            return ReadMemory8((uint)(ReadMemory16((uint)address + 0x2000) + y));
        }

        protected override void WriteZeroPage(uint address, byte Value)
        {
            WriteMemory8(address + 0x2000, Value);
        }

        protected override void WriteZeroPageX(uint address, byte value)
        {
            WriteMemory8(((address + 0x2000 + x) & 0xFF), value);
        }

        protected override void WriteZeroPageY(uint address, byte value)
        {
            WriteMemory8(((address + 0x2000 + y) & 0xFF), value);
        }

        protected void WriteIndirect(byte address, byte value)
        {
            WriteMemory8(ReadMemory16((uint)((address + 0x2000) & 0xFF)), value);
        }

        protected override void WriteIndirectX(byte address, byte value)
        {
            WriteMemory8(ReadMemory16((uint)((address + 0x2000 + x) & 0xFF)), value);
        }

        protected override void WriteIndirectY(byte address, byte value)
        {
            WriteMemory8((uint)(ReadMemory16(address) + 0x2000 + y), value);
        }

        protected override byte GetOperand(AddressingModes mode, int offset, bool incrementPc)
        {
            byte arg1 = ReadMemory8((ushort)(pc + 1 + offset));
            byte arg2 = ReadMemory8((ushort)(pc + 2 + offset));
            byte value = 0xFF;

            switch (mode)
            {
                case AddressingModes.Implied: break;
                case AddressingModes.Accumulator: value = a; break;
                case AddressingModes.Immediate: value = arg1; if (incrementPc) pc++; break;
                case AddressingModes.ZeroPage: value = ReadZeroPage(arg1); if (incrementPc) pc++; break;
                case AddressingModes.ZeroPageX: value = ReadZeroPageX(arg1); if (incrementPc) pc++; break;
                case AddressingModes.ZeroPageY: value = ReadZeroPageY(arg1); if (incrementPc) pc++; break;
                case AddressingModes.Absolute: value = ReadAbsolute(arg1, arg2); if (incrementPc) pc += 2; break;
                case AddressingModes.AbsoluteX: value = ReadAbsoluteX(arg1, arg2); if (incrementPc) pc += 2; break;
                case AddressingModes.AbsoluteY: value = ReadAbsoluteY(arg1, arg2); if (incrementPc) pc += 2; break;
                case AddressingModes.IndirectX: value = ReadIndirectX(arg1); if (incrementPc) pc++; break;
                case AddressingModes.IndirectY: value = ReadIndirectY(arg1); if (incrementPc) pc++; break;
                default: throw new Exception("6502 addressing mode error on read");
            }

            return value;
        }

        protected override void WriteValue(AddressingModes mode, byte value, bool incrementPc)
        {
            byte arg1 = ReadMemory8((ushort)(pc + 1));
            byte arg2 = ReadMemory8((ushort)(pc + 2));

            switch (mode)
            {
                case AddressingModes.Implied: break;
                case AddressingModes.Accumulator: a = value; break;
                case AddressingModes.ZeroPage: WriteZeroPage(arg1, value); if (incrementPc) pc++; break;
                case AddressingModes.ZeroPageX: WriteZeroPageX(arg1, value); if (incrementPc) pc++; break;
                case AddressingModes.ZeroPageY: WriteZeroPageY(arg1, value); if (incrementPc) pc++; break;
                case AddressingModes.Absolute: WriteAbsolute(arg1, arg2, value); if (incrementPc) pc += 2; break;
                case AddressingModes.AbsoluteX: WriteAbsoluteX(arg1, arg2, value); if (incrementPc) pc += 2; break;
                case AddressingModes.AbsoluteY: WriteAbsoluteY(arg1, arg2, value); if (incrementPc) pc += 2; break;
                case AddressingModes.Indirect: WriteIndirect(arg1, value); if (incrementPc) pc++; break;
                case AddressingModes.IndirectX: WriteIndirectX(arg1, value); if (incrementPc) pc++; break;
                case AddressingModes.IndirectY: WriteIndirectY(arg1, value); if (incrementPc) pc++; break;
                default: throw new Exception("6502 addressing mode error on write");
            }
        }

        private void OpSXY()
        {
            byte tmp = x;
            x = y;
            y = tmp;
        }

        private void OpSTi(int port)
        {
            // vdc port write!
            //Machine.VDC.WriteIO((uint)port, ReadMemory8((uint)(pc + 1)));
        }

        private void OpTSB(AddressingModes mode)
        {
            byte value = GetOperand(mode, false);

            SetClearFlagConditional(Flags.Zero, (a & value) == 0x00);
            SetClearFlagConditional(Flags.Sign, (value & 0x80) == 0x80);
            SetClearFlagConditional(Flags.Overflow, (value & 0x40) == 0x40);

            value |= a;
            WriteValue(mode, value, true);
        }

        private void OpRMBi(int bit)
        {
            byte value = GetOperand(AddressingModes.ZeroPage, false);
            value &= (byte)(~(1 << bit));
            WriteValue(AddressingModes.ZeroPage, value, true);
        }

        private void OpBBRi(int Bit)
        {
            byte value = GetOperand(AddressingModes.ZeroPage, true);
            sbyte branch = (sbyte)ReadMemory8((uint)(pc + 1));

            if ((value & (1 << Bit)) == 0)
            {
                if ((pc & 0xFF00) != ((pc + branch + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + branch);
            }
        }

        private void OpTRB(AddressingModes mode)
        {
            byte value = GetOperand(mode, false);

            SetClearFlagConditional(Flags.Zero, (a & value) == 0x00);
            SetClearFlagConditional(Flags.Sign, (value & 0x80) == 0x80);
            SetClearFlagConditional(Flags.Overflow, (value & 0x40) == 0x40);

            value &= (byte)~a;
            WriteValue(mode, value, true);
        }

        private void OpINA()
        {
            a++;

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, (a & 0x80) == 0x80);
        }

        private void OpSAX()
        {
            byte tmp = x;
            x = a;
            a = tmp;
        }

        private void OpDEA()
        {
            a--;

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, (a & 0x80) == 0x80);
        }

        private void OpSAY()
        {
            byte tmp = y;
            y = a;
            a = tmp;
        }

        private void OpTMAi()
        {
            byte data = GetOperand(AddressingModes.Immediate, true);
            for (byte register = 0; register < 8; register++)
            {
                if ((data & (1 << register)) != 0)
                    a = pagingRegisterReadDelegate(register);
            }
        }

        private void OpBSR()
        {
            sbyte branch = (sbyte)GetOperand(AddressingModes.Immediate, true);

            Push16((ushort)(pc - 1));

            if ((pc & 0xFF00) != ((pc + branch + 1) & 0xFF00))
                currentCycles += 1;

            pc = (ushort)(pc + branch);
        }

        private void OpTAM()
        {
            byte data = GetOperand(AddressingModes.Immediate, true);
            for (byte register = 0; register < 8; register++)
            {
                if ((data & (1 << register)) != 0)
                    pagingRegisterWriteDelegate(register, a);
            }
        }

        private void OpSTZ(AddressingModes mode)
        {
            WriteValue(mode, 0x00, true);
        }

        private void OpTII()
        {
            Push(y); Push(a); Push(x);

            ushort src = ReadMemory16((uint)(pc + 1));
            ushort dst = ReadMemory16((uint)(pc + 3));
            ushort len = ReadMemory16((uint)(pc + 5));

            currentCycles += (len * 6);

            while (len-- != 0)
                WriteMemory8(dst++, ReadMemory8(src++));

            x = Pull(); a = Pull(); y = Pull();
        }

        private void OpBRA()
        {
            sbyte branch = (sbyte)GetOperand(AddressingModes.Immediate, true);

            if ((pc & 0xFF00) != ((pc + branch + 2) & 0xFF00))
                currentCycles += 1;

            pc = (ushort)(pc + branch);
        }

        private void OpTST(AddressingModes mode)
        {
            byte arg1 = GetOperand(AddressingModes.Immediate, true);
            byte arg2 = GetOperand(mode, 1, true);

            SetClearFlagConditional(Flags.Zero, (arg2 & arg1) == 0x00);
            SetClearFlagConditional(Flags.Sign, (arg2 & 0x80) == 0x80);
            SetClearFlagConditional(Flags.Overflow, (arg2 & 0x40) == 0x40);
        }

        private void OpSMBi(int bit)
        {
            byte value = GetOperand(AddressingModes.ZeroPage, true);
            value |= (byte)(1 << bit);
            WriteValue(AddressingModes.ZeroPage, value, false);
        }

        private void OpBBSi(int bit)
        {
            byte value = GetOperand(AddressingModes.ZeroPage, true);
            sbyte branch = (sbyte)ReadMemory8((uint)(pc + 2));

            if ((value & (1 << bit)) != 0)
            {
                if ((pc & 0xFF00) != ((pc + branch + 2) & 0xFF00))
                    currentCycles += 1;

                pc = (ushort)(pc + branch);
            }
        }

        private void OpTDD()
        {
            Push(y); Push(a); Push(x);

            ushort src = ReadMemory16((uint)(pc + 1));
            ushort dst = ReadMemory16((uint)(pc + 3));
            ushort len = ReadMemory16((uint)(pc + 5));

            currentCycles += (len * 6);

            while (len-- != 0)
                WriteMemory8(dst--, ReadMemory8(src--));

            x = Pull(); a = Pull(); y = Pull();
        }

        private void OpTIN()
        {
            Push(y); Push(a); Push(x);

            ushort src = ReadMemory16((uint)(pc + 1));
            ushort dst = ReadMemory16((uint)(pc + 3));
            ushort len = ReadMemory16((uint)(pc + 5));

            currentCycles += (len * 6);

            while (len-- != 0)
                WriteMemory8(dst, ReadMemory8(src++));

            x = Pull(); a = Pull(); y = Pull();
        }

        private void OpTIA()
        {
            Push(y); Push(a); Push(x);

            ushort src = ReadMemory16((uint)(pc + 1));
            ushort dst = ReadMemory16((uint)(pc + 3));
            ushort len = ReadMemory16((uint)(pc + 5));

            currentCycles += (len * 6);

            uint alt = 0;
            while (len-- != 0)
            {
                WriteMemory8((dst + alt), ReadMemory8(src++));
                alt ^= 1;
            }

            x = Pull(); a = Pull(); y = Pull();
        }

        private void OpTAI()
        {
            Push(y); Push(a); Push(x);

            ushort src = ReadMemory16((uint)(pc + 1));
            ushort dst = ReadMemory16((uint)(pc + 3));
            ushort len = ReadMemory16((uint)(pc + 5));

            currentCycles += (len * 6);

            uint alt = 0;
            while (len-- != 0)
            {
                WriteMemory8(dst++, ReadMemory8(src + alt));
                alt ^= 1;
            }

            x = Pull(); a = Pull(); y = Pull();
        }
    }
}
