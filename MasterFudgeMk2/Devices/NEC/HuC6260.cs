using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Devices.NEC
{
    // HuC6260 (VCE; Video Color Encoder)

    public class HuC6260
    {
        ushort currentAddress;
        ushort[] data;
        byte controlRegister;

        public int ScanlineCount { get { return ((controlRegister & 4) != 0) ? 263 : 262; } }

        public HuC6260()
        {
            data = new ushort[512];
        }

        public void Startup()
        {
            Reset();
        }

        public void Reset()
        {
            currentAddress = 0;
            for (int i = 0; i < data.Length; i++) data[i] = 0;
            controlRegister = 0;
        }

        public byte Read(uint address)
        {
            byte retVal = 0xFF;

            switch ((address & 0x0007))
            {
                /* Data LSB */
                case 0x4:
                    retVal = (byte)(data[currentAddress] & 0xFF);
                    break;

                /* Data MSB */
                case 0x5:
                    retVal = (byte)((data[currentAddress] >> 8) | 0xFE);
                    currentAddress++;
                    currentAddress &= 0x01FF;
                    break;
            }

            return retVal;
        }

        public void Write(uint address, byte value)
        {
            switch ((address & 0x0007))
            {
                /* VCE Control */
                case 0x0:
                    controlRegister = value;
                    break;

                /* Unused */
                case 0x1:
                    break;

                /* Address LSB */
                case 0x2:
                    currentAddress &= 0xFF00;
                    currentAddress |= value;
                    break;

                /* Address MSB */
                case 0x3:
                    currentAddress &= 0x00FF;
                    currentAddress |= (ushort)((value & 0x01) << 8);
                    break;

                /* Data LSB */
                case 0x4:
                    data[currentAddress] &= 0xFF00;
                    data[currentAddress] |= value;
                    break;

                /* Data MSB */
                case 0x5:
                    data[currentAddress] &= 0x00FF;
                    data[currentAddress] |= (ushort)(value << 8);
                    currentAddress++;
                    currentAddress &= 0x01FF;
                    break;

                /* Unused */
                case 0x6:
                    break;

                /* Unused */
                case 0x7:
                    break;
            }
        }

        private uint GetColorRGBA(int address)
        {
            uint c = 0xFF000000;
            c |= (uint)(((data[address] >> 3) & 7) << 21);
            c |= (uint)(((data[address] >> 6) & 7) << 13);
            c |= (uint)(((data[address] >> 0) & 7) << 5);
            return c;
        }
    }
}
