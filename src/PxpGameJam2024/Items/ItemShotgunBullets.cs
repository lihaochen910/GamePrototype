using Spectre.Console;


namespace PxpGameJam2024;

public class ItemShotgunBullets : Item {

	public override string Name => "猎枪子弹";
	public int BulletCount = 5;
	public override async Task< bool > Use( Character character ) {
		AnsiConsole.MarkupLine( $"剩余弹药数：{BulletCount}" );
		AnsiConsole.MarkupLine( "需要配合[green]猎枪[/]一起使用" );
		await Game.Delay( 0.5f );
		return false;
	}
	
}
