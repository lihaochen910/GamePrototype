using Bang;
using Bang.Entities;
using Pixpil.Components;


namespace Pixpil.AI; 

public class CheckAgentInBuildingRange : GoapCondition {

	public readonly BuildingType BuildingType;

	public override bool OnCheck( World world, Entity entity ) {
		var agentInBuildingRangeComponent = entity.TryGetAgentInBuildingRange();
		if ( !agentInBuildingRangeComponent.HasValue ) {
			return false;
		}

		foreach ( var e in agentInBuildingRangeComponent.Value.GetInRangeEntities( world ) ) {
			if ( e.GetBuilding().Type == BuildingType ) {
				return true;
			}
		}

		return false;
	}
}


public class CheckAgentInBBVarBuildingRange : GoapCondition {
	
	public readonly string BBVar;

	public override bool OnCheck( World world, Entity entity ) {
		if ( entity.TryGetBlackboard() is BlackboardComponent blackboardComponent ) {
			
			if ( !blackboardComponent.HasVariableWithType( BBVar, typeof( int ) ) ) {
				return false;
			}

			if ( entity.TryGetAgentInBuildingRange() is {} agentInBuildingRangeComponent ) {
				if ( agentInBuildingRangeComponent.HasId( blackboardComponent.GetValue< int >( BBVar ) ) ) {
					return true;
				}
			}
		}

		return false;
	}
}


public class CheckAgentInConstructingTargetRange : GoapCondition {
	
	public override bool OnCheck( World world, Entity entity ) {
		var workerWorkConstructComponent = entity.TryGetWorkerWorkConstruct();
		if ( workerWorkConstructComponent is null ) {
			return false;
		}
		
		if ( entity.TryGetAgentInBuildingRange() is {} agentInBuildingRangeComponent ) {
			if ( agentInBuildingRangeComponent.HasId( workerWorkConstructComponent.Value.BuildingEntityId ) ) {
				return true;
			}
		}

		return false;
	}
	
}
