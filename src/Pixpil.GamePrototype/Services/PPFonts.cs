using System.Numerics;
using Murder.Core.Geometry;
using Murder.Core.Graphics;
using Murder.Services;
using Murder.Utilities.Attributes;


namespace Pixpil.Services;
	
[Font]
public enum PPFonts {
	// SmallFont = 103,
	PixelFont = 104,
	// LargeFont = 105,
	ZPix = 106,
	FusionPixel = 107,
	// Meslo = 108,
}


public static class PPFontsExtensions {
	
	public static Point DrawText( this Batch2D uiBatch, PPFonts font, string text, Vector2 position, DrawInfo? drawInfo = default ) =>
		RenderServices.DrawText( uiBatch, ( int )font, text, position, -1, -1, drawInfo ?? DrawInfo.Default );


	public static Point DrawText( this Batch2D uiBatch, PPFonts font, string text, Vector2 position, int maxWidth, int visibleCharacters, DrawInfo? drawInfo = default ) =>
		RenderServices.DrawText( uiBatch, ( int )font, text, position, maxWidth, visibleCharacters,
			drawInfo ?? DrawInfo.Default );

}
