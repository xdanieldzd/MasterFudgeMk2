using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace MasterFudgeMk2.Common
{
    internal class NativeMethods
    {
        /* http://gamedev.stackexchange.com/a/67652 */
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr Handle;
            public uint Message;
            public IntPtr WParameter;
            public IntPtr LParameter;
            public uint Time;
            public Point Location;
        }

        [DllImport("user32.dll")]
        private static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

        public static bool IsApplicationIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, 0, 0, 0) == 0;
        }
    }
}
