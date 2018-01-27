using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MasterFudgeMk2.Machines.Sega.SG1000
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Name { get { return "SG1000.xml"; } }

        [XmlIgnore]
        public Enum Pause { get; set; } = null;

        [XmlElement(ElementName = nameof(Pause)), ReadOnly(true)]
        public string PauseString { get { return Pause?.GetFullyQualifiedName(); } set { Pause = value?.GetEnumFromFullyQualifiedName(); } }

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

        public Configuration() : base() { }
    }
}
