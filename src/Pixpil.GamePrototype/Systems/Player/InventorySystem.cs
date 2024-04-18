using System.Collections.Immutable;
using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Pixpil.Components;
using Pixpil.Data;


namespace Pixpil.Systems; 

[Filter(ContextAccessorFilter.AnyOf, typeof(InventoryComponent))]
[Watch(typeof(InventoryComponent))]
public class InventoryInitializeSystem : IReactiveSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			// var inventoryComponent = entity.GetInventory();
			// if ( inventoryComponent.Inventory is null ) {
			// 	var inventory = new Inventory( inventoryComponent.InitialSize );
			// 	entity.SetInventory( inventoryComponent.InitialSize, inventory );
			// }
		}
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		
	}
}
