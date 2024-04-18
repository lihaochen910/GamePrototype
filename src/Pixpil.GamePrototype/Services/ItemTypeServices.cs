using System;
using System.Collections.Generic;
using Murder.Utilities;
using Pixpil.Data;


namespace Pixpil.Services; 

public static class ItemTypeServices {

	private static readonly Dictionary< string, ItemType > _itemTypes = new ();
	
	public static Lazy< Dictionary< string, ItemType > > ItemTypesLookup = new( () => {
		return CollectionHelper.ToStringDictionary( _itemTypes.Values, a => a.Id, a => a );
	} );

	public static void Initialize() {
		_itemTypes.Add( "stone", new ResourceItemType { Id = "stone" } );
		_itemTypes.Add( "wood", new ResourceItemType { Id = "wood" } );
		_itemTypes.Add( "food", new FoodItemType { Id = "food" } );
		_itemTypes.Add( "battery_core", new ItemType { Id = "battery_core" } );
		_itemTypes.Add( "batterypack_S", new ItemType { Id = "batterypack_S" } );
		_itemTypes.Add( "batterypack_M", new ItemType { Id = "batterypack_M" } );
		_itemTypes.Add( "batterypack_L", new ItemType { Id = "batterypack_L" } );
	}


	public static ItemType GetItemType( string id ) {
		if ( string.IsNullOrEmpty( id ) ) {
			return null;
		}
		
		if ( _itemTypes.TryGetValue( id, out var type ) ) {
			return type;
		}

		return null;
	}


	public static T GetItemType< T >( string id ) where T : ItemType {
		return GetItemType( id ) as T;
	}

}
