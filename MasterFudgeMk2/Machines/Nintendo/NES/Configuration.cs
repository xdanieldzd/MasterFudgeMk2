using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Machines.Nintendo.NES
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Filename { get { return "GameGear.xml"; } }

        /* Inputs */
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

        public Enum B
        {
            get { return InputConfig.GetString(MachineInputs.B.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.B.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum A
        {
            get { return InputConfig.GetString(MachineInputs.A.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.A.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Select
        {
            get { return InputConfig.GetString(MachineInputs.Select.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Select.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Start
        {
            get { return InputConfig.GetString(MachineInputs.Start.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Start.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Configuration() : base() { }
    }
}
