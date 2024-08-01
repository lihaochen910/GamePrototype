using System.Collections.Immutable;


namespace Pixpil.AI.HFSM;

public class HFSMStateScenario {
	
	public string Name;
	public bool IsStartState;
	public bool IsGhostState;
	
	public ImmutableArray< HFSMStateAction > Impl = ImmutableArray< HFSMStateAction >.Empty;
	public ImmutableArray< HFSMStateTransitionData > Transitions = ImmutableArray< HFSMStateTransitionData >.Empty;
	public ImmutableArray< HFSMStateGlobalTransitionData > GlobalTransitions = ImmutableArray< HFSMStateGlobalTransitionData >.Empty;


	[Bang.Serialize]
	public System.Numerics.Vector2 PositionInEditor; // for Editor use only.
	
	
	public ImmutableArray< HFSMStateAction > MakeClonedImpl() {
		var builder = ImmutableArray.CreateBuilder< HFSMStateAction >( Impl.Length );
		foreach ( var action in Impl ) {
			builder.Add( action.MakeACopy() );
		}
		return builder.ToImmutable();
	}
	
	public void SetStateName( string stateName ) {
		GetType().GetField( nameof( Name ) ).SetValue( this, stateName );
	}
	
	public void SetStartState( bool startState ) {
		GetType().GetField( nameof( IsStartState ) ).SetValue( this, startState );
	}
	
	public void SetGhostState( bool ghostState ) {
		GetType().GetField( nameof( IsGhostState ) ).SetValue( this, ghostState );
	}

	public bool HasTransition( string ev ) {
		foreach ( var hfsmStateTransitionData in Transitions ) {
			if ( hfsmStateTransitionData.Event == ev ) {
				return true;
			}
		}

		return false;
	}
	
	public void AddTransition( HFSMStateTransitionData transition ) {
		SetTransitions( Transitions.Add( transition ) );
	}
	
	public void RemoveTransition( HFSMStateTransitionData transition ) {
		SetTransitions( Transitions.Remove( transition ) );
	}
	
	public void SetTransitions( ImmutableArray< HFSMStateTransitionData > transitions ) {
		GetType().GetField( nameof( Transitions ) ).SetValue( this, transitions );
	}
    
	public bool HasGlobalTransition( string ev ) {
		foreach ( var hfsmStateGlobalTransitionData in GlobalTransitions ) {
			if ( hfsmStateGlobalTransitionData.Event == ev ) {
				return true;
			}
		}

		return false;
	}
	
	public void AddGlobalTransition( HFSMStateGlobalTransitionData transitionData ) {
		SetGlobalTransitions( GlobalTransitions.Add( transitionData ) );
	}
	
	public void RemoveGlobalTransition( HFSMStateGlobalTransitionData transitionData ) {
		SetGlobalTransitions( GlobalTransitions.Remove( transitionData ) );
	}
	
	public void SetGlobalTransitions( ImmutableArray< HFSMStateGlobalTransitionData > transitions ) {
		GetType().GetField( nameof( GlobalTransitions ) ).SetValue( this, transitions );
	}
	
	public void AddStateAction( HFSMStateAction stateAction ) {
		SetStateActions( Impl.Add( stateAction ) );
	}
	
	public void RemoveStateAction( HFSMStateAction stateAction ) {
		SetStateActions( Impl.Remove( stateAction ) );
	}

	public void SetStateActions( ImmutableArray< HFSMStateAction > stateActions ) {
		GetType().GetField( nameof( Impl ) ).SetValue( this, stateActions );
	}
}


public readonly struct HFSMStateTransitionData {

	public readonly string Event;
	public readonly string TransitionTo;
	
	public HFSMStateTransitionData( string ev, string transitionTo ) {
		Event = ev;
		TransitionTo = transitionTo;
	}

}


public readonly struct HFSMStateGlobalTransitionData {
	
	public readonly string Event;
	
	public HFSMStateGlobalTransitionData( string ev ) {
		Event = ev;
	}

}
