using Pixpil.RPGStatSystem;
using Spectre.Console;


namespace PxpGameJam2024;

public class SpecialItemCursedAnchor : SpecialItem {

	public override string Name => "诅咒船锚";

	private readonly RPGStatModTotalAdd _statMod = new ( +30 );
	private readonly RPGStatModTotalPercent _statMod2 = new ( 0 );

	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "使用了诅咒船锚" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "但是没有任何效果" );
		await Game.Delay( 0.5f );
		return false;
	}

	public override void OnCollect( Character character ) {
		character.Attack.AddModifier( _statMod );
		character.DiscoverPropsRate.AddModifier( _statMod2 );
	}
	
	public override void OnThowAway( Character character ) {
		character.Attack.RemoveModifier( _statMod );
		character.DiscoverPropsRate.RemoveModifier( _statMod2 );
	}

}
