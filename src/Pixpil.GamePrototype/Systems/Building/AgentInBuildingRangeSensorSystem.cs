using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Systems;
using DigitalRune.Linq;
using Murder.Components;
using Murder.Messages.Physics;


namespace Pixpil.Systems.Building; 

[Filter(typeof(AgentComponent))]
[Messager(typeof(OnCollisionMessage))]
public class AgentInBuildingRangeSensorSystem : IMessagerSystem {

	public void OnMessage( World world, Entity entity, IMessage message ) {
		var msg = ( OnCollisionMessage )message;

		if ( world.TryGetEntity( msg.EntityId ) is not Entity actor ) {
			return;
		}

		var iter = TreeHelper.GetSelfAndAncestors( actor, entity1 => {
			if ( entity1.Parent.HasValue ) {
				return world.TryGetEntity( entity1.Parent.Value );
			}

			return null;
		} );

		Entity foundBuildingEntity = default;
		foreach ( var e in iter ) {
			if ( e.HasBuilding() ) {
				foundBuildingEntity = e;
				break;
			}
		}

		if ( foundBuildingEntity is null ) {
			return;
		}

		var agentInBuildingRangeComponent = entity.TryGetAgentInBuildingRange();
		
		if ( msg.Movement == Murder.Utilities.CollisionDirection.Enter ) {
			if ( agentInBuildingRangeComponent.HasValue ) {
				entity.SetAgentInBuildingRange( agentInBuildingRangeComponent.Value.Add( foundBuildingEntity.EntityId ) );
			}
			else {
				entity.SetAgentInBuildingRange( foundBuildingEntity.EntityId );
			}
		}
		
		if ( msg.Movement == Murder.Utilities.CollisionDirection.Exit ) {
			if ( agentInBuildingRangeComponent.HasValue ) {
				entity.SetAgentInBuildingRange( agentInBuildingRangeComponent.Value.Remove( foundBuildingEntity.EntityId ) );
			}
			else {
				// do nothing.
			}
		}
	}
}
