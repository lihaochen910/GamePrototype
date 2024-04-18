using Bang.Components;
using Murder.Utilities.Attributes;


namespace Pixpil.Components;

[RuntimeOnly]
public readonly struct BuildingConstructionStatusComponent : IComponent {
	
	public readonly BuildingConstructionStatus Status;

	public BuildingConstructionStatusComponent( BuildingConstructionStatus status ) {
		Status = status;
	}
	
}


public enum BuildingConstructionStatus : byte {
	Building,
	Finished
}
