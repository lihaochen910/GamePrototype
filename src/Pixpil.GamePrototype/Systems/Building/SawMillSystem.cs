using System.Linq;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder;
using Pixpil.Components;
using Pixpil.Data;
using Pixpil.Services;


namespace Pixpil.Systems.Building; 

[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Write, typeof( BuildingComponent ), typeof( SawMillComponent ) )]
[Filter( ContextAccessorFilter.NoneOf, typeof( IsPlacingBuildingComponent ) )]
[Filter( ContextAccessorFilter.NoneOf, typeof( BuildingConstructionStatusComponent ) )]
public class SawMillSystem : IUpdateSystem {

	public void Update( Context context ) {
		var allResourceEntities = context.World.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( WorldResourceComponent ), typeof( InventoryComponent ) );
		var playerEntity = context.World.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( PlayerComponent ), typeof( InventoryComponent ) ).FirstOrDefault();
		if ( playerEntity is null ) {
			return;
		}

		var woodItemType = ItemTypeServices.GetItemType( "wood" );
		
		var speedOfTime = ( Game.Preferences as GamePrototypePreferences ).SpeedOfTime;
		var deltaTime = Game.DeltaTime * speedOfTime;
		
		bool CheckWorldHasResourcesEntityFor( Entity entity ) {
			
			foreach ( var resourceEntity in allResourceEntities ) {
				if ( resourceEntity.GetInventory().GetItemCount( woodItemType ) >= entity.GetSawMill().ProcessCount ) {
					return true;
				}
			}

			return false;
		}
		
		Entity FindWorldResourcesEntityFor( Entity entity ) {
			
			foreach ( var resourceEntity in allResourceEntities ) {
				if ( resourceEntity.GetInventory().GetItemCount( woodItemType ) >= entity.GetSawMill().ProcessCount ) {
					return resourceEntity;
				}
			}

			return null;
		}
		
		foreach ( var entity in context.Entities ) {

			if ( !CheckWorldHasResourcesEntityFor( entity ) ) {
				continue;
			}
			
			void DoProcess( int processCount ) {
				var resourceEntity = FindWorldResourcesEntityFor( entity );
				var ( result, count ) = resourceEntity.GetInventory().MoveItemToOther( woodItemType, processCount, resourceEntity, playerEntity );
				if ( result ) {
					entity.SetAgentDebugMessage( $"+{count} {woodItemType.Id}", 0.5f );
				}
			}
	
			var sawmillComponent = entity.GetSawMill();
			if ( !entity.HasSawMillProcessTimer() ) {
				entity.SetSawMillProcessTimer( sawmillComponent.ProcessTime );
			}
	
			var time = entity.GetSawMillProcessTimer().Time - deltaTime;
			if ( time < 0f ) {
				DoProcess( sawmillComponent.ProcessCount );
				time = sawmillComponent.ProcessTime;
				entity.SetSawMillProcessTimer( time );
				continue;
			}

			entity.SetSawMillProcessTimer( time );
		}
	}
}
