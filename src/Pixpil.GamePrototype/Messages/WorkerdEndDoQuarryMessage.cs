using Bang.Components;
using Bang.Entities;


namespace Pixpil.Messages; 

public readonly struct WorkerEndDoQuarryMessage : IMessage {
	
	public readonly Entity WorkerEntity { get; init; }
	public readonly int BuildingEntityId;

	public WorkerEndDoQuarryMessage( Entity entity, int buildingEntityId ) {
		WorkerEntity = entity;
		BuildingEntityId = buildingEntityId;
	}
}
