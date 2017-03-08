using System;

namespace MasterFudgeMk2.Common.VideoBackend
{
    public class RenderScreenEventArgs : EventArgs
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] FrameData { get; private set; }

        public RenderScreenEventArgs(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            FrameData = data;
        }
    }
}
