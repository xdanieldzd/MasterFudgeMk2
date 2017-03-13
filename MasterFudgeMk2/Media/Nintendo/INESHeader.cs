using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Media.Nintendo
{
    public class INESHeader
    {
        public string SignatureString { get; private set; }
        public byte PRGRomSize { get; private set; }
        public byte CHRRomSize { get; private set; }
        public byte Flags6 { get; private set; }
        public byte Flags7 { get; private set; }
        public byte[] Reserved { get; private set; }    // iNES 2.0, junk, etc., 8 bytes

        public bool IsSignatureCorrect { get { return (SignatureString == "NES\x1A"); } }

        public byte MapperNumber { get { return (byte)(((Flags6 & 0xF0) >> 4) | (Flags7 & 0xF0)); } }

        public INESHeader(byte[] romData)
        {
            SignatureString = Encoding.ASCII.GetString(romData, 0, 4);
            PRGRomSize = romData[4];
            CHRRomSize = romData[5];
            Flags6 = romData[6];
            Flags7 = romData[7];
            Reserved = new byte[8];
            Buffer.BlockCopy(romData, 8, Reserved, 0, Reserved.Length);
        }
    }
}
