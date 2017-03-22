using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MasterFudgeMk2.Machines.Various.MSX1
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Filename { get { return "MSX1.xml"; } }

        /* Settings */
        [Description("BIOS Path")]
        public string BiosPath
        {
            get { return SettingsConfig.GetString(nameof(BiosPath), string.Empty); }
            set { SettingsConfig.Set(nameof(BiosPath), value); }
        }

        //
    }
}
