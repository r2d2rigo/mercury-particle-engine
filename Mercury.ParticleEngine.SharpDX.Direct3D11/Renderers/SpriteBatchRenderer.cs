using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Collections.Generic;

namespace Mercury.ParticleEngine.Renderers
{
    public class SpriteBatchRenderer : System.IDisposable
    {
        private static readonly Vector2[] CornerOffsets;

        private readonly Device _device;
        private readonly DeviceContext _context;
        private readonly int _size;
        private readonly IReadOnlyDictionary<string, ShaderResourceView> _textureLookup;
        private readonly Buffer _vertexBuffer;
        private readonly VertexBufferBinding _vertexBufferBinding;
        private readonly Buffer _indexBuffer;
        private readonly InputLayout _inputLayout;
        private readonly VertexShader _vertexShader;
        private readonly PixelShader _pixelShader;
        private readonly Buffer _constantsBuffer;
        private ShaderParameters _parameters;
        private SpriteVertex[] _spriteVertices;
        private SamplerState _samplerState;
        private BlendState[] _blendStates;
        private DepthStencilState _depthState;

        private bool _enableFastFade;
        public bool EnableFastFade
        {
            get { return _enableFastFade; }
            set
            {
                if (value != _enableFastFade)
                {
                    _enableFastFade = value;
                    _parameters.IsFastFadeEnabled = value;
                }
            }
        }

        static SpriteBatchRenderer()
        {
            CornerOffsets = new[] { new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f) };
        }

        public SpriteBatchRenderer(Device device, int size, IReadOnlyDictionary<string, ShaderResourceView> textureLookup)
            : this(device, null, size, textureLookup)
        {
        }

        public SpriteBatchRenderer(Device device, DeviceContext context, int size, IReadOnlyDictionary<string, ShaderResourceView> textureLookup)
        {
            if (device == null)
                throw new System.ArgumentNullException("device");

            if (textureLookup == null)
                throw new System.ArgumentNullException("textureLookup");

            _parameters = new ShaderParameters();
            _device = device;
            _context = context;
            if (context == null)
            {
                this._context = this._device.ImmediateContext;
            }

            _size = size;
            int vertexCount = size * 4;
            int indexCount = size * 6;
            _textureLookup = textureLookup;

            this._samplerState = new SamplerState(_device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            });

            this._blendStates = new BlendState[3];

            this._blendStates[0] = CreateBlendState(BlendOperation.Add, BlendOption.InverseSourceAlpha);
            this._blendStates[1] = CreateBlendState(BlendOperation.Add, BlendOption.One);
            this._blendStates[2] = CreateBlendState(BlendOperation.ReverseSubtract, BlendOption.One);

