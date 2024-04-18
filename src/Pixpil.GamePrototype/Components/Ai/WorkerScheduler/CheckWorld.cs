using Bang;
using Bang.Contexts;
using Bang.Entities;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.AI.WorkerScheduler; 

public class CheckWorldHasIdleWorker : GoapCondition {

	public override bool OnCheck( World world, Entity entity ) {
		return WorkerService.HasIdleWorker( world );
	}
}


public class CheckWorldConstructingNeedWorker : GoapCondition {
	
	public override bool OnCheck( World world, Entity entity ) {
		var buildings = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( BuildingComponent ), typeof( BuildingWorkersInConstructingComponent ), typeof( BuildingConstructionStatusComponent ) );
		foreach ( var building in buildings ) {
			if ( building.HasIsPlacingBuilding() ) {
				continue;
			}
			
			var buildingWorkersInConstructing = building.GetBuildingWorkersInConstructing();
			var needWorkersCount = buildingWorkersInConstructing.SpaceRemaining();
			if ( needWorkersCount > 0 ) {
				return true;
			}
		}

		return false;
	}
	
}


public class CheckWorldSpecifyBuildingNeedWorker : GoapCondition {
	
	public readonly BuildingType Type;

	public override bool OnCheck( World world, Entity entity ) {
		var buildings = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( BuildingComponent ), typeof( BuildingWorkersInWorkingComponent ) );
		foreach ( var building in buildings ) {
			if ( building.HasIsPlacingBuilding() || building.HasBuildingConstructionStatus() ) {
				continue;
			}
			
			var buildingWorkersInWorking = building.GetBuildingWorkersInWorking();
			var needWorkersCount = buildingWorkersInWorking.SpaceRemaining();
			if ( needWorkersCount > 0 && building.GetBuilding().Type == Type ) {
				return true;
			}
		}

		return false;
	}
}


public class CheckWorldSpecifyBuildingConstructingNeedWorker : GoapCondition {
	
	public readonly BuildingType Type;

	public override bool OnCheck( World world, Entity entity ) {
		var buildings = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( BuildingComponent ), typeof( BuildingWorkersInConstructingComponent ), typeof( BuildingConstructionStatusComponent ) );
		foreach ( var building in buildings ) {
			if ( building.HasIsPlacingBuilding() ) {
				continue;
			}
			
			var buildingWorkersInConstructing = building.GetBuildingWorkersInConstructing();
			var needWorkersCount = buildingWorkersInConstructing.SpaceRemaining();
			if ( needWorkersCount > 0 && building.GetBuilding().Type == Type ) {
				return true;
			}
		}

		return false;
	}
}


public class CheckWorldSpecifyBuildingHasWorkerInWorking : GoapCondition {
	
	public readonly BuildingType Type;

	public override bool OnCheck( World world, Entity entity ) {
		var buildings = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( BuildingComponent ), typeof( BuildingWorkersInWorkingComponent ) );
		foreach ( var building in buildings ) {
			if ( building.GetBuilding().Type == Type ) {
				if ( !building.GetBuildingWorkersInWorking().Workers.IsDefaultOrEmpty ) {
					return true;
				}
			}
		}

		return false;
	}
}
