using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace MasterFudgeMk2.Common
{
    public class ScreenControl : Control
    {
        // TODO: OpenGL, because: control size == slowdown; the bigger the control, the slower things are b/c of GDI+ performance or lack thereof

        [DefaultValue(false)]
        public bool KeepAspectRatio { get; set; }
        [DefaultValue(typeof(Rectangle), "Empty")]  // doesn't work?
        public Rectangle Viewport { get; set; }
        [DefaultValue(null)]
        public Bitmap OutputBitmap { get; set; }

        public ScreenControl()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

            KeepAspectRatio = false;
            Viewport = Rectangle.Empty;
            OutputBitmap = null;
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (OutputBitmap != null)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

                e.Graphics.DrawImage(
                    OutputBitmap,
                    CalculateDestinationRectangle(ClientRectangle, new Size(Viewport.Width, Viewport.Height), KeepAspectRatio),
                    Viewport,
                    GraphicsUnit.Pixel);

                if (Text != null && Text != string.Empty)
                {
                    e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    e.Graphics.DrawString(Text, SystemFonts.MessageBoxFont, Brushes.Red, 0.0f, 0.0f);
                }
            }
        }

        private Rectangle CalculateDestinationRectangle(Rectangle clientRectangle, Size screenSize, bool keepAspect)
        {
            Rectangle destRect = Rectangle.Empty;

            if (keepAspect)
            {
                int aspectWidth = (clientRectangle.Height * screenSize.Width) / screenSize.Height;
                int aspectHeight = (clientRectangle.Width * screenSize.Height) / screenSize.Width;

                if (aspectHeight > clientRectangle.Height)
                {
                    destRect.Width = aspectWidth;
                    destRect.Height = clientRectangle.Height;
                }
                else
                {
                    destRect.Width = clientRectangle.Width;
                    destRect.Height = aspectHeight;
                }

                destRect.X = (clientRectangle.Width - destRect.Width) / 2;
                destRect.Y = (clientRectangle.Height - destRect.Height) / 2;
            }
            else
            {
                destRect.X = destRect.Y = 0;
                destRect.Width = clientRectangle.Width;
                destRect.Height = clientRectangle.Height;
            }

            return destRect;
        }
    }
}
