using Bang;
using Bang.Entities;
using Bang.Interactions;


namespace Pixpil.Systems.Interactions;

public readonly struct CollectWorldResourceOnInteract : IInteraction {
	
	public void Interact( World world, Entity interactor, Entity? interacted ) {
		if ( interacted.HasWorldResource() ) {
			if ( interacted.HasInventory() && interactor.HasInventory() ) {
				var interactedInventory = interacted.GetInventory();
				// interactedInventory.Inventory.MoveAllToOther(  );
			}
		}
		// interacted.Destroy();
		// GameLogger.Log( "CollectWorldResourceOnInteract!" );
	}
	
}
