using System.Collections.Immutable;
using System.Linq;
using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.Systems; 

[Watch( typeof( BuildingComponent ) )]
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( BuildingComponent ) )]
public class WorldBuildingBlackboardUpdateSystem : IStartupSystem, IReactiveSystem {
	
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
		var entities = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( DormitoryComponent ) );
		
		var iter = entities.Where( entity => {
			bool isConstructing = entity.HasBuildingConstructionStatus();
			return !isConstructing;
		} );

		var population = 0;
		foreach ( var entity in iter ) {
			population += entity.GetDormitory().Capacity;
		}

		blackboard.Population = population;
	}
}
