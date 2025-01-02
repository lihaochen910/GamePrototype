using Spectre.Console;


namespace PxpGameJam2024;

public class ActDebugSkipRound : CharacterAction {

	public override string Name => "[cyan]跳过回合(开发者)[/]";
	
	public override async Task Do( Character character ) {
		AnsiConsole.MarkupLine( "[cyan]开发者跳过一个回合[/]--->" );
		await Game.Delay( 0.5f );
		Game.PushPlayerCost( 1 );
	}
	
}
