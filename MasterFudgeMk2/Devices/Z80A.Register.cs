﻿using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MasterFudgeMk2.Devices
{
    public partial class Z80A
    {
        [DebuggerDisplay("{Word}")]
        [StructLayout(LayoutKind.Explicit)]
        public struct Register
        {
            [FieldOffset(0)]
            public byte Low;
            [FieldOffset(1)]
            public byte High;
            [FieldOffset(0)]
            public ushort Word;
        }
    }
}
