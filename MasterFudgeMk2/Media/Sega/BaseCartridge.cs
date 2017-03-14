using System.IO;

namespace MasterFudgeMk2.Media.Sega
{
    public abstract class BaseCartridge : IMedia
    {
        protected RomHeader romHeader;
        protected byte[] romData;

        public BaseCartridge() { }

        public virtual void Load(FileInfo fileInfo)
        {
            using (FileStream file = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if ((file.Length % 0x4000) == 0x200)
                {
                    /* Copier header */
                    romData = new byte[file.Length - (file.Length % 0x4000)];
                    file.Seek(file.Length % 0x4000, SeekOrigin.Begin);
                }
                else
                {
                    /* Normal ROM */
                    romData = new byte[file.Length];
                }
                file.Read(romData, 0, romData.Length);
            }

            romHeader = new RomHeader(romData);
        }

        public virtual void Reset() { }
        public virtual void Unload() { }
        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte value);
    }
}
