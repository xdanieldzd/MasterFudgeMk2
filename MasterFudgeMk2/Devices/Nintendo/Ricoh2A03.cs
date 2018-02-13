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
            uint sum = (uint)(a + data + (IsFlagSet(Flags.Carry) ? 1 : 0));

            SetClearFlagConditional(Flags.Overflow, ((a ^ sum) & (data ^ sum) & 0x80) != 0);
            SetClearFlagConditional(Flags.Carry, sum >= 0x100);

            a = (byte)(sum & 0xFF);

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }

        protected override void OpSBC(AddressingModes mode)
        {
            byte data = (byte)(GetOperand(mode) ^ 0xFF);
            uint sum = (uint)(a + data + (IsFlagSet(Flags.Carry) ? 1 : 0));

            SetClearFlagConditional(Flags.Overflow, ((a ^ sum) & (data ^ sum) & 0x80) != 0);
            SetClearFlagConditional(Flags.Carry, sum >= 0x100);

            a = (byte)(sum & 0xFF);

            SetClearFlagConditional(Flags.Zero, (a == 0x00));
            SetClearFlagConditional(Flags.Sign, ((a & 0x80) == 0x80));

            IncrementPC();
            IncrementCycles();
        }
    }
}
