using Bang.Components;
using Murder.Utilities.Attributes;
using Pixpil.Data;


namespace Pixpil.Components;

public readonly struct WorldResourceComponent : IComponent {
	
	public WorldResourceComponent() {}
	
}


public readonly struct WorldResourceRenewableComponent : IComponent {

	public readonly ItemType ItemType;
	
	/// <summary>
	/// 上限
	/// </summary>
	public readonly int Max;
	
	/// <summary>
	/// 再生速度
	/// </summary>
	public readonly float Speed;
	
	/// <summary>
	/// 触发再生时, 增加的数量
	/// </summary>
	public readonly int RenewCount;
	
	public WorldResourceRenewableComponent() {}
	
}


[RuntimeOnly]
public readonly struct WorldResourceRenewCountdownComponent : IComponent {
	
	public readonly float Time;

	public WorldResourceRenewCountdownComponent( float time ) => Time = time;
}
