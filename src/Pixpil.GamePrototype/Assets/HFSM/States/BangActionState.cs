using System.Collections.Immutable;
using Bang;
using Bang.Entities;
#if MURDER
using Murder.Core.Graphics;
#endif


namespace Pixpil.AI.HFSM;

public class BangActionState< TStateId > : StateBase< TStateId > {
	
	public World World {
		get => _world;
		internal set {
			_world = value;
			foreach ( var action in _actions ) {
				action.World = value;
			}
		}
	}
	private World _world;
	
	public Entity Entity {
		get => _entity;
		internal set {
			_entity = value;
			foreach ( var action in _actions ) {
				action.Entity = value;
			}
		}
	}
	private Entity _entity;

	private readonly ImmutableArray< HFSMStateAction > _actions;
	public ImmutableArray< HFSMStateAction > Actions => _actions;
	
	/// <summary>
	/// Initialises a new instance of the ActionState class.
	/// </summary>
	/// <inheritdoc cref="StateBase{T}(bool, bool)"/>
	public BangActionState( ImmutableArray< HFSMStateAction > actions, bool needsExitTime = false, bool isGhostState = false)
		: base(needsExitTime: needsExitTime, isGhostState: isGhostState) {
		_actions = actions;
	}

	internal void SetActionsOwnStateMachine( BangStateMachine fsm, BangStateMachine rootFsm ) {
		if ( !_actions.IsDefaultOrEmpty ) {
			foreach ( var hfsmStateAction in _actions ) {
				hfsmStateAction.Fsm = fsm;
				hfsmStateAction.RootFsm = rootFsm;
				hfsmStateAction.State = this as StateBase< string >;
			}
		}
	}

	public override void Init() {
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
	}

	public override void OnLogic() {
		foreach ( var action in _actions ) {
			if ( !action.IsActived ) {
				continue;
			}
			action.OnLogic();
		}
	}

	public void OnFixedUpdate() {
		foreach ( var action in _actions ) {
			if ( !action.IsActived ) {
				continue;
			}
			action.OnFixedUpdate();
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
	}
#endif

	public override void OnExit() {
		foreach ( var action in _actions ) {
			if ( !action.IsActived ) {
				continue;
			}
			action.OnExit();
		}
	}

}
