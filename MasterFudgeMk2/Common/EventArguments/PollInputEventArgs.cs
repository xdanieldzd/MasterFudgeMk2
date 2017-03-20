using System;
using System.Collections.Generic;

namespace MasterFudgeMk2.Common.EventArguments
{
    public class PollInputEventArgs : EventArgs
    {
        public IEnumerable<Enum> Pressed { get; set; }

        public PollInputEventArgs()
        {
            Pressed = null;
        }
    }
}
