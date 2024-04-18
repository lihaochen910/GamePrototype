using Bang.Components;
using Bang.Entities;


namespace Pixpil.Messages;

/// <summary>
/// Indicates that an agent is trying to perform an action symbolized by an InputButton
/// </summary>
public readonly struct StartPlacingBuildingMessage : IMessage {
	
	public readonly int BuildingTypeIndex;

	public StartPlacingBuildingMessage( int idx ) {
		BuildingTypeIndex = idx;
	}
}


public readonly struct FinishedPlacingBuildingMessage : IMessage {
	
	public readonly Entity Building;

	public FinishedPlacingBuildingMessage( Entity building ) {
		Building = building;
	}
}
