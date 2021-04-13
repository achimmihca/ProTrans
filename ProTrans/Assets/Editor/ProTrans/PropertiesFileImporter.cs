using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;

[ScriptedImporter( 1, "properties" )]
public class SrtImporter : ScriptedImporter
{
	public override void OnImportAsset( AssetImportContext ctx )
	{
		TextAsset subAsset = new TextAsset( File.ReadAllText( ctx.assetPath, Encoding.UTF8 ) );
		ctx.AddObjectToAsset( "text", subAsset );
		ctx.SetMainObject( subAsset );
	}
}
