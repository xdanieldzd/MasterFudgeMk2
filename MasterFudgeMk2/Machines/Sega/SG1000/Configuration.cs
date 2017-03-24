using System;
using System.ComponentModel;

namespace MasterFudgeMk2.Machines.Sega.SG1000
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Filename { get { return "SG1000.xml"; } }

        public Enum Pause
        {
            get { return InputConfig.GetString(MachineInputs.Pause.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Pause.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

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

        public Configuration() : base() { }
    }
}
