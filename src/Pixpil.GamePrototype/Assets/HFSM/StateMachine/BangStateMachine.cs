using System;
using System.Collections.Immutable;
using Bang;
using Bang.Entities;
#if MURDER
using Murder.Core.Graphics;
using Murder.Diagnostics;
#endif


namespace Pixpil.AI.HFSM;

public class BangStateMachine : StateMachine< string, string, string > {

	public World World { get; private set; }
	public Entity Entity { get; private set; }

	internal Guid AssetGuid;
	
	private ImmutableArray< HFSMStateAction > _actions = ImmutableArray< HFSMStateAction >.Empty;

	public BangStateMachine( bool needsExitTime = false, bool isGhostState = false, bool rememberLastState = false )
		: base( needsExitTime: needsExitTime, isGhostState: isGhostState, rememberLastState: rememberLastState ) {}

	public override void Init() {
		base.Init();
		foreach ( var action in _actions ) {
			action.Init();
		}
	}

	public override void OnEnter() {
		foreach ( var action in _actions ) {
			if ( !action.IsActived ) {
				continue;
			}
			action.OnEnter();
		}
		base.OnEnter();
	}

	public override void OnLogic() {
		EnsureIsInitializedFor("Running OnLogic");

		if (TryAllGlobalTransitions())
			goto runOnLogic;

		if (TryAllDirectTransitions())
			goto runOnLogic;

	runOnLogic:
		foreach ( var action in _actions ) {
			if ( !action.IsActived ) {
				continue;
			}
			action.OnLogic();
		}
	
		_activeState?.OnLogic();
	}

	public void OnFixedUpdate() {
		foreach ( var action in _actions ) {
			if ( !action.IsActived ) {
				continue;
			}
			action.OnFixedUpdate();
		}
		if ( _activeState is BangActionState< string > bangActionState ) {
			bangActionState.OnFixedUpdate();
		}
		else if ( _activeState is BangStateMachine bangStateMachine ) {
			bangStateMachine.OnFixedUpdate();
		}
	}
	
#if MURDER
	public void OnMurderDraw( RenderContext render ) {
		foreach ( var action in _actions ) {
			if ( !action.IsActived ) {
				continue;
			}
			action.OnMurderDraw( render );
		}
		if ( _activeState is BangActionState< string > bangActionState ) {
			bangActionState.OnMurderDraw( render );
		}
		else if ( _activeState is BangStateMachine bangStateMachine ) {
			bangStateMachine.OnMurderDraw( render );
		}
	}
#endif
	
	public override void OnExit() {
		foreach ( var action in _actions ) {
			if ( !action.IsActived ) {
				continue;
			}
			action.OnExit();
		}
		base.OnExit();
	}

	public void SetActions( ImmutableArray< HFSMStateAction > actions ) => _actions = actions;

	public void SetBangContext( World world, Entity entity ) {
		World = world;
		Entity = entity;

		if ( !_actions.IsDefaultOrEmpty ) {
			foreach ( var hfsmStateAction in _actions ) {
				hfsmStateAction.Fsm = this;
				hfsmStateAction.RootFsm = FindRootBangStateMachine();
			}
		}
		
		foreach ( var stateBundle in StateBundles.Values ) {
			if ( stateBundle.state is BangActionState< string > bangActionState ) {
				bangActionState.World = world;
				bangActionState.Entity = entity;
				bangActionState.SetActionsOwnStateMachine( this, FindRootBangStateMachine() );
			}
			else if ( stateBundle.state is BangStateMachine stateMachine ) {
				stateMachine.SetBangContext( world, entity );
            }
			else {
				GameLogger.Error( $"unsupported state: {stateBundle.state.GetType().Name}" );
			}
		}
	}

	private BangStateMachine FindRootBangStateMachine() {
		var root = ParentFsm as BangStateMachine;
		while ( root is { ParentFsm: BangStateMachine rootParentFsm } ) {
			root = rootParentFsm;
		}
		return root;
	}
	
}
