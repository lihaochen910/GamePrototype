using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;


namespace Pixpil.AI.HFSM;

public class HFSMStateMachineScenario : HFSMStateScenario {
	
	[JsonIgnore]
	public readonly HFSMStateMachineScenario ParentFsm;
	
	public ImmutableArray< HFSMStateScenario > States = ImmutableArray< HFSMStateScenario >.Empty;
	public ImmutableArray< HFSMStateMachineScenario > ChildrenStateMachine = ImmutableArray< HFSMStateMachineScenario >.Empty;
	
	public void SetStateMachineName( string stateMachineName ) {
		GetType().GetField( nameof( Name ) ).SetValue( this, stateMachineName );
	}

	public void SetParentFsm( HFSMStateMachineScenario parentFsm ) {
		GetType().GetField( nameof( ParentFsm ) ).SetValue( this, parentFsm );
	}
	
	public void SetStates( ImmutableArray< HFSMStateScenario > states ) {
		GetType().GetField( nameof( States ) ).SetValue( this, states );
	}

	public void SetStartState( HFSMStateScenario startState ) {
		foreach ( var hfsmStateScenario in States ) {
			if ( hfsmStateScenario != startState ) {
				hfsmStateScenario.SetStartState( false );
			}
		}
		startState.SetStartState( true );
	}
	
	public void SetChildrenStateMachine( ImmutableArray< HFSMStateMachineScenario > childrenStateMachine ) {
		GetType().GetField( nameof( ChildrenStateMachine ) ).SetValue( this, childrenStateMachine );
	}

	public void AddState( HFSMStateScenario stateScenario ) {
		SetStates( States.Add( stateScenario ) );
	}

	public void RemoveState( HFSMStateScenario stateScenario ) {
		SetStates( States.Remove( stateScenario ) );
	}
	
	public void AddChildStateMachine( HFSMStateMachineScenario stateMachineScenario ) {
		SetChildrenStateMachine( ChildrenStateMachine.Add( stateMachineScenario ) );
	}
	
	public void RemoveChildStateMachine( HFSMStateMachineScenario stateMachineScenario ) {
		SetChildrenStateMachine( ChildrenStateMachine.Remove( stateMachineScenario ) );
	}

	public HFSMStateMachineScenario FindStateScenarioBelongWith( HFSMStateScenario stateScenario ) {
		var machines = new Stack< HFSMStateMachineScenario >();
		machines.Push( this );
		while ( machines.Count > 0 ) {
			var top = machines.Pop();
			if ( top.States.Contains( stateScenario ) ) {
				return top;
			}

			if ( !top.ChildrenStateMachine.IsDefaultOrEmpty ) {
				foreach ( var childStateMachineScenario in top.ChildrenStateMachine ) {
					machines.Push( childStateMachineScenario );
				}
			}
		}

		return null;
	}

	public HFSMStateMachineScenario FindStateMachineScenarioBelongWith( HFSMStateMachineScenario stateMachineScenario ) {
		var machines = new Stack< HFSMStateMachineScenario >();
		machines.Push( this );
		while ( machines.Count > 0 ) {
			var top = machines.Pop();
			if ( top.ChildrenStateMachine.Contains( stateMachineScenario ) ) {
				return top;
			}

			if ( !top.ChildrenStateMachine.IsDefaultOrEmpty ) {
				foreach ( var childStateMachineScenario in top.ChildrenStateMachine ) {
					machines.Push( childStateMachineScenario );
				}
			}
		}

		return null;
	}

	public bool HasNoStartState() {
		foreach ( var hfsmStateScenario in States ) {
			if ( hfsmStateScenario.IsStartState ) {
				return false;
			}
		}

		return true;
	}

}
