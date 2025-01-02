using Pixpil.RPGStatSystem;
using Spectre.Console;


namespace PxpGameJam2024;

public class SpecialItemBearBell : SpecialItem {

	public override string Name => "熊铃";
	
	private readonly RPGStatModTotalPercent _statMod = new ( 0.5f );
	
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "使用了熊铃" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "没有任何效果" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "但是只要带在身上，就可以让遇敌概率下降" );
		await Game.Delay( 0.5f );
		return false;
	}

	public override void OnCollect( Character character ) {
		character.BattleEncounteringRate.AddModifier( _statMod );
	}

	public override void OnThowAway( Character character ) {
		character.BattleEncounteringRate.RemoveModifier( _statMod );
	}

}
