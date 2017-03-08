using System;

namespace MasterFudgeMk2.Common.AudioBackend
{
    public class AddSampleDataEventArgs : EventArgs
    {
        public short[] Samples { get; set; }

        public AddSampleDataEventArgs(short[] samples)
        {
            Samples = samples;
        }
    }
}
