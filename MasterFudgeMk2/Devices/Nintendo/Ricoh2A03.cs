using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Devices.Nintendo
{
    // 6502 w/o decimal mode (for ADC/SBC)

    public partial class Ricoh2A03 : MOS6502
    {
        public Ricoh2A03(double clockRate, double refreshRate, MemoryReadDelegate memoryRead, MemoryWriteDelegate memoryWrite) : base(clockRate, refreshRate, memoryRead, memoryWrite) { }

        protected override void OpADC(AddressingModes mode)
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

            IncrementPC();
            IncrementCycles();
        }

        protected override void OpSBC(AddressingModes mode)
        {
            byte data = GetOperand(mode);
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

            IncrementPC();
            IncrementCycles();
        }
    }
}
