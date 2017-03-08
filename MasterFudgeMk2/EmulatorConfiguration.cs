using System.IO;

using Nini.Config;

namespace MasterFudgeMk2
{
    public sealed class EmulatorConfiguration : ConfigFile
    {
        public override sealed string Filename { get { return "Emulator.xml"; } }

        const string sectionSettings = "Settings";
        public IConfig SettingsConfig
        {
            get
            {
                if (source.Configs[sectionSettings] == null) source.AddConfig(sectionSettings);
                return source.Configs[sectionSettings];
            }
        }

        public bool LimitFps
        {
            get { return SettingsConfig.GetBoolean(nameof(LimitFps), true); }
            set { SettingsConfig.Set(nameof(LimitFps), value); }
        }

        public bool MuteSound
        {
            get { return SettingsConfig.GetBoolean(nameof(MuteSound), true); }
            set { SettingsConfig.Set(nameof(MuteSound), value); }
        }

        public bool KeepAspectRatio
        {
            get { return SettingsConfig.GetBoolean(nameof(KeepAspectRatio), true); }
            set { SettingsConfig.Set(nameof(KeepAspectRatio), value); }
        }

        public bool AutoResize
        {
            get { return SettingsConfig.GetBoolean(nameof(AutoResize), true); }
            set { SettingsConfig.Set(nameof(AutoResize), value); }
        }

        public bool DebugMode
        {
            get { return SettingsConfig.GetBoolean(nameof(DebugMode), false); }
            set { SettingsConfig.Set(nameof(DebugMode), value); }
        }

        public string[] RecentFiles
        {
            get { return SettingsConfig.GetString(nameof(RecentFiles), string.Empty).Split('|'); }
            set { SettingsConfig.Set(nameof(RecentFiles), string.Join("|", value)); }
        }
    }
}
