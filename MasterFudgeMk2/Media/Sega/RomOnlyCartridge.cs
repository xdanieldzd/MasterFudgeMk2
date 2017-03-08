using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Media.Sega
{
    public class RomOnlyCartridge : IMedia
    {
        byte[] romData;

        public RomOnlyCartridge() { }

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
            //
        }

        public void Unload()
        {
            //
        }

        public byte Read(ushort address)
        {
            return romData[address & (romData.Length - 1)];
        }

        public void Write(ushort address, byte value)
        {
            /* Cannot write to cartridge */
            return;
        }
    }
}
