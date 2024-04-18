using System.Collections.Immutable;
using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DigitalRune.Linq;
using Pixpil.Components;


namespace Pixpil.Systems.Building; 

[Watch( typeof( InventoryComponent ) )]
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Write, typeof( InventoryComponent ), typeof( BuildingConstructRequireResourcesComponent ), typeof( BuildingConstructionStatusComponent ) )]
public class BuildingConstructionProgressMonitoringSystem : IReactiveSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		OnCheck( world, entities );
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		OnCheck( world, entities );
	}

	private void OnCheck( World world, ImmutableArray< Entity > entities ) {
		entities.ForEach( entity => {
			if ( entity.GetBuildingConstructRequireResources().CheckInventoryResourceEnough( entity ) ) {
				entity.GetBuildingConstructRequireResources().DoConsumeRequiredResources( entity );
				entity.SetBuildingConstructionStatus( BuildingConstructionStatus.Finished );
				entity.RemoveBuildingSelfBuildingBuildingSubmitTimer();
				entity.RemoveBuildingConstructionStatus();

				// force trigger event
				var buildingComponent = entity.GetBuilding();
				entity.ReplaceComponent( buildingComponent, typeof( BuildingComponent ), true );
			}
		} );
	}
}
