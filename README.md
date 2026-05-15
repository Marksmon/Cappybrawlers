# Capybrawlers

A 2D simultaneous-turn tactical creature battler for iOS and Android. Collect capybara creatures called Capybrawlers, each with a permanent build of one Nature and three equipment pieces. Assemble teams of three, then outwit opponents by secretly committing actions that resolve in speed order — prediction and composition beat grinding every time.

## Prerequisites

Before cloning or opening the project, install:

| Tool | Notes |
|------|-------|
| [Unity Hub](https://unity.com/download) | Required to manage Unity Editor versions |
| Unity Editor LTS | Install via Unity Hub. Recommended: Unity 6 (`6000.x`) or Unity 2022.3 LTS. Include **Android Build Support** (Android SDK/NDK Tools + OpenJDK) and **iOS Build Support** modules. |
| [Git LFS](https://git-lfs.com/) | Run `git lfs install` once before cloning |
| Rider or Visual Studio 2022 | VS 2022 needs the "Game development with Unity" workload |
| Xcode 15+ (macOS only) | Required for iOS builds — iOS builds **cannot** be done on Windows |

## First-Time Setup

```bash
# 1. Ensure Git LFS is installed globally
git lfs install

# 2. Clone the repo
git clone https://github.com/Marksmon/Cappybrawlers.git
cd Cappybrawlers
```

Then in **Unity Hub**:
1. Click **Add** → **Add project from disk** → select the cloned `Cappybrawlers/` folder
2. Open with the correct Unity LTS version (install it via Unity Hub if not present)
3. Wait for the initial import to complete

## Important: Steps After First Open

Do these **before writing any code**, in order:

1. **Force Text serialization** — Edit > Project Settings > Editor > Asset Serialization Mode → `Force Text`
   This makes `.unity`, `.prefab`, and `.asset` files text-diffable YAML.

2. **Set player identity** — Edit > Project Settings > Player:
   - Company Name: `Capybrawlers`
   - Product Name: `Capybrawlers`
   - Bundle ID (iOS + Android): `com.capybrawlers.game`

3. **Install UPM packages** — Window > Package Manager, then add:
   - Universal Render Pipeline (`com.unity.render-pipelines.universal`)
   - Input System (`com.unity.inputsystem`)
   - Addressables (`com.unity.addressables`)
   - Unity IAP (`com.unity.purchasing`)
   - TextMeshPro (`com.unity.textmeshpro`)
   - Cinemachine (`com.unity.cinemachine`)
   - Test Framework (`com.unity.test-framework`)

4. **Create the `Assets/_Game/` folder hierarchy** as documented in [CLAUDE.md](CLAUDE.md)

5. Commit this baseline as the first real commit

## Project Structure

See [CLAUDE.md](CLAUDE.md) for the full folder structure, C# conventions, and system architecture.

See [docs/](docs/) for game design, art pipeline, and build documentation.

## Building

### Android (Windows or macOS)
Set build target in **File > Build Settings > Android**, connect a device or configure an emulator, then click **Build**.

For CI headless builds, see the CLI commands in [CLAUDE.md](CLAUDE.md#build-instructions).

### iOS (macOS only)
Set build target to **iOS** in Build Settings. Unity exports an Xcode project; open it in Xcode to archive and submit to TestFlight or the App Store.

iOS builds require a paid Apple Developer account and valid provisioning profiles. See [docs/BUILD_PROFILES.md](docs/BUILD_PROFILES.md) for details.

## Contributing

- Branch off `main` for all features: `feature/<name>`, `fix/<name>`
- No direct pushes to `main`
- Follow the C# conventions in [CLAUDE.md](CLAUDE.md)
- Run PlayMode and EditMode tests before opening a PR: Window > General > Test Runner
