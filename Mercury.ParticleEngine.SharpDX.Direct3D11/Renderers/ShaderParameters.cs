using SharpDX;
using System.Runtime.InteropServices;

namespace Mercury.ParticleEngine.Renderers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ShaderParameters
    {
        public Matrix WorldViewProjection;
        public bool IsFastFadeEnabled;
        private Vector3 padding;
    }
}
