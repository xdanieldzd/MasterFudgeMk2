using System;
using System.Windows.Forms;
using System.ComponentModel;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

using Device = SharpDX.Direct3D11.Device;
using FactoryD2D = SharpDX.Direct2D1.Factory;
using FactoryDXGI = SharpDX.DXGI.Factory1;

using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.Common.VideoBackend
{
    /* https://gist.github.com/axefrog/db3a8ce7b00abb13d2d3
     * https://katyscode.wordpress.com/2013/08/24/c-directx-api-face-off-slimdx-vs-sharpdx-which-should-you-choose/
     * https://github.com/GoldenCrystal/CrystalBoy/blob/master/CrystalBoy.Emulator.Rendering.SharpDX/Direct2DRenderer.cs
     * http://andrew.hedges.name/experiments/aspect_ratio/
     */

    [Description("Direct2D (SharpDX)")]
    public class SharpDXBackend : BaseVideoBackend
    {
        Device device;
        SwapChain swapChain;
        WindowRenderTarget renderTarget;
        BitmapRenderTarget bitmapRenderTarget;

        Bitmap bitmap;
        RawRectangleF destinationRectangle, sourceRectangle;

        public override bool KeepAspectRatio { get { return keepAspectRatio; } set { keepAspectRatio = value; ResizeRenderTargetAndDestinationRectangle(); } }
        public override bool ForceSquarePixels { get { return forceSquarePixels; } set { forceSquarePixels = value; ResizeRenderTargetAndDestinationRectangle(); } }
        public override float AspectRatio { get { return aspectRatio; } set { aspectRatio = value; ResizeRenderTargetAndDestinationRectangle(); } }
        public override System.Drawing.Rectangle ScreenViewport { get { return screenViewport; } set { screenViewport = value; ResizeRenderTargetAndDestinationRectangle(); } }

        public SharpDXBackend(Control control) : base(control)
        {
            var swapChainDesc = new SwapChainDescription()
            {
                BufferCount = 2,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = control.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.B8G8R8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, swapChainDesc, out device, out swapChain);

            using (var factory = new FactoryD2D())
            {
                var dpi = factory.DesktopDpi;
                renderTarget = new WindowRenderTarget(factory, new RenderTargetProperties()
                {
                    DpiX = dpi.Width,
                    DpiY = dpi.Height,
                    MinLevel = SharpDX.Direct2D1.FeatureLevel.Level_DEFAULT,
                    PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore),
                    Type = RenderTargetType.Default,
                    Usage = RenderTargetUsage.None
                },
                new HwndRenderTargetProperties()
                {
                    Hwnd = control.Handle,
                    PixelSize = new Size2(control.ClientSize.Width, control.ClientSize.Height),
                    PresentOptions = PresentOptions.Immediately
                });

                bitmapRenderTarget = new BitmapRenderTarget(renderTarget, CompatibleRenderTargetOptions.None);
            }

            using (var factory = swapChain.GetParent<FactoryDXGI>())
                factory.MakeWindowAssociation(control.Handle, WindowAssociationFlags.IgnoreAltEnter);

            keepAspectRatio = false;
            aspectRatio = 1.0f;

            ResizeRenderTargetAndDestinationRectangle();
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bitmap != null) bitmap.Dispose();
                if (renderTarget != null) renderTarget.Dispose();
                if (swapChain != null) swapChain.Dispose();
                if (device != null) device.Dispose();
            }
        }

        private void ResizeRenderTargetAndDestinationRectangle()
        {
            sourceRectangle = new RawRectangleF(screenViewport.X, screenViewport.Y, (screenViewport.X + screenViewport.Width), (screenViewport.Y + screenViewport.Height));
            renderTarget.Resize(new Size2(outputControl.ClientSize.Width, outputControl.ClientSize.Height));

            if (keepAspectRatio)
            {
                float screenWidth = (sourceRectangle.Right - sourceRectangle.Left), screenHeight = (sourceRectangle.Bottom - sourceRectangle.Top);
                float outputWidth = outputControl.ClientSize.Width, outputHeight = outputControl.ClientSize.Height;

                float tempRatio = ((screenWidth / screenHeight) * (forceSquarePixels ? 1.0f : aspectRatio));
                if ((outputHeight * tempRatio) <= outputWidth)
                {
                    float left = ((outputWidth - (outputHeight * tempRatio)) / 2.0f);
                    destinationRectangle = new RawRectangleF(left, 0.0f, (left + (outputHeight * tempRatio)), outputHeight);
                }
                else
                {
                    float top = ((outputHeight - (outputWidth / tempRatio)) / 2.0f);
                    destinationRectangle = new RawRectangleF(0.0f, top, outputWidth, (top + (outputWidth / tempRatio)));
                }
            }
            else
            {
                destinationRectangle = new RawRectangleF(0.0f, 0.0f, outputControl.ClientSize.Width, outputControl.ClientSize.Height);
            }

            destinationRectangle.Right += 0.5f;
            destinationRectangle.Bottom += 0.5f;
        }

        public override void OnOutputResized(object sender, OutputResizedEventArgs e)
        {
            ResizeRenderTargetAndDestinationRectangle();
        }

        public override void OnRenderScreen(object sender, RenderScreenEventArgs e)
        {
            if (bitmap == null)
                bitmap = new Bitmap(renderTarget, new Size2(e.Width, e.Height), new BitmapProperties(renderTarget.PixelFormat));

            bitmap.CopyFromMemory(e.FrameData, e.Width * 4);
            bitmapRenderTarget.BeginDraw();
            bitmapRenderTarget.Clear(Color.Black);
            bitmapRenderTarget.DrawBitmap(bitmap, 1.0f, BitmapInterpolationMode.NearestNeighbor);
            bitmapRenderTarget.EndDraw();

            renderTarget.BeginDraw();
            renderTarget.Clear(Color.Black);
            renderTarget.DrawBitmap(bitmapRenderTarget.Bitmap, destinationRectangle, 1.0f, BitmapInterpolationMode.Linear, sourceRectangle);
            renderTarget.EndDraw();
        }
    }
}
