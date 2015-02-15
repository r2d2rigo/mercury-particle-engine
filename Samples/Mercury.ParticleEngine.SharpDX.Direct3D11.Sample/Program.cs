using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using Mercury.ParticleEngine.Modifiers;
using Mercury.ParticleEngine.Profiles;
using Mercury.ParticleEngine.Renderers;
using System.Windows.Input;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.DirectInput;

namespace Mercury.ParticleEngine
{

    static class Program
    {
        [STAThread]
        static void Main()
        {
            var worldSize = new Size2(1024, 768);
            var renderSize = new Size2(1024, 768);
            const bool windowed = true;
            var directInput = new DirectInput();
            var keyboard = new Keyboard(directInput);
            keyboard.Properties.BufferSize = 128;
            keyboard.Acquire();

            var form = new RenderForm("Mercury Particle Engine - SharpDX.Direct3D11 Sample")
            {
                Size = new System.Drawing.Size(renderSize.Width, renderSize.Height)
            };

            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            SharpDX.Direct3D11.Device device;
            SharpDX.DXGI.SwapChain swapChain;
            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, desc, out device, out swapChain);
            var context = device.ImmediateContext;

            // Ignore all windows events
            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            var renderView = new RenderTargetView(device, backBuffer);

            var depthBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = form.ClientSize.Width,
                Height = form.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            var depthView = new DepthStencilView(device, depthBuffer);

