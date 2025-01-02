using Spectre.Console;


namespace PxpGameJam2024; 

public class ActDebugAddAllItems : CharacterAction {

	public override string Name => "[#fc8403]立刻获得所有道具(开发者)[/]";
	public override async Task Do( Character character ) {
		Game.DebugAddAllItemToPlayer();
		AnsiConsole.MarkupLine( "[green]Done![/]" );
		await Game.Delay( 0.5f );
	}
	
}
