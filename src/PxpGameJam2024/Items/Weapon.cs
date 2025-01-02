using Pixpil.RPGStatSystem;
using Spectre.Console;


namespace PxpGameJam2024;

public abstract class Weapon : Item {
	
	public virtual RPGStatModifier AttrMod { get; }

	public virtual bool CanFire( Character character ) => true;

	public virtual float Fire( Character character, Character target ) {
		target.Hp.StatValueCurrent -= character.Attack.StatValue;
		return character.Attack.StatValue;
	}

	public override async Task< bool > Use( Character character ) {
		if ( character.EquipedWeapon != this ) {
			character.EquipedWeapon = this;
			AnsiConsole.MarkupLine( $"装备了武器：[green]{Name}[/]" );
		}
		else {
			character.EquipedWeapon = null;
			AnsiConsole.MarkupLine( $"卸下了武器：[green]{Name}[/]" );
		}
		await Game.Delay( 0.5f );
		return false;
	}
	
}
