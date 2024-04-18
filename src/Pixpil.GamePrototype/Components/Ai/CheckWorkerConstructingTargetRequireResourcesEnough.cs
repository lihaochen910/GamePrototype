using Bang;
using Bang.Entities;
using Murder.Diagnostics;
using Pixpil.Components;


namespace Pixpil.AI; 

public class CheckWorkerConstructingTargetRequireResourcesEnough : GoapCondition {

	public override bool OnCheck( World world, Entity entity ) {
		var workerWorkConstructComponent = entity.TryGetWorkerWorkConstruct();
		if ( workerWorkConstructComponent is null ) {
			return true;
		}

		var buildingEntity = world.TryGetEntity( workerWorkConstructComponent.Value.BuildingEntityId );
		if ( buildingEntity is null ) {
			GameLogger.Error( "null WorkerWorkConstruct target!" );
			return false;
		}

		var playerEntity = world.TryGetUniqueEntity< PlayerComponent >();
		if ( buildingEntity.HasBuildingConstructRequireResources() ) {
			return buildingEntity.GetBuildingConstructRequireResources().CheckInventoryHasResource( playerEntity );
		}
		
		return true;
	}
}
