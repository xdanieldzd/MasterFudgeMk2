﻿using System;
using System.ComponentModel;

using MasterFudgeMk2.Common;

namespace MasterFudgeMk2.AudioBackends
{
    [Description("Null Audio")]
    public class NullAudioBackend : MustInitialize<int>, IAudioBackend
    {
        public NullAudioBackend(int sampleRate, int numChannels) : base(sampleRate, numChannels) { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) { }
        }

        public void AddSampleData(short[] samples) { }
        public void Play() { }
        public void Stop() { }
        public void Reset() { }
    }
}
