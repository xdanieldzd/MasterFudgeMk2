using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using Nini.Config;

namespace MasterFudgeMk2
{
    public sealed class EmulatorConfigurationNew : ConfigFile
    {
        public override sealed string Filename { get { return "EmulatorNew.xml"; } }

        const string sectionSettings = "Settings";
        public IConfig SettingsConfig
        {
            get
            {
                if (source.Configs[sectionSettings] == null) source.AddConfig(sectionSettings);
                return source.Configs[sectionSettings];
            }
        }

        public Type VideoBackend
        {
            get { return Type.GetType(SettingsConfig.GetString(nameof(VideoBackend)) ?? typeof(VideoBackends.Direct2DBackend).AssemblyQualifiedName); }
            set { SettingsConfig.Set(nameof(VideoBackend), value.AssemblyQualifiedName); }
        }

        public Type AudioBackend
        {
            get { return Type.GetType(SettingsConfig.GetString(nameof(AudioBackend)) ?? typeof(AudioBackends.NullAudioBackend).AssemblyQualifiedName); }
            set { SettingsConfig.Set(nameof(AudioBackend), value.AssemblyQualifiedName); }
        }

        public Type InputBackend
        {
            get { return Type.GetType(SettingsConfig.GetString(nameof(InputBackend)) ?? typeof(InputBackends.DirectInputBackend).AssemblyQualifiedName); }
            set { SettingsConfig.Set(nameof(InputBackend), value.AssemblyQualifiedName); }
        }

        public bool LimitFps
        {
            get { return SettingsConfig.GetBoolean(nameof(LimitFps), true); }
            set { SettingsConfig.Set(nameof(LimitFps), value); }
        }

        public bool KeepAspectRatio
        {
            get { return SettingsConfig.GetBoolean(nameof(KeepAspectRatio), true); }
            set { SettingsConfig.Set(nameof(KeepAspectRatio), value); }
        }

        public bool ForceSquarePixels
        {
            get { return SettingsConfig.GetBoolean(nameof(ForceSquarePixels), false); }
            set { SettingsConfig.Set(nameof(ForceSquarePixels), value); }
        }

        public bool DebugMode
        {
            get { return SettingsConfig.GetBoolean(nameof(DebugMode), false); }
            set { SettingsConfig.Set(nameof(DebugMode), value); }
        }

        public List<string> RecentFiles
        {
            get { return SettingsConfig.GetString(nameof(RecentFiles), string.Empty).Split('|').ToList(); }
            set { SettingsConfig.Set(nameof(RecentFiles), string.Join("|", value)); }
        }

        public EmulatorConfigurationNew() : base() { }
    }
}
