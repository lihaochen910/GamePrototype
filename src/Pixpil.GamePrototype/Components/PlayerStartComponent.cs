using Bang.Components;
using Murder.Components;


namespace Pixpil.Components;

[Unique]
[Requires(typeof(PositionComponent))]
public readonly struct PlayerStartComponent : IComponent {
	
	public PlayerStartComponent() {}
	
}
