using System;

using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.Devices
{
    interface ISoundDevice
    {
        event EventHandler<AddSampleDataEventArgs> OnAddSampleData;
    }
}
