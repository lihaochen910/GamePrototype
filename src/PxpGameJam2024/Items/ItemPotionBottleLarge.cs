using Spectre.Console;


namespace PxpGameJam2024;

public class ItemPotionBottleLarge : Item {

	public override string Name => "大瓶药水";
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "咕咚咕咚。。。" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "喝下了一大瓶药水" );
		await Game.Delay( 0.5f );
		character.Hp.StatValueCurrent += 100f;
		AnsiConsole.MarkupLine( "Hp恢复了100" );
		await Game.Delay( 0.5f );
		return true;
	}
	
}
