using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MasterFudgeMk2.Machines.Sega.SG1000
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Name { get { return "SG1000.xml"; } }

        [XmlElement]
        public Enum Pause { get; set; } = null;

        [XmlElement]
        public Enum P1Up { get; set; } = null;
        [XmlElement]
        public Enum P1Down { get; set; } = null;
        [XmlElement]
        public Enum P1Left { get; set; } = null;
        [XmlElement]
        public Enum P1Right { get; set; } = null;
        [XmlElement]
        public Enum P1Button1 { get; set; } = null;
        [XmlElement]
        public Enum P1Button2 { get; set; } = null;

        public Configuration() : base() { }
    }
}
