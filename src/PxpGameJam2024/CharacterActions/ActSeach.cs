using Spectre.Console;


namespace PxpGameJam2024;

public class ActSeach : CharacterAction {

	public override string Name => "搜索";
	
	private readonly string[] _spinner = [ "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" ];
	
	public override async Task Do( Character character ) {
		AnsiConsole.Clear();
		
		var spinnerIndex = 0;
		await AnsiConsole.Live( Text.Empty )
			.StartAsync(async ctx => {
				var total = 30;

				for ( var i = 0; i <= total; i++ ) {
					var currentSpinner = _spinner[ spinnerIndex++ % _spinner.Length ];
					var progress = new string( '█', i ) + new string( '░', total - i );
					var percent = i * 100 / total;

					ctx.UpdateTarget( new Text( $"{currentSpinner} 正在搜索区域：{Game.TranslateGameMapLocation( character.Location )} [{progress}] {percent}%" ) );

					await Task.Delay( Game.Range( 10, 30 ) );
				}
			});
		await Game.Delay( 0.5f );
		
		var nextSearchEvent = Game.GetNextSearchEvent();
		switch ( nextSearchEvent ) {
			case SearchEvent.ItemFound:
				var hasLocationItemNotCollect = !Game.IsLocationPropCollected( character, character.Location );
				if ( hasLocationItemNotCollect && Game.Chance( 50 ) ) {
					var item = Game.GenerateLocationProp( character.Location );
					var searchableItem = new SearchableItem( item );
					await searchableItem.Collect( character );
				}
				else {
					var item = Game.GenerateARandomItem();
					var searchableItem = new SearchableItem( item );
					await searchableItem.Collect( character );
				}
				break;
			case SearchEvent.EnemyEncounter:
				Character npc = null;
				if ( character.Location is GameMapLocation.Loc_E &&
					 character.HasItem< SpecialItemDivineBody >() &&
					 !Game.FinalBossDefeated ) {
					npc = Game.GenerateFinalBossCharacter();
					AnsiConsole.MarkupLine( "因为身上带着御神体" );
					await Game.Delay( 0.5f );
					AnsiConsole.MarkupLine( $"{Game.FinalBossName}出现了！" );
					await Game.Delay( 2f );
				}
				else {
					npc = Game.GetARandomNpcCharacter();
				}
				var searchableBattle = new SearchableBattle( npc );
				await searchableBattle.Collect( character );
				break;
			case SearchEvent.Nothing:
				AnsiConsole.MarkupLine( "什么也没有发现。。。" );
				await Game.Delay( 1f );
				break;
		}
		
		Game.AnyButtonToContinue();
		Game.PushPlayerCost( 1 );
	}
	
}
