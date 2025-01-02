using Spectre.Console;


namespace PxpGameJam2024;

public class ActDebugKillAllNPCCharacter : CharacterAction {
	
	public override string Name => "[#fc8403]立刻杀死其他玩家(开发者)[/]";
	
	public override async Task Do( Character character ) {
		foreach ( var npcCharacter in Game.NonPlayerControledCharacters ) {
			npcCharacter.Hp.StatValueCurrent = 0;
			var npcName = npcCharacter.Name is null ? npcCharacter.GetHashCode().ToString() : npcCharacter.Name;
			AnsiConsole.MarkupLine( $"[red]x[/] 杀死了NPC: {npcName}" );
			await Game.Delay( 25 );
		}
		Game.PushPlayerCost( 1 );
		AnsiConsole.MarkupLine( "[red]开发者击杀了其他玩家[/]--->" );
		await Game.Delay( 0.5f );
	}
	
}
