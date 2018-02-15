using System;

namespace MasterFudgeMk2.Media.Nintendo.NES
{
    public class Mapper0Cartridge : BaseCartridge
    {
        protected byte[] sramData;

        public Mapper0Cartridge()
        {
            // Family Basic only; just provide 8k regardless
            sramData = new byte[0x2000];
        }

        public override byte Read(uint address)
        {
            switch (address & 0xF000)
            {
                case 0x4000:
                case 0x5000:
                    return 0x00;

                case 0x6000:
                case 0x7000:
                    return sramData[address & (sramData.Length - 1)];

                case 0x8000:
                case 0x9000:
                case 0xA000:
                case 0xB000:
                case 0xC000:
                case 0xD000:
                case 0xE000:
                case 0xF000:
                    return prgData[(address & (prgData.Length - 1)) & 0x7FFF];

                default:
                    throw new Exception($"iNES Mapper 0: invalid read from 0x{address:X4}");
            }
        }

        public override byte ReadChr(uint address)
        {
            return chrData[(address & (chrData.Length - 1)) & 0x3FFF];
        }
    }
}
