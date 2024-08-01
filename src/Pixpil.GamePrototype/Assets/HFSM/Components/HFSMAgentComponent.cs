using System;
using Bang.Components;
using Murder.Attributes;
using Pixpil.AI;
using Pixpil.AI.HFSM;


namespace Pixpil.Components;

public readonly struct HFSMAgentComponent : IComponent {
	
	[GameAssetId< HFSMScenarioAsset >]
	public readonly Guid FsmAsset;
	
	[System.Text.Json.Serialization.JsonIgnore, HideInEditor]
	public readonly BangStateMachine StateMachine;
	
	public HFSMAgentComponent( Guid fsmAsset ) {
		FsmAsset = fsmAsset;
	}
	
	public HFSMAgentComponent( Guid fsmAsset, BangStateMachine stateMachine ) {
		FsmAsset = fsmAsset;
		StateMachine = stateMachine;
	}

	public void Subscribe( Action notification ) {}

	public void Unsubscribe( Action notification ) {}
	
}
