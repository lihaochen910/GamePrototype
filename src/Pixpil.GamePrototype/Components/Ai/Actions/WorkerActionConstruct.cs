using Bang;
using Bang.Entities;
using Pixpil.Components;
using Pixpil.Messages;


namespace Pixpil.AI.Actions;

public class WorkerActionConstruct : AiAction {

	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {

		if ( entity.TryGetWorkerWorkConstruct() is {} workerWorkConstructComponent ) {
			var buildingId = workerWorkConstructComponent.BuildingEntityId;
			var buildingEntity = world.GetEntity( buildingId );
			if ( buildingEntity is not null ) {
				entity.SetWorkerBuildingBuilding( buildingId );
				return AiActionExecuteStatus.Running;
			}
		}

		return AiActionExecuteStatus.Failure;
	}

	public override AiActionExecuteStatus OnExecute( World world, Entity entity ) {

		if ( entity.TryGetWorkerBuildingBuilding() is {} workerBuildingBuilding ) {
			var buildingEntity = world.GetEntity( workerBuildingBuilding.BuildingEntityId );
			if ( buildingEntity is not null ) {
				var status = buildingEntity.TryGetBuildingConstructionStatus();
				if ( status is not null ) {
					if ( status.Value.Status is BuildingConstructionStatus.Building ) {
						return AiActionExecuteStatus.Running;
					}
					else {
						return AiActionExecuteStatus.Success;
					}
				}
				else {
					return AiActionExecuteStatus.Success;
				}
			}
			else {
				return AiActionExecuteStatus.Failure;
			}
		}
		
		return AiActionExecuteStatus.Success;
	}

	public override void OnPostExecute( World World, Entity entity ) {
		entity.RemoveWorkerBuildingBuilding();
		entity.RemoveWorkerBuildingBuildingSubmitTimer();
	}

}


public class SendWorkerEndConstructMessage : AiAction {
	
	public override void OnPostExecute( World world, Entity entity ) {
		if ( !entity.HasWorkerWorkConstruct() ) {
			return;
		}
		
		if ( world.TryGetUniqueEntity< WorkerSchedulerComponent >() is {} workerSchedulerEntity ) {
			workerSchedulerEntity.SendMessage( new WorkerEndConstructMessage( entity, entity.GetWorkerWorkConstruct().BuildingEntityId ) );
		}
	}
	
}
