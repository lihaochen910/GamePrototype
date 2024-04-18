using Microsoft.Xna.Framework;
using Murder.Editor;
using Murder.Editor.Data;
using Game = Murder.Game;


namespace Pixpil; 

public class PixpilArchitect : Architect {

	public PixpilArchitect( IMurderArchitect? game = null, EditorDataManager? editorDataManager = null ) : base( game, editorDataManager ) {}

	protected override void ApplyGameSettingsImpl() {
		
		base.ApplyGameSettingsImpl();

		if ( Game.Data.GameProfile.IsVSyncEnabled ) {
			_graphics.SynchronizeWithVerticalRetrace = true;
			IsFixedTimeStep = true;
		}
	}

	// protected override void Draw( GameTime gameTime ) {
	// 	base.Draw( gameTime );
	// 	
	// }
	//
	// protected override void EndDraw() {
	// 	GraphicsDevice.SetRenderTarget( null );
	// 	base.EndDraw();
	// }

}