            this._depthState = new DepthStencilState(_device, new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                });

            _vertexBuffer = new Buffer(_device, vertexCount * 4 * Utilities.SizeOf<SpriteVertex>(), ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            _vertexBufferBinding = new VertexBufferBinding(this._vertexBuffer, Utilities.SizeOf<SpriteVertex>(), 0);

            int[] indices = new int[indexCount];
            _spriteVertices = new SpriteVertex[vertexCount];

            for (int i = 0; i < _size; ++i)
            {
                int offset = i * 6;
                int index = i * 4;

                indices[offset] = index;
                indices[offset + 1] = index + 1;
                indices[offset + 2] = index + 2;

                indices[offset + 3] = index;
                indices[offset + 4] = index + 2;
                indices[offset + 5] = index + 3;

                for (int j = 0; j < 4; j++)
                {
                    _spriteVertices[index + j].TexCoords = new Vector2(CornerOffsets[j].X + 0.5f, -CornerOffsets[j].Y + 0.5f);
                }
            }

            _indexBuffer = Buffer.Create<int>(this._device, BindFlags.IndexBuffer, indices);

            var inputElements = new[]
            {
                //new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32_Float, 0, 0),
                //new InputElement("COLOR", 0, SharpDX.DXGI.Format.R8G8B8A8_UNorm, 8, 0),
                //new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0),
                //new InputElement("COLOR", 1, SharpDX.DXGI.Format.R32_Float, 20, 0),

                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32_Float, 0, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 8, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 24, 0),
                new InputElement("COLOR", 1, SharpDX.DXGI.Format.R32_Float, 32, 0),
            };

            using (CompilationResult vsCompilation = ShaderBytecode.Compile(Resources.SpriteBatchShader, "SpriteVertexShader", "vs_4_0_level_9_1", ShaderFlags.None, EffectFlags.None))
            {
                this._vertexShader = new VertexShader(this._device, vsCompilation.Bytecode);
                this._inputLayout = new InputLayout(this._device, ShaderSignature.GetInputSignature(vsCompilation.Bytecode), inputElements);
            }

            using (CompilationResult psCompilation = ShaderBytecode.Compile(Resources.SpriteBatchShader, "SpritePixelShader", "ps_4_0_level_9_1", ShaderFlags.None, EffectFlags.None))
            {
                this._pixelShader = new PixelShader(this._device, psCompilation.Bytecode);
            }

            _constantsBuffer = new Buffer(this._device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        public void Render(ParticleEffect effect, Matrix worldViewProjection)
        {
            for (var i = 0; i < effect.Emitters.Length; i++)
            {
                Render(effect.Emitters[i], worldViewProjection);
            }
        }

        internal unsafe void Render(Emitter emitter, Matrix worldViewProjection)
        {
            if (emitter.ActiveParticles == 0)
                return;

            if (emitter.ActiveParticles > _size)
                throw new System.Exception("Cannot render this emitter, vertex buffer not big enough");

            _parameters.WorldViewProjection = worldViewProjection;
            _parameters.WorldViewProjection.Transpose();
            this._context.UpdateSubresource(ref this._parameters, this._constantsBuffer);

            //switch (emitter.RenderingOrder)
            //{
            //    case RenderingOrder.FrontToBack:
            //        {
            //            emitter.Buffer.CopyTo(vertexDataPointer);
            //            break;
            //        }
            //    case RenderingOrder.BackToFront:
            //        {
            //            emitter.Buffer.CopyToReverse(vertexDataPointer);
            //            break;
            //        }
            //}

            System.IntPtr particleAddress = emitter.Buffer.NativePointer;
            Vector2 rotation;
            Vector2 position;

            for (int i = 0; i < emitter.ActiveParticles; i++)
            {
                int offset = i * 4;

                Particle* particle = (Particle*)particleAddress;

                rotation.X = (float)System.Math.Cos(particle->Rotation);
                rotation.Y = (float)System.Math.Sin(particle->Rotation);

                for (int j = 0; j < 4; j++)
                {
                    var corner = CornerOffsets[j];
                    position.X = corner.X * particle->Scale;
                    position.Y = corner.Y * particle->Scale;

                    _spriteVertices[offset + j].Position.X = particle->Position[0] + (position.X * rotation.X) - (position.Y * rotation.Y);
                    _spriteVertices[offset + j].Position.Y = particle->Position[1] + (position.X * rotation.Y) + (position.Y * rotation.X);

                    _spriteVertices[offset + j].TexCoords.X = corner.X + 0.5f;
                    _spriteVertices[offset + j].TexCoords.Y = -corner.Y + 0.5f;

                    _spriteVertices[offset + j].Color.X = particle->Colour[0];
                    _spriteVertices[offset + j].Color.Y = particle->Colour[1];
                    _spriteVertices[offset + j].Color.Z = particle->Colour[2];
                    _spriteVertices[offset + j].Color.W = particle->Opacity;
                }

                particleAddress = particleAddress + Particle.SizeInBytes;
            }

            DataStream dataStream;
            this._context.MapSubresource(this._vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None, out dataStream);
            dataStream.WriteRange<SpriteVertex>(this._spriteVertices);
            this._context.UnmapSubresource(this._vertexBuffer, 0);
            dataStream.Dispose();

            switch (emitter.BlendMode)
            {
                default:
                case BlendMode.Alpha:
                    _context.OutputMerger.SetBlendState(_blendStates[0]);
                    break;
                case BlendMode.Add:
                    _context.OutputMerger.SetBlendState(_blendStates[1]);
                    break;
                case BlendMode.Subtract:
                    _context.OutputMerger.SetBlendState(_blendStates[2]);
                    break;
            }

            _context.OutputMerger.SetDepthStencilState(_depthState);

            this._context.InputAssembler.InputLayout = this._inputLayout;
            this._context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this._context.InputAssembler.SetVertexBuffers(0, this._vertexBufferBinding);
            this._context.InputAssembler.SetIndexBuffer(this._indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
            this._context.VertexShader.SetConstantBuffer(0, this._constantsBuffer);
            this._context.VertexShader.Set(this._vertexShader);
            this._context.PixelShader.Set(this._pixelShader);
            this._context.PixelShader.SetShaderResource(0, this._textureLookup[emitter.TextureKey]);
            this._context.PixelShader.SetSampler(0, this._samplerState);

            this._context.DrawIndexed(emitter.ActiveParticles * 6, 0, 0);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._constantsBuffer.Dispose();
                this._pixelShader.Dispose();
                this._vertexShader.Dispose();
                this._inputLayout.Dispose();
                this._indexBuffer.Dispose();
                this._vertexBuffer.Dispose();
            }
        }

        ~SpriteBatchRenderer()
        {
            Dispose(false);
        }

        private BlendState CreateBlendState(BlendOperation blendOperation, BlendOption destinationBlend)
        {
            var description = new BlendStateDescription();
            description.RenderTarget[0] = new RenderTargetBlendDescription(true,
                BlendOption.SourceAlpha,
                destinationBlend,
                blendOperation,
                BlendOption.SourceAlpha,
                BlendOption.InverseSourceAlpha,
                BlendOperation.Add,
                ColorWriteMaskFlags.All);

            return new BlendState(_device, description);
        }
    }
}