using System;
using System.Linq;

namespace Tetti.UVCUnity.Models
{
    [Serializable]
    public sealed class CameraCapability
    {
        public int width = 640;
        public int height = 480;
        public int[] fpsOptions = Array.Empty<int>();

        public string ToDisplayString()
        {
            if (fpsOptions == null || fpsOptions.Length == 0)
            {
                return $"{width} x {height}";
            }

            return $"{width} x {height} @ {string.Join("/", fpsOptions.Distinct())} fps";
        }
    }
}
