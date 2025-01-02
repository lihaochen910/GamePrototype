using Pixpil.RPGStatSystem;
using Spectre.Console;


namespace PxpGameJam2024;

public class SpecialItemLuckyWhaleOil : SpecialItem {

	public override string Name => "幸运鲸油";
	
	private readonly RPGStatModTotalPercent _statMod = new ( 1.5f );

	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "使用了幸运鲸油" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "没有任何效果" );
		await Game.Delay( 1f );
		AnsiConsole.MarkupLine( "但是只要带在身上，就可以让搜索发现道具概率上升" );
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
