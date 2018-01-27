using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MasterFudgeMk2.Machines.Coleco.ColecoVision
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Name { get { return "ColecoVision.xml"; } }

        /* Settings */
        [Description("BIOS Path")]
        [XmlElement]
        public string BiosPath { get; set; } = string.Empty;

        /* Joystick input */
        [XmlIgnore]
        public Enum Up { get; set; } = null;
        [XmlIgnore]
        public Enum Down { get; set; } = null;
        [XmlIgnore]
        public Enum Left { get; set; } = null;
        [XmlIgnore]
        public Enum Right { get; set; } = null;
        [XmlIgnore]
        public Enum LeftButton { get; set; } = null;
        [XmlIgnore]
        public Enum RightButton { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad1 { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad2 { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad3 { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad4 { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad5 { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad6 { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad7 { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad8 { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad9 { get; set; } = null;
        [XmlIgnore]
        public Enum Keypad0 { get; set; } = null;
        [XmlIgnore]
        public Enum KeypadStar { get; set; } = null;
        [XmlIgnore]
        public Enum KeypadNumberSign { get; set; } = null;

        [XmlElement(ElementName = nameof(Up)), ReadOnly(true)]
        public string UpString { get { return Up?.GetFullyQualifiedName(); } set { Up = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Down)), ReadOnly(true)]
        public string DownString { get { return Down?.GetFullyQualifiedName(); } set { Down = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Left)), ReadOnly(true)]
        public string LeftString { get { return Left?.GetFullyQualifiedName(); } set { Left = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Right)), ReadOnly(true)]
        public string RightString { get { return Right?.GetFullyQualifiedName(); } set { Right = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(LeftButton)), ReadOnly(true)]
        public string LeftButtonString { get { return LeftButton?.GetFullyQualifiedName(); } set { LeftButton = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(RightButton)), ReadOnly(true)]
        public string RightButtonString { get { return RightButton?.GetFullyQualifiedName(); } set { RightButton = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad1)), ReadOnly(true)]
        public string Keypad1String { get { return Keypad1?.GetFullyQualifiedName(); } set { Keypad1 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad2)), ReadOnly(true)]
        public string Keypad2String { get { return Keypad2?.GetFullyQualifiedName(); } set { Keypad2 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad3)), ReadOnly(true)]
        public string Keypad3String { get { return Keypad3?.GetFullyQualifiedName(); } set { Keypad3 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad4)), ReadOnly(true)]
        public string Keypad4String { get { return Keypad4?.GetFullyQualifiedName(); } set { Keypad4 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad5)), ReadOnly(true)]
        public string Keypad5String { get { return Keypad5?.GetFullyQualifiedName(); } set { Keypad5 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad6)), ReadOnly(true)]
        public string Keypad6String { get { return Keypad6?.GetFullyQualifiedName(); } set { Keypad6 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad7)), ReadOnly(true)]
        public string Keypad7String { get { return Keypad7?.GetFullyQualifiedName(); } set { Keypad7 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad8)), ReadOnly(true)]
        public string Keypad8String { get { return Keypad8?.GetFullyQualifiedName(); } set { Keypad8 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad9)), ReadOnly(true)]
        public string Keypad9String { get { return Keypad9?.GetFullyQualifiedName(); } set { Keypad9 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(Keypad0)), ReadOnly(true)]
        public string Keypad0String { get { return Keypad0?.GetFullyQualifiedName(); } set { Keypad0 = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(KeypadStar)), ReadOnly(true)]
        public string KeypadStarString { get { return KeypadStar?.GetFullyQualifiedName(); } set { KeypadStar = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(KeypadNumberSign)), ReadOnly(true)]
        public string KeypadNumberSignString { get { return KeypadNumberSign?.GetFullyQualifiedName(); } set { KeypadNumberSign = value?.GetEnumFromFullyQualifiedName(); } }

        public Configuration() : base() { }
    }
}
