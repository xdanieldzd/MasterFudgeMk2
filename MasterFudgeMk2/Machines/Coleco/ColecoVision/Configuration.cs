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
        [XmlElement]
        public Enum Up { get; set; } = null;
        [XmlElement]
        public Enum Down { get; set; } = null;
        [XmlElement]
        public Enum Left { get; set; } = null;
        [XmlElement]
        public Enum Right { get; set; } = null;
        [XmlElement]
        public Enum LeftButton { get; set; } = null;
        [XmlElement]
        public Enum RightButton { get; set; } = null;
        [XmlElement]
        public Enum Keypad1 { get; set; } = null;
        [XmlElement]
        public Enum Keypad2 { get; set; } = null;
        [XmlElement]
        public Enum Keypad3 { get; set; } = null;
        [XmlElement]
        public Enum Keypad4 { get; set; } = null;
        [XmlElement]
        public Enum Keypad5 { get; set; } = null;
        [XmlElement]
        public Enum Keypad6 { get; set; } = null;
        [XmlElement]
        public Enum Keypad7 { get; set; } = null;
        [XmlElement]
        public Enum Keypad8 { get; set; } = null;
        [XmlElement]
        public Enum Keypad9 { get; set; } = null;
        [XmlElement]
        public Enum Keypad0 { get; set; } = null;
        [XmlElement]
        public Enum KeypadStar { get; set; } = null;
        [XmlElement]
        public Enum KeypadNumberSign { get; set; } = null;

        public Configuration() : base() { }
    }
}
