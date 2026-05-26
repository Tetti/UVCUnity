package com.tetti.uvcunity;

import android.app.Activity;
import android.content.Context;
import android.graphics.SurfaceTexture;
import android.hardware.camera2.CameraAccessException;
import android.hardware.camera2.CameraCharacteristics;
import android.hardware.camera2.CameraManager;
import android.hardware.camera2.params.StreamConfigurationMap;
import android.hardware.usb.UsbConstants;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbInterface;
import android.hardware.usb.UsbManager;
import android.os.Build;
import android.util.Range;
import android.util.Size;

import com.unity3d.player.UnityPlayer;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.LinkedHashSet;
import java.util.Map;
import java.util.Set;

public final class CameraBridge {
    private CameraBridge() {
    }

    public static String scanDevices(Activity activity) {
        final JSONObject root = new JSONObject();
        final JSONArray devices = new JSONArray();
        final StringBuilder status = new StringBuilder();

        try {
            addBuiltInCameraDevices(activity, devices, status);
            addUsbDevices(activity, devices, status);

            root.put("status", status.toString().trim());
            root.put("devices", devices);
        } catch (Exception exception) {
            try {
                root.put("status", "Android bridge scan failed: " + exception.getMessage());
                root.put("devices", devices);
            } catch (JSONException ignored) {
                // Ignore serialization fallback errors in placeholder plugin.
            }
        }

        return root.toString();
    }

    public static boolean startPreview(Activity activity, String deviceId, int width, int height, String unityObjectName, String callbackMethodName) {
        sendStatus(unityObjectName, callbackMethodName,
            "Placeholder Android bridge selected " + deviceId + " at " + width + "x" + height
                + ". Built-in and USB/UVC enumeration are implemented, but true native preview requires third-party UVC/native streaming AARs.");
        return false;
    }

    public static void stopPreview() {
        // Placeholder for future native preview teardown.
    }

    private static void addBuiltInCameraDevices(Activity activity, JSONArray devices, StringBuilder status)
        throws CameraAccessException, JSONException {
        final CameraManager cameraManager = (CameraManager) activity.getSystemService(Context.CAMERA_SERVICE);
        if (cameraManager == null) {
            status.append("CameraManager unavailable. ");
            return;
        }

        int count = 0;
        for (String cameraId : cameraManager.getCameraIdList()) {
            final CameraCharacteristics characteristics = cameraManager.getCameraCharacteristics(cameraId);
            final JSONObject device = new JSONObject();
            final int lensFacing = getLensFacing(characteristics);
            final String facingLabel = lensFacing == CameraCharacteristics.LENS_FACING_FRONT ? "front"
                : lensFacing == CameraCharacteristics.LENS_FACING_BACK ? "back"
                : lensFacing == CameraCharacteristics.LENS_FACING_EXTERNAL ? "external"
                : "unknown";

            device.put("id", "builtin:" + cameraId);
            device.put("displayName", "Built-in camera " + cameraId + " (" + facingLabel + ")");
            device.put("kind", "BuiltIn");
            device.put("androidCameraId", cameraId);
            device.put("unityWebCamName", "");
            device.put("vendorId", -1);
            device.put("productId", -1);
            device.put("isBuiltIn", true);
            device.put("isUsb", false);
            device.put("canUseUnityWebCamTexture", true);
            device.put("requiresNativePlugin", false);
            device.put("hasUsbPermission", false);
            addFov(characteristics, device);
            device.put("capabilities", getCapabilities(characteristics));

            devices.put(device);
            count++;
        }

        status.append("Built-in cameras: ").append(count).append(". ");
    }

    private static void addUsbDevices(Activity activity, JSONArray devices, StringBuilder status) throws JSONException {
        final UsbManager usbManager = (UsbManager) activity.getSystemService(Context.USB_SERVICE);
        if (usbManager == null) {
            status.append("UsbManager unavailable. ");
            return;
        }

        int count = 0;
        for (Map.Entry<String, UsbDevice> entry : usbManager.getDeviceList().entrySet()) {
            final UsbDevice usbDevice = entry.getValue();
            if (!looksLikeVideoDevice(usbDevice)) {
                continue;
            }

            final JSONObject device = new JSONObject();
            device.put("id", "usb:" + usbDevice.getDeviceId());
            device.put("displayName", buildUsbDisplayName(usbDevice));
            device.put("kind", "UsbUvc");
            device.put("androidCameraId", "");
            device.put("unityWebCamName", "");
            device.put("vendorId", usbDevice.getVendorId());
            device.put("productId", usbDevice.getProductId());
            device.put("horizontalFov", -1.0);
            device.put("verticalFov", -1.0);
            device.put("isBuiltIn", false);
            device.put("isUsb", true);
            device.put("canUseUnityWebCamTexture", false);
            device.put("requiresNativePlugin", true);
            device.put("hasUsbPermission", usbManager.hasPermission(usbDevice));
            device.put("capabilities", new JSONArray());
            devices.put(device);
            count++;
        }

        status.append("USB/UVC candidates: ").append(count)
            .append(". Resolution/FOV discovery for UVC devices requires a native/plugin dependency.");
    }

