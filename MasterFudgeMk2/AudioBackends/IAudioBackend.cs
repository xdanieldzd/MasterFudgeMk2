using System;

namespace MasterFudgeMk2.AudioBackends
{
    interface IAudioBackend : IDisposable
    {
        void AddSampleData(short[] samples);

        void Play();
        void Stop();
        void Reset();
    }
}
