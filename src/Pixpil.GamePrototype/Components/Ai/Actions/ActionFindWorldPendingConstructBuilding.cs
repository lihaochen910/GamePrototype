using System.Linq;
using Bang.Entities;
using Pixpil.Services;


namespace Pixpil.AI.Actions; 

public class ActionFindWorldPendingConstructBuilding : GoapAction {

	public readonly string WriteToBBValue;
	
	public override GoapActionExecuteStatus OnPreExecute() {
		var gameplayBlackboard = SaveServices.GetGameplay();
		if ( gameplayBlackboard.PendingConstructBuildings.IsDefaultOrEmpty ) {
			return GoapActionExecuteStatus.Failure;
		}

		if ( string.IsNullOrEmpty( WriteToBBValue ) ) {
			return GoapActionExecuteStatus.Failure;
		}

		var building = World.TryGetEntity( gameplayBlackboard.PendingConstructBuildings.First() );
		if ( building is null ) {
			return GoapActionExecuteStatus.Failure;
		}

		var myBlackboard = Entity.TryGetBlackboard();
		if ( myBlackboard is null ) {
			return GoapActionExecuteStatus.Failure;
		}

		Entity.SetBlackboard( myBlackboard.Value.SetValue( WriteToBBValue, building.EntityId ) );
		return GoapActionExecuteStatus.Success;
	}
	
}
