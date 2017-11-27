using System;
using System.IO;

namespace MasterFudgeMk2.Media.Sega
{
    public class CodemastersCartridge : BaseCartridge
    {
        // TODO: Ernie Els Golf cartridge RAM?

        byte[] pagingSlots;
        byte bankMask;

        public CodemastersCartridge() : base()
        {
            pagingSlots = new byte[3];
            pagingSlots[0] = 0x00;
            pagingSlots[1] = 0x01;
            pagingSlots[2] = 0x02;
        }

        public override void Load(FileInfo fileInfo)
        {
            base.Load(fileInfo);

            bankMask = (byte)((romData.Length >> 14) - 1);
        }

        public override void Reset()
        {
            // TODO: save ram handling
        }

        public override void Unload()
        {
            // TODO: save ram handling
        }

        public override byte Read(ushort address)
        {
            switch (address & 0xC000)
            {
                case 0x0000: return romData[((pagingSlots[0] << 14) | (address & 0x3FFF))];
                case 0x4000: return romData[((pagingSlots[1] << 14) | (address & 0x3FFF))];
                case 0x8000: return romData[((pagingSlots[2] << 14) | (address & 0x3FFF))];
                default: throw new Exception(string.Format("Codemasters mapper: Cannot read from cartridge address 0x{0:X4}", address));
            }
        }

        public override void Write(ushort address, byte value)
        {
            switch (address)
            {
                case 0x0000: pagingSlots[0] = value; break;
                case 0x4000: pagingSlots[1] = value; break;
                case 0x8000: pagingSlots[2] = value; break;
            }
        }
    }
}
