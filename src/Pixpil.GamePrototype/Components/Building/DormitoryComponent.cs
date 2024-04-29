using System.Collections.Immutable;
using Bang.Components;


namespace Pixpil.Components; 

public readonly struct DormitoryComponent : IComponent {
	public readonly int Capacity;
}


/// <summary>
/// 描述住房中人员情况
/// 总数不应该超过DormitoryComponent组件中的Capacity字段
/// </summary>
[Requires( typeof( DormitoryComponent ) )]
public readonly struct DormitoryPersonnelSituationComponent : IComponent {
	
	/// <summary>
	/// 指示有多少空闲的人
	/// </summary>
	public readonly ImmutableArray< int > IdleStaff = [];
	
	/// <summary>
	/// 有多少忙碌的人
	/// </summary>
	public readonly ImmutableArray< int > BusyStaff = [];
	
	
	public int Totals {
		get {
			var workersInIdle = !IdleStaff.IsDefaultOrEmpty ? IdleStaff.Length : 0;
			var workersInBusy = !BusyStaff.IsDefaultOrEmpty ? BusyStaff.Length : 0;
			return workersInIdle + workersInBusy;
		}
	}


	public DormitoryPersonnelSituationComponent( ImmutableArray< int > idleStaff, ImmutableArray< int > busyStaff ) {
		IdleStaff = idleStaff;
		BusyStaff = busyStaff;
	}
}
