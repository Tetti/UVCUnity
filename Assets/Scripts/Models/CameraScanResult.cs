using System;

namespace Tetti.UVCUnity.Models
{
    [Serializable]
    public sealed class CameraScanResult
    {
        public string status = string.Empty;
        public CameraDeviceInfo[] devices = Array.Empty<CameraDeviceInfo>();
    }
}
