using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MasterFudgeMk2.Machines.Sega.GameGear
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Filename { get { return "GameGear.xml"; } }

        /* Settings */
        [Description("Bootstrap Path")]
        public string BootstrapPath
        {
            get { return SettingsConfig.GetString(nameof(BootstrapPath), string.Empty); }
            set { SettingsConfig.Set(nameof(BootstrapPath), value); }
        }

        [Description("Enable Bootstrap")]
        public bool UseBootstrap
        {
            get { return SettingsConfig.GetBoolean(nameof(UseBootstrap), false); }
            set { SettingsConfig.Set(nameof(UseBootstrap), value); }
        }

        [Description("Emulate Export System")]
        public bool IsExportSystem
        {
            get { return SettingsConfig.GetBoolean(nameof(IsExportSystem), true); }
            set { SettingsConfig.Set(nameof(IsExportSystem), value); }
        }

        /* Inputs */
        public Enum Start
        {
            get { return InputConfig.GetString(MachineInputs.Start.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Start.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

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

        public Enum Button1
        {
            get { return InputConfig.GetString(MachineInputs.Button1.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Button1.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Button2
        {
            get { return InputConfig.GetString(MachineInputs.Button2.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Button2.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Configuration() : base() { }
    }
}
