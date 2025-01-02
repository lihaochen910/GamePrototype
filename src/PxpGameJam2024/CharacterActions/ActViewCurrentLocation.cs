using Spectre.Console;


namespace PxpGameJam2024;

public class ActViewCurrentLocation : CharacterAction {

	public override string Name => "查看当前所在位置";
	public override string Description => string.Empty;


	public bool CheckAvailable() => true;
	
	public override async Task Do( Character character ) {
		AnsiConsole.Clear();
		AnsiConsole.MarkupLine( "正在打开定位装置..." );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( $"  当前位置：{Game.TranslateGameMapLocation( character.Location )}" );
		await Game.Delay( 1f );
	}
	
}
