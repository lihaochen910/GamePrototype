using Pixpil.RPGStatSystem;
using Spectre.Console;


namespace PxpGameJam2024;

public class ItemStimulants : Item {

	public override string Name => "兴奋剂";
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "喝下了兴奋剂" );
		await Game.Delay( 0.5f );

		var statMod = new RPGStatModTotalAdd( 10, stacks: true );
		character.Attack.AddModifier( statMod );
		
		AnsiConsole.MarkupLine( "  感觉浑身充满了力量，攻击力增加了10" );
		await Game.Delay( 1f );
		return true;
	}
	
}
