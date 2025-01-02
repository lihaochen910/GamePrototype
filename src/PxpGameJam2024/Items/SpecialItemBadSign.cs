using Spectre.Console;


namespace PxpGameJam2024;

public class SpecialItemBadSign : SpecialItem {

	public override string Name => "下下签";
	
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "使用了下下签" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "下次搜索将会遇敌！" );
		Game.NextSearchEventPending = SearchEvent.EnemyEncounter;
		await Game.Delay( 0.5f );
		return false;
	}
	
}
