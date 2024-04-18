using System.Collections.Immutable;
using System.Linq;
using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DigitalRune.Linq;
using Murder;
using Pixpil.Components;
using Pixpil.Data;


namespace Pixpil.Systems.Worker;


[Watch( typeof( WorkerBuildingBuilding ) )]
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( WorkerComponent ), typeof( WorkerConstructionAbilityComponent ), typeof( WorkerBuildingBuilding ) )]
public class WorkerDoBuildingSystem : IUpdateSystem, IReactiveSystem {

	public void Update( Context context ) {
		var speedOfTime = ( Game.Preferences as GamePrototypePreferences ).SpeedOfTime;
		var deltaTime = Game.DeltaTime * speedOfTime;
		
		void DoSubmit( Entity buidingEntity, int submitCount ) {
			var playerEntity = context.World.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( PlayerComponent ),
				typeof( InventoryComponent ) ).FirstOrDefault();
			buidingEntity.GetBuildingConstructRequireResources().SubmitResourceCount( submitCount, playerEntity, buidingEntity );
		}
		
		context.Entities.ForEach( entity => {
			var workerConstructionAbility = entity.GetWorkerConstructionAbility();
			
			var workerBuildingBuilding = entity.GetWorkerBuildingBuilding();
			var buildingEntity = context.World.TryGetEntity( workerBuildingBuilding.BuildingEntityId );
			if ( buildingEntity is not null ) {
				var time = entity.GetWorkerBuildingBuildingSubmitTimer().SubmitTime - deltaTime;
				if ( time < 0f ) {
					DoSubmit( buildingEntity, workerConstructionAbility.SubmitCount );
					time = workerConstructionAbility.SubmitTime;
				}
				entity.SetWorkerBuildingBuildingSubmitTimer( time );
			}
		} );
	}

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		entities.ForEach( entity => {
			var workerConstructionAbility = entity.GetWorkerConstructionAbility();
			
			if ( entity.TryGetBatteryConsuming() is BatteryConsumingComponent batteryConsumingComponent ) {
				if ( batteryConsumingComponent.Speed is not null && workerConstructionAbility.BatteryPowerConsumption != null ) {
					batteryConsumingComponent.Speed.AddModifier( workerConstructionAbility.BatteryPowerConsumption );
				}
			}
			
			var workerBuildingBuilding = entity.GetWorkerBuildingBuilding();
			if ( world.TryGetEntity( workerBuildingBuilding.BuildingEntityId ) is not null ) {
				entity.SetWorkerBuildingBuildingSubmitTimer( workerConstructionAbility.SubmitTime );
			}
		} );
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		entities.ForEach( entity => {
			var workerConstructionAbility = entity.GetWorkerConstructionAbility();
			
			if ( entity.TryGetBatteryConsuming() is BatteryConsumingComponent batteryConsumingComponent ) {
				if ( batteryConsumingComponent.Speed is not null && workerConstructionAbility.BatteryPowerConsumption != null ) {
					batteryConsumingComponent.Speed.RemoveModifier( workerConstructionAbility.BatteryPowerConsumption );
				}
			}

			if ( entity.HasWorkerBuildingBuildingSubmitTimer() ) {
				entity.RemoveWorkerBuildingBuildingSubmitTimer();
			}
		} );
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {}
	
}
