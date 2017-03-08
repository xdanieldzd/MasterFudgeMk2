using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Media.Sega
{
    public class RomRam32kCartridge : IMedia
    {
        byte[] romData, ramData;

        public RomRam32kCartridge()
        {
            ramData = new byte[0x4000];
        }

        public void Load(byte[] rawData)
        {
            romData = rawData;
        }

        public void Startup()
        {
            //
        }

        public void Reset()
        {
            // TODO: save ram handling
        }

        public void Unload()
        {
            // TODO: save ram handling
        }

        public byte Read(ushort address)
        {
            if ((address & 0x8000) == 0x8000)
                return ramData[address & (ramData.Length - 1)];
            else
                return romData[address & (romData.Length - 1)];
        }

        public void Write(ushort address, byte value)
        {
            if ((address & 0x8000) == 0x8000)
                ramData[address & (ramData.Length - 1)] = value;
        }
    }
}
