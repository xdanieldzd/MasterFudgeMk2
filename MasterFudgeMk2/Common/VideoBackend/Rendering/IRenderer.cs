using System;
using System.Drawing;

namespace MasterFudgeMk2.Common.VideoBackend.Rendering
{
    interface IRenderer : IDisposable
    {
        bool KeepAspectRatio { get; set; }
        bool ForceSquarePixels { get; set; }
        float AspectRatio { get; set; }

        void OnOutputResized(object sender, OutputResizedEventArgs e);
        void OnRenderScreen(object sender, RenderScreenEventArgs e);
        void OnScreenViewportChange(object sender, ScreenViewportChangeEventArgs e);
    }
}
