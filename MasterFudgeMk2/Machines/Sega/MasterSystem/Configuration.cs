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

namespace MasterFudgeMk2.Machines.Sega.MasterSystem
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Filename { get { return "MasterSystem.xml"; } }

        /* Settings */
        public string BootstrapPath
        {
            get { return SettingsConfig.GetString(nameof(BootstrapPath), string.Empty); }
            set { SettingsConfig.Set(nameof(BootstrapPath), value); }
        }

        public bool UseBootstrap
        {
            get { return SettingsConfig.GetBoolean(nameof(UseBootstrap), false); }
            set { SettingsConfig.Set(nameof(UseBootstrap), value); }
        }

        public bool IsPalSystem
        {
            get { return SettingsConfig.GetBoolean(nameof(IsPalSystem), true); }
            set { SettingsConfig.Set(nameof(IsPalSystem), value); }
        }

        public bool IsExportSystem
        {
            get { return SettingsConfig.GetBoolean(nameof(IsExportSystem), true); }
            set { SettingsConfig.Set(nameof(IsExportSystem), value); }
        }

        /* Inputs (General) */
        public Enum Pause
        {
            get { return InputConfig.GetString(MachineInputs.Pause.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Pause.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Reset
        {
            get { return InputConfig.GetString(MachineInputs.Reset.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Reset.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        /* Inputs (P1) */
        public Enum P1Up
        {
            get { return InputConfig.GetString(MachineInputs.P1Up.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Up.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P1Down
        {
            get { return InputConfig.GetString(MachineInputs.P1Down.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Down.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P1Left
        {
            get { return InputConfig.GetString(MachineInputs.P1Left.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Left.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P1Right
        {
            get { return InputConfig.GetString(MachineInputs.P1Right.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Right.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P1Button1
        {
            get { return InputConfig.GetString(MachineInputs.P1Button1.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Button1.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P1Button2
        {
            get { return InputConfig.GetString(MachineInputs.P1Button2.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Button2.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        /* Inputs (P2) */
        public Enum P2Up
        {
            get { return InputConfig.GetString(MachineInputs.P2Up.GetFullyQualifiedName(), string.Empty).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Up.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Down
        {
            get { return InputConfig.GetString(MachineInputs.P2Down.GetFullyQualifiedName(), string.Empty).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Down.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Left
        {
            get { return InputConfig.GetString(MachineInputs.P2Left.GetFullyQualifiedName(), string.Empty).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Left.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Right
        {
            get { return InputConfig.GetString(MachineInputs.P2Right.GetFullyQualifiedName(), string.Empty).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Right.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Button1
        {
            get { return InputConfig.GetString(MachineInputs.P2Button1.GetFullyQualifiedName(), string.Empty).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Button1.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Button2
        {
            get { return InputConfig.GetString(MachineInputs.P2Button2.GetFullyQualifiedName(), string.Empty).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Button2.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Configuration() : base()
        {

        }
    }
}
