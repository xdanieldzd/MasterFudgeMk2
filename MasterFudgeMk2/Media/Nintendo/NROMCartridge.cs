using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Media.Nintendo
{
    public class NROMCartridge : IMedia
    {
        byte[] romData;

        public NROMCartridge() { }

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
            return 0;
            // TODO: prg & chr rom page crap
            return romData[address & (romData.Length - 1)];
        }

        public void Write(ushort address, byte value)
        {
            /* Cannot write to cartridge */
            return;
        }
    }
}
