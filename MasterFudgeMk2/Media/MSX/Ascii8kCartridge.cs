namespace MasterFudgeMk2.Media.MSX
{
    /*  ASCII 8KB */
    public class Ascii8kCartridge : BaseCartridge
    {
        byte[] romBankMapping;

        public Ascii8kCartridge() : base()
        {
            romBankMapping = new byte[4];
            romBankMapping[0] = 0x00;   /* Address 4000, selected through 6000 */
            romBankMapping[1] = 0x01;   /* Address 6000, selected through 6800 */
            romBankMapping[2] = 0x02;   /* Address 8000, selected through 7000 */
            romBankMapping[3] = 0x03;   /* Address A000, selected through 7800 */
        }

        public override byte Read(ushort address)
        {
            if (address >= 0x4000 && address <= 0xBFFF) address -= 0x4000;
            else return 0x00;

            return romData[((romBankMapping[(address >> 13)] << 13) | (address & 0x1FFF)) & (romData.Length - 1)];
        }

        public override void Write(ushort address, byte value)
        {
            if (address == 0x6000 || address == 0x6800 || address == 0x7000 || address == 0x7800)
                romBankMapping[(((address - 0x4000) >> 11) & 0x03)] = value;
        }
    }
}
