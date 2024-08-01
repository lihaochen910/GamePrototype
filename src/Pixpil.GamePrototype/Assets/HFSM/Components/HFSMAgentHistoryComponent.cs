using System;
using System.Text.Json.Serialization;
using Bang.Components;
using DigitalRune.Collections;
using Murder.Attributes;
using Pixpil.AI.HFSM;


namespace Pixpil.Components;

public record struct HFSMStateHistroyEntry {
	
	public StateBase< string > From;
	public StateBase< string > To;
	public Transition TransitionTaken;
	public float Time;
	public string StackTrace;
		
	public HFSMStateHistroyEntry( StateBase< string > from, StateBase< string > to, Transition transitionTaken, float time, string stackTrace ) {
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
	public readonly Action< StateBase< string >, StateBase< string >, TransitionBase< string > > StateChangedCallback;
	
	public HFSMAgentHistoryComponent( int capacity ) {
		Capacity = capacity;
		Deque = null;
		StateChangedCallback = null;
	}

	public HFSMAgentHistoryComponent( int capacity, Deque< HFSMStateHistroyEntry > deque ) {
		Capacity = capacity;
		Deque = deque;
		StateChangedCallback = null;
	}
	
	public HFSMAgentHistoryComponent( int capacity, Deque< HFSMStateHistroyEntry > deque, Action< StateBase< string >, StateBase< string >, TransitionBase< string > > stateChangedCallback ) {
		Capacity = capacity;
		Deque = deque;
		StateChangedCallback = stateChangedCallback;
	}

	public void Subscribe( Action notification ) {}

	public void Unsubscribe( Action notification ) {}
	
}
