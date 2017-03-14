namespace MasterFudgeMk2.Media.Sega
{
    public class RomRam32kCartridge : BaseCartridge
    {
        byte[] ramData;

        public RomRam32kCartridge() : base()
        {
            ramData = new byte[0x4000];
        }
        
        public override void Reset()
        {
            // TODO: save ram handling
        }

        public override void Unload()
        {
            // TODO: save ram handling
        }

        public override byte Read(ushort address)
        {
            if ((address & 0x8000) == 0x8000)
                return ramData[address & (ramData.Length - 1)];
            else
                return romData[address & (romData.Length - 1)];
        }

        public override void Write(ushort address, byte value)
        {
            if ((address & 0x8000) == 0x8000)
                ramData[address & (ramData.Length - 1)] = value;
        }
    }
}
