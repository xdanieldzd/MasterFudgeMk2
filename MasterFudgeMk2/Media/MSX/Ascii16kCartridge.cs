namespace MasterFudgeMk2.Media.MSX
{
    /*  ASCII 16KB */
    public class Ascii16kCartridge : BaseCartridge
    {
        byte[] romBankMapping;

        public Ascii16kCartridge() : base()
        {
            romBankMapping = new byte[2];
            romBankMapping[0] = 0x00;   /* Address 4000, selected through 6000 */
            romBankMapping[1] = 0x00;   /* Address 8000, selected through 7000 */
        }

        public override byte Read(ushort address)
        {
            if (address >= 0x4000 && address <= 0xBFFF) address -= 0x4000;
            else return 0x00;

            return romData[((romBankMapping[(address >> 14)] << 14) | (address & 0x3FFF)) & (romData.Length - 1)];
        }

        public override void Write(ushort address, byte value)
        {
            if (address == 0x6000 || address == 0x7000)
                romBankMapping[((address >> 12) & 0x01)] = value;
        }
    }
}
