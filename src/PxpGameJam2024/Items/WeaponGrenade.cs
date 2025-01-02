using Pixpil.RPGStatSystem;
using Spectre.Console;


namespace PxpGameJam2024;

public class WeaponGrenade : Weapon {

	public override string Name => "古董手榴弹";
	public override RPGStatModifier AttrMod => _attrMod;

	private readonly RPGStatModTotalAdd _attrMod = new ( 50f );
	
	public override async Task< bool > Use( Character character ) {
		character.EquipedWeapon = this;
		AnsiConsole.MarkupLine( $"装备了武器：[green]{Name}[/]" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "  这个武器虽然杀伤力极强" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "  但是[#fc8403]有概率会在手上爆炸[/]还请注意" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( "  至于[cyan]概率[/]是多少，我觉得你还是不知道的为好" );
		await Game.Delay( 1f );
		return false;
	}
	
}
