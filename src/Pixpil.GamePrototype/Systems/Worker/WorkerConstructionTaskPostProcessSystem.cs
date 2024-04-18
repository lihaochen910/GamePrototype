using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Systems;
using Pixpil.AI;
using Pixpil.Components;
using Pixpil.Messages;
using Pixpil.Services;


namespace Pixpil.Systems.Worker;

[Filter( typeof( WorkerSchedulerComponent ) )]
[Messager( typeof( WorkerEndConstructMessage ), typeof( WorkerEndDoQuarryMessage ) )]
public class WorkerConstructionTaskPostProcessSystem : IMessagerSystem {

	public void OnMessage( World world, Entity entity, IMessage message ) {
		if ( message is WorkerEndConstructMessage workerEndConstructMessage ) {

			var buildingEntity = world.GetEntity( workerEndConstructMessage.BuildingEntityId );
			var buildingWorkersInConstructing = buildingEntity.GetBuildingWorkersInConstructing();
			workerEndConstructMessage.WorkerEntity.RemoveWorkerWorkConstruct();
			
			if ( !buildingWorkersInConstructing.Workers.IsDefaultOrEmpty ) {
				buildingEntity.SetBuildingWorkersInConstructing( buildingWorkersInConstructing.Capcity, buildingWorkersInConstructing.Workers.Remove( workerEndConstructMessage.WorkerEntity.EntityId ) );
			}

			var utilityAiAgent = workerEndConstructMessage.WorkerEntity.GetUtilityAiAgent();
			workerEndConstructMessage.WorkerEntity.SetUtilityAiAgent( LibraryServices.GetLibrary().WorkerAI_Idle, utilityAiAgent.EvaluateMethod, utilityAiAgent.EvaluateInterval );
			workerEndConstructMessage.WorkerEntity.SendMessage< RequestUtilityAiAgentEvaluateMessage >();
			
			// entity.SendMessage< RequestUtilityAiAgentEvaluateMessage >();
			return;
		}

		if ( message is WorkerEndDoQuarryMessage workerEndDoQuarryMessage ) {
			var buildingEntity = world.GetEntity( workerEndDoQuarryMessage.BuildingEntityId );
			var buildingWorkersInWorking = buildingEntity.GetBuildingWorkersInWorking();
			workerEndDoQuarryMessage.WorkerEntity.RemoveWorkerDoWorking();
			
			if ( !buildingWorkersInWorking.Workers.IsDefaultOrEmpty ) {
				buildingEntity.SetBuildingWorkersInWorking( buildingWorkersInWorking.Capcity, buildingWorkersInWorking.Workers.Remove( workerEndDoQuarryMessage.WorkerEntity.EntityId ) );
			}

			var utilityAiAgent = workerEndDoQuarryMessage.WorkerEntity.GetUtilityAiAgent();
			workerEndDoQuarryMessage.WorkerEntity.SetUtilityAiAgent( LibraryServices.GetLibrary().WorkerAI_Idle, utilityAiAgent.EvaluateMethod, utilityAiAgent.EvaluateInterval );
			
			entity.SendMessage< RequestUtilityAiAgentEvaluateMessage >();
			return;
		}
		
		if ( message is WorkerEndDoSawMillMessage workerEndDoSawMillMessage ) {
			var buildingEntity = world.GetEntity( workerEndDoSawMillMessage.BuildingEntityId );
			var buildingWorkersInWorking = buildingEntity.GetBuildingWorkersInWorking();
			workerEndDoSawMillMessage.WorkerEntity.RemoveWorkerDoWorking();
			
			if ( !buildingWorkersInWorking.Workers.IsDefaultOrEmpty ) {
				buildingEntity.SetBuildingWorkersInWorking( buildingWorkersInWorking.Capcity, buildingWorkersInWorking.Workers.Remove( workerEndDoSawMillMessage.WorkerEntity.EntityId ) );
			}

			var utilityAiAgent = workerEndDoSawMillMessage.WorkerEntity.GetUtilityAiAgent();
			workerEndDoSawMillMessage.WorkerEntity.SetUtilityAiAgent( LibraryServices.GetLibrary().WorkerAI_Idle, utilityAiAgent.EvaluateMethod, utilityAiAgent.EvaluateInterval );
			
			entity.SendMessage< RequestUtilityAiAgentEvaluateMessage >();
			return;
		}
	}

}
