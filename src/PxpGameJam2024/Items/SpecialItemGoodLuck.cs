using Spectre.Console;


namespace PxpGameJam2024;

public class SpecialItemGoodLuck : SpecialItem {

	public override string Name => "上上签";
	
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( "使用了上上签" );
		await Game.Delay( 0.5f );

		var killedCharacter = Game.KillNextNPCCharacter();
		if ( killedCharacter is null ) {
			AnsiConsole.MarkupLine( "但是没有任何效果" );
			await Game.Delay( 0.5f );
		}
		else {
			AnsiConsole.MarkupLine( "老爹主机使用了毁灭力量" );
			await Game.Delay( 0.5f );
			
			var currentBackground = AnsiConsole.Background;
			AnsiConsole.Background = Color.White;
			await Game.Delay( 300 );
			AnsiConsole.Background = currentBackground;
			await Game.Delay( 0.5f );
			
			AnsiConsole.MarkupLine( $"击杀了NPC玩家：{killedCharacter.Name}" );
			await Game.Delay( 0.5f );
		}
		
		Game.PushPlayerCost( 1 );
		if ( Game.Chance( 50 ) ) {
			return true;
		}
		
		return false;
	}
	
}
