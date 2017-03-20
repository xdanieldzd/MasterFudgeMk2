using System;

namespace MasterFudgeMk2.Common.AudioBackend
{
    interface IAudioBackend : IDisposable
    {
        void AddSampleData(short[] samples);

        void Play();
        void Stop();
        void Reset();

        float Volume { get; set; }
    }
}
