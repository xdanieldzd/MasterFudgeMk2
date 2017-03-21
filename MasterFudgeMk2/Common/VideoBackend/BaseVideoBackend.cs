using System.Windows.Forms;
using System.Drawing;

using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.Common.VideoBackend
{
    public abstract class BaseVideoBackend : IVideoBackend
    {
        protected Control outputControl;

        protected bool keepAspectRatio, forceSquarePixels;
        protected float aspectRatio;
        protected Rectangle screenViewport;

        public virtual bool KeepAspectRatio { get { return keepAspectRatio; } set { keepAspectRatio = value; } }
        public virtual bool ForceSquarePixels { get { return forceSquarePixels; } set { forceSquarePixels = value; } }
        public virtual float AspectRatio { get { return aspectRatio; } set { aspectRatio = value; } }
        public virtual Rectangle ScreenViewport { get { return screenViewport; } set { screenViewport = value; } }

        public BaseVideoBackend(Control control)
        {
            outputControl = control;
        }

        public abstract void Dispose();

        public abstract Bitmap GetRawScreenshot();

        public abstract void OnOutputResized(object sender, OutputResizedEventArgs e);
        public abstract void OnRenderScreen(object sender, RenderScreenEventArgs e);
    }
}
