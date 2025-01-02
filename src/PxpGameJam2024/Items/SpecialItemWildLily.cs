using Spectre.Console;


namespace PxpGameJam2024;

public class SpecialItemWildLily : SpecialItem {

	public override string Name => "野百合";
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "野百合：" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "  虽然没有什么卵用，但是带着通关会有不一样的通关画面" );
		await Game.Delay( 0.5f );
		return false;
	}
	
}
