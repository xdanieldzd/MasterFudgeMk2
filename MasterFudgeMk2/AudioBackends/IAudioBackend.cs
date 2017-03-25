﻿using System;

using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.AudioBackends
{
    interface IAudioBackend : IDisposable
    {
        void OnAddSampleData(object sender, AddSampleDataEventArgs e);

        void Play();
        void Stop();
        void Reset();
    }
}
