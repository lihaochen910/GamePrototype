using Bang;
using Bang.Entities;
using Murder;
using Pixpil.Components;
using Pixpil.Messages;


namespace Pixpil.AI.Actions; 

public class WorkerActionDoQuarry : AiAction {
	
	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {

		if ( entity.TryGetWorkerWorkDoQuarry() is {} workerWorkDoQuarryComponent ) {
			var buildingId = workerWorkDoQuarryComponent.BuildingEntityId;
			var buildingEntity = world.GetEntity( buildingId );
			if ( buildingEntity is not null ) {
				entity.SetWorkerDoWorking( buildingId );
				return AiActionExecuteStatus.Running;
			}
		}

		return AiActionExecuteStatus.Failure;
	}

	public override AiActionExecuteStatus OnExecute( World world, Entity entity ) {

		if ( !entity.HasWorkerWorkAbility() ) {
			return AiActionExecuteStatus.Failure;
		}

		var workAbility = entity.GetWorkerWorkAbility();
		if ( entity.TryGetWorkerDoWorking() is {} workerDoWorking ) {
			var buildingEntity = world.GetEntity( workerDoWorking.BuildingEntityId );
			if ( buildingEntity is not null ) {
				if ( buildingEntity.TryGetQuarryProcessTimer() is {} processTimer ) {
					
					var processedTime = processTimer.Time;
					processedTime -= workAbility.ProcessSpeed * Game.DeltaTime;
					buildingEntity.SetQuarryProcessTimer( processedTime );
					
					if ( processedTime > 0 ) {
						return AiActionExecuteStatus.Running;
					}
					else {
						return AiActionExecuteStatus.Success;
					}
				}
				else {
					return AiActionExecuteStatus.Failure;
				}
			}
			else {
				return AiActionExecuteStatus.Failure;
			}
		}
		
		return AiActionExecuteStatus.Success;
	}

	public override void OnPostExecute( World World, Entity entity ) {
		entity.RemoveWorkerDoWorking();
	}
	
}


public class SendWorkerEndDoQuarryMessage : AiAction {
	
	public override void OnPostExecute( World world, Entity entity ) {
		if ( world.TryGetUniqueEntity< WorkerSchedulerComponent >() is {} workerSchedulerEntity ) {
			workerSchedulerEntity.SendMessage( new WorkerEndDoQuarryMessage( entity, entity.GetWorkerWorkDoQuarry().BuildingEntityId ) );
		}
	}
	
}
