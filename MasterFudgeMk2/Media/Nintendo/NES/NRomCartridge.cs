using System.IO;

namespace MasterFudgeMk2.Media.Nintendo.NES
{
    public class NRomCartridge : BaseCartridge
    {
        public NRomCartridge() { }

        public override void Load(FileInfo fileInfo)
        {
            base.Load(fileInfo);

            // TODO: correct?
            MapPrgData(0, 0, 32);
            MapChrData(0, 0, 8);
        }

        public override void Write(uint address, byte value)
        {
            /* Cannot write to cartridge */
            return;
        }
    }
}
