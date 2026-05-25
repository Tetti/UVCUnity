using System;
using System.Linq;
using Tetti.UVCUnity.Models;
using UnityEngine;

namespace Tetti.UVCUnity.Runtime
{
    public static class AndroidCameraBridge
    {
        private const string BridgeClassName = "com.tetti.uvcunity.CameraBridge";
        private const string FallbackStatus = "Using fallback camera enumeration. Drop in native `.aar`/`.jar` dependencies to enable UVC-specific preview support.";

        public static CameraScanResult ScanDevices()
        {
            if (!Application.isPlaying)
            {
                return CreateFallbackResult("Unity editor fallback active. Enter Play Mode to scan devices in-editor.");
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var bridgeClass = new AndroidJavaClass(BridgeClassName))
                {
                    var json = bridgeClass.CallStatic<string>("scanDevices", activity);
                    var result = JsonUtility.FromJson<CameraScanResult>(json);
                    if (result != null && result.devices != null && result.devices.Length > 0)
                    {
                        return result;
                    }
                }
            }
            catch (Exception exception)
            {
                return CreateFallbackResult($"Android plugin unavailable: {exception.Message}");
            }
#endif

            return CreateFallbackResult(FallbackStatus);
        }

        public static bool TryStartNativePreview(CameraDeviceInfo device, CameraCapability capability, string gameObjectName, string callbackMethodName, out string status)
        {
            status = string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var bridgeClass = new AndroidJavaClass(BridgeClassName))
                {
                    var started = bridgeClass.CallStatic<bool>(
                        "startPreview",
                        activity,
                        device.id,
                        capability.width,
                        capability.height,
                        gameObjectName,
                        callbackMethodName);

                    status = started
                        ? $"Native preview requested for {device.displayName}."
                        : $"Native preview placeholder only for {device.displayName}. Add a real UVC/native streaming dependency to complete this path.";
                    return started;
                }
            }
            catch (Exception exception)
            {
                status = $"Native preview bridge unavailable: {exception.Message}";
                return false;
            }
#else
            status = "Native Android preview is only available on-device. Editor uses WebCamTexture or placeholder behavior.";
            return false;
#endif
        }

        public static void StopNativePreview()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var bridgeClass = new AndroidJavaClass(BridgeClassName))
                {
                    bridgeClass.CallStatic("stopPreview");
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to stop native preview: {exception.Message}");
            }
#endif
        }

        private static CameraScanResult CreateFallbackResult(string reason)
        {
            var devices = WebCamTexture.devices
                .Select(device => new CameraDeviceInfo
                {
                    id = $"unity:{device.name}",
                    displayName = device.name,
                    kind = "UnityWebCam",
                    unityWebCamName = device.name,
                    canUseUnityWebCamTexture = true,
                    capabilities = DefaultCapabilities(),
                    isBuiltIn = !device.isFrontFacing,
                    isUsb = false,
                    requiresNativePlugin = false,
                })
                .ToArray();

            if (devices.Length == 0)
            {
                devices = new[]
                {
                    new CameraDeviceInfo
                    {
                        id = "placeholder:builtin",
                        displayName = "Built-in Android camera (placeholder)",
                        kind = "BuiltIn",
                        isBuiltIn = true,
                        canUseUnityWebCamTexture = false,
                        capabilities = DefaultCapabilities(),
                    },
                    new CameraDeviceInfo
                    {
                        id = "placeholder:usb-uvc",
                        displayName = "USB/UVC camera (placeholder)",
                        kind = "UsbUvc",
                        isUsb = true,
                        vendorId = 0x0000,
                        productId = 0x0000,
                        canUseUnityWebCamTexture = false,
                        requiresNativePlugin = true,
                        capabilities = new[]
                        {
                            new CameraCapability { width = 640, height = 480, fpsOptions = new[] { 30 } }
                        }
                    }
                };
            }

            return new CameraScanResult
            {
                status = reason,
                devices = devices,
            };
        }

        private static CameraCapability[] DefaultCapabilities()
        {
            return new[]
            {
                new CameraCapability { width = 640, height = 480, fpsOptions = new[] { 15, 30 } },
                new CameraCapability { width = 1280, height = 720, fpsOptions = new[] { 30 } },
                new CameraCapability { width = 1920, height = 1080, fpsOptions = new[] { 30 } },
            };
        }
    }
}
