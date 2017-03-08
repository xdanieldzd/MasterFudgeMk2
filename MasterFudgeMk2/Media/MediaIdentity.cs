using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Media
{
    public class MediaIdentity
    {
        public string Name { get; set; }
        public Type MediaType { get; set; }

        public MediaIdentity()
        {
            Name = string.Empty;
            MediaType = null;
        }
    }
}
