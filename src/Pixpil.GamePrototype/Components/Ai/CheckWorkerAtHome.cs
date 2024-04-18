using Bang;
using Bang.Entities;


namespace Pixpil.AI; 

public class CheckWorkerAtHome : GoapCondition {

	public override bool OnCheck( World world, Entity entity ) {
		var workerBelongWhichDormitoryComponent = entity.TryGetWorkerBelongWhichDormitory();
		if ( workerBelongWhichDormitoryComponent is null ) {
			return false;
		}
		
		if ( entity.TryGetAgentInBuildingRange() is {} agentInBuildingRangeComponent ) {
			if ( agentInBuildingRangeComponent.HasId( workerBelongWhichDormitoryComponent.Value.DormitoryEntityId ) ) {
				return true;
			}
		}

		return false;
	}
}
