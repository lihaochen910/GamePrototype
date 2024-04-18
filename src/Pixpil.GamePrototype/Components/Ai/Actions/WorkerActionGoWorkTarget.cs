using System.Collections.Generic;
using Bang;
using Bang.Entities;
using Bang.StateMachines;
using Murder.Core.Ai;
using Murder.Services;
using Murder.Utilities;


namespace Pixpil.AI.Actions; 

public class WorkerActionGoWorkTarget : AiAction {
	
	private bool _finished;
	
	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
		_finished = false;
		
		if ( !entity.HasWorkerWorkDoQuarry() && !entity.HasWorkerWorkDoSawMill() ) {
			return AiActionExecuteStatus.Failure;
		}
		
		entity.RunCoroutine( Go( world, entity ) );
		return base.OnPreExecute( world, entity );
	}

	public override AiActionExecuteStatus OnExecute( World world, Entity entity ) {
		if ( !_finished ) {
			return AiActionExecuteStatus.Running;
		}
		return AiActionExecuteStatus.Success;
	}

	public override void OnPostExecute( World world, Entity entity ) {
		entity.RemovePathfind();
		entity.RemoveRoute();
		entity.RemoveStateMachine();
		entity.RemoveMoveTo();
	}

	private IEnumerator< Wait > Go( World world, Entity entity ) {
		Entity targetEntity = default;
		if ( entity.HasWorkerWorkDoQuarry() ) {
			targetEntity = world.GetEntity( entity.GetWorkerWorkDoQuarry().BuildingEntityId );
		}
		if ( entity.HasWorkerWorkDoSawMill() ) {
			targetEntity = world.GetEntity( entity.GetWorkerWorkDoSawMill().BuildingEntityId );
		}
		
		entity.SetPathfind( targetEntity.GetPosition().ToVector2(), PathfindAlgorithmKind.Astar );
		
		// Wait Pathfind
		yield return Wait.ForSeconds( 0.1f );

		while ( entity.HasMoveTo() ) {
			yield return Wait.NextFrame;
		}
		
		_finished = true;
	}
}
