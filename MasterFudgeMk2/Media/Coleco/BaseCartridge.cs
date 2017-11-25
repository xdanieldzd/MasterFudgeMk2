using System.IO;

namespace MasterFudgeMk2.Media.Coleco
{
    public abstract class BaseCartridge : IMedia
    {
        protected byte[] romData;

        public BaseCartridge() { }

        public virtual void Load(FileInfo fileInfo)
        {
            using (FileStream file = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                romData = new byte[file.Length];
                file.Read(romData, 0, romData.Length);
            }
        }

        public virtual void Reset() { }
        public virtual void Unload() { }
        public virtual void Step() { }
        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte value);
    }
}
