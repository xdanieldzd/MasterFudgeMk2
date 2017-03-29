using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.InputBackends
{
    public interface IInputBackend : IDisposable
    {
        IEnumerable<Enum> GetInputValues();
        string GetInputDescription(Enum input, int value);
        void OnPollInput(object sender, PollInputEventArgs e);
    }
}
