using System;
using Bang;
using Bang.Entities;
using Bang.Interactions;
using Murder.Assets;
using Murder.Attributes;
using Pixpil.Services;
using Pixpil.StateMachines;


namespace Pixpil.Interactions;

public readonly struct MonologueInteraction : IInteraction {

	[GameAssetId( typeof( CharacterAsset ) ), ShowInEditor]
	public readonly Guid Character = Guid.Empty;

	/// <summary>
	/// This is the starter situation for the interaction.
	/// </summary>
	public readonly int Situation = 0;

	[Tooltip( "Whether this will pause the game when being played." )]
	public readonly bool Pause = true;

	public readonly bool AllowInterrupt = false;

	public readonly MessageType MessageType = MessageType.Monologue;

	public MonologueInteraction() { }

	public void Interact( World world, Entity interactor, Entity? interacted ) {
		DialogueServices.TriggerDialogue( world, new ( Character, Situation ),
			Pause ? InputType.PauseGame : InputType.Time, MessageType, canInterrupt: AllowInterrupt );
	}
}
