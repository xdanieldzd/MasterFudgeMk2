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
        [XmlIgnore]
        public Enum Pause { get; set; } = null;
        [XmlIgnore]
        public Enum Reset { get; set; } = null;

        [XmlElement(ElementName = nameof(Pause)), ReadOnly(true)]
        public string PauseString { get { return Pause?.GetFullyQualifiedName(); } set { Pause = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Reset)), ReadOnly(true)]
        public string ResetString { get { return Reset?.GetFullyQualifiedName(); } set { Reset = value?.GetEnumFromFullyQualifiedName(); } }

        /* Inputs (P1) */
        [XmlIgnore]
        public Enum P1Up { get; set; } = null;
        [XmlIgnore]
        public Enum P1Down { get; set; } = null;
        [XmlIgnore]
        public Enum P1Left { get; set; } = null;
        [XmlIgnore]
        public Enum P1Right { get; set; } = null;
        [XmlIgnore]
        public Enum P1Button1 { get; set; } = null;
        [XmlIgnore]
        public Enum P1Button2 { get; set; } = null;

        [XmlElement(ElementName = nameof(P1Up)), ReadOnly(true)]
        public string P1UpString { get { return P1Up?.GetFullyQualifiedName(); } set { P1Up = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Down)), ReadOnly(true)]
        public string P1DownString { get { return P1Down?.GetFullyQualifiedName(); } set { P1Down = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Left)), ReadOnly(true)]
        public string P1LeftString { get { return P1Left?.GetFullyQualifiedName(); } set { P1Left = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Right)), ReadOnly(true)]
        public string P1RightString { get { return P1Right?.GetFullyQualifiedName(); } set { P1Right = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Button1)), ReadOnly(true)]
        public string P1Button1String { get { return P1Button1?.GetFullyQualifiedName(); } set { P1Button1 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Button2)), ReadOnly(true)]
        public string P1Button2String { get { return P1Button2?.GetFullyQualifiedName(); } set { P1Button2 = value?.GetEnumFromFullyQualifiedName(); } }

        /* Inputs (P2) */
        [XmlIgnore]
        public Enum P2Up { get; set; } = null;
        [XmlIgnore]
        public Enum P2Down { get; set; } = null;
        [XmlIgnore]
        public Enum P2Left { get; set; } = null;
        [XmlIgnore]
        public Enum P2Right { get; set; } = null;
        [XmlIgnore]
        public Enum P2Button1 { get; set; } = null;
        [XmlIgnore]
        public Enum P2Button2 { get; set; } = null;

        [XmlElement(ElementName = nameof(P2Up)), ReadOnly(true)]
        public string P2UpString { get { return P2Up?.GetFullyQualifiedName(); } set { P2Up = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P2Down)), ReadOnly(true)]
        public string P2DownString { get { return P2Down?.GetFullyQualifiedName(); } set { P2Down = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P2Left)), ReadOnly(true)]
        public string P2LeftString { get { return P2Left?.GetFullyQualifiedName(); } set { P2Left = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P2Right)), ReadOnly(true)]
        public string P2RightString { get { return P2Right?.GetFullyQualifiedName(); } set { P2Right = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P2Button1)), ReadOnly(true)]
        public string P2Button1String { get { return P2Button1?.GetFullyQualifiedName(); } set { P2Button1 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P2Button2)), ReadOnly(true)]
        public string P2Button2String { get { return P2Button2?.GetFullyQualifiedName(); } set { P2Button2 = value?.GetEnumFromFullyQualifiedName(); } }

        public Configuration() : base() { }
    }
}
