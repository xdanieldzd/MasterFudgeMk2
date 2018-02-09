namespace MasterFudgeMk2.Media.NEC
{
    public class RomOnlyCartridge : BaseCartridge
    {
        public RomOnlyCartridge() : base() { }

        public override byte Read(uint address)
        {
            return romData[address & (romData.Length - 1)];
        }

        public override void Write(uint address, byte value)
        {
            /* Cannot write to cartridge */
            return;
        }
    }
}
