using Bang;
using Bang.Contexts;
using Bang.Entities;
using Pixpil.Components;


namespace Pixpil.AI; 

public class CheckWorldHasSpecifyBuilding : GoapCondition {
	
	public readonly BuildingType BuildingType;

	public override bool OnCheck( World world, Entity entity ) {
		var buildings = world.GetEntitiesWith( ContextAccessorFilter.AnyOf, typeof( BuildingComponent ) );
		if ( buildings.IsDefaultOrEmpty ) {
			return false;
		}
		
		foreach ( var building in buildings ) {
			if ( building.GetBuilding().Type == BuildingType ) {
				return true;
			}
		}

		return false;
	}
}
