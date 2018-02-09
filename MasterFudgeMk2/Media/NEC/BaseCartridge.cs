using System.IO;

namespace MasterFudgeMk2.Media.NEC
{
    public abstract class BaseCartridge : IMedia
    {
        protected byte[] romData;

        public BaseCartridge() { }

        public virtual void Load(FileInfo fileInfo)
        {
            using (FileStream file = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int headerSize = (int)(file.Length & 0x200);
                romData = new byte[file.Length - headerSize];
                file.Seek(headerSize, SeekOrigin.Begin);
                file.Read(romData, 0, romData.Length);

                /* Check for bitswapped TG-16 ROMs, unswap if needed */
                if (romData[0x1FFF] < 0xE0)
                {
                    for (int i = 0; i < romData.Length; i++)
                    {
                        romData[i] = (byte)(
                            ((romData[i] & 0x01) << 7) | ((romData[i] & 0x02) << 5) | ((romData[i] & 0x04) << 3) | ((romData[i] & 0x08) << 1) |
                            ((romData[i] & 0x10) >> 1) | ((romData[i] & 0x20) >> 3) | ((romData[i] & 0x40) >> 5) | ((romData[i] & 0x80) >> 7));
                    }
                }
            }
        }

        public virtual void Reset() { }
        public virtual void Unload() { }
        public virtual void Step() { }
        public abstract byte Read(uint address);
        public abstract void Write(uint address, byte value);
    }
}
