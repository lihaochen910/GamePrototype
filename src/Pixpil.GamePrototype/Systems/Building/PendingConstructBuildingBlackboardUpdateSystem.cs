using System.Collections.Immutable;
using System.Linq;
using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.Systems; 

[Watch( typeof( BuildingConstructionStatusComponent ) )]
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( BuildingConstructionStatusComponent ) )]
public class PendingConstructBuildingBlackboardUpdateSystem : IStartupSystem, IReactiveSystem {
	
	public void Start( Context context ) {
		OnCheck( context.World );
	}

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		OnCheck( world );
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		OnCheck( world );
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		OnCheck( world );
	}

	private void OnCheck( World world ) {
		var blackboard = SaveServices.GetGameplay();
		var entities = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( BuildingConstructionStatusComponent ) );
		
		blackboard.HasPendingConstructBuilding = entities.FirstOrDefault( entity => entity.GetBuildingConstructionStatus().Status == BuildingConstructionStatus.Building ) != null;
		blackboard.PendingConstructBuildings = entities.Where( entity => entity.GetBuildingConstructionStatus().Status == BuildingConstructionStatus.Building ).Select( entity => entity.EntityId ).ToImmutableArray();
	}
}
