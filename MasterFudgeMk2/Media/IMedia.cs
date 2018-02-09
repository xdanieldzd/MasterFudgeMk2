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

        void Load(FileInfo fileInfo);
        void Reset();
        void Unload();
        void Step();
        byte Read(uint address);
        void Write(uint address, byte value);
    }
}
