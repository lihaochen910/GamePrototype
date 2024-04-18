using Bang.Components;
using Bang.Entities;
using Murder.Attributes;
using Murder.Utilities.Attributes;


namespace Pixpil.Components;

[RuntimeOnly]
public readonly struct IsPlacingBuildingComponent : IComponent {
	
	public readonly Entity Commander;

	public IsPlacingBuildingComponent( Entity commander ) {
		Commander = commander;
	}
	
}


[RuntimeOnly, DoNotPersistOnSave]
public readonly struct PlacingBuildingHasObstacleComponent : IComponent;
