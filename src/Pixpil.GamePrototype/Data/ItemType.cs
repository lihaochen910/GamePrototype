using System;
using Pixpil.Services;


namespace Pixpil.Data; 

public class ItemType {
	
	public string Id;

	public override int GetHashCode() {
		return Id.GetHashCode();
	}

	public override bool Equals( object obj ) {
		if ( obj is ItemType otherItemType && otherItemType != null ) {
			return Id == otherItemType.Id;
		}
		return base.Equals( obj );
	}
}


public class ResourceItemType : ItemType;


public class FoodItemType : ItemType;


// public class ItemTypeConverter : JsonConverter {
// 	
// 	public override bool CanConvert( Type objectType ) {
// 		return objectType.IsAssignableTo( typeof( ItemType ) );
// 	}
//
// 	public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer ) {
// 		var itemType = value as ItemType;
// 		writer.WriteValue( itemType?.Id );
// 	}
//
// 	public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) {
// 		return ItemTypeServices.GetItemType( reader.Value as string );
// 	}
// 	
// }
