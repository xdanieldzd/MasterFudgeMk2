using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpDX.DirectInput;

using MasterFudgeMk2.AudioBackends;
using MasterFudgeMk2.InputBackends;
using MasterFudgeMk2.VideoBackends;
using MasterFudgeMk2.Machines;

namespace MasterFudgeMk2
{
    public partial class NewConfigForm : Form
    {
        MachineConfiguration activeMachineConfiguration;
        IVideoBackend activeVideoBackend;
        IAudioBackend activeAudioBackend;
        IInputBackend activeInputBackend;

        public NewConfigForm(MachineConfiguration machineConfig, IVideoBackend videoBackend, IAudioBackend audioBackend, IInputBackend inputBackend)
        {
            InitializeComponent();

            activeMachineConfiguration = machineConfig;
            activeVideoBackend = videoBackend;
            activeAudioBackend = audioBackend;
            activeInputBackend = inputBackend;

            var tmp = activeInputBackend.GetInputValues();
        }
    }
}
