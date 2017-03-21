using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.Common.VideoBackend
{
    [Description("Null Video")]
    public class NullVideoBackend : BaseVideoBackend
    {
        public NullVideoBackend(Control control) : base(control) { }

        public override void Dispose() { }

        public override Bitmap GetRawScreenshot()
        {
            return new Bitmap(screenViewport.Width, screenViewport.Height);
        }

        public override void OnOutputResized(object sender, OutputResizedEventArgs e) { }
        public override void OnRenderScreen(object sender, RenderScreenEventArgs e)
        {
            using (Graphics g = outputControl.CreateGraphics())
            {
                g.Clear(Color.Black);
            }
        }
    }
}
