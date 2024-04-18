using System.Linq;
using Bang;
using Bang.Entities;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.AI; 

public class CheckBuildingConstructComplete : GoapCondition {
	
	public override bool OnCheck( World world, Entity entity ) {
		if ( entity.TryGetBuildingConstructionStatus() is {} buildingConstructionStatusComponent ) {
			return buildingConstructionStatusComponent.Status is BuildingConstructionStatus.Finished;
		}

		return true;
	}
}


public class CheckBuildingConstructRequireResourcesEnough : GoapCondition {
	public override bool OnCheck( World world, Entity entity ) {
		if ( entity.HasBuildingConstructRequireResources() ) {
			return entity.GetBuildingConstructRequireResources().CheckInventoryResourceEnough( entity );
		}

		return true;
	}
}


public class CheckBuildingInPlacingMode : GoapCondition {
	public override bool OnCheck( World world, Entity entity ) {
		return entity.HasIsPlacingBuilding();
	}
}


public class CheckWorldHasPendingConstructBuilding : GoapCondition {
	public override bool OnCheck( World world, Entity entity ) {
		var gameplayBlackboard = SaveServices.GetGameplay();
		if ( gameplayBlackboard.PendingConstructBuildings.IsDefaultOrEmpty ) {
			return false;
		}
		
		var building = world.TryGetEntity( gameplayBlackboard.PendingConstructBuildings.First() );
		if ( building is null ) {
			return false;
		}

		return true;
	}
}
