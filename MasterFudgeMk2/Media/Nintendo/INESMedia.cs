using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Media.Nintendo
{
    interface INESMedia : IMedia
    {
        RomHeaderINES RomHeader { get; set; }
        void Tick();
    }
}
