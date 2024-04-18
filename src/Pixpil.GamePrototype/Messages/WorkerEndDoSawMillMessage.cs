using Bang.Components;
using Bang.Entities;


namespace Pixpil.Messages; 

public readonly struct WorkerEndDoSawMillMessage : IMessage {
	
	public readonly Entity WorkerEntity { get; init; }
	public readonly int BuildingEntityId;

	public WorkerEndDoSawMillMessage( Entity entity, int buildingEntityId ) {
		WorkerEntity = entity;
		BuildingEntityId = buildingEntityId;
	}
}
