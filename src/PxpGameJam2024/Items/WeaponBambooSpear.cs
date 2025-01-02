using Pixpil.RPGStatSystem;


namespace PxpGameJam2024;

public class WeaponBambooSpear : Weapon {

	public override string Name => "竹枪";
	public override RPGStatModifier AttrMod => _attrMod;

	private readonly RPGStatModTotalAdd _attrMod = new ( 5f );

}
