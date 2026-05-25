using System.Collections.Generic;
using Tetti.UVCUnity.Models;
using Tetti.UVCUnity.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Tetti.UVCUnity.UI
{
    [DisallowMultipleComponent]
    public sealed class UvcCameraPanelController : MonoBehaviour
    {
        [SerializeField] private Button refreshButton;
        [SerializeField] private Dropdown cameraDropdown;
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Button startPreviewButton;
        [SerializeField] private Button stopPreviewButton;
        [SerializeField] private Text statusText;
        [SerializeField] private RawImagePreviewView previewView;

        private CameraDeviceInfo[] devices = System.Array.Empty<CameraDeviceInfo>();
        private WebCamTexture activeWebCamTexture;
        private PreviewState previewState;

        private void Awake()
        {
            BindButton(refreshButton, RefreshDevices);
            BindButton(startPreviewButton, StartPreview);
            BindButton(stopPreviewButton, StopPreview);

            if (cameraDropdown != null)
            {
                cameraDropdown.onValueChanged.AddListener(_ => RefreshResolutions());
            }

            if (previewView != null)
            {
                previewView.Clear();
            }
        }

        private void Start()
        {
            RefreshDevices();
        }

        private void OnDestroy()
        {
            StopPreview();
        }

        public void RefreshDevices()
        {
            StopPreview();
            var result = AndroidCameraBridge.ScanDevices();
            devices = result.devices ?? System.Array.Empty<CameraDeviceInfo>();

            PopulateDropdown(cameraDropdown, devices, device => device.ToDisplayString());
            RefreshResolutions();
            SetStatus(string.IsNullOrWhiteSpace(result.status) ? "Scan complete." : result.status);
        }

        public void StartPreview()
        {
            if (!TryGetSelection(out var device, out var capability))
            {
                SetStatus("Select a camera and resolution first.");
                return;
            }

            StopPreview();

            if (previewView == null)
            {
                SetStatus("Preview view is not assigned.");
                return;
            }

            if (device.canUseUnityWebCamTexture)
            {
                var requestedFps = capability.fpsOptions != null && capability.fpsOptions.Length > 0 ? capability.fpsOptions[0] : 30;
                activeWebCamTexture = string.IsNullOrEmpty(device.unityWebCamName)
                    ? new WebCamTexture(capability.width, capability.height, requestedFps)
                    : new WebCamTexture(device.unityWebCamName, capability.width, capability.height, requestedFps);
                activeWebCamTexture.Play();
                previewView.BindTexture(activeWebCamTexture);
                previewState = PreviewState.UsingUnityWebCamTexture;
                SetStatus($"Previewing {device.displayName} with Unity WebCamTexture at {capability.ToDisplayString()}. {device.GetFovLabel()}.");
                return;
            }

            previewView.ShowPlaceholder(device.displayName);
            previewState = PreviewState.PlaceholderNativeBridge;
            AndroidCameraBridge.TryStartNativePreview(device, capability, gameObject.name, nameof(OnNativeBridgeStatus), out var nativeStatus);
            SetStatus($"{nativeStatus} {device.GetFovLabel()}");
        }

        public void StopPreview()
        {
            if (activeWebCamTexture != null)
            {
                activeWebCamTexture.Stop();
                Destroy(activeWebCamTexture);
                activeWebCamTexture = null;
            }

            AndroidCameraBridge.StopNativePreview();
            previewState = PreviewState.Idle;

            if (previewView != null)
            {
                previewView.Clear();
            }
        }

        public void OnNativeBridgeStatus(string message)
        {
            SetStatus(message);
        }

        private void RefreshResolutions()
        {
            if (!TryGetSelectedDevice(out var device))
            {
                PopulateDropdown(resolutionDropdown, System.Array.Empty<CameraCapability>(), capability => capability.ToDisplayString());
                return;
            }

            var capabilities = device.capabilities == null || device.capabilities.Length == 0
                ? new[] { new CameraCapability { width = 640, height = 480, fpsOptions = new[] { 30 } } }
                : device.capabilities;

            PopulateDropdown(resolutionDropdown, capabilities, capability => capability.ToDisplayString());
            SetStatus($"Selected {device.displayName}. {device.GetFovLabel()}. {DescribeDeviceMode(device)}");
        }

        private string DescribeDeviceMode(CameraDeviceInfo device)
        {
            if (device.canUseUnityWebCamTexture)
            {
                return "Preview path: Unity WebCamTexture fallback.";
            }

            if (device.requiresNativePlugin)
            {
                return "Preview path: native Android/UVC plugin placeholder.";
            }

            return "Preview path: placeholder bridge.";
        }

        private bool TryGetSelection(out CameraDeviceInfo device, out CameraCapability capability)
        {
            capability = null;
            if (!TryGetSelectedDevice(out device))
            {
                return false;
            }

            var capabilities = device.capabilities == null || device.capabilities.Length == 0
                ? new[] { new CameraCapability { width = 640, height = 480, fpsOptions = new[] { 30 } } }
                : device.capabilities;

            if (resolutionDropdown == null || resolutionDropdown.value < 0 || resolutionDropdown.value >= capabilities.Length)
            {
                capability = capabilities[0];
                return true;
            }

            capability = capabilities[resolutionDropdown.value];
            return true;
        }

        private bool TryGetSelectedDevice(out CameraDeviceInfo device)
        {
            device = null;
            if (devices == null || devices.Length == 0 || cameraDropdown == null)
            {
                return false;
            }

            if (cameraDropdown.value < 0 || cameraDropdown.value >= devices.Length)
            {
                return false;
            }

            device = devices[cameraDropdown.value];
            return true;
        }

        private static void PopulateDropdown<T>(Dropdown dropdown, IReadOnlyList<T> items, System.Func<T, string> labelSelector)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.ClearOptions();
            var options = new List<Dropdown.OptionData>();
            for (var index = 0; index < items.Count; index++)
            {
                options.Add(new Dropdown.OptionData(labelSelector(items[index])));
            }

            if (options.Count == 0)
            {
                options.Add(new Dropdown.OptionData("(none)"));
            }

            dropdown.AddOptions(options);
            dropdown.value = 0;
            dropdown.RefreshShownValue();
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }

            Debug.Log($"[UVCUnity] {message}");
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }
}
