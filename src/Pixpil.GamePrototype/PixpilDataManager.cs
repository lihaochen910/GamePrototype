using Murder;
using Murder.Data;
using Pixpil.Services;


namespace Pixpil.Editor.Data;

public class PixpilDataManager : GameDataManager {

	public PixpilDataManager( IMurderGame game ) : base( game ) {}

	public override void LoadContent() {
		// ConvertBitmapFontToSpriteFont();
		base.LoadContent();
		
		ItemTypeServices.Initialize();
	}
	
}
