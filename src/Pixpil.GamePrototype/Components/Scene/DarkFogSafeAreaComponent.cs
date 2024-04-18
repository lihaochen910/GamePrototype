using Bang.Components;
using Murder.Components;


namespace Pixpil.Components;

[Requires( typeof( PositionComponent ), typeof( ColliderComponent ) )]
public readonly struct DarkFogSafeAreaComponent : IComponent {
	
	public DarkFogSafeAreaComponent() {}
	
}
