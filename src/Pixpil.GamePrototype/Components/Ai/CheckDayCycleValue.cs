using Bang;
using Bang.Entities;
using DigitalRune.Mathematics;
using Murder.Attributes;
using Pixpil.Components;


namespace Pixpil.AI; 

public class CheckDayPercentileValue : GoapCondition {
	
	public readonly CompareMethod Method;
	
	[Slider( 0, 1 )]
	public readonly float Value;
	
	public override bool OnCheck( World world, Entity entity ) {
		var dayCycleComponent = world.TryGetUnique< DayCycleComponent >();
		if ( dayCycleComponent is null ) {
			return false;
		}

		return NumericCompareHelper.Compare( dayCycleComponent.Value.DayCycle.DayPercentile, Method, Value );
	}
}
