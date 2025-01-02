using Spectre.Console;


namespace PxpGameJam2024;

public class ItemPotionBottleM : Item {

	public override string Name => "药水";
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "喝下了药水" );
		await Game.Delay( 0.5f );
		character.Hp.StatValueCurrent += 50f;
		AnsiConsole.MarkupLine( "Hp恢复了50" );
		await Game.Delay( 0.5f );
		return true;
	}
	
}
