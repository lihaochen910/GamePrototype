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
public class ActionAssignDoWorkTask : AiAction {
	
	public readonly BuildingType TargetType;

	[GameAssetId< UtilityAiAsset >]
	public readonly Guid JobAI;

	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {

		var buildingEntity = WorkerService.FindSpecifyBuildingNeedWorkerDoJob( world, TargetType );
		var idleWorkerEntity = WorkerService.GetIdleWorker( world );
		if ( buildingEntity != null && idleWorkerEntity != null ) {
			
			var buildingWorkersInWorking = buildingEntity.GetBuildingWorkersInWorking();
			var needWorkersCount = buildingWorkersInWorking.SpaceRemaining();
			if ( needWorkersCount > 0 ) {

				switch ( TargetType ) {
					case BuildingType.Quarry:
						idleWorkerEntity.SetWorkerWorkDoQuarry( buildingEntity.EntityId );
						buildingEntity.SetBuildingWorkersInWorking( buildingWorkersInWorking.Capcity, buildingWorkersInWorking.Workers.Add( idleWorkerEntity.EntityId ) );
						break;
					case BuildingType.SawMill:
						idleWorkerEntity.SetWorkerWorkDoSawMill( buildingEntity.EntityId );
						buildingEntity.SetBuildingWorkersInWorking( buildingWorkersInWorking.Capcity, buildingWorkersInWorking.Workers.Add( idleWorkerEntity.EntityId ) );
						break;
					default:
						throw new NotImplementedException();
				}
				
				if ( Game.Data.TryGetAsset< UtilityAiAsset >( JobAI ) is not null ) {
					idleWorkerEntity.SendMessage< StopExecutingAiActionMessage >();
					idleWorkerEntity.SetUtilityAiAgent( JobAI, UtilityAiEvaluateMethod.Message, 1f );
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
