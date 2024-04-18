using Murder;
using Pixpil.Assets;


namespace Pixpil.Services;

public static class LibraryServices {
	
	public static LibraryAsset GetLibrary() {
		return Game.Data.GetAsset< LibraryAsset >( GamePrototypeMurderGame.Profile.Library );
	}


	public static UiSkinAsset GetUiSkin() {
		return Game.Data.GetAsset< UiSkinAsset >( GetLibrary().UiSkin );
	}

}
