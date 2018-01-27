using System;
using System.Drawing;
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

        [XmlIgnore]
        public Type VideoBackend { get; set; } = typeof(VideoBackends.Direct2DBackend);
        [XmlIgnore]
        public Type AudioBackend { get; set; } = typeof(AudioBackends.NullAudioBackend);

        [XmlElement(ElementName = nameof(WindowLocation))]
        public string WindowLocationString
        {
            get { return $"{WindowLocation.X},{WindowLocation.Y}"; }
            set
            {
                string[] splitValues = value.Split(',');
                WindowLocation = new Point(int.Parse(splitValues[0]), int.Parse(splitValues[1]));
            }
        }
        [XmlElement(ElementName = nameof(WindowSize))]
        public string WindowSizeString
        {
            get { return $"{WindowSize.Width},{WindowSize.Height}"; }
            set
            {
                string[] splitValues = value.Split(',');
                WindowSize = new Size(int.Parse(splitValues[0]), int.Parse(splitValues[1]));
            }
        }

        [XmlIgnore]
        public Point WindowLocation { get; set; } = Point.Empty;
        [XmlIgnore]
        public Size WindowSize { get; set; } = new Size(623, 532);

        [XmlElement]
        public bool LimitFps { get; set; } = true;
        [XmlElement]
        public bool ForceSquarePixels { get; set; } = false;
        [XmlElement]
        public bool LinearInterpolation { get; set; } = true;
        [XmlElement]
        public bool DebugMode { get; set; } = false;

        [XmlArray]
        public List<string> RecentFiles { get; set; } = new List<string>();
    }
}
