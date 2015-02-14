using SharpDX;
using System.Runtime.InteropServices;

namespace Mercury.ParticleEngine.Renderers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SpriteVertex
    {
        public Vector2 Position;
        public Vector4 Color;
        public Vector2 TexCoords;
        public float Age;
    }
}
