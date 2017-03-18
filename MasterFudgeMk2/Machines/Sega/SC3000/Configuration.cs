using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MasterFudgeMk2.Machines.Sega.SC3000
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Filename { get { return "SC3000.xml"; } }

        [Description("Emulate PAL System")]
        public bool IsPalSystem
        {
            get { return SettingsConfig.GetBoolean(nameof(IsPalSystem), false); }
            set { SettingsConfig.Set(nameof(IsPalSystem), value); }
        }

        //

        public Configuration() : base() { }
    }
}
