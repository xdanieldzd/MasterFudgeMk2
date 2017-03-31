using System;
using System.ComponentModel;

namespace MasterFudgeMk2.Machines.Various.MSX2
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Filename { get { return "MSX2.xml"; } }

        /* Settings */
        [Description("BIOS/BASIC Path")]
        public string BiosPath
        {
            get { return SettingsConfig.GetString(nameof(BiosPath), string.Empty); }
            set { SettingsConfig.Set(nameof(BiosPath), value); }
        }

        [Description("SUB ROM Path")]
        public string SubRomPath
        {
            get { return SettingsConfig.GetString(nameof(SubRomPath), string.Empty); }
            set { SettingsConfig.Set(nameof(SubRomPath), value); }
        }

        [Description("DISK ROM Path")]
        public string DiskRomPath
        {
            get { return SettingsConfig.GetString(nameof(DiskRomPath), string.Empty); }
            set { SettingsConfig.Set(nameof(DiskRomPath), value); }
        }

        [Description("Internal RAM")]
        public InternalRamSizes InternalRam
        {
            get { return (InternalRamSizes)(SettingsConfig.GetString(nameof(InternalRam)).GetEnumFromFullyQualifiedName() ?? InternalRamSizes.Int64Kilobyte); }
            set { SettingsConfig.Set(nameof(InternalRam), value.GetFullyQualifiedName()); }
        }

        /* Joysticks */
        public Enum J1Up
        {
            get { return InputConfig.GetString(MachineInputs.J1Up.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.J1Up.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum J1Down
        {
            get { return InputConfig.GetString(MachineInputs.J1Down.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.J1Down.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum J1Left
        {
            get { return InputConfig.GetString(MachineInputs.J1Left.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.J1Left.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum J1Right
        {
            get { return InputConfig.GetString(MachineInputs.J1Right.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.J1Right.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum J1TriggerA
        {
            get { return InputConfig.GetString(MachineInputs.J1TriggerA.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.J1TriggerA.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum J1TriggerB
        {
            get { return InputConfig.GetString(MachineInputs.J1TriggerB.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.J1TriggerB.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        /* Keyboard */
        public Enum D0
        {
            get { return InputConfig.GetString(MachineInputs.D0.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D0.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D1
        {
            get { return InputConfig.GetString(MachineInputs.D1.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D1.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D2
        {
            get { return InputConfig.GetString(MachineInputs.D2.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D2.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D3
        {
            get { return InputConfig.GetString(MachineInputs.D3.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D3.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D4
        {
            get { return InputConfig.GetString(MachineInputs.D4.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D4.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D5
        {
            get { return InputConfig.GetString(MachineInputs.D5.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D5.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D6
        {
            get { return InputConfig.GetString(MachineInputs.D6.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D6.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D7
        {
            get { return InputConfig.GetString(MachineInputs.D7.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D7.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D8
        {
            get { return InputConfig.GetString(MachineInputs.D8.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D8.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D9
        {
            get { return InputConfig.GetString(MachineInputs.D9.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D9.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Minus
        {
            get { return InputConfig.GetString(MachineInputs.Minus.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Minus.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum EqualSign
        {
            get { return InputConfig.GetString(MachineInputs.EqualSign.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.EqualSign.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Backslash
        {
            get { return InputConfig.GetString(MachineInputs.Backslash.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Backslash.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum BracketOpen
        {
            get { return InputConfig.GetString(MachineInputs.BracketOpen.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.BracketOpen.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum BracketClose
        {
            get { return InputConfig.GetString(MachineInputs.BracketClose.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.BracketClose.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Semicolon
        {
            get { return InputConfig.GetString(MachineInputs.Semicolon.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Semicolon.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Grave
        {
            get { return InputConfig.GetString(MachineInputs.Grave.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Grave.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Apostrophe
        {
            get { return InputConfig.GetString(MachineInputs.Apostrophe.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Apostrophe.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Comma
        {
            get { return InputConfig.GetString(MachineInputs.Comma.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Comma.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Period
        {
            get { return InputConfig.GetString(MachineInputs.Period.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Period.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Slash
        {
            get { return InputConfig.GetString(MachineInputs.Slash.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Slash.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum DeadKey
        {
            get { return InputConfig.GetString(MachineInputs.DeadKey.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.DeadKey.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum A
        {
            get { return InputConfig.GetString(MachineInputs.A.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.A.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum B
        {
            get { return InputConfig.GetString(MachineInputs.B.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.B.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum C
        {
            get { return InputConfig.GetString(MachineInputs.C.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.C.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D
        {
            get { return InputConfig.GetString(MachineInputs.D.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum E
        {
            get { return InputConfig.GetString(MachineInputs.E.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.E.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum F
        {
            get { return InputConfig.GetString(MachineInputs.F.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.F.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum G
        {
            get { return InputConfig.GetString(MachineInputs.G.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.G.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum H
        {
            get { return InputConfig.GetString(MachineInputs.H.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.H.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum I
        {
            get { return InputConfig.GetString(MachineInputs.I.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.I.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum J
        {
            get { return InputConfig.GetString(MachineInputs.J.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.J.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum K
        {
            get { return InputConfig.GetString(MachineInputs.K.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.K.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum L
        {
            get { return InputConfig.GetString(MachineInputs.L.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.L.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum M
        {
            get { return InputConfig.GetString(MachineInputs.M.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.M.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum N
        {
            get { return InputConfig.GetString(MachineInputs.N.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.N.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum O
        {
            get { return InputConfig.GetString(MachineInputs.O.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.O.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P
        {
            get { return InputConfig.GetString(MachineInputs.P.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Q
        {
            get { return InputConfig.GetString(MachineInputs.Q.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Q.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum R
        {
            get { return InputConfig.GetString(MachineInputs.R.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.R.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum S
        {
            get { return InputConfig.GetString(MachineInputs.S.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.S.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum T
        {
            get { return InputConfig.GetString(MachineInputs.T.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.T.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum U
        {
            get { return InputConfig.GetString(MachineInputs.U.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.U.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum V
        {
            get { return InputConfig.GetString(MachineInputs.V.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.V.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum W
        {
            get { return InputConfig.GetString(MachineInputs.W.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.W.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum X
        {
            get { return InputConfig.GetString(MachineInputs.X.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.X.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Y
        {
            get { return InputConfig.GetString(MachineInputs.Y.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Y.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Z
        {
            get { return InputConfig.GetString(MachineInputs.Z.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Z.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Shift
        {
            get { return InputConfig.GetString(MachineInputs.Shift.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Shift.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Ctrl
        {
            get { return InputConfig.GetString(MachineInputs.Ctrl.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Ctrl.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Graph
        {
            get { return InputConfig.GetString(MachineInputs.Graph.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Graph.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Cap
        {
            get { return InputConfig.GetString(MachineInputs.Cap.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Cap.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Code
        {
            get { return InputConfig.GetString(MachineInputs.Code.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Code.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum F1
        {
            get { return InputConfig.GetString(MachineInputs.F1.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.F1.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum F2
        {
            get { return InputConfig.GetString(MachineInputs.F2.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.F2.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum F3
        {
            get { return InputConfig.GetString(MachineInputs.F3.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.F3.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum F4
        {
            get { return InputConfig.GetString(MachineInputs.F4.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.F4.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum F5
        {
            get { return InputConfig.GetString(MachineInputs.F5.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.F5.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Esc
        {
            get { return InputConfig.GetString(MachineInputs.Esc.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Esc.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Tab
        {
            get { return InputConfig.GetString(MachineInputs.Tab.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Tab.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Stop
        {
            get { return InputConfig.GetString(MachineInputs.Stop.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Stop.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum BS
        {
            get { return InputConfig.GetString(MachineInputs.BS.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.BS.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Select
        {
            get { return InputConfig.GetString(MachineInputs.Select.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Select.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Return
        {
            get { return InputConfig.GetString(MachineInputs.Return.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Return.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Space
        {
            get { return InputConfig.GetString(MachineInputs.Space.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Space.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Home
        {
            get { return InputConfig.GetString(MachineInputs.Home.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Home.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Ins
        {
            get { return InputConfig.GetString(MachineInputs.Ins.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Ins.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Del
        {
            get { return InputConfig.GetString(MachineInputs.Del.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Del.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Left
        {
            get { return InputConfig.GetString(MachineInputs.Left.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Left.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
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

        public Enum Right
        {
            get { return InputConfig.GetString(MachineInputs.Right.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Right.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum NumMultiply
        {
            get { return InputConfig.GetString(MachineInputs.NumMultiply.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.NumMultiply.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum NumPlus
        {
            get { return InputConfig.GetString(MachineInputs.NumPlus.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.NumPlus.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum NumDivide
        {
            get { return InputConfig.GetString(MachineInputs.NumDivide.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.NumDivide.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num0
        {
            get { return InputConfig.GetString(MachineInputs.Num0.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num0.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num1
        {
            get { return InputConfig.GetString(MachineInputs.Num1.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num1.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num2
        {
            get { return InputConfig.GetString(MachineInputs.Num2.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num2.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num3
        {
            get { return InputConfig.GetString(MachineInputs.Num3.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num3.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num4
        {
            get { return InputConfig.GetString(MachineInputs.Num4.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num4.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num5
        {
            get { return InputConfig.GetString(MachineInputs.Num5.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num5.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num6
        {
            get { return InputConfig.GetString(MachineInputs.Num6.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num6.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num7
        {
            get { return InputConfig.GetString(MachineInputs.Num7.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num7.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num8
        {
            get { return InputConfig.GetString(MachineInputs.Num8.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num8.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Num9
        {
            get { return InputConfig.GetString(MachineInputs.Num9.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Num9.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum NumMinus
        {
            get { return InputConfig.GetString(MachineInputs.NumMinus.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.NumMinus.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum NumComma
        {
            get { return InputConfig.GetString(MachineInputs.NumComma.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.NumComma.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum NumPeriod
        {
            get { return InputConfig.GetString(MachineInputs.NumPeriod.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.NumPeriod.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Configuration() : base() { }
    }
}
