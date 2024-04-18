using System.Linq;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder;
using Pixpil.Components;
using Pixpil.Data;


namespace Pixpil.Systems.Building; 

/// <summary>
/// 调试用: 指定建筑可以自行建造
/// </summary>
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Write, typeof( BuildingConstructionStatusComponent ), typeof( BuildingSelfConstructionAbilityComponent ), typeof( BuildingConstructRequireResourcesComponent ), typeof( InventoryComponent ) )]
[Filter( ContextAccessorFilter.NoneOf, typeof( IsPlacingBuildingComponent ) )]
public class BuildingConstructSystem : IUpdateSystem {

	public void Update( Context context ) {
		
		var playerEntity = context.World.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( PlayerComponent ), typeof( InventoryComponent ) ).FirstOrDefault();
		if ( playerEntity is null ) {
			return;
		}
		
		var speedOfTime = ( Game.Preferences as GamePrototypePreferences ).SpeedOfTime;
		var deltaTime = Game.DeltaTime * speedOfTime;

		bool CheckBuildingConstrctPreCondition( Entity entity ) {
			var requireComponent = entity.GetBuildingConstructRequireResources();
			foreach ( var entry in requireComponent.Requires ) {
				if ( playerEntity.GetInventory().GetItemCount( entry.ItemType ) < entry.Count - entity.GetInventory().GetItemCount( entry.ItemType ) ) {
					return false;
				}
			}

			return true;
		}
		
		foreach ( var entity in context.Entities ) {

			if ( entity.GetBuildingConstructionStatus().Status != BuildingConstructionStatus.Building ) {
				continue;
			}

			if ( !CheckBuildingConstrctPreCondition( entity ) ) {
				continue;
			}
			
			void DoSubmit( int submitCount ) {
				entity.GetBuildingConstructRequireResources().SubmitResourceCount( submitCount, playerEntity, entity );
			}
		
			var buildingSelfConstructionAbility = entity.GetBuildingSelfConstructionAbility();
			if ( !entity.HasBuildingSelfBuildingBuildingSubmitTimer() ) {
				entity.SetBuildingSelfBuildingBuildingSubmitTimer( buildingSelfConstructionAbility.SubmitTime );
			}
		
			var time = entity.GetBuildingSelfBuildingBuildingSubmitTimer().SubmitTime - deltaTime;
			if ( time < 0f ) {
				DoSubmit( buildingSelfConstructionAbility.SubmitCount );
				time = buildingSelfConstructionAbility.SubmitTime;
				entity.SetBuildingSelfBuildingBuildingSubmitTimer( time );
				continue;
			}
		
			entity.SetBuildingSelfBuildingBuildingSubmitTimer( time );
		}
		
	}
}
