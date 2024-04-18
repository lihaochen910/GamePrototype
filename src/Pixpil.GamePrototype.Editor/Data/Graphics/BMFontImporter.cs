using System;
using System.Collections.Generic;
using Murder.Assets.Graphics;
using Murder.Core.Geometry;
using Murder.Core.Graphics;
using Murder.Serialization;
using System.Collections.Immutable;
using System.IO;
using System.Xml;
using Murder;
using Murder.Diagnostics;
using Murder.Editor;


namespace Pixpil.Editor.Data.Graphics;

internal class BMFontImporter {
	
	public static string SourcePackedPath => FileHelper.GetPath(Architect.EditorSettings.SourcePackedPath, Game.Profile.FontsPath);

    public static bool GenerateFontJsonAndPng( int fontIndex, string fontPath, Point fontOffset, string name ) {
        string sourcePackedPath = SourcePackedPath;
		string binResourcesPath = FileHelper.GetPath( Architect.EditorSettings.BinResourcesPath, Game.Profile.FontsPath );

        string jsonFile = name + ".json";
        string pngFile = name + ".png";

		string jsonSourcePackedPath = Path.Join( sourcePackedPath, jsonFile );
		string pngSourcePackedPath = Path.Join( sourcePackedPath, pngFile );
		
		if ( File.Exists( jsonSourcePackedPath ) && File.Exists( pngSourcePackedPath ) ) {
			// File already exists.
			// TODO: Check for the font size at this point.
			return false;
		}

		FileHelper.CreateDirectoryPathIfNotExists( sourcePackedPath );
		string pngRawPath = Path.Combine( Path.GetDirectoryName( fontPath ), pngFile );
		if ( !File.Exists( pngRawPath ) ) {
			GameLogger.Error( "bmfont page filename must be same as .fnt file." );
			return false;
		}
		if ( !File.Exists( pngSourcePackedPath ) ) {
			File.Copy( pngRawPath, pngSourcePackedPath, true );
		}
		
        {
            var kernings = new List< Kerning >();
            var characters = new Dictionary< int , PixelFontCharacter >();
            
            var document = new XmlDocument();
			var pageData = new SortedDictionary< int, Page >();
			var kerningDictionary = new Dictionary< Kerning, int >();

			document.LoadXml( File.ReadAllText( fontPath ) );
			var root = document.DocumentElement;

			// load the basic attributes
			var properties = root.SelectSingleNode( "info" );
			// var FamilyName = properties.Attributes[ "face" ].Value;
			// var FontSize = Convert.ToInt32( properties.Attributes[ "size" ].Value );
			// var Bold = Convert.ToInt32( properties.Attributes[ "bold" ].Value ) != 0;
			// var Italic = Convert.ToInt32( properties.Attributes[ "italic" ].Value ) != 0;
			// var Unicode = properties.Attributes[ "unicode" ].Value != "0";
			// var StretchedHeight = Convert.ToInt32( properties.Attributes[ "stretchH" ].Value );
			// var Charset = properties.Attributes[ "charset" ].Value;
			// var Smoothed = Convert.ToInt32( properties.Attributes[ "smooth" ].Value ) != 0;
			// var SuperSampling = Convert.ToInt32( properties.Attributes[ "aa" ].Value );
			// var Padding = ParsePadding( properties.Attributes[ "padding" ].Value );
			// var Spacing = BitmapFontLoader.ParseInt2( properties.Attributes[ "spacing" ].Value );
			// var OutlineSize = properties.Attributes[ "outline" ] != null
			// 	? Convert.ToInt32( properties.Attributes[ "outline" ].Value )
			// 	: 0;

			// common attributes
			properties = root.SelectSingleNode( "common" );
			var baseHeight = Convert.ToInt32( properties.Attributes[ "base" ].Value );
			var lineHeight = Convert.ToInt32( properties.Attributes[ "lineHeight" ].Value );
			// var TextureSize = new Point( Convert.ToInt32( properties.Attributes[ "scaleW" ].Value ),
			// 	Convert.ToInt32( properties.Attributes[ "scaleH" ].Value ) );
			// var Packed = Convert.ToInt32( properties.Attributes[ "packed" ].Value ) != 0;
			//
			// var AlphaChannel = properties.Attributes[ "alphaChnl" ] != null
			// 	? Convert.ToInt32( properties.Attributes[ "alphaChnl" ].Value )
			// 	: 0;
			// var RedChannel = properties.Attributes[ "redChnl" ] != null
			// 	? Convert.ToInt32( properties.Attributes[ "redChnl" ].Value )
			// 	: 0;
			// var GreenChannel = properties.Attributes[ "greenChnl" ] != null
			// 	? Convert.ToInt32( properties.Attributes[ "greenChnl" ].Value )
			// 	: 0;
			// var BlueChannel = properties.Attributes[ "blueChnl" ] != null
			// 	? Convert.ToInt32( properties.Attributes[ "blueChnl" ].Value )
			// 	: 0;

			// load texture information
			foreach ( XmlNode node in root.SelectNodes( "pages/page" ) ) {
				var page = new Page();
				page.Id = Convert.ToInt32( node.Attributes[ "id" ].Value );
				page.Filename = node.Attributes[ "file" ].Value;

				pageData.Add( page.Id, page );
			}

			// var Pages = BitmapFontLoader.ToArray( pageData.Values );

			// load character information
			foreach ( XmlNode node in root.SelectNodes( "chars/char" ) ) {
				var character = new PixelFontCharacter();
				character.Character = ( char )Convert.ToInt32( node.Attributes[ "id" ].Value );
				character.Glyph = new Rectangle(
					Convert.ToInt32( node.Attributes[ "x" ].Value ),
					Convert.ToInt32( node.Attributes[ "y" ].Value ),
					Convert.ToInt32( node.Attributes[ "width" ].Value ),
					Convert.ToInt32( node.Attributes[ "height" ].Value )
				);
				character.XOffset = Convert.ToInt32( node.Attributes[ "xoffset" ].Value );
				character.YOffset = Convert.ToInt32( node.Attributes[ "yoffset" ].Value );
				character.XAdvance = Convert.ToInt32( node.Attributes[ "xadvance" ].Value );
				character.Page = Convert.ToInt32( node.Attributes[ "page" ].Value );
				// character.Channel = Convert.ToInt32( node.Attributes[ "chnl" ].Value );

				characters[ character.Character ] = character;
			}
			
			// loading kerning information
			foreach ( XmlNode node in root.SelectNodes( "kernings/kerning" ) ) {
				var key = new Kerning {
					First = Convert.ToInt32( node.Attributes[ "first" ].Value ),
					Second = Convert.ToInt32( node.Attributes[ "second" ].Value ),
					Amount = Convert.ToInt32( node.Attributes[ "amount" ].Value )
				};

				if ( !kerningDictionary.ContainsKey( key ) ) {
					kerningDictionary.Add( key, key.Amount );
				}
			}

			FontAsset fontAsset = new ( fontIndex, characters, kernings.ToImmutableArray(), lineHeight, fontPath, -baseHeight, fontOffset );

            // Save characters to JSON
			FileHelper.SaveSerialized( fontAsset, jsonSourcePackedPath, false );
		}

        // Copy files to binaries path.
		FileHelper.CreateDirectoryPathIfNotExists( binResourcesPath );
		File.Copy( pngSourcePackedPath, Path.Join( binResourcesPath, pngFile ), true );
		File.Copy( jsonSourcePackedPath, Path.Join( binResourcesPath, jsonFile ), true );

        return true;
    }


	/// <summary>
	/// Represents a texture page.
	/// </summary>
	private struct Page {
		public string Filename;
		public int Id;

		public Page( int id, string filename ) {
			Filename = filename;
			Id = id;
		}

		public override string ToString() => string.Format( "{0} ({1})", Id, Path.GetFileName( Filename ) );
	}
}
