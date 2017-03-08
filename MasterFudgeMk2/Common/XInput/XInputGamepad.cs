using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace MasterFudgeMk2.Common.XInput
{
    /* https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.reference.xinput_gamepad%28v=vs.85%29.aspx */
    [StructLayout(LayoutKind.Explicit)]
    public struct XInputGamepad
    {
        [FieldOffset(0)]
        ushort wButtons;
        [FieldOffset(2)]
        public byte bLeftTrigger;
        [FieldOffset(3)]
        public byte bRightTrigger;
        [FieldOffset(4)]
        public short sThumbLX;
        [FieldOffset(6)]
        public short sThumbLY;
        [FieldOffset(8)]
        public short sThumbRX;
        [FieldOffset(10)]
        public short sThumbRY;

        public const int LeftThumbDeadzone = 7849;
        public const int RightThumbDeadzone = 8689;
        public const int TriggerThreshold = 30;

        public Buttons Buttons { get { return (Buttons)wButtons; } }
    }

    [Flags]
    [TypeConverter(typeof(DescriptionTypeConverter))]
    [Description("XInput")]
    public enum Buttons
    {
        [Description("None")]
        None = 0x0000,
        [Description("D-Pad Up")]
        DPadUp = 0x0001,
        [Description("D-Pad Down")]
        DPadDown = 0x0002,
        [Description("D-Pad Left")]
        DPadLeft = 0x0004,
        [Description("D-Pad Right")]
        DPadRight = 0x0008,
        [Description("Start")]
        Start = 0x0010,
        [Description("Back")]
        Back = 0x0020,
        [Description("Left Thumbstick")]
        LeftThumb = 0x0040,
        [Description("Right Thumbstick")]
        RightThumb = 0x0080,
        [Description("Left Shoulder")]
        LeftShoulder = 0x0100,
        [Description("Right Shoulder")]
        RightShoulder = 0x0200,
        [Description("A")]
        A = 0x1000,
        [Description("B")]
        B = 0x2000,
        [Description("X")]
        X = 0x4000,
        [Description("Y")]
        Y = 0x8000
    }
}
