namespace MasterFudgeMk2.Media.MSX
{
    /* Konami 8K without SCC */
    public class Konami8kCartridge : BaseCartridge
    {
        byte[] romBankMapping;

        public Konami8kCartridge() : base()
        {
            romBankMapping = new byte[4];
            romBankMapping[0] = 0x00;   /* Fixed bank 0 */
            romBankMapping[1] = 0x01;   /* Selected through 6000 */
            romBankMapping[2] = 0x02;   /* Selected through 8000 */
            romBankMapping[3] = 0x03;   /* Selected through A000 */
        }

        public override byte Read(ushort address)
        {
            if (address >= 0x4000 && address <= 0xBFFF) address -= 0x4000;
            else return 0x00;
            return romData[((romBankMapping[(address >> 13)] << 13) | (address & 0x1FFF)) & (romData.Length - 1)];
        }

        public override void Write(ushort address, byte value)
        {
            if (address == 0x6000 || address == 0x8000 || address == 0xA000)
                romBankMapping[((address - 0x4000) >> 13)] = value;
        }
    }
}