    private static JSONArray getCapabilities(CameraCharacteristics characteristics) throws JSONException {
        final JSONArray capabilities = new JSONArray();
        final StreamConfigurationMap map = characteristics.get(CameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);
        if (map == null) {
            return capabilities;
        }

        final Size[] sizes = map.getOutputSizes(SurfaceTexture.class);
        if (sizes == null) {
            return capabilities;
        }

        final Set<Integer> fpsOptions = new LinkedHashSet<>();
        final Range<Integer>[] fpsRanges = characteristics.get(CameraCharacteristics.CONTROL_AE_AVAILABLE_TARGET_FPS_RANGES);
        if (fpsRanges != null) {
            for (Range<Integer> range : fpsRanges) {
                fpsOptions.add(range.getUpper());
            }
        }

        for (Size size : sizes) {
            final JSONObject capability = new JSONObject();
            capability.put("width", size.getWidth());
            capability.put("height", size.getHeight());

            final JSONArray fps = new JSONArray();
            for (Integer value : fpsOptions) {
                fps.put(value);
            }
            capability.put("fpsOptions", fps);
            capabilities.put(capability);
        }

        return capabilities;
    }

    private static void addFov(CameraCharacteristics characteristics, JSONObject device) throws JSONException {
        final float[] focalLengths = characteristics.get(CameraCharacteristics.LENS_INFO_AVAILABLE_FOCAL_LENGTHS);
        final android.util.SizeF sensorSize = characteristics.get(CameraCharacteristics.SENSOR_INFO_PHYSICAL_SIZE);
        if (focalLengths == null || focalLengths.length == 0 || sensorSize == null) {
            device.put("horizontalFov", -1.0);
            device.put("verticalFov", -1.0);
            return;
        }

        final float focalLength = focalLengths[0];
        final double horizontalFov = 2.0d * Math.toDegrees(Math.atan(sensorSize.getWidth() / (2.0d * focalLength)));
        final double verticalFov = 2.0d * Math.toDegrees(Math.atan(sensorSize.getHeight() / (2.0d * focalLength)));
        device.put("horizontalFov", horizontalFov);
        device.put("verticalFov", verticalFov);
    }

    private static int getLensFacing(CameraCharacteristics characteristics) {
        final Integer value = characteristics.get(CameraCharacteristics.LENS_FACING);
        return value != null ? value : -1;
    }

    private static boolean looksLikeVideoDevice(UsbDevice usbDevice) {
        if (usbDevice.getDeviceClass() == UsbConstants.USB_CLASS_VIDEO) {
            return true;
        }

        for (int index = 0; index < usbDevice.getInterfaceCount(); index++) {
            final UsbInterface usbInterface = usbDevice.getInterface(index);
            if (usbInterface.getInterfaceClass() == UsbConstants.USB_CLASS_VIDEO) {
                return true;
            }
        }

        return false;
    }

    private static String buildUsbDisplayName(UsbDevice usbDevice) {
        final String productName = Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP ? usbDevice.getProductName() : null;
        final String manufacturerName = Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP ? usbDevice.getManufacturerName() : null;

        if (productName != null && !productName.isEmpty()) {
            return manufacturerName != null && !manufacturerName.isEmpty()
                ? manufacturerName + " " + productName
                : productName;
        }

        return "USB/UVC camera " + usbDevice.getVendorId() + ":" + usbDevice.getProductId();
    }

    private static void sendStatus(String unityObjectName, String callbackMethodName, String message) {
        if (unityObjectName == null || unityObjectName.isEmpty() || callbackMethodName == null || callbackMethodName.isEmpty()) {
            return;
        }

        UnityPlayer.UnitySendMessage(unityObjectName, callbackMethodName, message);
    }
}
