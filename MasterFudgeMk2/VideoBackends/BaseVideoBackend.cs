using System.Windows.Forms;
using System.Drawing;

using MasterFudgeMk2.Common;
using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.VideoBackends
{
    public abstract class BaseVideoBackend : MustInitialize<Control>, IVideoBackend
    {
        protected Control outputControl;

        protected bool forceSquarePixels, linearInterpolation;
        protected float aspectRatio;
        protected Rectangle screenViewport;

        public virtual bool ForceSquarePixels { get { return forceSquarePixels; } set { forceSquarePixels = value; } }
        public virtual bool LinearInterpolation { get { return linearInterpolation; } set { linearInterpolation = value; } }
        public virtual float AspectRatio { get { return aspectRatio; } set { aspectRatio = value; } }

        public BaseVideoBackend(Control control)
        {
            outputControl = control;
        }

        public abstract void Dispose();

        public abstract Bitmap GetRawScreenshot();

        public abstract void OnOutputResized(object sender, OutputResizedEventArgs e);
        public abstract void OnRenderScreen(object sender, RenderScreenEventArgs e);
        public abstract void OnScreenViewportChange(object sender, ScreenViewportChangeEventArgs e);
    }
}
