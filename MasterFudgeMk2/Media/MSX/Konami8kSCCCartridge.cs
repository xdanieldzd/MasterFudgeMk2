namespace MasterFudgeMk2.Media.MSX
{
    /* Konami 8K with SCC */
    public class Konami8kSccCartridge : BaseCartridge
    {
        // TODO: maybe implement the SCC, if we ever get the basic sound stuff working, etc., etc...

        byte[] romBankMapping;

        public Konami8kSccCartridge() : base()
        {
            romBankMapping = new byte[4];
            romBankMapping[0] = 0x00;   /* Selected through 5000 */
            romBankMapping[1] = 0x01;   /* Selected through 7000 */
            romBankMapping[2] = 0x02;   /* Selected through 9000 */
            romBankMapping[3] = 0x03;   /* Selected through B000 */
        }

        public override byte Read(ushort address)
        {
            if (address >= 0x4000 && address <= 0xBFFF) address -= 0x4000;
            else return 0x00;
            return romData[((romBankMapping[(address >> 13)] << 13) | (address & 0x1FFF)) & (romData.Length - 1)];
        }

        public override void Write(ushort address, byte value)
        {
            if (address == 0x5000 || address == 0x7000 || address == 0x9000 || address == 0xB000)
                romBankMapping[((address - 0x4000) >> 13)] = value;
        }
    }
}
