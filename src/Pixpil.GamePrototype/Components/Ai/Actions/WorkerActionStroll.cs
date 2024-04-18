using System.Collections.Generic;
using Bang;
using Bang.Entities;
using Bang.StateMachines;
using Murder;
using Murder.Core.Ai;
using Murder.Diagnostics;
using Murder.Services;
using Murder.Utilities;
using Pixpil.Utilities;


namespace Pixpil.AI.Actions; 

public class WorkerActionStroll : AiAction {

	public readonly int StrollRadius;

	private bool _finished;
	private bool _hasError;
	
	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
		_finished = false;
		_hasError = false;
		entity.RunCoroutine( Stroll( entity ) );
		return base.OnPreExecute( world, entity );
	}

	public override AiActionExecuteStatus OnExecute(  World world, Entity entity ) {
		if ( !_finished ) {
			return AiActionExecuteStatus.Running;
		}
		return _hasError ? AiActionExecuteStatus.Failure : AiActionExecuteStatus.Success;
	}

	public override void OnPostExecute( World world, Entity entity ) {
		entity.RemovePathfind();
		entity.RemoveRoute();
		entity.RemoveStateMachine();
		entity.RemoveMoveTo();
	}

	private IEnumerator< Wait > Stroll( Entity entity ) {
		var targetPos = entity.GetPosition().ToVector2() + Game.Random.RandomPointInUnitCircle() * StrollRadius;
		entity.SetPathfind( targetPos, PathfindAlgorithmKind.Astar );
		
		// Wait Pathfind
		yield return Wait.NextFrame;

		if ( entity.HasPathfind() && entity.HasRoute() ) {
			GameLogger.Log( "Pathfind Success." );
		}
		else {
			GameLogger.Warning( "Pathfind Fail." );
			_finished = true;
			_hasError = true;
			yield break;
		}

		while ( entity.HasMoveTo() ) {
			yield return Wait.NextFrame;
		}
		
		// Idle
		yield return Wait.ForSeconds( Game.Random.NextFloat( 1f, 3f ) );

		_finished = true;
	}
}
