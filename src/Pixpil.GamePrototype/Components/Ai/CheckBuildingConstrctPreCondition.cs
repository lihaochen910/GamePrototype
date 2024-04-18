using Bang;
using Bang.Entities;
using Pixpil.Components;


namespace Pixpil.AI; 

public class CheckBuildingConstrctPreCondition : GoapCondition {
	
	public override bool OnCheck( World world, Entity entity ) {
		if ( !entity.HasBuildingConstructionStatus() ) {
			return true;
		}

		if ( !entity.HasBuildingConstructRequireResources() ) {
			return true;
		}
		
		if ( !entity.HasInventory() ) {
			return false;
		}

		var playerEntity = world.TryGetUniqueEntity< PlayerComponent >();
		if ( playerEntity is null ) {
			return false;
		}
		
		var requireComponent = entity.GetBuildingConstructRequireResources();
		foreach ( var entry in requireComponent.Requires ) {
			if ( playerEntity.GetInventory().GetItemCount( entry.ItemType ) < entry.Count ) {
				return false;
			}
		}

		return true;
	}
	
}
