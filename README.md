# UVCUnity scaffold for Unity 6000.3.11f1

This repository now contains a minimal Unity project scaffold for **Unity 6000.3.11f1** focused on **Android camera discovery** and **`RawImage` preview hookup**.

## What is implemented now

- Minimal Unity project structure (`Assets/`, `Packages/`, `ProjectSettings/`)
- Unity-side camera models for devices, capabilities, and preview state
- Android bridge wrapper using `AndroidJavaClass` / `AndroidJavaObject`
- Android placeholder plugin scaffold under `Assets/Plugins/Android/`
- Runtime UI controller logic for:
  - Refresh / scan devices
  - Camera selector
  - Resolution selector
  - Start preview
  - Stop preview
  - Status text
- `RawImage` preview view component with placeholder texture fallback
- Editor menu item to generate a demo scene quickly:
  - `Tools > UVC Unity > Create Demo Scene`
- Android native-side enumeration scaffold for:
  - built-in cameras via `Camera2`
  - USB/UVC candidate devices via `UsbManager`
- Built-in camera capability listing and FOV estimation where Android exposes it
- Editor / non-Android fallback that uses `WebCamTexture.devices` or placeholder devices

## What is scaffolded / placeholder only

- True USB/UVC camera streaming into Unity on Android
- USB/UVC resolution/FPS/FOV extraction from vendor-specific or third-party UVC libraries
- External texture / `SurfaceTexture` native preview path into the `RawImage`
- Hot-plug lifecycle handling beyond the basic refresh / rescan workflow

This is deliberate: **WebCamTexture alone is not sufficient for reliable USB/UVC support on Android**, so the project keeps the Unity UI layer stable while leaving clean integration points for future native plugin work.

## Repository layout

- `Assets/Scripts/Models/` - shared camera device and capability models
- `Assets/Scripts/Runtime/AndroidCameraBridge.cs` - Unity Android bridge / fallback wrapper
- `Assets/Scripts/UI/` - `RawImage` preview hookup and UI controller
- `Assets/Scripts/Editor/UvcDemoSceneCreator.cs` - menu item that builds a demo scene in-editor
- `Assets/Plugins/Android/UvcUnityBridge.androidlib/` - Android plugin scaffold and Java bridge

## Demo scene setup

1. Open the project in **Unity 6000.3.11f1**.
2. Switch the build target to **Android** if Unity prompts you.
3. Use **Tools > UVC Unity > Create Demo Scene**.
4. Open the generated scene at `Assets/Scenes/UvcCameraDemo.unity`.
5. Enter Play Mode.
6. Use the UI controls:
   - **Refresh / Scan Devices**
   - **Camera** dropdown
   - **Resolution** dropdown
   - **Start Preview**
   - **Stop Preview**

The generated demo scene is intentionally simple and targets a `RawImage` preview workflow.

## Android permissions and USB camera support

The Android bridge manifest currently requests / declares:

- `android.permission.CAMERA`
- `android.hardware.camera.any` (optional)
- `android.hardware.usb.host` (optional)

### Built-in cameras

Built-in cameras are enumerated through **Camera2**. For these devices the placeholder bridge already reports:

- camera IDs
- lens facing labels
- preview output sizes
- AE FPS ranges
- estimated horizontal / vertical FOV when Android exposes focal length + sensor size

### USB / UVC cameras

USB devices are enumerated through **UsbManager** and filtered for likely video/UVC devices using the USB video class. This scaffold can currently report:

- USB vendor ID / product ID
- product / manufacturer name when Android exposes them
- whether Android has granted USB permission for that device

However, reliable **USB/UVC preview streaming** and deeper **capability discovery** generally require a dedicated Android/UVC library or native implementation. The scaffold is designed so those dependencies can be added **without changing the Unity UI layer**.

## Where `.aar` files should be placed

Preferred location:

- `Assets/Plugins/Android/UvcUnityBridge.androidlib/libs/`

See `Assets/Plugins/Android/README.md` for the intended dependency wiring.

## Known limitations

- **USB/UVC preview is placeholder-only** until third-party/native Android dependencies are added.
- **FOV is often unavailable for USB/UVC cameras** on Android. Built-in cameras can often estimate FOV from Camera2 metadata, but many USB webcams do not expose equivalent information in a reliable or portable way.
- `WebCamTexture` preview remains useful as an editor-safe fallback and for some cameras, but it should not be treated as a robust Android UVC solution.
- Resolution and FPS capability discovery for USB/UVC devices is intentionally left as an integration point because Android support is highly device- and library-dependent.

## Next integration step for real UVC support

1. Add your native/UVC `.aar` dependencies under `Assets/Plugins/Android/UvcUnityBridge.androidlib/libs/`.
2. Update `Assets/Plugins/Android/UvcUnityBridge.androidlib/build.gradle` with any explicit `implementation(name: ..., ext: 'aar')` lines required by the library.
3. Replace the placeholder `startPreview` / `stopPreview` implementation in `com.tetti.uvcunity.CameraBridge`.
4. Keep the Unity UI controller and `RawImagePreviewView` unchanged.

## Validation status

This repository did not contain any existing Unity test/build infrastructure before the scaffold was added, so there were no pre-existing automated tests to run. Validation for this PR focuses on repository structure, manifest/package correctness, and manual review of the integration points.
