namespace MasterFudgeMk2.Media.MSX
{
    public class RawRomCartridge : BaseCartridge
    {
        public RawRomCartridge() : base() { }

        public override byte Read(ushort address)
        {
            return romData[address & (romData.Length - 1)];
        }

        public override void Write(ushort address, byte value)
        {
            /* Cannot write to cartridge */
            return;
        }
    }
}
