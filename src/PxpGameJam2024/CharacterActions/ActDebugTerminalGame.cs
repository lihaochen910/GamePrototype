using System.Reflection;


namespace PxpGameJam2024;

public class ActDebugTerminateGame : CharacterAction {

	public override string Name => "[red]向老爹主机发送终止信号[/]";
	public override async Task Do( Character character ) {
		typeof( Game ).GetProperty( nameof( Game.DeveloperTerminationSignal ), BindingFlags.Public | BindingFlags.Static ).SetValue( null, true );
		Game.PushPlayerCost( 1 );
		await Game.Delay( 0.5f );
	}
	
}
