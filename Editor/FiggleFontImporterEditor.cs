using System;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

namespace Figgle.Unity.Editor
{
    [CustomEditor(typeof(FiggleFontImporter))]
    public sealed class FiggleFontImporterEditor : ScriptedImporterEditor
    {
        const string PreviewTextKey = "FiggleFont.Unity.PreviewText";
        const string PreviewSizeKey = "FiggleFont.Unity.PreviewSize";
        protected override bool needsApplyRevert => false;
        public override bool showImportedObject => false;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var path = ((FiggleFontImporter)target).assetPath;
            root.Add(new Button(() => AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(path))) { text = "Open .flf source" });

            FiggleFont font;
            try
            {
                font = FiggleFontParser.ParseString(File.ReadAllText(FileUtil.GetPhysicalPath(path)));
            }
            catch (Exception exception) when (exception is FiggleException || exception is IOException)
            {
                root.Add(new HelpBox(exception.Message, HelpBoxMessageType.Error));
                return root;
            }

            root.Add(new Label($"Height: {font.Height} rows   Baseline: {font.Baseline}   Direction: {font.Direction}"));
            var input = new TextField("Preview text")
            {
                name = "figgle-input",
                value = SessionState.GetString(PreviewTextKey, "Hello 123"),
                maxLength = 256
            };
            root.Add(input);
            var size = new SliderInt("Preview size", 8, 32) { value = SessionState.GetInt(PreviewSizeKey, 16) };
            root.Add(size);

            var output = new Label { name = "figgle-output", enableRichText = false };
            output.style.whiteSpace = WhiteSpace.NoWrap;
            output.style.unityFontDefinition = FontDefinition.FromFont((Font)EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf"));
            output.style.fontSize = size.value;
            output.style.color = new Color(0.9f, 0.9f, 0.9f);
            var scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scroll.style.minHeight = 100;
            scroll.style.maxHeight = 400;
            scroll.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            scroll.Add(output);
            root.Add(scroll);

            var dimensions = new Label();
            root.Add(dimensions);
            var error = new HelpBox(string.Empty, HelpBoxMessageType.Error) { name = "figgle-error" };
            root.Add(error);
            var copy = new Button(() => EditorGUIUtility.systemCopyBuffer = output.text) { text = "Copy rendered text" };
            root.Add(copy);

            void Render(string text)
            {
                try
                {
                    output.text = font.Render(text);
                    var rows = output.text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
                    var width = 0;
                    foreach (var row in rows) width = Math.Max(width, row.Length);
                    dimensions.text = $"{width} columns × {(output.text.Length == 0 ? 0 : rows.Length)} rows";
                    error.style.display = DisplayStyle.None;
                    copy.SetEnabled(output.text.Length > 0);
                }
                catch (FiggleException exception)
                {
                    output.text = string.Empty;
                    dimensions.text = string.Empty;
                    error.text = exception.Message;
                    error.style.display = DisplayStyle.Flex;
                    copy.SetEnabled(false);
                }
            }

            input.RegisterValueChangedCallback(change =>
            {
                SessionState.SetString(PreviewTextKey, change.newValue);
                Render(change.newValue);
            });
            size.RegisterValueChangedCallback(change =>
            {
                SessionState.SetInt(PreviewSizeKey, change.newValue);
                output.style.fontSize = change.newValue;
            });
            Render(input.value);
            return root;
        }
    }
}
