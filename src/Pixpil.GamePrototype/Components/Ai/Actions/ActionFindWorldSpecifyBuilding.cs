using Bang.Contexts;
using Bang.Entities;
using Pixpil.Components;


namespace Pixpil.AI.Actions; 

public class ActionFindWorldSpecifyBuilding : GoapAction {
	
	public readonly BuildingType BuildingType;
	public readonly string WriteToBBValue;

	public override GoapActionExecuteStatus OnPreExecute() {
		
		if ( string.IsNullOrEmpty( WriteToBBValue ) ) {
			return GoapActionExecuteStatus.Failure;
		}

		var buildings = World.GetEntitiesWith( ContextAccessorFilter.AnyOf, typeof( BuildingComponent ) );
		if ( buildings.IsDefaultOrEmpty ) {
			return GoapActionExecuteStatus.Failure;
		}

		var myBlackboard = Entity.TryGetBlackboard();
		if ( myBlackboard is null ) {
			return GoapActionExecuteStatus.Failure;
		}

		foreach ( var building in buildings ) {
			if ( building.GetBuilding().Type == BuildingType ) {
				Entity.SetBlackboard( myBlackboard.Value.SetValue( WriteToBBValue, building.EntityId ) );
				return GoapActionExecuteStatus.Success;
			}
		}
		
		return GoapActionExecuteStatus.Failure;
	}
}
