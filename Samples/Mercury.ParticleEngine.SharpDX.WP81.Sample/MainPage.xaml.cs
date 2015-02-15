using Mercury.ParticleEngine.Modifiers;
using Mercury.ParticleEngine.Profiles;
using Mercury.ParticleEngine.Renderers;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.IO;
using SharpDX.SimpleInitializer;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Mercury.ParticleEngine.SharpDX.WP81.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SharpDXContext context;
        private Size2 worldSize;
        private Size2 renderSize;
        private ParticleEffect smokeEffect;
        private SpriteBatchRenderer renderer;
        private bool emit;
        private Vector3 tapPosition;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.SwapChainPanel.Loaded += SwapChainPanel_Loaded;
            this.SwapChainPanel.ManipulationStarted += SwapChainPanel_ManipulationStarted;
            this.SwapChainPanel.ManipulationDelta += SwapChainPanel_ManipulationDelta;
            this.SwapChainPanel.ManipulationCompleted += SwapChainPanel_ManipulationCompleted;
        }

        void SwapChainPanel_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            emit = false;
        }

        void SwapChainPanel_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            emit = true;
            tapPosition = new Vector3((float)e.Position.X, (float)e.Position.Y, 0);
        }

        void SwapChainPanel_ManipulationStarted(object sender, Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs e)
        {
            emit = true;
            tapPosition = new Vector3((float)e.Position.X, (float)e.Position.Y, 0);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.SwapChainPanel.Loaded -= SwapChainPanel_Loaded;
            this.SwapChainPanel.ManipulationStarted -= SwapChainPanel_ManipulationStarted;
            this.SwapChainPanel.ManipulationDelta -= SwapChainPanel_ManipulationDelta;
            this.SwapChainPanel.ManipulationCompleted -= SwapChainPanel_ManipulationCompleted;

            base.OnNavigatedFrom(e);
        }

        void SwapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            context = new SharpDXContext();
            context.DeviceReset += context_DeviceReset;
            context.Render += context_Render;
            context.BindToControl(this.SwapChainPanel);
        }

        void context_DeviceReset(object sender, DeviceResetEventArgs e)
        {
            worldSize = new Size2((int)context.BackBufferSize.Width, (int)context.BackBufferSize.Height);
            renderSize = worldSize;

            smokeEffect = new ParticleEffect
            {
                Emitters = new[] {
                    new Emitter(2000, TimeSpan.FromSeconds(3), Profile.Point()) {
                        Parameters = new ReleaseParameters {
                            Colour   = new Colour(0f, 0f, 0.6f),
                            Opacity  = 1f,
                            Quantity = 5,
                            Speed    = new RangeF(0f, 100f),
                            Scale    = 32f,
                            Rotation = new RangeF((float)-Math.PI, (float)Math.PI),
                            Mass     = new RangeF(8f, 12f)
                        },
                        ReclaimFrequency = 5f,
                        BlendMode = BlendMode.Alpha,
                        RenderingOrder = RenderingOrder.BackToFront,
                        TextureKey = "Cloud",
                        Modifiers = new Modifier[] {
                            new DragModifier {
                                Frequency       = 10f,
                                DragCoefficient = 0.47f,
                                Density         = 0.125f
                            },
                            new ScaleInterpolator2 {
                                Frequency       = 60f,
                                InitialScale    = 32f,
                                FinalScale      = 256f
                            },
                            new RotationModifier {
                                Frequency       = 15f,
                                RotationRate    = 1f
                            },
                            new OpacityInterpolator2 {
                                Frequency       = 25f,
                                InitialOpacity  = 0.3f,
                                FinalOpacity    = 0.0f
                            }
                        },
                    }
                }
            };

            var textureLookup = new Dictionary<String, Texture2D> {
                { "Cloud",    LoadTexture("Cloud001.png") },
            };

            var textureResourceViews = new Dictionary<string, ShaderResourceView>
            {
                { "Cloud",    new ShaderResourceView(context.D3DDevice, textureLookup["Cloud"]) },
            };

            renderer = new SpriteBatchRenderer(context.D3DDevice, 10000, textureResourceViews)
            {
                //EnableFastFade = true
            };
        }

        void context_Render(object sender, EventArgs e)
        {
            var view = new Matrix(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, -1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, -1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
            var proj = Matrix.OrthoOffCenterLH(worldSize.Width * -0.5f, worldSize.Width * 0.5f, worldSize.Height * 0.5f, worldSize.Height * -0.5f, 0f, 1f);
            var wvp = Matrix.Identity * view * proj;

                //var frameTime = ((float)totalTimer.Elapsed.TotalSeconds) - totalTime;
                //totalTime = (float)totalTimer.Elapsed.TotalSeconds;

                //var clientMousePosition = form.PointToClient(RenderForm.MousePosition);
                //previousMousePosition = mousePosition;
                //mousePosition = Vector3.Unproject(new Vector3(clientMousePosition.X, clientMousePosition.Y, 0f), 0, 0, renderSize.Width, renderSize.Height, 0f, 1f, wvp);

                //var mouseMovementLine = new LineSegment(new Coordinate(previousMousePosition.X, previousMousePosition.Y), new Coordinate(mousePosition.X, mousePosition.Y));


            if (emit)
            {
                var mousePosition = Vector3.Unproject(new Vector3(tapPosition.X, tapPosition.Y, 0f), 0, 0, renderSize.Width, renderSize.Height, 0f, 1f, wvp);
                smokeEffect.Trigger(new Coordinate(mousePosition.X, mousePosition.Y));
            }
                //if (RenderForm.MouseButtons.HasFlag(System.Windows.Forms.MouseButtons.Left))
                //{
                //    currentEffect.Trigger(mouseMovementLine);
                //}

                //updateTimer.Restart();
                smokeEffect.Update(0.01666f);
                //sparkEffect.Update(frameTime);
                //ringEffect.Update(frameTime);
                //loadTestEffect.Update(frameTime);
                //updateTimer.Stop();

                context.D3DContext.OutputMerger.SetTargets(context.DepthStencilView, context.BackBufferView);
                context.D3DContext.ClearDepthStencilView(context.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
                context.D3DContext.ClearRenderTargetView(context.BackBufferView, Color.CornflowerBlue);

                //renderTimer.Restart();
                renderer.Render(smokeEffect, wvp);
                //renderer.Render(sparkEffect, wvp);
                //renderer.Render(ringEffect, wvp);
                //renderer.Render(loadTestEffect, wvp);
                //renderTimer.Stop();

                //var updateTime = (float)updateTimer.Elapsed.TotalSeconds;
                //var renderTime = (float)renderTimer.Elapsed.TotalSeconds;

                //if (keyboardState.IsPressed(Key.Escape))
                //    Environment.Exit(0);
        }

        private Texture2D LoadTexture(string filename)
        {
            var imagingFactory = new ImagingFactory();

            var pngDecoder = new PngBitmapDecoder(imagingFactory);

            using (var fileStream = new WICStream(imagingFactory, filename, NativeFileAccess.Read))
            {
                pngDecoder.Initialize(fileStream, DecodeOptions.CacheOnDemand);
            }

            var result = new FormatConverter(imagingFactory);

            result.Initialize(
                pngDecoder.GetFrame(0),
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None,
                null,
                0.0,
                BitmapPaletteType.Custom);

            int stride = result.Size.Width * 4;

            using (var buffer = new DataStream(result.Size.Height * stride, true, true))
            {
                result.CopyPixels(stride, buffer);
                
                return new Texture2D(context.D3DDevice, new Texture2DDescription()
                {
                    Width = result.Size.Width,
                    Height = result.Size.Height,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = global::SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new global::SharpDX.DXGI.SampleDescription(1, 0),
                }, new DataRectangle(buffer.DataPointer, stride));
            }
        }
    }
}
