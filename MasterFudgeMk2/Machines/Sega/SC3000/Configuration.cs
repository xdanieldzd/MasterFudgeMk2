using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MasterFudgeMk2.Machines.Sega.SC3000
{
    public sealed class Configuration : MachineConfiguration
    {
        public override sealed string Filename { get { return "SC3000.xml"; } }

        [Description("Emulate PAL System")]
        public bool IsPalSystem
        {
            get { return SettingsConfig.GetBoolean(nameof(IsPalSystem), false); }
            set { SettingsConfig.Set(nameof(IsPalSystem), value); }
        }

        public Enum Reset
        {
            get { return InputConfig.GetString(MachineInputs.Reset.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Reset.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
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

        public Enum P1Up
        {
            get { return InputConfig.GetString(MachineInputs.P1Up.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Up.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Q
        {
            get { return InputConfig.GetString(MachineInputs.Q.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Q.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum W
        {
            get { return InputConfig.GetString(MachineInputs.W.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.W.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum E
        {
            get { return InputConfig.GetString(MachineInputs.E.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.E.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum R
        {
            get { return InputConfig.GetString(MachineInputs.R.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.R.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum T
        {
            get { return InputConfig.GetString(MachineInputs.T.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.T.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Y
        {
            get { return InputConfig.GetString(MachineInputs.Y.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Y.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum U
        {
            get { return InputConfig.GetString(MachineInputs.U.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.U.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P1Down
        {
            get { return InputConfig.GetString(MachineInputs.P1Down.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Down.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum A
        {
            get { return InputConfig.GetString(MachineInputs.A.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.A.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum S
        {
            get { return InputConfig.GetString(MachineInputs.S.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.S.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum D
        {
            get { return InputConfig.GetString(MachineInputs.D.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
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

        public Enum J
        {
            get { return InputConfig.GetString(MachineInputs.J.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.J.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P1Left
        {
            get { return InputConfig.GetString(MachineInputs.P1Left.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Left.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Z
        {
            get { return InputConfig.GetString(MachineInputs.Z.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Z.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum X
        {
            get { return InputConfig.GetString(MachineInputs.X.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.X.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum C
        {
            get { return InputConfig.GetString(MachineInputs.C.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.C.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum V
        {
            get { return InputConfig.GetString(MachineInputs.V.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.V.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum B
        {
            get { return InputConfig.GetString(MachineInputs.B.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.B.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum N
        {
            get { return InputConfig.GetString(MachineInputs.N.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.N.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum M
        {
            get { return InputConfig.GetString(MachineInputs.M.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.M.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P1Right
        {
            get { return InputConfig.GetString(MachineInputs.P1Right.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Right.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum EngDiers
        {
            get { return InputConfig.GetString(MachineInputs.EngDiers.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.EngDiers.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Space
        {
            get { return InputConfig.GetString(MachineInputs.Space.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Space.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum HomeClr
        {
            get { return InputConfig.GetString(MachineInputs.HomeClr.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.HomeClr.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum InsDel
        {
            get { return InputConfig.GetString(MachineInputs.InsDel.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.InsDel.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped36
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped36.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped36.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped37
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped37.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped37.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped38
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped38.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped38.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P1Button1
        {
            get { return InputConfig.GetString(MachineInputs.P1Button1.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Button1.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
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

        public Enum Pi
        {
            get { return InputConfig.GetString(MachineInputs.Pi.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Pi.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
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

        public Enum P1Button2
        {
            get { return InputConfig.GetString(MachineInputs.P1Button2.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P1Button2.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
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

        public Enum Semicolon
        {
            get { return InputConfig.GetString(MachineInputs.Semicolon.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Semicolon.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Colon
        {
            get { return InputConfig.GetString(MachineInputs.Colon.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Colon.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum BracketClose
        {
            get { return InputConfig.GetString(MachineInputs.BracketClose.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.BracketClose.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum CR
        {
            get { return InputConfig.GetString(MachineInputs.CR.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.CR.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Up
        {
            get { return InputConfig.GetString(MachineInputs.Up.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Up.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Up
        {
            get { return InputConfig.GetString(MachineInputs.P2Up.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Up.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum I
        {
            get { return InputConfig.GetString(MachineInputs.I.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.I.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
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

        public Enum At
        {
            get { return InputConfig.GetString(MachineInputs.At.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.At.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum BracketOpen
        {
            get { return InputConfig.GetString(MachineInputs.BracketOpen.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.BracketOpen.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped61
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped61.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped61.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped62
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped62.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped62.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Down
        {
            get { return InputConfig.GetString(MachineInputs.P2Down.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Down.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
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

        public Enum D0
        {
            get { return InputConfig.GetString(MachineInputs.D0.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.D0.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Minus
        {
            get { return InputConfig.GetString(MachineInputs.Minus.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Minus.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Caret
        {
            get { return InputConfig.GetString(MachineInputs.Caret.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Caret.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Yen
        {
            get { return InputConfig.GetString(MachineInputs.Yen.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Yen.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Break
        {
            get { return InputConfig.GetString(MachineInputs.Break.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Break.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Left
        {
            get { return InputConfig.GetString(MachineInputs.P2Left.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Left.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped72
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped72.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped72.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped73
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped73.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped73.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped74
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped74.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped74.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped75
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped75.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped75.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped76
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped76.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped76.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped77
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped77.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped77.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Graph
        {
            get { return InputConfig.GetString(MachineInputs.Graph.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Graph.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Right
        {
            get { return InputConfig.GetString(MachineInputs.P2Right.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Right.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped80
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped80.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped80.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped81
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped81.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped81.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped82
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped82.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped82.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped83
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped83.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped83.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped84
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped84.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped84.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped85
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped85.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped85.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Ctrl
        {
            get { return InputConfig.GetString(MachineInputs.Ctrl.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Ctrl.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Button1
        {
            get { return InputConfig.GetString(MachineInputs.P2Button1.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Button1.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped88
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped88.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped88.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped89
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped89.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped89.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped90
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped90.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped90.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped91
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped91.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped91.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Unmapped92
        {
            get { return InputConfig.GetString(MachineInputs.Unmapped92.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Unmapped92.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Func
        {
            get { return InputConfig.GetString(MachineInputs.Func.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Func.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum Shift
        {
            get { return InputConfig.GetString(MachineInputs.Shift.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.Shift.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Enum P2Button2
        {
            get { return InputConfig.GetString(MachineInputs.P2Button2.GetFullyQualifiedName()).GetEnumFromFullyQualifiedName(); }
            set { InputConfig.Set(MachineInputs.P2Button2.GetFullyQualifiedName(), value.GetFullyQualifiedName()); }
        }

        public Configuration() : base() { }
    }
}
