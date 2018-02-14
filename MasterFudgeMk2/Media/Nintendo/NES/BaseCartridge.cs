using System.IO;

namespace MasterFudgeMk2.Media.Nintendo.NES
{
    public abstract class BaseCartridge : IMedia
    {
        protected const ushort prgBankSize = 0x4000;
        protected const ushort chrBankSize = 0x2000;

        protected INesHeader inesHeader;
        protected byte[] prgData, chrData;

        public BaseCartridge() { }

        public virtual void Load(FileInfo fileInfo)
        {
            using (FileStream file = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Read and parse iNES header
                byte[] header = new byte[0x10];
                file.Read(header, 0, header.Length);
                inesHeader = new INesHeader(header);

                // Prepare PRG data
                prgData = new byte[inesHeader.PRGRomSize * prgBankSize];
                file.Read(prgData, 0, prgData.Length);

                // Prepare CHR data
                if (!inesHeader.HasCHRRam)
                {
                    chrData = new byte[inesHeader.CHRRomSize * chrBankSize];
                    file.Read(chrData, 0, chrData.Length);
                }
                else
                {
                    // TODO: verify me?
                    chrData = new byte[inesHeader.PRGRomSize * chrBankSize];
                }
            }
        }

        public Devices.Nintendo.Ricoh2C02.Mirroring GetMirroring()
        {
            switch (inesHeader.Mirroring)
            {
                case INesMirroring.Horizontal: return Devices.Nintendo.Ricoh2C02.Mirroring.Horizontal;
                case INesMirroring.Vertical: return Devices.Nintendo.Ricoh2C02.Mirroring.Vertical;
                case INesMirroring.FourScreen: return Devices.Nintendo.Ricoh2C02.Mirroring.FourScreen;
                default: throw new System.NotImplementedException($"Unsupported mirroring mode {inesHeader.Mirroring}");
            }
        }

        public virtual void Reset() { }
        public virtual void Unload() { }
        public virtual void Step() { }

        public abstract byte Read(uint address);
        public virtual void Write(uint address, byte value) { }

        public abstract byte ReadChr(uint address);
        public virtual void WriteChr(uint address, byte value) { }
    }
}
