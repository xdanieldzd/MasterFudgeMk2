﻿using System;
using System.Drawing;

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

        public Type VideoBackend
        {
            get { return Type.GetType(SettingsConfig.GetString(nameof(VideoBackend))); }
            set { SettingsConfig.Set(nameof(VideoBackend), value.AssemblyQualifiedName); }
        }

        public Type AudioBackend
        {
            get { return Type.GetType(SettingsConfig.GetString(nameof(AudioBackend))); }
            set { SettingsConfig.Set(nameof(AudioBackend), value.AssemblyQualifiedName); }
        }

        public Point WindowLocation
        {
            get
            {
                string[] coords = SettingsConfig.GetString(nameof(WindowLocation), "0;0").Split(';');
                return new Point(int.Parse(coords[0]), int.Parse(coords[1]));
            }
            set
            {
                SettingsConfig.Set(nameof(WindowLocation), string.Format("{0};{1}", value.X, value.Y));
            }
        }

        public Size WindowSize
        {
            get
            {
                string[] coords = SettingsConfig.GetString(nameof(WindowSize), "528;526").Split(';');
                return new Size(int.Parse(coords[0]), int.Parse(coords[1]));
            }
            set
            {
                SettingsConfig.Set(nameof(WindowSize), string.Format("{0};{1}", value.Width, value.Height));
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

        public string[] RecentFiles
        {
            get { return SettingsConfig.GetString(nameof(RecentFiles), string.Empty).Split('|'); }
            set { SettingsConfig.Set(nameof(RecentFiles), string.Join("|", value)); }
        }
    }
}
