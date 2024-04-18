using System;
using Bang;
using Bang.Entities;
using Murder;
using Murder.Attributes;
using Pixpil.Assets;
using Pixpil.Components;
using Pixpil.Messages;
using Pixpil.Services;


namespace Pixpil.AI.Actions; 

/// <summary>
/// 分配建造任务
/// </summary>
public class ActionAssignConstructionTask : AiAction {
	
	public readonly BuildingType TargetType;

	[GameAssetId< UtilityAiAsset >]
	public readonly Guid ConstructAI;

	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {

		var buildingEntity = WorkerService.FindSpecifyBuildingNeedWorker( world, TargetType );
		var idleWorkerEntity = WorkerService.GetIdleWorker( world );
		if ( buildingEntity != null && idleWorkerEntity != null ) {
			
			var buildingWorkersInConstructing = buildingEntity.GetBuildingWorkersInConstructing();
			var needWorkersCount = buildingWorkersInConstructing.SpaceRemaining();
			if ( needWorkersCount > 0 ) {
				buildingWorkersInConstructing = buildingEntity.GetBuildingWorkersInConstructing();
				
				idleWorkerEntity.SetWorkerWorkConstruct( buildingEntity.EntityId );
				buildingEntity.SetBuildingWorkersInConstructing( buildingWorkersInConstructing.Capcity, buildingWorkersInConstructing.Workers.Add( idleWorkerEntity.EntityId ) );
				
				if ( Game.Data.TryGetAsset< UtilityAiAsset >( ConstructAI ) is not null ) {
					idleWorkerEntity.SendMessage< StopExecutingAiActionMessage >();
					idleWorkerEntity.SetUtilityAiAgent( ConstructAI, UtilityAiEvaluateMethod.Message, 1f );
					// idleWorkerEntity.RemoveAiActionInExecuting();
					idleWorkerEntity.SendMessage< RequestUtilityAiAgentEvaluateMessage >();
				}
				else {
					return AiActionExecuteStatus.Failure;
				}
			}

			return AiActionExecuteStatus.Success;
		}
		
		return AiActionExecuteStatus.Failure;
	}
}
