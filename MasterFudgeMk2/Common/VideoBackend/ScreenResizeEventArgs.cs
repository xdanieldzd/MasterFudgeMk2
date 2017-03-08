using System;

namespace MasterFudgeMk2.Common.VideoBackend
{
    public class ScreenResizeEventArgs : EventArgs
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public ScreenResizeEventArgs(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
