using Pixpil.RPGStatSystem;


namespace PxpGameJam2024;

public class WeaponKitchenKnife : Weapon {

	public override string Name => "菜刀";
	public override RPGStatModifier AttrMod => _attrMod;

	private readonly RPGStatModTotalAdd _attrMod = new ( 5f );

}
