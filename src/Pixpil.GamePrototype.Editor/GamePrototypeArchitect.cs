using System.Text.Json;
using Murder.Editor;

namespace Pixpil.Editor;

public class GamePrototypeArchitect : GamePrototypeMurderGame, IMurderArchitect {
	
	public JsonSerializerOptions Options => Murder.Serialization.PixpilGamePrototypeSerializerOptionsExtensions.Options;
	
	public void Initialize() {
		base.Initialize();
	
		// Game.Data._fonts.ContainsKey( 103 );
	}
	
}
