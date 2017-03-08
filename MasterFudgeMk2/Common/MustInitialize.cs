using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Common
{
    /* http://stackoverflow.com/a/41685659 */
    public abstract class MustInitialize<T>
    {
        public MustInitialize(params T[] parameters) { }
    }
}
