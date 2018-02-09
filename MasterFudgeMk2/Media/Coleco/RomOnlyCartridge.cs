namespace MasterFudgeMk2.Media.Coleco
{
    public class RomOnlyCartridge : BaseCartridge
    {
        // TODO: actually emulate the 4 ROM regions? http://atariage.com/forums/topic/210168-colecovision-bank-switching/

        public RomOnlyCartridge() : base() { }

        public override byte Read(uint address)
        {
            address &= 0xFFFF;
            address -= 0x8000;
            if (address >= romData.Length) address -= (ushort)romData.Length;
            return romData[address];
        }

        public override void Write(uint address, byte value)
        {
            /* Cannot write to cartridge */
            return;
        }
    }
}
