using System.Collections.Generic;
using Bang;
using Bang.Entities;
using Bang.StateMachines;
using Murder.Core.Ai;
using Murder.Services;
using Murder.Utilities;


namespace Pixpil.AI.Actions; 

public class WorkerActionGoConstructingTarget : AiAction {
	
	private bool _finished;
	
	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
		_finished = false;
		
		if ( !entity.HasWorkerWorkConstruct() ) {
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
		var targetPos = world.GetEntity( entity.GetWorkerWorkConstruct().BuildingEntityId );
		entity.SetPathfind( targetPos.GetPosition().ToVector2(), PathfindAlgorithmKind.Astar );
		
		// Wait Pathfind
		yield return Wait.ForSeconds( 0.1f );

		while ( entity.HasMoveTo() ) {
			yield return Wait.NextFrame;
		}
		
		_finished = true;
	}
}
