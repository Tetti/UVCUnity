using System;

namespace Tetti.UVCUnity.Models
{
    [Serializable]
    public sealed class CameraDeviceInfo
    {
        public string id = string.Empty;
        public string displayName = string.Empty;
        public string kind = "Unknown";
        public string androidCameraId = string.Empty;
        public string unityWebCamName = string.Empty;
        public int vendorId = -1;
        public int productId = -1;
        public float horizontalFov = -1f;
        public float verticalFov = -1f;
        public bool isBuiltIn;
        public bool isUsb;
        public bool canUseUnityWebCamTexture;
        public bool requiresNativePlugin;
        public bool hasUsbPermission;
        public CameraCapability[] capabilities = Array.Empty<CameraCapability>();

        public string ToDisplayString()
        {
            if (isUsb && vendorId >= 0 && productId >= 0)
            {
                return $"{displayName} (USB {vendorId:X4}:{productId:X4})";
            }

            return displayName;
        }

        public string GetFovLabel()
        {
            if (horizontalFov > 0f && verticalFov > 0f)
            {
                return $"FOV {horizontalFov:0.#}° x {verticalFov:0.#}°";
            }

            return "FOV unavailable";
        }
    }
}
