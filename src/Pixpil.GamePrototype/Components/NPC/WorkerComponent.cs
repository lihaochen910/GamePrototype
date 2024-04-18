using Bang.Components;
using Murder.Attributes;
using Murder.Utilities.Attributes;
using Pixpil.RPGStatSystem;


namespace Pixpil.Components;

[DoNotPersistEntityOnSave]
public readonly struct WorkerComponent : IComponent {
	
	public WorkerComponent() {}
	
}


public readonly struct WorkerBuildingBuilding : IComponent {
	
	public readonly int BuildingEntityId;

	public WorkerBuildingBuilding( int buildingEntityId ) {
		BuildingEntityId = buildingEntityId;
	}
}


public readonly struct WorkerDoWorking : IComponent {
	
	public readonly int BuildingEntityId;

	public WorkerDoWorking( int buildingEntityId ) {
		BuildingEntityId = buildingEntityId;
	}
}


[RuntimeOnly, DoNotPersistOnSave]
public readonly struct WorkerBuildingBuildingSubmitTimer : IComponent {
	
	public readonly float SubmitTime;

	public WorkerBuildingBuildingSubmitTimer( float submitTime ) {
		SubmitTime = submitTime;
	}
}


[Requires(typeof( WorkerComponent ))]
public readonly struct WorkerConstructionAbilityComponent : IComponent {
	
	/// <summary>
	/// 建造速度为多久提交一次素材
	/// </summary>
	public readonly float SubmitTime;

	/// <summary>
	/// 每次提交素材的数量
	/// </summary>
	public readonly int SubmitCount;

	public readonly RPGStatModifier BatteryPowerConsumption;

	public WorkerConstructionAbilityComponent( float submitTime, int submitCount, RPGStatModifier batteryPowerConsumption ) {
		SubmitTime = submitTime;
		SubmitCount = submitCount;
		BatteryPowerConsumption = batteryPowerConsumption;
	}
	
}


[Requires(typeof( WorkerComponent ))]
public readonly struct WorkerWorkAbilityComponent : IComponent {
	
	/// <summary>
	/// 建造速度为多久提交一次素材
	/// </summary>
	public readonly float ProcessSpeed;
	
	public WorkerWorkAbilityComponent( float processSpeed ) {
		ProcessSpeed = processSpeed;
	}
	
}


/// <summary>
/// 描述Worker属于住房
/// </summary>
[Requires(typeof( WorkerComponent ))]
public readonly struct WorkerBelongWhichDormitoryComponent : IComponent {
	
	public readonly int DormitoryEntityId;

	public WorkerBelongWhichDormitoryComponent( int dormitoryEntityId ) {
		DormitoryEntityId = dormitoryEntityId;
	}
}


public interface IWorkerTaskComponent : IComponent;


/// <summary>
/// 任务标志: 建造
/// </summary>
public readonly struct WorkerWorkConstructComponent : IWorkerTaskComponent {
	
	public readonly int BuildingEntityId;

	public WorkerWorkConstructComponent( int buildingEntityId ) {
		BuildingEntityId = buildingEntityId;
	}
}


/// <summary>
/// 任务标志: 采石场工作
/// </summary>
public readonly struct WorkerWorkDoQuarryComponent : IWorkerTaskComponent {
	
	public readonly int BuildingEntityId;

	public WorkerWorkDoQuarryComponent( int buildingEntityId ) {
		BuildingEntityId = buildingEntityId;
	}
}


/// <summary>
/// 任务标志: 伐木场工作
/// </summary>
public readonly struct WorkerWorkDoSawMillComponent : IWorkerTaskComponent {
	
	public readonly int BuildingEntityId;

	public WorkerWorkDoSawMillComponent( int buildingEntityId ) {
		BuildingEntityId = buildingEntityId;
	}
}
