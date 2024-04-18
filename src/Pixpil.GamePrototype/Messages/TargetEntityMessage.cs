using Bang.Components;
using Bang.Entities;


namespace Pixpil.Messages;

public readonly struct TargetEntityMessage : IMessage {
	public readonly Entity Entity { get; init; }

	public TargetEntityMessage( Entity entity ) => Entity = entity;
}
