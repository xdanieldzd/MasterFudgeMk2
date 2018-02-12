using System.IO;

namespace MasterFudgeMk2.Media.Nintendo.NES
{
    public abstract class BaseCartridge : IMedia
    {
        protected const ushort prgBankSize = 0x4000;
        protected const ushort chrBankSize = 0x2000;

        protected INesHeader inesHeader;
        protected byte[][] prgData, chrData;

        protected ushort prgDataMask, chrDataMask;

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
                prgData = new byte[inesHeader.PRGRomSize][];
                for (int i = 0; i < prgData.Length; i++)
                {
                    prgData[i] = new byte[prgBankSize];
                    file.Read(prgData[i], 0, prgData[i].Length);
                }

                // Prepare CHR pages
                if (!inesHeader.HasCHRRam)
                {
                    chrData = new byte[inesHeader.CHRRomSize][];
                    for (int i = 0; i < chrData.Length; i++)
                    {
                        chrData[i] = new byte[chrBankSize];
                        file.Read(chrData[i], 0, chrData[i].Length);
                    }
                }
                else
                {
                    // TODO: verify me?
                    chrData = new byte[inesHeader.PRGRomSize][];
                    for (int i = 0; i < chrData.Length; i++)
                        chrData[i] = new byte[0x2000];
                }

                // Set masks
                prgDataMask = (prgBankSize - 1);
                chrDataMask = (chrBankSize - 1);
            }
        }

        public Devices.Nintendo.PPUMirroring GetMirroring()
        {
            switch (inesHeader.Mirroring)
            {
                case INesMirroring.Horizontal: return Devices.Nintendo.PPUMirroring.Horizontal;
                case INesMirroring.Vertical: return Devices.Nintendo.PPUMirroring.Vertical;
                case INesMirroring.FourScreen: return Devices.Nintendo.PPUMirroring.FourScreen;
                default: throw new System.NotImplementedException($"Unsupported mirroring mode {inesHeader.Mirroring}");
            }
        }

        public virtual void Reset() { }
        public virtual void Unload() { }
        public virtual void Step() { }

        public abstract byte Read(uint address);
        public abstract byte ReadPrg(uint address);
        public abstract byte ReadChr(uint address);

        public virtual void Write(uint address, byte value) { }
        public virtual void WriteChr(uint address, byte value) { }
    }
}
