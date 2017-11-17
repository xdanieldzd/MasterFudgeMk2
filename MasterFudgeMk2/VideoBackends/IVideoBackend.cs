using System;
using System.Drawing;

using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.VideoBackends
{
    public interface IVideoBackend : IDisposable
    {
        bool ForceSquarePixels { get; set; }
        bool LinearInterpolation { get; set; }
        float AspectRatio { get; set; }

        Bitmap GetRawScreenshot();

        void OnOutputResized(object sender, OutputResizedEventArgs e);
        void OnRenderScreen(object sender, RenderScreenEventArgs e);
        void OnScreenViewportChange(object sender, ScreenViewportChangeEventArgs e);
    }
}
