using System;

using MasterFudgeMk2.Common.AudioBackend;

namespace MasterFudgeMk2.Devices
{
    interface ISoundDevice
    {
        event EventHandler<AddSampleDataEventArgs> OnAddSampleData;
    }
}
