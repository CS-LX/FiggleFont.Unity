# FiggleFont.Unity

[English](README.md)

为 [Figgle](https://github.com/drewnoakes/figgle) 提供 Unity 原生 `.flf` 导入和 Inspector 预览。导入结果是 `TextAsset`，运行时直接交给 Figgle 绘制。

## 安装

要求 Unity **2022.3 LTS** 或更新版本，已验证版本为 **2022.3.62f3**。先通过以下任一方式安装 **Figgle 0.6.6**：

- **NuGetForUnity**：在 NuGet → Manage NuGet Packages 中安装 `Figgle`。仅使用内置字体库时才需要额外安装 `Figgle.Fonts`。
- **手动 DLL**：下载 [Figgle 0.6.6](https://www.nuget.org/packages/Figgle/0.6.6)，将 `.nupkg` 作为 ZIP 解压，把 `lib/netstandard2.0/Figgle.dll` 放入 `Assets/Plugins/Figgle/`。允许 Editor 和目标平台加载；使用默认程序集时保持 Auto Reference 开启。

实际程序集名称是 **Figgle.dll**。两种方式选一种，避免重复 DLL。UPM 不负责安装 NuGet 依赖，必须先完成上述步骤。

然后在 Package Manager 的 **Add package from git URL** 中输入：

```text
https://github.com/CS-LX/FiggleFont.Unity.git#v0.1.0
```

包标识：`com.linxium.figglefont.unity`。使用自定义 asmdef 且启用 Override References 时，把 `Figgle.dll` 加入 Precompiled References。

## 使用

将 UTF-8 编码的 `.flf` 放入项目并选中，可以输入预览文字、调整字号、滚动查看、复制结果、打开源文件，也能查看字体高度、基线、方向和输出尺寸。尺寸列数按 UTF-16 字符计算。预览输入最多 256 字符，使用 UI Toolkit 和等宽字体。

预览配置只在当前编辑器会话内保存，不修改字体资源。源文件修改后自动重新导入；Assets 和 UPM 包内的字体都支持。格式错误会在导入时和 Inspector 中显示，仍保留 TextAsset 以方便修复；使用前应先修复错误。未提供的字形槽位遵循 Figgle 自身行为，可能渲染为空白。

运行时可用 Inspector 引用 `TextAsset`；也可以把字体放在 `Assets/Resources/Fonts/MyFont.flf`，这样加载：

```csharp
using Figgle;
using UnityEngine;

// 在初始化阶段解析一次，之后复用 font。
var source = Resources.Load<TextAsset>("Fonts/MyFont");
var font = FiggleFontParser.ParseString(source.text);
var banner = font.Render("Hello 123");
```

本包不修改 Figgle 的大小写、字距、换行或拼字逻辑，不提供第二套绘制器。显示输出时使用覆盖相应字符的等宽字体；输出包含尖括号时关闭富文本解析。包本身仅包含 Editor 工具，不向运行时引入 UI 框架。

## 测试与许可

在项目 manifest 的 `testables` 中加入 `com.linxium.figglefont.unity`，安装 Unity Test Framework 后运行 `Figgle.Unity.Tests` EditMode 测试。测试不依赖任何游戏内容。

其中 UI Toolkit 面板测试需要图形设备，在 `-nographics` 模式下跳过；四项导入测试支持无图形模式。去掉 `-nographics` 可运行全部五项。

Unity 适配代码使用 [MIT 许可](LICENSE.md)。本项目是独立适配包；上游 Figgle 使用 Apache-2.0，各第三方字体遵循自己的许可。本包不附带第三方 DLL 或字体。
