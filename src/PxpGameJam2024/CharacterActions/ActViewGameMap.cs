using Spectre.Console;


namespace PxpGameJam2024;

public class ActViewGameMap : CharacterAction {

	public override string Name => "查看地图";
	
	public override async Task Do( Character character ) {
		AnsiConsole.Clear();
		AnsiConsole.MarkupLine( "正在打开定位装置..." );
		await Game.Delay( 0.5f );
		Game.PrintGameMap();
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( $"当前位置：[green]{Game.TranslateGameMapLocation( character.Location )}[/]" );
		await Game.Delay( 1f );
	}
	
}
