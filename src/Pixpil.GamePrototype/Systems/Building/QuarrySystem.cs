using System.Linq;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder;
using Pixpil.Components;
using Pixpil.Data;
using Pixpil.Services;


namespace Pixpil.Systems.Building; 

[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Write, typeof( BuildingComponent ), typeof( QuarryComponent ) )]
[Filter( ContextAccessorFilter.NoneOf, typeof( IsPlacingBuildingComponent ), typeof( BuildingConstructionStatusComponent ) )]
public class QuarrySystem : IStartupSystem, IUpdateSystem {
	
	public void Start( Context context ) {
		var quarrys = context.World.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( BuildingComponent ), typeof( QuarryComponent ) );
		foreach ( var quarry in quarrys ) {
			quarry.SetQuarryProcessTimer( quarry.GetQuarry().ProcessTime );
		}
	}

	public void Update( Context context ) {
		var allResourceEntities = context.World.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( WorldResourceComponent ), typeof( InventoryComponent ) );
		var playerEntity = context.World.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( PlayerComponent ), typeof( InventoryComponent ) ).FirstOrDefault();
		if ( playerEntity is null ) {
			return;
		}

		var stoneItemType = ItemTypeServices.GetItemType( "stone" );
		
		var speedOfTime = ( Game.Preferences as GamePrototypePreferences ).SpeedOfTime;
		var deltaTime = Game.DeltaTime * speedOfTime;
		
		bool CheckWorldHasResourcesEntityFor( Entity entity ) {
			
			foreach ( var resourceEntity in allResourceEntities ) {
				if ( resourceEntity.GetInventory().GetItemCount( stoneItemType ) >= entity.GetQuarry().ProcessCount ) {
					return true;
				}
			}

			return false;
		}
		
		Entity FindWorldResourcesEntityFor( Entity entity ) {
			
			foreach ( var resourceEntity in allResourceEntities ) {
				if ( resourceEntity.GetInventory().GetItemCount( stoneItemType ) >= entity.GetQuarry().ProcessCount ) {
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
				var ( result, count ) = resourceEntity.GetInventory().MoveItemToOther( stoneItemType, processCount, resourceEntity, playerEntity );
				if ( result ) {
					entity.SetAgentDebugMessage( $"+{count} {stoneItemType.Id}", 0.5f );
				}
			}
	
			var quarryComponent = entity.GetQuarry();
			if ( !entity.HasQuarryProcessTimer() ) {
				entity.SetQuarryProcessTimer( quarryComponent.ProcessTime );
			}
	
			var time = entity.GetQuarryProcessTimer().Time;
			if ( time < 0f ) {
				DoProcess( quarryComponent.ProcessCount );
				entity.SetQuarryProcessTimer( quarryComponent.ProcessTime );
			}
		}
	}
	
}
