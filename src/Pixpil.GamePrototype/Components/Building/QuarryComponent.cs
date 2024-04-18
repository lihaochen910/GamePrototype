using Bang.Components;
using Murder.Attributes;
using Murder.Utilities.Attributes;


namespace Pixpil.Components; 

[Requires(typeof( BuildingComponent ))]
public readonly struct QuarryComponent : IComponent {
	
	/// <summary>
	/// 多久产出一次素材
	/// </summary>
	public readonly float ProcessTime;

	/// <summary>
	/// 每次产出素材的数量
	/// </summary>
	public readonly int ProcessCount;
	
	public QuarryComponent( float processTime, int processCount ) {
		ProcessTime = processTime;
		ProcessCount = processCount;
	}
	
}


[RuntimeOnly, DoNotPersistOnSave]
public readonly struct QuarryProcessTimerComponent : IComponent {
	
	/// <summary>
	/// 多久产出一次素材
	/// </summary>
	public readonly float Time;
	
	public QuarryProcessTimerComponent( float time ) {
		Time = time;
	}
	
}
