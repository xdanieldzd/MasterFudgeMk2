using System;

namespace MasterFudgeMk2.Common.VideoBackend
{
    public class OutputResizedEventArgs : EventArgs
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public OutputResizedEventArgs(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
