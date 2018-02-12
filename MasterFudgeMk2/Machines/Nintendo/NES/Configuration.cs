﻿using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MasterFudgeMk2.Machines.Nintendo.NES
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Name { get { return "NES.xml"; } }

        /* Settings */
        [Description("Emulate PAL System")]
        [XmlElement]
        public bool IsPalSystem { get; set; } = false;
        
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
        public Enum P1Start { get; set; } = null;
        [XmlIgnore]
        public Enum P1Select { get; set; } = null;
        [XmlIgnore]
        public Enum P1B { get; set; } = null;
        [XmlIgnore]
        public Enum P1A { get; set; } = null;

        [XmlElement(ElementName = nameof(P1Up)), ReadOnly(true)]
        public string P1UpString { get { return P1Up?.GetFullyQualifiedName(); } set { P1Up = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Down)), ReadOnly(true)]
        public string P1DownString { get { return P1Down?.GetFullyQualifiedName(); } set { P1Down = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Left)), ReadOnly(true)]
        public string P1LeftString { get { return P1Left?.GetFullyQualifiedName(); } set { P1Left = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Right)), ReadOnly(true)]
        public string P1RightString { get { return P1Right?.GetFullyQualifiedName(); } set { P1Right = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Start)), ReadOnly(true)]
        public string P1StartString { get { return P1Start?.GetFullyQualifiedName(); } set { P1Start = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1Select)), ReadOnly(true)]
        public string P1SelectString { get { return P1Select?.GetFullyQualifiedName(); } set { P1Select = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1B)), ReadOnly(true)]
        public string P1BString { get { return P1B?.GetFullyQualifiedName(); } set { P1B = value?.GetEnumFromFullyQualifiedName(); } }
        [XmlElement(ElementName = nameof(P1A)), ReadOnly(true)]
        public string P1AString { get { return P1A?.GetFullyQualifiedName(); } set { P1A = value?.GetEnumFromFullyQualifiedName(); } }

        public Configuration() : base() { }
    }
}
