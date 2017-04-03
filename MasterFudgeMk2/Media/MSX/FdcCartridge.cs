using MasterFudgeMk2.Common;
using MasterFudgeMk2.Devices;

namespace MasterFudgeMk2.Media.MSX
{
    /* Common MMIO-based disk controller cartridge */
    public class FdcCartridge : RawRomCartridge
    {
        FD179x fdc;

        /* https://sourceforge.net/p/openmsx/mailman/message/7377547/ */

        public FdcCartridge() : base()
        {
            fdc = new FD179x();
        }

        public override void Reset()
        {
            fdc.Reset();
        }

        public override void Step()
        {
            fdc.Step();
        }

        public override byte Read(ushort address)
        {
            switch (address)
            {
                case 0x7FF8: return fdc.StatusRegister;             /* Status register */
                case 0x7FF9: return fdc.TrackRegister;              /* Track register */
                case 0x7FFA: return fdc.SectorRegister;             /* Sector register */
                case 0x7FFB: return fdc.DataRegister;               /* Data register */

                case 0x7FFF:
                    /* Interrupt/data request flags */
                    return (byte)((fdc.DataRequest ? (1 << 7) : 0) | (fdc.InterruptRequest ? (1 << 6) : 0));

                default: return base.Read(address);
            }
        }

        public override void Write(ushort address, byte value)
        {
            switch (address)
            {
                case 0x7FF8: fdc.CommandRegister = value; break;    /* Command register */
                case 0x7FF9: fdc.TrackRegister = value; break;      /* Track register */
                case 0x7FFA: fdc.SectorRegister = value; break;     /* Sector register */
                case 0x7FFB: fdc.DataRegister = value; break;       /* Data register */

                case 0x7FFC:
                    /* Side number */
                    fdc.SideNumber = (byte)(value & 0x01);
                    break;

                case 0x7FFD:
                    /* Drive number (00 = A, 01 = B, 02 = A, 03 = nothing?) & motor on */
                    fdc.DriveNumber = (byte)(value & 0x03);
                    fdc.MotorOn = BitUtilities.IsBitSet(value, 7);
                    break;

                default: base.Write(address, value); break;
            }
        }
    }
}
