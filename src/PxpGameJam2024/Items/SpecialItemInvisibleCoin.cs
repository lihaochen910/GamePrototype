using Spectre.Console;


namespace PxpGameJam2024;

public class SpecialItemInvisibleCoin : SpecialItem {
	
	public override string Name => "隐身铜钱";
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "隐身铜钱：" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "  虽然无法直接使用，但是带着不容易被敌人发现" );
		AnsiConsole.MarkupLine( "  并且更加容易偷袭敌人" );
		await Game.Delay( 0.5f );
		return false;
	}
	
}
