using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common;

namespace MasterFudgeMk2.Media.Sega
{
    public class StandardMapperCartridge : IMedia
    {
        byte[] romData, pagingRegisters, ramData;
        byte bankMask;
        bool hasCartRam;

        bool isRamEnabled { get { return Utilities.IsBitSet(pagingRegisters[0], 3); } }
        bool isRomWriteEnable { get { return Utilities.IsBitSet(pagingRegisters[0], 7); } }
        int ramBank { get { return ((pagingRegisters[0] >> 2) & 0x01); } }
        int romBank0 { get { return pagingRegisters[1]; } }
        int romBank1 { get { return pagingRegisters[2]; } }
        int romBank2 { get { return pagingRegisters[3]; } }

        public StandardMapperCartridge()
        {
            pagingRegisters = new byte[0x04];
            pagingRegisters[0] = 0x00;  /* Mapper control */
            pagingRegisters[1] = 0x00;  /* Page 0 ROM bank */
            pagingRegisters[2] = 0x01;  /* Page 1 ROM bank */
            pagingRegisters[3] = 0x02;  /* Page 2 ROM bank */

            ramData = new byte[0x8000];
            bankMask = 0xFF;
        }

        public void Load(byte[] rawData)
        {
            romData = rawData;

            bankMask = (byte)((romData.Length >> 14) - 1);
        }

        public void Startup()
        {
            //
        }

        public void Reset()
        {
            // TODO: save ram handling
        }

        public void Unload()
        {
            // TODO: save ram handling
        }

        public byte Read(ushort address)
        {
            switch (address & 0xC000)
            {
                case 0x0000:
                    if (address < 0x400)
                        /* First 1kb is constant to preserve interrupt vectors */
                        return romData[address];
                    else
                        return romData[((romBank0 << 14) | (address & 0x3FFF))];

                case 0x4000:
                    return romData[((romBank1 << 14) | (address & 0x3FFF))];

                case 0x8000:
                    if (isRamEnabled)
                        return ramData[((ramBank << 14) | (address & 0x3FFF))];
                    else
                        return romData[((romBank2 << 14) | (address & 0x3FFF))];

                default:
                    throw new Exception(string.Format("Sega mapper: Cannot read from cartridge address 0x{0:X4}", address));
            }
        }

        public void Write(ushort address, byte value)
        {
            if (address >= 0xFFFC && address <= 0xFFFF)
            {
                /* Write to paging register */
                if ((address & 0x0003) != 0x00) value &= bankMask;
                pagingRegisters[address & 0x0003] = value;

                /* Check if RAM ever gets enabled; if it is, indicate that we'll need to save the RAM */
                if (!hasCartRam && isRamEnabled && (address & 0x0003) == 0x0000)
                    hasCartRam = true;
            }
            if (isRamEnabled && (address & 0xC000) == 0x8000)
            {
                /* Cartridge RAM */
                ramData[((ramBank << 14) | (address & 0x3FFF))] = value;
            }
            else if (isRomWriteEnable)
            {
                /* ROM write enabled...? */
            }

            /* Otherwise ignore writes to ROM, as some games seem to be doing that? (ex. Gunstar Heroes GG to 0000) */
        }
    }
}
