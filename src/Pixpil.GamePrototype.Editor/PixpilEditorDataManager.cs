using System.IO;
using Murder;
using Murder.Assets;
using Murder.Diagnostics;
using Murder.Editor.Data;
using Murder.Editor.Data.Graphics;
using Murder.Serialization;
using Pixpil.Assets;
using Pixpil.Editor.Data.Graphics;
using Pixpil.Services;


namespace Pixpil.Editor.Data;

public partial class PixpilEditorDataManager : EditorDataManager {

	public PixpilEditorDataManager( IMurderGame game ) : base( game ) {}

	public override void LoadContent() {
		ConvertBitmapFontToSpriteFont();
		base.LoadContent();
		
		ItemTypeServices.Initialize();
	}
	

	internal void ConvertBitmapFontToSpriteFont() {
		string fntFontsPath = FileHelper.GetPath( EditorSettings.RawResourcesPath, Game.Profile.FontsPath );
		if ( !Directory.Exists( fntFontsPath ) ) {
			// No font directory, so skip.
			return;
		}
		
		// Load the "config" file with all the fonts settings.
		FontLookup lookup = new( fntFontsPath + "fonts.murder" );

		var fntFiles = Directory.GetFiles( fntFontsPath, "*.fnt", SearchOption.AllDirectories );
		foreach ( var fntFile in fntFiles ) {
			var fontName = Path.GetFileNameWithoutExtension( fntFile );

			if ( lookup.GetInfo( fontName + ".fnt" ) is FontLookup.FontInfo info ) {
				if ( BMFontImporter.GenerateFontJsonAndPng( info.Index, fntFile, info.Offset, fontName ) ) {
					GameLogger.Log( $"Converting {fntFile}..." );
				}
			}
			else {
				GameLogger.Warning( $"File {fntFile} doesn't having a matching name in fonts.murder. Maybe there's a typo?" );
			}
		}
	}

	protected override void OnAssetLoadError( GameAsset asset ) {
		if ( asset is GoapScenarioAsset goapScenarioAsset ) {
			goapScenarioAsset.Name = goapScenarioAsset.Name;
		}
	}
}