            var view = new Matrix(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, -1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, -1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
            var proj = Matrix.OrthoOffCenterLH(worldSize.Width * -0.5f, worldSize.Width * 0.5f, worldSize.Height * 0.5f, worldSize.Height * -0.5f, 0f, 1f);
            var wvp = Matrix.Identity * view * proj;

            var smokeEffect = new ParticleEffect
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

            var sparkEffect = new ParticleEffect
            {
                Emitters = new[] {
                    new Emitter(2000, TimeSpan.FromSeconds(2), Profile.Point()) {
                        Parameters = new ReleaseParameters {
                            Colour   = new Colour(50f, 0.8f, 0.5f),
                            Opacity  = 1f,
                            Quantity = 10,
                            Speed    = new RangeF(0f, 100f),
                            Scale    = 64f,
                            Mass     = new RangeF(8f, 12f)
                        },
                        ReclaimFrequency = 5f,
                        BlendMode = BlendMode.Add,
                        RenderingOrder = RenderingOrder.FrontToBack,
                        TextureKey = "Particle",
                        Modifiers = new Modifier[] {
                            new LinearGravityModifier(Axis.Down, 30f) {
                                Frequency = 15f
                            },
                            new OpacityFastFadeModifier() {
                                Frequency = 10f
                            }
                        }
                    }
                }
            };

            var ringEffect = new ParticleEffect
            {
                Emitters = new[] {
                    new Emitter(2000, TimeSpan.FromSeconds(3), Profile.Spray(Axis.Up, 0.5f)) {
                        Parameters = new ReleaseParameters {
                            Colour   = new ColourRange(new Colour(210f, 0.5f, 0.6f), new Colour(230f, 0.7f, 0.8f)),
                            Opacity  = 1f,
                            Quantity = 1,
                            Speed    = new RangeF(300f, 700f),
                            Scale    = 64f,
                            Mass     = new RangeF(4f, 12f),
                        },
                        ReclaimFrequency = 5f,
                        BlendMode = BlendMode.Alpha,
                        RenderingOrder = RenderingOrder.FrontToBack,
                        TextureKey = "Ring",
                        Modifiers = new Modifier[] {
                            new LinearGravityModifier(Axis.Down, 100f) {
                                Frequency              = 20f
                            },
                            new OpacityFastFadeModifier() {
                                Frequency              = 10f,
                            },
                            new ContainerModifier {
                                Frequency              = 15f,
                                Width                  = worldSize.Width,
                                Height                 = worldSize.Height,
                                Position               = new Coordinate(worldSize.Width / 2f, worldSize.Height / 2f),
                                RestitutionCoefficient = 0.75f
                            }
                        }
                    }
                }
            };

            var loadTestEffect = new ParticleEffect
            {
                Emitters = new[] {
                    new Emitter(1000000, TimeSpan.FromSeconds(2), Profile.Point()) {
                        Parameters = new ReleaseParameters {
                            Quantity = 10000,
                            Speed    = new RangeF(0f, 200f),
                            Scale    = 1f,
                            Mass     = new RangeF(4f, 12f),
                            Opacity  = 0.4f
                        },
                        ReclaimFrequency = 5f,
                        BlendMode = BlendMode.Add,
                        TextureKey = "Pixel",
                        Modifiers = new Modifier[] {
                            new LinearGravityModifier(Axis.Down, 30f) {
                                Frequency = 15f
                            },
                            new OpacityFastFadeModifier() {
                                Frequency = 10f
                            },
                            new ContainerModifier {
                                Frequency              = 30f,
                                Width                  = worldSize.Width,
                                Height                 = worldSize.Height,
                                Position               = new Coordinate(worldSize.Width / 2f, worldSize.Height / 2f),
                                RestitutionCoefficient = 0.75f
                            },
                            new DragModifier {
                                Frequency       = 10f,
                                DragCoefficient = 0.47f,
                                Density         = 0.125f
                            },
                            new HueInterpolator2 {
                                Frequency = 10f,
                                InitialHue = 0f,
                                FinalHue = 150f
                            }
                        }
                    }
                }
            };

            var textureLookup = new Dictionary<String, Texture2D> {
                { "Particle", Texture2D.FromFile<Texture2D>(device, "Particle.dds") },
                { "Pixel",    Texture2D.FromFile<Texture2D>(device, "Pixel.dds")    },
                { "Cloud",    Texture2D.FromFile<Texture2D>(device, "Cloud001.png") },
                { "Ring",     Texture2D.FromFile<Texture2D>(device, "Ring001.png")  }
            };

            var textureResourceViews = new Dictionary<string, ShaderResourceView>
            {
                { "Particle", new ShaderResourceView(device, textureLookup["Particle"]) },
                { "Pixel",    new ShaderResourceView(device, textureLookup["Pixel"]) },
                { "Cloud",    new ShaderResourceView(device, textureLookup["Cloud"]) },
                { "Ring",     new ShaderResourceView(device, textureLookup["Ring"]) },
            };

            var renderer = new SpriteBatchRenderer(device, 1000000, textureResourceViews)
            {
                //EnableFastFade = true
            };

            var totalTime = 0f;
            var totalTimer = Stopwatch.StartNew();
            var updateTimer = new Stopwatch();
            var renderTimer = new Stopwatch();

            var currentEffect = smokeEffect;

            Vector3 mousePosition = Vector3.Zero;
            Vector3 previousMousePosition = Vector3.Zero;

            RenderLoop.Run(form, () =>
            {
                // ReSharper disable AccessToDisposedClosure
                var frameTime = ((float)totalTimer.Elapsed.TotalSeconds) - totalTime;
                totalTime = (float)totalTimer.Elapsed.TotalSeconds;

                var clientMousePosition = form.PointToClient(RenderForm.MousePosition);
                previousMousePosition = mousePosition;
                mousePosition = Vector3.Unproject(new Vector3(clientMousePosition.X, clientMousePosition.Y, 0f), 0, 0, renderSize.Width, renderSize.Height, 0f, 1f, wvp);

                var mouseMovementLine = new LineSegment(new Coordinate(previousMousePosition.X, previousMousePosition.Y), new Coordinate(mousePosition.X, mousePosition.Y));

                var keyboardState = keyboard.GetCurrentState();
                
                if (keyboardState.IsPressed(Key.D1))
                    currentEffect = smokeEffect;

                if (keyboardState.IsPressed(Key.D2))
                    currentEffect = sparkEffect;

                if (keyboardState.IsPressed(Key.D3))
                    currentEffect = ringEffect;

                if (keyboardState.IsPressed(Key.D4))
                    currentEffect = loadTestEffect;

                if (RenderForm.MouseButtons.HasFlag(System.Windows.Forms.MouseButtons.Left))
                {
                    currentEffect.Trigger(mouseMovementLine);
                }

                updateTimer.Restart();
                smokeEffect.Update(frameTime);
                sparkEffect.Update(frameTime);
                ringEffect.Update(frameTime);
                loadTestEffect.Update(frameTime);
                updateTimer.Stop();

                context.OutputMerger.SetTargets(depthView, renderView);
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                context.ClearRenderTargetView(renderView, Color.Black);
                context.Rasterizer.SetViewport(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f));

                renderTimer.Restart();
                renderer.Render(smokeEffect, wvp);
                renderer.Render(sparkEffect, wvp);
                renderer.Render(ringEffect, wvp);
                renderer.Render(loadTestEffect, wvp);
                renderTimer.Stop();

                var updateTime = (float)updateTimer.Elapsed.TotalSeconds;
                var renderTime = (float)renderTimer.Elapsed.TotalSeconds;

                swapChain.Present(0, PresentFlags.None);

                if (keyboardState.IsPressed(Key.Escape))
                    Environment.Exit(0);
            });

            renderer.Dispose();
            form.Dispose();
            device.Dispose();
        }
    }
}
