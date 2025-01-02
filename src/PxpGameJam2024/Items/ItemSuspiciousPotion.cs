using Pixpil.RPGStatSystem;
using Spectre.Console;


namespace PxpGameJam2024;

public class ItemSuspiciousPotion : Item {

	public override string Name => "可疑药水";
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "摇了摇瓶子，并喝下了药水" );
		await Game.Delay( 0.5f );

		if ( Game.Chance( 50 ) ) {
			var statMod = new RPGStatModBaseAdd( 20, stacks: true );
			character.Hp.AddModifier( statMod );
			AnsiConsole.MarkupLine( "  [cyan]一阵猛烈的抽搐[/]" );
			await Game.Delay( 1f );
			AnsiConsole.MarkupLine( "  最大生命增加了20" );
		}
		else {
			var statMod = new RPGStatModBaseAdd( -20, stacks: true );
			character.Hp.AddModifier( statMod );
			AnsiConsole.MarkupLine( "  [#fc8403]有种不妙的感觉。。。[/]" );
			await Game.Delay( 1f );
			AnsiConsole.MarkupLine( "  最大生命减少了20" );
		}
		
		await Game.Delay( 1f );
		return true;
	}
	
}