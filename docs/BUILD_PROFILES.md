# Cappybrawlers — Build Profiles

## Build Configurations

Two configurations are used across all platforms:

| Config | Unity Build Options | Define Symbols | Notes |
|---|---|---|---|
| Development | `Development Build` checked | `DEVELOPMENT_BUILD`, `ENABLE_CHEATS` | Profiler attached, verbose logging, cheat menu |
| Production | None | _(none)_ | Stripped, IL2CPP, all logging disabled |

Never ship a Development build to stores. CI should always produce Production builds for release tracks.

## Scripting Backend

Both platforms must use **IL2CPP** for production. Mono is acceptable for local dev builds to speed up iteration.

| Platform | Production Backend | Dev Backend |
|---|---|---|
| Android | IL2CPP, ARM64 + ARMv7 | Mono (faster builds) |
| iOS | IL2CPP (required by Apple) | IL2CPP (no choice) |

## Android

### Bundle ID
`com.cappybrawlers.game`

### Keystore Setup
Never commit the keystore file or its passwords to git. Store them in environment variables or a secrets manager.

1. Generate keystore (one-time):
   ```powershell
   keytool -genkey -v -keystore cappybrawlers.keystore `
     -alias cappybrawlers -keyalg RSA -keysize 2048 -validity 10000
   ```
2. Store the `.keystore` file outside the repo (e.g., `C:\keys\cappybrawlers.keystore`)
3. In Unity: Edit > Project Settings > Player > Android > Publishing Settings:
   - Keystore path → point to the file above
   - Keystore password → from env var `ANDROID_KEYSTORE_PASS`
   - Key alias password → from env var `ANDROID_KEY_PASS`

### Build Output
Produce an `.aab` (Android App Bundle) for Play Store submissions — not `.apk`.

In Edit > Project Settings > Player > Android: check **Build App Bundle (Google Play)**.

### Play Store Release Tracks

| Track | Audience | When to Use |
|---|---|---|
| Internal Testing | Team only (up to 100 testers) | Every merge to `main` |
| Closed Testing (Alpha) | Invited testers | Feature-complete milestone |
| Open Testing (Beta) | Public opt-in | Pre-launch |
| Production | All users | Launch / updates |

### Android Minimum API Level
- Minimum: API 23 (Android 6.0 Marshmallow)
- Target: API 35 (latest, required by Play Store policy)

## iOS

### Bundle ID
`com.cappybrawlers.game`

### Apple Developer Account Requirements
- Paid Apple Developer Program membership ($99/year)
- App ID registered at developer.apple.com matching the Bundle ID
- Development and Distribution certificates in Keychain
- Provisioning profiles:
  - **Development**: for device testing
  - **Distribution (App Store)**: for TestFlight and App Store submission

### Build Workflow

Unity exports an **Xcode project** (not a final `.ipa`). The final `.ipa` is produced by Xcode.

```
Unity (Windows or macOS) → Export Xcode project
         ↓
macOS machine with Xcode
         ↓
Archive (Product > Archive)
         ↓
Distribute (Xcode Organizer)
    ├── TestFlight (internal/external beta)
    └── App Store submission
```

**iOS builds require a Mac** — Unity can export on Windows, but Xcode archiving requires macOS.

### TestFlight Workflow
1. Archive in Xcode → Distribute App → App Store Connect → Upload
2. In App Store Connect, add the build to a TestFlight group
3. Internal testers get access immediately; external testers require Apple review (~1 day)

### iOS Minimum Deployment Target
iOS 15.0

## Scripting Define Symbols

Define symbols are set in Edit > Project Settings > Player > Other Settings > Scripting Define Symbols, separated by semicolons.

| Symbol | Used For |
|---|---|
| `DEVELOPMENT_BUILD` | Enables verbose logging, dev-only UI overlays |
| `ENABLE_CHEATS` | Unlocks cheat menu (infinite currency, skip battles) |
| `DISABLE_IAP` | Disables real IAP for testing; simulates purchases |
| `MOCK_NETWORK` | Uses mock matchmaking/server responses |

## Headless CI Build Script

The `Assets/Editor/BuildPipeline.cs` editor script exposes static methods for CI:

```csharp
// Android
Unity.exe -batchmode -quit \
  -projectPath "c:\Source\repos\Cappybrawlers\Cappybrawlers" \
  -buildTarget Android \
  -executeMethod Cappybrawlers.Editor.BuildPipeline.BuildAndroid \
  -logFile build_android.log

// iOS (on macOS CI runner)
Unity.app/Contents/MacOS/Unity -batchmode -quit \
  -projectPath "/path/to/Cappybrawlers" \
  -buildTarget iOS \
  -executeMethod Cappybrawlers.Editor.BuildPipeline.BuildIOS \
  -logFile build_ios.log
```

The `BuildPipeline.cs` implementation uses `UnityEditor.BuildPipeline.BuildPlayer()` and reads keystore credentials from environment variables to avoid hardcoding secrets.

## Environment Variables (CI / Local)

| Variable | Value | Used By |
|---|---|---|
| `ANDROID_KEYSTORE_PATH` | Path to `.keystore` file | Android builds |
| `ANDROID_KEYSTORE_PASS` | Keystore password | Android builds |
| `ANDROID_KEY_ALIAS` | Key alias name | Android builds |
| `ANDROID_KEY_PASS` | Key alias password | Android builds |
| `UNITY_LICENSE` | Unity license file contents (base64) | CI headless activation |
| `UNITY_EMAIL` | Unity account email | CI activation |
| `UNITY_PASSWORD` | Unity account password | CI activation |

Never commit values for any of these. Use GitHub Actions secrets or equivalent.
