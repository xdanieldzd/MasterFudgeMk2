using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

using Nini.Config;

using MasterFudgeMk2.Common.XInput;

namespace MasterFudgeMk2.Machines.Coleco.ColecoVision
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Filename { get { return "ColecoVision.xml"; } }

        /* Settings */
        [Description("BIOS Path")]
        public string BiosPath
        {
            get { return SettingsConfig.GetString(nameof(BiosPath), string.Empty); }
            set { SettingsConfig.Set(nameof(BiosPath), value); }
        }

        /* Joystick input */
        public Enum Up
        {
            get { return InputConfig.GetString(MachineInputs.Up.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Up.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Down
        {
            get { return InputConfig.GetString(MachineInputs.Down.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Down.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Left
        {
            get { return InputConfig.GetString(MachineInputs.Left.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Left.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Right
        {
            get { return InputConfig.GetString(MachineInputs.Right.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Right.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum LeftButton
        {
            get { return InputConfig.GetString(MachineInputs.LeftButton.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.LeftButton.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum RightButton
        {
            get { return InputConfig.GetString(MachineInputs.RightButton.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.RightButton.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad1
        {
            get { return InputConfig.GetString(MachineInputs.Keypad1.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad1.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad2
        {
            get { return InputConfig.GetString(MachineInputs.Keypad2.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad2.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad3
        {
            get { return InputConfig.GetString(MachineInputs.Keypad3.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad3.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad4
        {
            get { return InputConfig.GetString(MachineInputs.Keypad4.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad4.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad5
        {
            get { return InputConfig.GetString(MachineInputs.Keypad5.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad5.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad6
        {
            get { return InputConfig.GetString(MachineInputs.Keypad6.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad6.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad7
        {
            get { return InputConfig.GetString(MachineInputs.Keypad7.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad7.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad8
        {
            get { return InputConfig.GetString(MachineInputs.Keypad8.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad8.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad9
        {
            get { return InputConfig.GetString(MachineInputs.Keypad9.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad9.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Keypad0
        {
            get { return InputConfig.GetString(MachineInputs.Keypad0.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Keypad0.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum KeypadStar
        {
            get { return InputConfig.GetString(MachineInputs.KeypadStar.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.KeypadStar.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum KeypadNumberSign
        {
            get { return InputConfig.GetString(MachineInputs.KeypadNumberSign.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.KeypadNumberSign.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Configuration() : base() { }
    }
}
