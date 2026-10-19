using System.IO;
using Figgle;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Figgle.Unity.Editor
{
    [ScriptedImporter(1, "flf")]
    public sealed class FiggleFontImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var source = File.ReadAllText(FileUtil.GetPhysicalPath(context.assetPath));
            var font = new TextAsset(source) { name = Path.GetFileNameWithoutExtension(context.assetPath) };
            context.AddObjectToAsset("font", font);
            context.SetMainObject(font);
            try
            {
                FiggleFontParser.ParseString(source);
            }
            catch (FiggleException exception)
            {
                context.LogImportError($"Invalid FIGlet font: {exception.Message}", font);
            }
        }
    }
}
