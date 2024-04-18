using System;
using Bang;
using Bang.Entities;
using Murder;
using Murder.Assets;
using Murder.Attributes;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.AI; 

public class CheckPlayerInventory : GoapCondition {

	public readonly string ItemId;
	public readonly CompareMethod Method;
	public readonly int CompareToCount;

	public override bool OnCheck( World world, Entity entity ) {
		var playerEntity = world.TryGetUniqueEntity< PlayerComponent >();
		if ( playerEntity is not null ) {
			var inventory = playerEntity.TryGetComponent< InventoryComponent >();
			if ( inventory is not null ) {
				var itemType = ItemTypeServices.GetItemType( ItemId );
				if ( itemType is not null ) {
					int count = inventory.Value.GetItemCount( itemType );
					return NumericCompareHelper.Compare( count, Method, CompareToCount );
				}
			}
		}

		return false;
	}
}


public class CheckPlayerInventoryReadyForBuild : GoapCondition {
	
	[GameAssetId(typeof( PrefabAsset ))]
	public readonly Guid BuildingPrefab;
	
	public override bool OnCheck( World world, Entity entity ) {
		var playerEntity = world.TryGetUniqueEntity< PlayerComponent >();
		var buildingPrefabAsset = Game.Data.TryGetAsset< PrefabAsset >( BuildingPrefab );
		if ( playerEntity is not null && buildingPrefabAsset is not null ) {
			if ( !buildingPrefabAsset.HasComponent( typeof( BuildingConstructRequireResourcesComponent ) ) ) {
				return true;
			}
			
			var inventory = playerEntity.TryGetComponent< InventoryComponent >();
			if ( inventory is not null ) {
				
				var require = ( BuildingConstructRequireResourcesComponent )buildingPrefabAsset.GetComponent( typeof( BuildingConstructRequireResourcesComponent ) );
				foreach ( var entry in require.Requires ) {
					var count = inventory.Value.GetItemCount( entry.ItemType );
					if ( count <= 0 && entry.Count > 0 ) {
						return false;
					}
				}

				return true;
			}
		}

		return false;
	}
}
