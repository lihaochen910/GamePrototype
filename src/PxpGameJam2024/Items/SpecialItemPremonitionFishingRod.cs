using Spectre.Console;


namespace PxpGameJam2024;

public class SpecialItemPremonitionFishingRod : SpecialItem {

	public override string Name => "预感鱼竿";
	public SearchEvent NextSearchEvent { get; private set; }

	public override async Task< bool > Use( Character character ) {
		
		AnsiConsole.MarkupLine( "使用了预感鱼竿" );
		await Game.Delay( 0.5f );
		
		// TODO: effect apply
		var baseRate = 0.25f;
		if ( Game.Chance( baseRate ) ) {
			NextSearchEvent = SearchEvent.ItemFound;
			Game.NextSearchEventPending = SearchEvent.ItemFound;
			AnsiConsole.MarkupLine( "下次探索将会发现一些有用道具！" );
			await Game.Delay( 0.5f );
			return false;
		}

		baseRate += 0.25f;
		if ( Game.Chance( baseRate ) ) {
			NextSearchEvent = SearchEvent.EnemyEncounter;
			Game.NextSearchEventPending = SearchEvent.EnemyEncounter;
			AnsiConsole.MarkupLine( "下次探索将会遭遇敌人！" );
			await Game.Delay( 0.5f );
			return false;
		}

		// baseRate += 0.25f;
		// if ( Game.Chance( baseRate ) ) {
		// 	NextSearchEvent = SearchEvent.EnemyMet;
		// 	AnsiConsole.MarkupLine( "下次探索将会发现敌人！(如果有合适的道具则可以进行偷袭)" );
		// 	await Game.Delay( 0.5f );
		// 	return false;
		// }
		
		NextSearchEvent = SearchEvent.Nothing;
		Game.NextSearchEventPending = SearchEvent.Nothing;
		AnsiConsole.MarkupLine( "很遗憾，下次探索没有什么发现" );
		await Game.Delay( 0.5f );
		return false;
	}
	
}
