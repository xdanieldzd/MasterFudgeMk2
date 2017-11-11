using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MasterFudgeMk2.Machines.Sega.GameGear
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Name { get { return "GameGear.xml"; } }

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

        /* Inputs */
        [XmlElement]
        public Enum Start { get; set; } = null;
        [XmlElement]
        public Enum Up { get; set; } = null;
        [XmlElement]
        public Enum Down { get; set; } = null;
        [XmlElement]
        public Enum Left { get; set; } = null;
        [XmlElement]
        public Enum Right { get; set; } = null;
        [XmlElement]
        public Enum Button1 { get; set; } = null;
        [XmlElement]
        public Enum Button2 { get; set; } = null;

        public Configuration() : base() { }
    }
}
