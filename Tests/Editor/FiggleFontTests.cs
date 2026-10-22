using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Figgle.Unity.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Figgle.Unity.Tests
{
    public sealed class FiggleFontTests
    {
        const string FixturePath = "Packages/com.linxium.figglefont.unity/Tests/Editor/Fixtures/Bracket.flf";
        string path;
        string source;

        [SetUp]
        public void SetUp()
        {
            source = File.ReadAllText(FileUtil.GetPhysicalPath(FixturePath));
            // Do not reuse an asset identity whose import diagnostics may still be queued.
            path = $"Assets/FiggleFontTest_{System.Guid.NewGuid():N}.flf";
            Import(source);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (!string.IsNullOrEmpty(path)) AssetDatabase.DeleteAsset(path);
            // Let asset deletion and delayed import notifications settle before the next test.
            yield return null;
        }

        [Test]
        public void Import_PreservesUnicodeSourceAsTextAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            Assert.That(AssetImporter.GetAtPath(path), Is.TypeOf<FiggleFontImporter>());
            Assert.That(asset.name, Is.EqualTo(Path.GetFileNameWithoutExtension(path)));
            Assert.That(asset.text, Is.EqualTo(source));
            Assert.That(FiggleFontParser.ParseString(asset.text).Render("AÄ").TrimEnd('\r', '\n'), Is.EqualTo("[A][Ä]"));
        }

        [Test]
        public void PackageFont_ImportsThroughResolvedPackagePath()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(FixturePath);
            Assert.That(asset, Is.Not.Null);
            Assert.That(asset.text, Is.EqualTo(source));
            Assert.That(FiggleFontParser.ParseString(asset.text).Render("AB").TrimEnd('\r', '\n'), Is.EqualTo("[A][B]"));
        }

        [Test]
        public void Reimport_UpdatesGlyphsAndPreservesReferences()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long fileId), Is.True);
            Import(source.Replace("[A]", "{A}"));
            asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            Assert.That(FiggleFontParser.ParseString(asset.text).Render("AB").TrimEnd('\r', '\n'), Is.EqualTo("{A}[B]"));
            Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string newGuid, out long newFileId), Is.True);
            Assert.That(newGuid, Is.EqualTo(guid));
            Assert.That(newFileId, Is.EqualTo(fileId));
        }

        [UnityTest]
        public IEnumerator InvalidFont_ReportsErrorAndKeepsSourceAvailableForRepair()
        {
            LogAssert.Expect(LogType.Error, new Regex("Invalid FIGlet font:"));
            Import("invalid font");
            // ForceSynchronousImport completes the import, not necessarily the queued log delivery.
            yield return null;
            LogAssert.NoUnexpectedReceived();
            Assert.That(AssetDatabase.LoadAssetAtPath<TextAsset>(path).text, Is.EqualTo("invalid font"));
            var editor = UnityEditor.Editor.CreateEditor(AssetImporter.GetAtPath(path));
            try
            {
                Assert.That(editor.CreateInspectorGUI().Q<HelpBox>().messageType, Is.EqualTo(HelpBoxMessageType.Error));
            }
            finally
            {
                Object.DestroyImmediate(editor);
            }
        }

        [UnityTest]
        public IEnumerator Preview_UpdatesThroughFiggleAndKeepsMarkupLiteral()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("The UI Toolkit panel test requires a graphics device; run without -nographics.");
            var previousText = SessionState.GetString("FiggleFont.Unity.PreviewText", "Hello 123");
            var editor = UnityEditor.Editor.CreateEditor(AssetImporter.GetAtPath(path));
            var window = ScriptableObject.CreateInstance<FigglePreviewTestWindow>();
            try
            {
                Assert.That(editor, Is.TypeOf<FiggleFontImporterEditor>());
                var root = editor.CreateInspectorGUI();
                window.rootVisualElement.Add(root);
                window.ShowUtility();
                yield return null;
                var input = root.Q<TextField>("figgle-input");
                var output = root.Q<Label>("figgle-output");
                input.value = "AB";
                Assert.That(output.text.TrimEnd('\r', '\n'), Is.EqualTo("[A][B]"));
                input.value = "<>";
                Assert.That(output.text.TrimEnd('\r', '\n'), Is.EqualTo("[<][>]"));
                Assert.That(output.enableRichText, Is.False);
                input.value = string.Empty;
                Assert.That(output.text.TrimEnd('\r', '\n'), Is.Empty);
            }
            finally
            {
                SessionState.SetString("FiggleFont.Unity.PreviewText", previousText);
                window.Close();
                Object.DestroyImmediate(editor);
            }
        }

        void Import(string text)
        {
            File.WriteAllText(path, text);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }
    }

    sealed class FigglePreviewTestWindow : EditorWindow { }
}
