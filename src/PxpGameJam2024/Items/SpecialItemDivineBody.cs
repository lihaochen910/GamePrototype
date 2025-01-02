using Spectre.Console;


namespace PxpGameJam2024;

public class SpecialItemDivineBody : SpecialItem {
	
	public override string Name => "御神体";
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "御神体：" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "  虽然无法直接使用，但是带着来到山顶" );
		AnsiConsole.MarkupLine( "  可以挑战Boss" );
		await Game.Delay( 0.5f );
		return false;
	}
	
}
