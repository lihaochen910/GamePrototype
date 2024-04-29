using System;
using System.Collections.Immutable;
using Bang.Components;
using Bang.Entities;
using Pixpil.Data;


namespace Pixpil.Components; 

public readonly struct BuildingConstructRequireResourcesComponent : IComponent {
	
	public readonly ImmutableArray< ReadOnlyInventoryEntry > Requires = [];

	public BuildingConstructRequireResourcesComponent( ImmutableArray< ReadOnlyInventoryEntry > requires ) {
		Requires = requires;
	}


	public void SubmitResourceCount( int resourceCount, Entity from, Entity to ) {
		if ( !from.HasInventory() ) {
			return;
		}

		var fromInventory = from.GetInventory();
		if ( to.TryGetInventory() is {} inventoryComponent ) {
			var remainCount = resourceCount;
			if ( !Requires.IsDefaultOrEmpty ) {
				foreach ( var entry in Requires ) {
					var fromInventoryCount = fromInventory.GetItemCount( entry.ItemType );
					var inventoryCount = inventoryComponent.GetItemCount( entry.ItemType );
					int submited;
					if ( inventoryCount < entry.Count ) {
						submited = Math.Clamp( entry.Count - inventoryCount, 0, Math.Min( remainCount, fromInventoryCount ) );
						
						// // clamp to from
						// if ( fromInventoryCount < submited ) {
						// 	submited = fromInventoryCount;
						// }
						if ( inventoryComponent.AddItem( entry.ItemType, submited, to ) ) {
							fromInventory.RemoveItem( entry.ItemType, submited, from );
							remainCount -= submited;
						}
					}
					
					if ( remainCount <= 0 ) {
						break;
					}
				}
				
				// target.SetInventory( inventoryComponent );
				// to.ReplaceComponent( inventoryComponent, typeof( InventoryComponent ), true );
			}
		}
	}


	public bool CheckInventoryResourceEnough( Entity target ) {
		if ( target.TryGetInventory() is {} inventoryComponent ) {
			foreach ( var entry in Requires ) {
				if ( inventoryComponent.GetItemCount( entry.ItemType ) < entry.Count ) {
					return false;
				}
			}

			return true;
		}

		return false;
	}


	public bool CheckInventoryHasResource( Entity target ) {
		if ( target.TryGetInventory() is {} inventoryComponent ) {
			foreach ( var entry in Requires ) {
				if ( inventoryComponent.GetItemCount( entry.ItemType ) > 0 ) {
					return true;
				}
			}

			return false;
		}

		return false;
	}


	public void DoConsumeRequiredResources( Entity target ) {
		if ( target.TryGetInventory() is {} inventoryComponent ) {
			foreach ( var entry in Requires ) {
				inventoryComponent.RemoveItem( entry.ItemType, entry.Count, target );
			}
		}
	}
}
