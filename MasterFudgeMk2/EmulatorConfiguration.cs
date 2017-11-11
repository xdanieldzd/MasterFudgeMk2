using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MasterFudgeMk2
{
    public sealed class EmulatorConfiguration : ConfigFile
    {
        public override sealed string Name { get { return "Emulator.xml"; } }

        [XmlElement(ElementName = nameof(VideoBackend))]
        public string VideoBackendString
        {
            get { return VideoBackend.AssemblyQualifiedName; }
            set { VideoBackend = Type.GetType(value); }
        }
        [XmlElement(ElementName = nameof(AudioBackend))]
        public string AudioBackendString
        {
            get { return AudioBackend.AssemblyQualifiedName; }
            set { AudioBackend = Type.GetType(value); }
        }
        [XmlElement(ElementName = nameof(InputBackend))]
        public string InputBackendString
        {
            get { return InputBackend.AssemblyQualifiedName; }
            set { InputBackend = Type.GetType(value); }
        }

        [XmlIgnore]
        public Type VideoBackend { get; set; } = typeof(VideoBackends.Direct2DBackend);
        [XmlIgnore]
        public Type AudioBackend { get; set; } = typeof(AudioBackends.NullAudioBackend);
        [XmlIgnore]
        public Type InputBackend { get; set; } = typeof(InputBackends.DInputKeyboardBackend);

        [XmlElement]
        public bool LimitFps { get; set; } = true;
        [XmlElement]
        public bool KeepAspectRatio { get; set; } = true;
        [XmlElement]
        public bool ForceSquarePixels { get; set; } = false;
        [XmlElement]
        public bool DebugMode { get; set; } = false;

        [XmlArray]
        public List<string> RecentFiles { get; set; } = new List<string>();

        public EmulatorConfiguration() : base() { }
    }
}
