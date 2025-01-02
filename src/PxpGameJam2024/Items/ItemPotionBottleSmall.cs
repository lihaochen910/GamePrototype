using Spectre.Console;


namespace PxpGameJam2024;

public class ItemPotionBottleSmall : Item {

	public override string Name => "小瓶药水";
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "喝下了药水" );
		await Game.Delay( 0.5f );
		character.Hp.StatValueCurrent += 20f;
		AnsiConsole.MarkupLine( "Hp恢复了20" );
		await Game.Delay( 0.5f );
		return true;
	}
	
}
