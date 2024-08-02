using System;
using System.Text.Json.Serialization;
using Bang;
using Bang.Entities;
using Murder.Attributes;
using Murder.Core.Graphics;


namespace Pixpil.AI.HFSM;

public abstract class HFSMStateAction {
	
	[Bang.Serialize]
	[HideInEditor]
	public bool IsActived = true;
	
	[JsonIgnore]
	[HideInEditor]
	public World World { get; internal set; }
	
	[JsonIgnore]
	[HideInEditor]
	public Entity Entity { get; internal set; }
	
	[JsonIgnore]
	[HideInEditor]
	public BangStateMachine Fsm { get; internal set; }
	
	[JsonIgnore]
	[HideInEditor]
	public BangStateMachine RootFsm { get; internal set; }
	
	[JsonIgnore]
	[HideInEditor]
	public StateBase< string > State { get; internal set; }

	[JsonIgnore]
	[HideInEditor]
	public string StateName => State.Name;

	[JsonIgnore]
	[HideInEditor]
	public BangActionState< string > ActiveState => Fsm.ActiveState as BangActionState< string >;


	/// <summary>
	/// Called to initialise the state.
	/// </summary>
	public virtual void Init() {}
	
	
	/// <summary>
	/// Called when the state machine transitions to this state (enters this state).
	/// </summary>
	public virtual void OnEnter() {}
	
	
	/// <summary>
	/// Called while this action is active.
	/// </summary>
	public virtual void OnLogic() {}
	
	
	/// <summary>
	/// Called while this action is active.
	/// </summary>
	public virtual void OnFixedUpdate() {}
	
	
#if MURDER
	/// <summary>
	/// 
	/// </summary>
	/// <param name="render"></param>
	public virtual void OnMurderDraw( RenderContext render ) {}
#endif

	
	/// <summary>
	/// Called when the state machine transitions from this state to another state (exits this state).
	/// </summary>
	public virtual void OnExit() {}


	public void Trigger( string trigger ) {
		// Entity.SendMessage( new HFSMTriggerMessage( trigger ) );
		Fsm.Trigger( trigger );
	}


	public void TriggerLocally( string trigger ) => Fsm.TriggerLocally( trigger );


	public void TriggerParent( string trigger ) {
		if ( Fsm.ParentFsm is ITriggerable< string > triggerable ) {
			triggerable.Trigger( trigger );
		}
	}


	public void TriggerRoot( string trigger ) => RootFsm.Trigger( trigger );
	
	internal HFSMStateAction MakeACopy() {
		return ( HFSMStateAction )MemberwiseClone();
	}
}


public class TestHFSMStateAction : HFSMStateAction {

	public string Msg;

	public override void Init() {
		Console.WriteLine( Msg );
	}

}


public class TestHFSMStateAction_2 : HFSMStateAction {

	public int Val_1;
	public float Val_2;
	
}
