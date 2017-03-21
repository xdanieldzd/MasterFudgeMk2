using System;
using System.Drawing;

using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.Common.VideoBackend
{
    interface IVideoBackend : IDisposable
    {
        bool KeepAspectRatio { get; set; }
        bool ForceSquarePixels { get; set; }
        float AspectRatio { get; set; }
        Rectangle ScreenViewport { get; set; }

        Bitmap GetRawScreenshot();

        void OnOutputResized(object sender, OutputResizedEventArgs e);
        void OnRenderScreen(object sender, RenderScreenEventArgs e);
    }
}
