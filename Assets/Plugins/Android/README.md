# Android plugin scaffold

This Unity project includes a placeholder Android library at:

- `Assets/Plugins/Android/UvcUnityBridge.androidlib/`

## What is implemented now

- Built-in Android camera enumeration through `CameraManager` / `Camera2`
- USB device enumeration through `UsbManager`
- Detection of likely USB/UVC devices by USB video class
- Built-in camera capability listing (preview sizes and AE FPS ranges)
- Built-in camera FOV estimation from `SENSOR_INFO_PHYSICAL_SIZE` and `LENS_INFO_AVAILABLE_FOCAL_LENGTHS`
- Unity bridge integration point at `com.tetti.uvcunity.CameraBridge`

## What is scaffolded / placeholder only

- USB/UVC capability discovery (resolution/FPS/FOV) beyond simple USB device detection
- Native preview streaming into Unity for USB/UVC devices
- External texture / `SurfaceTexture` bridging into Unity `RawImage`
- USB attach/detach broadcast handling beyond what Android provides during the current session

## Where to place `.aar` files

Preferred location for future native/UVC dependencies:

- `Assets/Plugins/Android/UvcUnityBridge.androidlib/libs/`

The included `build.gradle` already contains:

```gradle
implementation fileTree(dir: 'libs', include: ['*.jar', '*.aar'])
```

If your vendor library requires explicit declarations, add them to `Assets/Plugins/Android/UvcUnityBridge.androidlib/build.gradle`, for example:

```gradle
implementation(name: 'uvccamera-release', ext: 'aar')
implementation(name: 'usb-camera-common', ext: 'aar')
```

You can also place standalone `.aar` files directly under `Assets/Plugins/Android/` if preferred, but keeping them inside the `.androidlib/libs/` folder keeps the bridge module self-contained.
