using System;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DigitalRune.Mathematics;
using Murder;
using Pixpil.Components;
using Pixpil.RPGStatSystem;


namespace Pixpil.Components {

	public readonly struct BatteryComponent : IComponent {
		
		public readonly float MaxPower;
		public readonly float Power;

		public bool HasPower => Power > 0f;
		public bool IsFullPower => Numeric.AreEqual( Power, MaxPower );

		public BatteryComponent( float maxPower, float power ) {
			MaxPower = maxPower;
			Power = power;
		}


		// public BatteryComponent( in BatteryComponent other ) {
		// 	MaxPower = other.MaxPower;
		// 	Power = other.Power;
		// }
	}


	public readonly struct BatteryConsumingComponent : IComponent {
		public readonly RPGStatModifiable Speed;
	}


	public readonly struct BatteryChargingComponent : IComponent {
		public readonly RPGStatModifiable Speed;
	}

}


namespace Pixpil.Systems {

	[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Write, typeof( BatteryComponent ), typeof( BatteryConsumingComponent ) )]
	public class BatteryConsumeSystem : IUpdateSystem {

		public void Update( Context context ) {
			foreach ( var owner in context.Entities ) {
				var batteryComponent = owner.GetBattery();
				var batteryConsumeComponent = owner.GetBatteryConsuming();
				if ( batteryConsumeComponent.Speed != null ) {
					var power = Math.Clamp(
						batteryComponent.Power - batteryConsumeComponent.Speed.StatValue * Game.DeltaTime, 0,
						batteryComponent.MaxPower );
					owner.SetBattery( batteryComponent.MaxPower, power );
				}
			}
		}
	}
	
	
	[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Write, typeof( BatteryComponent ), typeof( BatteryChargingComponent ) )]
	public class BatteryChargeSystem : IUpdateSystem {

		public void Update( Context context ) {
			foreach ( var owner in context.Entities ) {
				var batteryComponent = owner.GetBattery();
				var batteryChargingComponent = owner.GetBatteryCharging();
				if ( batteryChargingComponent.Speed != null ) {
					var power = Math.Clamp(
						batteryComponent.Power + batteryChargingComponent.Speed.StatValue * Game.DeltaTime, 0,
						batteryComponent.MaxPower );
					owner.SetBattery( batteryComponent.MaxPower, power );
				}
			}
		}
		
	}

}
