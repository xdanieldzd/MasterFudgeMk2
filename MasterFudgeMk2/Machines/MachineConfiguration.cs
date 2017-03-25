using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Nini.Config;

namespace MasterFudgeMk2.Machines
{
    public abstract class MachineConfiguration : ConfigFile
    {
        const string sectionInput = "Input";
        public IConfig InputConfig
        {
            get
            {
                if (source.Configs[sectionInput] == null) source.AddConfig(sectionInput);
                return source.Configs[sectionInput];
            }
        }

        const string sectionSettings = "Settings";
        public IConfig SettingsConfig
        {
            get
            {
                if (source.Configs[sectionSettings] == null) source.AddConfig(sectionSettings);
                return source.Configs[sectionSettings];
            }
        }

        public string LastDirectory
        {
            get { return SettingsConfig.GetString(nameof(LastDirectory), string.Empty); }
            set { SettingsConfig.Set(nameof(LastDirectory), value); }
        }

        public MachineConfiguration() : base() { }
    }
}
