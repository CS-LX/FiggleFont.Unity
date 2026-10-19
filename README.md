# FiggleFont.Unity

[中文说明](README.zh-CN.md)

A small Unity Package Manager integration for [Figgle](https://github.com/drewnoakes/figgle).
Import real `.flf` files, preview their output in the Inspector, and use the imported `TextAsset` with Figgle at runtime.

## Requirements

- Unity **2022.3 LTS** or newer. Tested on 2022.3.62f3; other versions are not yet verified.
- **Figgle 0.6.6**, targeting **.NET Standard 2.0**. The upstream assembly is named `Figgle.dll`, not `FiggleFont.dll`.
- `Figgle.Fonts` is optional and is only needed for Figgle's built-in font collection.

This package contains editor tooling only. It does not bundle Figgle binaries, a font collection, a second renderer, or a dependency on a particular UI framework at runtime.

## Installation

Install **one** copy of Figgle using either route below, **before** installing this package.

### Option A: NuGetForUnity

1. Install [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity).
2. In **NuGet → Manage NuGet Packages**, install `Figgle` version `0.6.6`.
3. Optionally install `Figgle.Fonts` version `0.6.6` if you need built-in fonts.

### Option B: Manual DLL

1. Download `Figgle` version `0.6.6` from [NuGet](https://www.nuget.org/packages/Figgle/0.6.6).
2. Open the `.nupkg` as a ZIP and copy `lib/netstandard2.0/Figgle.dll` into `Assets/Plugins/Figgle/`.
3. In Unity's Plugin Inspector, allow the Editor and your target platforms. Keep **Auto Reference** enabled for scripts in predefined assemblies.

Do not install both copies: duplicate assemblies cause compilation errors. Figgle 0.6.6's .NET Standard 2.0 core has no additional NuGet dependencies.

### Install the Unity package

In **Window → Package Manager → + → Add package from git URL**, enter:

```text
https://github.com/CS-LX/FiggleFont.Unity.git#v0.1.0
```

Or add this entry to `Packages/manifest.json`:

```json
"com.linxium.figglefont.unity": "https://github.com/CS-LX/FiggleFont.Unity.git#v0.1.0"
```

UPM does not resolve NuGet dependencies. If Unity reports that `Figgle.dll` is missing, finish the dependency installation first.

## Import and preview

Drop a UTF-8 FIGlet `.flf` file into `Assets/` and select it. The Inspector offers:

- Editable preview text, with Figgle's own layout and spacing.
- Monospaced output, horizontal/vertical scrolling, and an 8–32 point size control.
- Font height, baseline, text direction, and output dimensions in UTF-16 columns.
- **Copy rendered text** and **Open .flf source**.
- Clear import/preview errors for malformed fonts or unsupported text.

The inspector uses **UI Toolkit**. Preview settings are editor-session preferences; they do not modify the font or get shipped to players. Preview input is limited to 256 characters to keep the Inspector responsive. Empty glyph slots may render blank, according to Figgle's normal behavior.

The imported asset remains a `TextAsset`, including when validation fails, so the source can be inspected and repaired. Fix import errors before using that font at runtime. Editing the source triggers Unity reimport, while stable subasset identity keeps references intact. Sources in UPM packages are supported too.

## Runtime use

Reference the imported font directly in the Inspector:

```csharp
using Figgle;
using UnityEngine;

public sealed class Banner : MonoBehaviour
{
    [SerializeField] TextAsset fontSource;
    FiggleFont font;

    void Awake()
    {
        font = FiggleFontParser.ParseString(fontSource.text);
        Debug.Log(font.Render("Hello 123"));
    }
}
```

Or put a font at `Assets/Resources/Fonts/MyFont.flf` and load it with:

```csharp
var source = Resources.Load<TextAsset>("Fonts/MyFont");
var font = FiggleFontParser.ParseString(source.text);
var banner = font.Render("Hello 123");
```

Parse once and reuse the `FiggleFont` for repeated rendering. The package preserves the source and Figgle output exactly; casing, spacing, trimming, caching, and presenting the result remain the caller's choice. Display the result with a monospaced font that contains the glyphs your `.flf` uses. Disable rich text if the output contains literal markup characters.

With custom `.asmdef` files using **Override References**, add `Figgle.dll` to **Precompiled References**. No reference to `FiggleFont.Unity.Editor` is needed in runtime assemblies.

## Tests

To expose the package's EditMode tests, add `com.linxium.figglefont.unity` to `testables` in your project manifest and install Unity Test Framework. Run the `Figgle.Unity.Tests` suite. The tests create isolated temporary font assets and remove them afterward; they do not depend on a game's fonts or scenes.

The live UI Toolkit panel test requires a graphics device and is skipped with `-nographics`. The four import tests support headless execution; omit `-nographics` to run all five.

## License

The Unity integration is [MIT licensed](LICENSE.md) and is an independent project, not an official Figgle distribution. [Figgle](https://github.com/drewnoakes/figgle) is separately distributed under Apache-2.0. Third-party font files retain their own licenses; no third-party fonts or DLLs are included here.
