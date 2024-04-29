using System.Collections.Immutable;
using Bang.Components;
using Bang.Entities;
using Murder.Attributes;
using Murder.Utilities.Attributes;


namespace Pixpil.Components;

public enum BuildingType : short {
	/// <summary>
	/// 住房
	/// </summary>
	Dormitory,
	
	/// <summary>
	/// 灯塔
	/// </summary>
	LightHouse,
	
	/// <summary>
	/// 采石场
	/// </summary>
	Quarry,
	
	GatherYa,
	
	/// <summary>
	/// 伐木场
	/// </summary>
	SawMill,
	
	/// <summary>
	/// 农场
	/// </summary>
	Farm,
	
	/// <summary>
	/// 泉水收集
	/// </summary>
	SpringCollector
}


public readonly struct BuildingComponent : IComponent {
	
	public readonly BuildingType Type;

	public bool CheckBuildingReady( in Entity entity ) {
		return !entity.HasIsPlacingBuilding() && !entity.HasBuildingWorkersInConstructing();
	}
}


/// <summary>
/// 正在建造该建筑的工人
/// </summary>
[Requires(typeof( BuildingComponent ))]
public readonly struct BuildingWorkersInConstructingComponent : IComponent {
	
	public readonly int Capcity = 0;
	public readonly ImmutableArray< int > Workers = [];
	
	
	public BuildingWorkersInConstructingComponent( int capcity ) {
		Capcity = capcity;
	}

	public BuildingWorkersInConstructingComponent( int capcity, ImmutableArray< int > workers ) {
		Capcity = capcity;
		Workers = workers;
	}

	public bool HasSpace() => Workers.Length < Capcity;

	public int SpaceRemaining() => Capcity - Workers.Length;
}


/// <summary>
/// 正在该建筑中工作的工人
/// </summary>
[Requires(typeof( BuildingComponent ))]
public readonly struct BuildingWorkersInWorkingComponent : IComponent {
	
	public readonly int Capcity = 0;
	public readonly ImmutableArray< int > Workers = [];
	
	
	public BuildingWorkersInWorkingComponent( int capcity ) {
		Capcity = capcity;
	}

	public BuildingWorkersInWorkingComponent( int capcity, ImmutableArray< int > workers ) {
		Capcity = capcity;
		Workers = workers;
	}

	public bool HasSpace() => Workers.Length < Capcity;

	public int SpaceRemaining() => Capcity - Workers.Length;
}


[Requires(typeof( BuildingComponent ))]
public readonly struct BuildingSelfConstructionAbilityComponent : IComponent {
	
	/// <summary>
	/// 建造速度为多久提交一次素材
	/// </summary>
	public readonly float SubmitTime;

	/// <summary>
	/// 每次提交素材的数量
	/// </summary>
	public readonly int SubmitCount;
	
	public BuildingSelfConstructionAbilityComponent( float submitTime, int submitCount ) {
		SubmitTime = submitTime;
		SubmitCount = submitCount;
	}
	
}


[RuntimeOnly, DoNotPersistOnSave]
public readonly struct BuildingSelfBuildingBuildingSubmitTimer : IComponent {
	
	public readonly float SubmitTime;

	public BuildingSelfBuildingBuildingSubmitTimer( float submitTime ) {
		SubmitTime = submitTime;
	}
}


[RuntimeOnly, DoNotPersistOnSave]
public readonly struct BuildingSelfPausedComponent : IComponent;
