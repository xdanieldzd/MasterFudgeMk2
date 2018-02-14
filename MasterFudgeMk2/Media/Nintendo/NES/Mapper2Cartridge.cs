using System;

namespace MasterFudgeMk2.Media.Nintendo.NES
{
    public class Mapper2Cartridge : BaseCartridge
    {
        byte bankSelect;

        public override void Reset()
        {
            bankSelect = 0;
        }

        public override byte Read(uint address)
        {
            switch (address & 0xF000)
            {
                case 0x4000:
                case 0x5000:
                case 0x6000:
                case 0x7000:
                    return 0x00;

                case 0x8000:
                case 0x9000:
                case 0xA000:
                case 0xB000:
                    return prgData[bankSelect][address & prgDataMask];

                case 0xC000:
                case 0xD000:
                case 0xE000:
                case 0xF000:
                    return prgData[prgData.Length - 1][address & prgDataMask];

                default:
                    throw new Exception($"iNES Mapper 2: invalid read from 0x{address:X4}");
            }
        }

        public override void Write(uint address, byte value)
        {
            if ((address & 0x8000) == 0x8000)
            {
                bankSelect = (byte)(value & 0x07);
            }
            else
            {
                throw new Exception($"iNES Mapper 2: invalid write to 0x{address:X4}, value 0x{value:X2}");
            }
        }

        public override byte ReadPrg(uint address)
        {
            throw new NotImplementedException();
        }

        public override byte ReadChr(uint address)
        {
            return chrData[((address - 0x4000) / 0x2000) % chrData.Length][address & chrDataMask];
        }

        public override void WriteChr(uint address, byte value)
        {
            chrData[((address - 0x4000) / 0x2000) % chrData.Length][address & chrDataMask] = value;
        }
    }
}
