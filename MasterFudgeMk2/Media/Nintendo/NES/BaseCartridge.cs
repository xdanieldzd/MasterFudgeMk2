using System;
using System.IO;

namespace MasterFudgeMk2.Media.Nintendo.NES
{
    public abstract class BaseCartridge : IMedia
    {
        protected INesHeader inesHeader;
        protected byte[][] prgData, chrData;
        protected byte[] sramData;

        protected uint[] prgSlots, chrSlots;

        public BaseCartridge() { }

        public virtual void Load(FileInfo fileInfo)
        {
            using (FileStream file = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Read and parse iNES header
                byte[] header = new byte[0x10];
                file.Read(header, 0, header.Length);
                inesHeader = new INesHeader(header);

                // Prepare PRG pages
                prgData = new byte[inesHeader.PRGRomSize * 4][];
                for (int i = 0; i < prgData.Length; i++)
                {
                    prgData[i] = new byte[0x1000];
                    file.Read(prgData[i], 0, 0x1000);
                }

                // Prepare CHR pages
                if (!inesHeader.HasCHRRam)
                {
                    chrData = new byte[inesHeader.CHRRomSize * 8][];
                    for (int i = 0; i < chrData.Length; i++)
                    {
                        chrData[i] = new byte[0x400];
                        file.Read(chrData[i], 0, 0x400);
                    }
                }
                else
                {
                    chrData = new byte[(inesHeader.PRGRomSize * 4) * 16][];
                    for (int i = 0; i < chrData.Length; i++)
                        chrData[i] = new byte[0x400];
                }

                sramData = new byte[8192];

                prgSlots = new uint[4];
                chrSlots = new uint[8];
            }
        }

        public void MapPrgData(int slot, int bank, int bankSize)
        {
            // TODO
        }

        public void MapChrData(int slot, int bank, int bankSize)
        {
            // TODO
        }

        public byte ReadPrg(uint address)
        {
            return prgData[prgSlots[(address >> 12) & 0x0F]][address & 0x0FFF];
        }

        public byte ReadChr(uint address)
        {
            return chrData[chrSlots[(address >> 10) & 0x3F]][address & 0x03FF];
        }

        public void WriteChr(ushort address, byte value)
        {
            if (inesHeader.HasCHRRam)
                chrData[chrSlots[(address >> 10) & 0x3F]][address & 0x03FF] = value;
        }

        public virtual void Reset() { }
        public virtual void Unload() { }
        public virtual void Step() { }

        public virtual byte Read(uint address)
        {
            if (address >= 0x6000 && address < 0x8000)
            {
                return sramData[address - 0x6000];
            }
            else if (address >= 0x8000 && address <= 0xFFFF)
            {
                return ReadPrg(address - 0x8000);
            }
            else
                throw new Exception("NES cartridge read out of range");
        }

        public abstract void Write(uint address, byte value);
    }
}
