using Spectre.Console;


namespace PxpGameJam2024;

public class ActCheckPlayerStrength : CharacterAction {

	public override string Name => "状态";
	public override async Task Do( Character character ) {
		AnsiConsole.MarkupLine( $"当前生命值：[green]{character.Hp.StatValueCurrent}[/]" );
		await Game.Delay( 200 );
		AnsiConsole.MarkupLine( $"最大生命值：[green]{character.Hp.StatValue}[/]" );
		await Game.Delay( 200 );
		AnsiConsole.MarkupLine( $"攻击力：[green]{character.Attack.StatValue}[/]" );
		await Game.Delay( 200 );
		var currentWeaponName = character.EquipedWeapon is not null ? character.EquipedWeapon.Name : "徒手";
		AnsiConsole.MarkupLine( $"当前装备：[green]{currentWeaponName}[/]" );
		await Game.Delay( 500 );
		AnsiConsole.MarkupLine( $"生存回合：[green]{Game.CurrentRound}[/]" );
		Game.AnyButtonToContinue();
	}
	
}
