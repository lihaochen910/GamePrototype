using System.Collections.Generic;
using Bang;
using Bang.Entities;
using Bang.StateMachines;
using Murder.Core.Ai;
using Murder.Services;
using Murder.Utilities;


namespace Pixpil.AI.Actions; 

public class WorkerActionBackDormitory : AiAction {
	
	private bool _finished;
	
	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
		_finished = false;

		if ( CheckArrived( world, entity ) ) {
			return AiActionExecuteStatus.Success;
		}
		
		// entity.RunCoroutine( Back( world, entity ) );
		
		var targetPos = world.GetEntity( entity.GetWorkerBelongWhichDormitory().DormitoryEntityId );
		entity.SetPathfind( targetPos.GetPosition().ToVector2(), PathfindAlgorithmKind.Astar );
		
		return AiActionExecuteStatus.Running;
	}

	public override AiActionExecuteStatus OnExecute( World world, Entity entity ) {
		if ( entity.HasMoveTo() ) {
			
			if ( CheckArrived( world, entity ) ) {
				return AiActionExecuteStatus.Success;
			}
			
			return AiActionExecuteStatus.Running;
		}
		
		return AiActionExecuteStatus.Success;
	}

	public override void OnPostExecute( World world, Entity entity ) {
		entity.RemovePathfind();
		entity.RemoveRoute();
		// entity.RemoveStateMachine();
		entity.RemoveMoveTo();
	}


	private bool CheckArrived( World world, Entity entity ) {
		if ( world.TryGetEntity( entity.GetWorkerBelongWhichDormitory().DormitoryEntityId ) is {} dormitoryEntity ) {
			if ( entity.TryGetAgentInBuildingRange() is {} rangeComponent ) {
				return rangeComponent.HasId( dormitoryEntity.EntityId );
			}
		}

		return true;
	}

	private IEnumerator< Wait > Back( World world, Entity entity ) {
		var targetPos = world.GetEntity( entity.GetWorkerBelongWhichDormitory().DormitoryEntityId );
		entity.SetPathfind( targetPos.GetPosition().ToVector2(), PathfindAlgorithmKind.Astar );
		
		// Wait Pathfind
		yield return Wait.ForSeconds( 0.1f );

		while ( entity.HasMoveTo() ) {

			if ( CheckArrived( world, entity ) ) {
				goto Arrived;
			}
			
			yield return Wait.NextFrame;
		}
		
Arrived:
		_finished = true;
		yield return Wait.Stop;
	}
}