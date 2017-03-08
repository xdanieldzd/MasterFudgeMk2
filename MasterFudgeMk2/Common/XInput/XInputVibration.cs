using System.Runtime.InteropServices;

namespace MasterFudgeMk2.Common.XInput
{
    /* https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.reference.xinput_vibration%28v=vs.85%29.aspx */
    [StructLayout(LayoutKind.Explicit)]
    public struct XInputVibration
    {
        [FieldOffset(0)]
        public ushort wLeftMotorSpeed;
        [FieldOffset(2)]
        public ushort wRightMotorSpeed;
    }
}
