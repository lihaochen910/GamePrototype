using Bang;
using Bang.Contexts;
using Bang.Entities;
using Pixpil.Components;


namespace Pixpil.Services; 

public static class WorkerService {
	
	
	public static Entity FindSpecifyBuildingNeedWorker( World world, BuildingType buildingType ) {
		var buildings = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( BuildingComponent ), typeof( BuildingWorkersInConstructingComponent ), typeof( BuildingConstructionStatusComponent ) );
		foreach ( var building in buildings ) {
			if ( building.HasIsPlacingBuilding() ) {
				continue;
			}
			
			var buildingWorkersInConstructing = building.GetBuildingWorkersInConstructing();
			var needWorkersCount = buildingWorkersInConstructing.SpaceRemaining();
			if ( needWorkersCount > 0 && building.GetBuilding().Type == buildingType ) {
				return building;
			}
		}

		return null;
	}


	public static Entity FindSpecifyBuildingNeedWorkerDoJob( World world, BuildingType buildingType ) {
		var buildings = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( BuildingComponent ), typeof( BuildingWorkersInWorkingComponent ) );
		foreach ( var building in buildings ) {
			if ( building.HasIsPlacingBuilding() || building.HasBuildingConstructionStatus() ) {
				continue;
			}
			
			var buildingWorkersInWorking = building.GetBuildingWorkersInWorking();
			var needWorkersCount = buildingWorkersInWorking.SpaceRemaining();
			if ( needWorkersCount > 0 && building.GetBuilding().Type == buildingType ) {
				return building;
			}
		}

		return null;
	}

	
	public static Entity GetIdleWorker( World world ) {
		var workers = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( WorkerComponent ) );
		foreach ( var worker in workers ) {
			
			if ( worker.HasUtilityAiAgent() && worker.GetUtilityAiAgent().UtilityAiAsset != LibraryServices.GetLibrary().WorkerAI_Idle ) {
				continue;
			}
			
			if ( !worker.HasWorkerWorkConstruct() ) {
				return worker;
			}
		}

		return null;
	}


	public static bool HasIdleWorker( World world ) {
		return GetIdleWorker( world ) is not null;
	}


	public static bool CheckWorkerHasTask( Entity workerEntity ) {
		return workerEntity.HasWorkerWorkConstruct();
	}
	
	
}
