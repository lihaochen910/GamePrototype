using Bang.Components;
using Murder.Components;


namespace Pixpil.Components;

[Requires( typeof( ColliderComponent ), typeof( PositionComponent ) )]
public readonly struct ClearDarkFogOnTouchComponent : IComponent;


[Requires( typeof( ColliderComponent ), typeof( PositionComponent ) )]
public readonly struct ClearDarkFogOnAttachComponent : IComponent;


[Requires( typeof( ColliderComponent ), typeof( PositionComponent ) )]
public readonly struct DispelTheDarkFogComponent : IComponent {
	
	public readonly bool Active;

	public DispelTheDarkFogComponent( bool active ) => Active = active;
}


[Requires( typeof( DispelTheDarkFogComponent ) )]
public readonly struct ShowRangeDispelTheDarkFogComponent : IComponent;


[Requires( typeof( PositionComponent ) )]
public readonly struct DarkFogAgentComponent : IComponent;
