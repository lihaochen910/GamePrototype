using Bang;
using Bang.Entities;
using Bang.Interactions;


namespace Pixpil.Systems.Interactions; 

public readonly struct SetAgentInBuildingOnInteract : IInteraction {
	
	public void Interact( World world, Entity interactor, Entity? interacted ) {

		if ( !interactor.HasAgent() ) {
			return;
		}

		if ( interacted is { Parent: not null } ) {
			var buildingComponent = world.GetEntity( interacted.Parent.Value ).TryGetBuilding();
			if ( buildingComponent.HasValue ) {
				// interactor.SetAgentInBuildingRange( interacted );
			}
		}
	}
	
}
