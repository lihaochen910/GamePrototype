using System;
using System.Collections.Generic;
using System.Numerics;
using Bang.Entities;
using Bang.StateMachines;
using DigitalRune.Mathematics;
using Murder.Components;
using Murder.Diagnostics;
using Murder.Messages;
using Murder.Services;
using Pixpil.AI;


namespace Pixpil.StateMachines; 

public class WorkerActionStateMachine : StateMachine {
	
	public WorkerActionStateMachine() {
		State( Idle );
	}
	
	protected override void OnStart() {
		
	}

	private void DecideNextGoal() {
		var goapAgent = Entity.GetGoapAgent();
		if ( goapAgent.CheckCondition( "TimeNearDusk", World, Entity ) ) {
			// goapAgent.Goal = "Go Sleep";
			goto GoalSet;
		}
		
		if ( goapAgent.CheckCondition( "BatteryIsEmpty", World, Entity ) ) {
			// goapAgent.Goal = "Rest";
			goto GoalSet;
		}

		if ( goapAgent.CheckCondition( "HasPendingConstructBuilding", World, Entity ) ) {
			// goapAgent.Goal = "Work";
			goto GoalSet;
		}
		
		// default is Rest
		// goapAgent.Goal = "Rest";
		
GoalSet:
		Entity.SetGoapAgent( goapAgent );
		GameLogger.Log( $"goal set: {goapAgent.Goal}" );
	}

	private Func< IEnumerator< Wait > > ResolveGoapPlanAction( GoapScenarioAction action ) {
		switch ( action.Name ) {
			case "Idle": return Idle;
		}

		return null;
	}

	private IEnumerator< Wait > Idle() {
		bool replanFlag = false;
Replan:
		if ( replanFlag ) {
			yield return Wait.ForSeconds( 3f );
		}
		else {
			yield return Wait.ForSeconds( 1f );
		}
		
		DecideNextGoal();
		
		Entity.SendMessage< RequestGoapAgentEvaluateMessage >();
		yield return Wait.ForMessage< GoapAgentEvaluateFinishedMessage >();

		var goapPlan = Entity.GetGoapPlan();
		if ( goapPlan.Action is null ) {
			GameLogger.Log( $"no plan for: {goapPlan.Goal?.Name}" );
			replanFlag = true;
			goto Replan;
		}

		GameLogger.Log( $"next action: {goapPlan.Action.Name}" );
		var route = ResolveGoapPlanAction( goapPlan.Action );
		if ( route is not null ) {
			// yield return GoTo( route );
			goto Replan;
		}
		else {
			// GameLogger.Log( $"cannot resolve Goap action: {goapPlan.Action.Name}" );
			replanFlag = true;
			goto Replan;
		}
	}


	private IEnumerator< Wait > Collect() {
		if ( Entity.HasCollisionCache() ) {
			foreach ( var entity in Entity.GetCollisionCache().GetCollidingEntities( World ) ) {
				if ( entity.HasWorldResource() ) {
					entity.SendMessage( new InteractMessage( Entity ) );
				}
			}
		}
		yield return GoTo( Idle );
	}
}


public class WorkerAnimationStateMachine : StateMachine {

	private Entity _parent;
	
	public WorkerAnimationStateMachine() {
		State( Idle );
	}

	protected override void OnStart() {
		_parent = Entity.TryFetchParent()!;
	}

	private bool CheckVelocityNearZero() {
		if ( !_parent.HasVelocity() ) {
			return true;
		}
		var velocity = _parent.GetVelocity().Velocity;
		if ( Numeric.IsZero( velocity.X ) && Numeric.IsZero( velocity.Y ) ) {
			return true;
		}

		return false;
	}


	private bool CheckBlackboardBool( string bbVar ) {
		// if ( _parent.TryGetBlackboard() is BlackboardComponent blackboardComponent ) {
		// 	return blackboardComponent.GetValue< bool >( bbVar );
		// }

		return false;
	}


	private bool CheckInConstructing() {
		return _parent.HasWorkerBuildingBuilding();
	}


	private string GetActionStateMachineState() => _parent.GetStateMachine().State;

	private IEnumerator< Wait > Idle() {
		EntityServices.PlaySpriteAnimation( Entity, "idle" );
		
		while ( true ) {
			
			if ( !CheckVelocityNearZero() ) {
				yield return GoTo( Move );
			}

			if ( CheckInConstructing() ) {
				yield return GoTo( Work );
			}
			
			yield return Wait.NextFrame;
		}
	}
	
	
	private IEnumerator< Wait > Move() {
		EntityServices.PlaySpriteAnimation( Entity, "walk_d" );
		
		while ( true ) {
			
			if ( CheckVelocityNearZero() ) {
				yield return GoTo( Idle );
			}

			if ( CheckInConstructing() ) {
				yield return GoTo( Work );
			}
			
			yield return Wait.NextFrame;
		}
	}


	private IEnumerator< Wait > Work() {
		EntityServices.PlaySpriteAnimation( Entity, "work_d" );
		
		while ( true ) {
			
			if ( !CheckInConstructing() ) {
				yield return GoTo( Idle );
			}
			
			yield return Wait.NextFrame;
		}
	}
	
}
