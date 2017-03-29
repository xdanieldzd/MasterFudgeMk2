namespace MasterFudgeMk2.Media.MSX
{
    /* Raw ROM without Mapper */
    public class RawRomCartridge : BaseCartridge
    {
        public RawRomCartridge() : base() { }

        public override byte Read(ushort address)
        {
            if (address >= 0x4000 && address <= 0xBFFF)
                address -= 0x4000;

            return romData[address & (romData.Length - 1)];
        }

        public override void Write(ushort address, byte value)
        {
            /* Cannot write to cartridge */
            return;
        }
    }
}
