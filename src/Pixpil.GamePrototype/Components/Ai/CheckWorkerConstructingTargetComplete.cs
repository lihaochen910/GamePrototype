using Bang;
using Bang.Entities;
using Murder.Diagnostics;
using Pixpil.Components;


namespace Pixpil.AI; 

public class CheckWorkerConstructingTargetComplete : GoapCondition {

	public override bool OnCheck( World world, Entity entity ) {
		var workerWorkConstructComponent = entity.TryGetWorkerWorkConstruct();
		if ( workerWorkConstructComponent is null ) {
			return true;
		}

		var buildingEntity = world.TryGetEntity( workerWorkConstructComponent.Value.BuildingEntityId );
		if ( buildingEntity is null ) {
			GameLogger.Error( $"null WorkerWorkConstruct target." );
			return false;
		}
		
		if ( buildingEntity.TryGetBuildingConstructionStatus() is {} buildingConstructionStatusComponent ) {
			return buildingConstructionStatusComponent.Status is BuildingConstructionStatus.Finished;
		}

		return true;
	}
}
