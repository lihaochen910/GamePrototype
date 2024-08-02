using System;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Bang.Components;
using DigitalRune.Collections;
using Murder.Attributes;
using Pixpil.AI.HFSM;


namespace Pixpil.Components;

public record struct HFSMStateHistroyEntry {
	
	public StateMachine< string, string, string > Fsm;
	public StateBase< string > From;
	public StateBase< string > To;
	public Transition TransitionTaken;
	public float Time;
	public string StackTrace;
		
	public HFSMStateHistroyEntry( StateMachine< string, string, string > fsm, StateBase< string > from, StateBase< string > to, Transition transitionTaken, float time, string stackTrace ) {
		Fsm = fsm;
		From = from;
		To = to;
		TransitionTaken = transitionTaken;
		Time = time;
		StackTrace = stackTrace;
	}

}

/// <summary>
/// for debug.
/// </summary>
[DoNotPersistOnSave]
[Requires( typeof( HFSMAgentComponent ) )]
public readonly struct HFSMAgentHistoryComponent : IModifiableComponent {
	
	public readonly int Capacity = 6;
	
	[JsonIgnore]
	public readonly Deque< HFSMStateHistroyEntry > Deque;
	
	[JsonIgnore]
	public readonly ImmutableArray< Action< StateBase< string >, StateBase< string >, TransitionBase< string > > > StateChangedCallbacks;
	
	public HFSMAgentHistoryComponent( int capacity ) {
		Capacity = capacity;
		Deque = null;
		StateChangedCallbacks = ImmutableArray< Action< StateBase< string > , StateBase< string >, TransitionBase< string > > >.Empty;
	}

	public HFSMAgentHistoryComponent( int capacity, Deque< HFSMStateHistroyEntry > deque ) {
		Capacity = capacity;
		Deque = deque;
		StateChangedCallbacks = ImmutableArray< Action< StateBase< string > , StateBase< string >, TransitionBase< string > > >.Empty;
	}
	
	public HFSMAgentHistoryComponent( int capacity, Deque< HFSMStateHistroyEntry > deque, ImmutableArray< Action< StateBase< string >, StateBase< string >, TransitionBase< string > > > stateChangedCallbacks ) {
		Capacity = capacity;
		Deque = deque;
		StateChangedCallbacks = stateChangedCallbacks;
	}

	public void Subscribe( Action notification ) {}

	public void Unsubscribe( Action notification ) {}
	
}
