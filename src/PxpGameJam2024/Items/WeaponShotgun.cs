using Pixpil.RPGStatSystem;
using Spectre.Console;


namespace PxpGameJam2024;

public class WeaponShotgun : Weapon {

	public override string Name => "猎枪";
	public override RPGStatModifier AttrMod => _attrMod;

	private readonly RPGStatModTotalAdd _attrMod = new ( 20f );
	
	public override async Task< bool > Use( Character character ) {
		character.EquipedWeapon = this;
		AnsiConsole.MarkupLine( $"装备了武器：[green]{Name}[/]" );
		AnsiConsole.MarkupLine( "  这个武器需要有[green]猎枪子弹[/]才能正常使用，还请注意" );
		await Game.Delay( 0.5f );
		return false;
	}

	public override bool CanFire( Character character ) {
		return character.HasItem< ItemShotgunBullets >();
	}
	
}
