using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MasterFudgeMk2.Machines.Sega.MasterSystem
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Name { get { return "MasterSystem.xml"; } }

        /* Settings */
        [Description("Bootstrap Path")]
        [XmlElement]
        public string BootstrapPath { get; set; } = string.Empty;
        [Description("Enable Bootstrap")]
        [XmlElement]
        public bool UseBootstrap { get; set; } = false;
        [Description("Emulate Export System")]
        [XmlElement]
        public bool IsExportSystem { get; set; } = true;
        [Description("Emulate PAL System")]
        [XmlElement]
        public bool IsPalSystem { get; set; } = false;

        /* Inputs (General) */
        [XmlElement]
        public Enum Pause { get; set; } = null;
        [XmlElement]
        public Enum Reset { get; set; } = null;

        /* Inputs (P1) */
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

        /* Inputs (P2) */
        [XmlElement]
        public Enum P2Up { get; set; } = null;
        [XmlElement]
        public Enum P2Down { get; set; } = null;
        [XmlElement]
        public Enum P2Left { get; set; } = null;
        [XmlElement]
        public Enum P2Right { get; set; } = null;
        [XmlElement]
        public Enum P2Button1 { get; set; } = null;
        [XmlElement]
        public Enum P2Button2 { get; set; } = null;

        public Configuration() : base() { }
    }
}
