using Pixpil.RPGStatSystem;


namespace PxpGameJam2024;

public class WeaponKatana : Weapon {

	public override string Name => "武士刀";
	public override RPGStatModifier AttrMod => _attrMod;

	private readonly RPGStatModTotalAdd _attrMod = new ( 10f );

}
