using Bang.Components;
using Bang.Entities;


namespace Pixpil.Messages; 

/// <summary>
/// message from worker to scheduler
/// </summary>
public readonly struct WorkerEndConstructMessage : IMessage {
	
	public readonly Entity WorkerEntity { get; init; }
	public readonly int BuildingEntityId;

	public WorkerEndConstructMessage( Entity entity, int buildingEntityId ) {
		WorkerEntity = entity;
		BuildingEntityId = buildingEntityId;
	}
}
