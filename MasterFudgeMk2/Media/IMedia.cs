using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MasterFudgeMk2.Media
{
    public interface IMedia
    {
        // TODO: restructure for SRAM loading

        void Load(byte[] rawData);
        void Startup();
        void Reset();
        void Unload();
        byte Read(ushort address);
        void Write(ushort address, byte value);
    }
}
