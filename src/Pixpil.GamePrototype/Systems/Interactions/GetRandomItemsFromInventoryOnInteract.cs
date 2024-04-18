using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Interactions;
using Bang.StateMachines;
using Pixpil.StateMachines;


namespace Pixpil.Components {

	public readonly struct ChestGetRewardInteractionDataComponent : IComponent {
		
		public readonly int InteractorId;
		public readonly int InteractedId;

		public ChestGetRewardInteractionDataComponent( int interactorId, int interactedId ) {
			InteractorId = interactorId;
			InteractedId = interactedId;
		}
	}

}


namespace Pixpil.Systems.Interactions {

	public readonly struct GetRandomItemsFromInventoryOnInteract : IInteraction {

		public void Interact( World world, Entity interactor, Entity interacted ) {
			if ( !interactor.HasInventory() || !interacted.HasInventory() ) {
				return;
			}

			interacted.RemoveInteractOnCollision();
			interacted.SetChestGetRewardInteractionData( interactor.EntityId, interacted.EntityId );
			interacted.SetStateMachine( new StateMachineComponent< ChestGetRewardStateMachine >() );
		}
		
	}

}
