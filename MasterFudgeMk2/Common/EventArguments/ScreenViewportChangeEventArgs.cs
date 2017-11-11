using System;

namespace MasterFudgeMk2.Common.EventArguments
{
    public class ScreenViewportChangeEventArgs : EventArgs
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public ScreenViewportChangeEventArgs(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
