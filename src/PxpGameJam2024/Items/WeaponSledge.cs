using Pixpil.RPGStatSystem;


namespace PxpGameJam2024;

public class WeaponSledge : Weapon {

	public override string Name => "大锤";
	public override RPGStatModifier AttrMod => _attrMod;

	private readonly RPGStatModTotalAdd _attrMod = new ( 10f );

}
