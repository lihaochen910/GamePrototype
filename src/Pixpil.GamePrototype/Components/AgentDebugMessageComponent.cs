using Bang.Components;
using Murder.Utilities.Attributes;


namespace Pixpil.Components; 

[RuntimeOnly]
public readonly struct AgentDebugMessageComponent : IComponent {
	
	public readonly string Msg;
	public readonly float DestroyDelay;
	public readonly float Size;

	public AgentDebugMessageComponent( string msg, float destroyDelay = 3f ) {
		Msg = msg;
		DestroyDelay = destroyDelay;
		Size = 1f;
	}
	
	public AgentDebugMessageComponent( string msg, float destroyDelay = 3f, float size = 1f ) {
		Msg = msg;
		DestroyDelay = destroyDelay;
		Size = size;
	}
	
}
