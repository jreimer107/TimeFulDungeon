using System.IO;
using UnityEditor.AssetImporters;

namespace APIShift.AsepriteAnimationWorkflow
{
  [ScriptedImporter(1, new[] { "aseprite", "ase" })]
  public class AsepriteImporter : ScriptedImporter
  {
    public AsepriteImporterSettings Settings = new AsepriteImporterSettings();

    public override void OnImportAsset(AssetImportContext ctx)
    {
      var loader = new AsepriteLoader();
      var file = loader.LoadFile(ctx.assetPath);
      var name = Path.GetFileNameWithoutExtension(ctx.assetPath);
      var assets = file.CreateAssets(Settings, name);
      assets.AddToContext(ctx);
    }
  }
}