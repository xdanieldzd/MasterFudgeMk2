using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MasterFudgeMk2.Machines.NEC.PCEngine
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Name { get { return "PCEngine.xml"; } }

        /* Settings */
        [Description("Emulate Export System")]
        [XmlElement]
        public bool IsExportSystem { get; set; } = true;

        /* Inputs */
        [XmlIgnore]
        public Enum Up { get; set; } = null;
        [XmlIgnore]
        public Enum Down { get; set; } = null;
        [XmlIgnore]
        public Enum Left { get; set; } = null;
        [XmlIgnore]
        public Enum Right { get; set; } = null;
        [XmlIgnore]
        public Enum Button1 { get; set; } = null;
        [XmlIgnore]
        public Enum Button2 { get; set; } = null;
        [XmlIgnore]
        public Enum Run { get; set; } = null;
        [XmlIgnore]
        public Enum Select { get; set; } = null;

        [XmlElement(ElementName = nameof(Up)), ReadOnly(true)]
        public string UpString { get { return Up?.GetFullyQualifiedName(); } set { Up = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Down)), ReadOnly(true)]
        public string DownString { get { return Down?.GetFullyQualifiedName(); } set { Down = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Left)), ReadOnly(true)]
        public string LeftString { get { return Left?.GetFullyQualifiedName(); } set { Left = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Right)), ReadOnly(true)]
        public string RightString { get { return Right?.GetFullyQualifiedName(); } set { Right = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Button1)), ReadOnly(true)]
        public string Button1String { get { return Button1?.GetFullyQualifiedName(); } set { Button1 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Button2)), ReadOnly(true)]
        public string Button2String { get { return Button2?.GetFullyQualifiedName(); } set { Button2 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Run)), ReadOnly(true)]
        public string RunString { get { return Run?.GetFullyQualifiedName(); } set { Run = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Select)), ReadOnly(true)]
        public string SelectString { get { return Select?.GetFullyQualifiedName(); } set { Select = value?.GetEnumFromFullyQualifiedName(); } }

        public Configuration() : base() { }
    }
}
