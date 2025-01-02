using Spectre.Console;


namespace PxpGameJam2024;

public class ActDebugKillPlayer : CharacterAction {

	public override string Name => "[#fc8403]立刻杀死玩家(开发者)[/]";
	
	public override async Task Do( Character character ) {
		Game.PlayerCharacter.Hp.StatValueCurrent = 0;
		Game.PushPlayerCost( 1 );
		AnsiConsole.MarkupLine( "[red]开发者击杀了玩家[/]--->" );
		await Game.Delay( 0.5f );
	}
	
}
