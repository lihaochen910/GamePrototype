using Bang;
using Bang.Entities;
using DigitalRune.Mathematics;


namespace Pixpil.AI; 

public class CheckBatteryPowerCondition : GoapCondition {

	public readonly CompareMethod Method;
	public readonly float Value;
	
	public override bool OnCheck( World world, Entity entity ) {
		var battery = entity.TryGetBattery();
		if ( battery is null ) {
			return false;
		}
		
		return NumericCompareHelper.Compare( battery.Value.Power, Method, Value );
	}

}
