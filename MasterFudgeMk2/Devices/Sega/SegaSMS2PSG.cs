using System;

using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.Devices.Sega
{
    public class SegaSMS2PSG : SN76489
    {
        /* LFSR is 16 bits, tapped bits are 0 and 3 (mask 0x0009), going into bit 15 */
        protected override ushort noiseLfsrMask => 0xFFFF;
        protected override ushort noiseTappedBits => 0x0009;
        protected override int noiseBitShift => 15;

        public SegaSMS2PSG(double clockRate, double refreshRate, int sampleRate, int numOutputChannels, EventHandler<AddSampleDataEventArgs> addSampleDataEvent) : base(clockRate, refreshRate, sampleRate, numOutputChannels, addSampleDataEvent) { }

        public override void Reset()
        {
            base.Reset();

            noiseLfsr = 0x8000;
        }
    }
}
