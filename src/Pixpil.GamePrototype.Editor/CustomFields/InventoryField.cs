using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using ImGuiNET;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomFields;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using Pixpil.Data;
using Pixpil.Services;


namespace Pixpil.Editor.CustomFields;

// [CustomFieldOf( typeof( Inventory ) )]
// public class InventoryField : CustomField {
//
// 	private int _newSize;
//
// 	public override (bool modified, object result) ProcessInput( EditorMember member, object fieldValue ) {
// 		bool modified = false;
// 		var inventory = fieldValue as Inventory;
// 		if ( inventory is not null ) {
// 			modified |= CustomComponent.DrawMemberForTarget( ref inventory, nameof( Inventory.Cells ) );
// 			return ( modified, inventory );
// 		}
//
// 		if ( ImGui.Button( "Resize" ) ) {
// 			_newSize = inventory != null ? inventory.Capacity : 5;
// 			ImGui.OpenPopup( "Resize Inventory" );
// 		}
//
// 		if ( ImGui.BeginPopup( "Resize Inventory" ) ) {
//
// 			ImGui.InputInt( "Size", ref _newSize );
// 			if ( ImGui.Button( "Confirm" ) ) {
//
// 				inventory = new Inventory( _newSize );
// 				modified = true;
// 				
// 				ImGui.CloseCurrentPopup();
// 			}
// 			
// 			ImGui.EndPopup();
// 		}
// 		
// 		return ( modified, inventory );
// 	}
// 	
// }


[CustomFieldOf( typeof( ImmutableArray< InventoryEntry > ) )]
internal class ImmutableArrayInventoryEntryField : ImmutableArrayField< InventoryEntry > {
	
	protected override bool Add( in EditorMember member, [NotNullWhen( true )] out InventoryEntry element ) {

		if ( SearchBox.Search( "sInventoryEntry_add", false, "Select a ItemType", ItemTypeServices.ItemTypesLookup, SearchBoxFlags.None, out var newItemType ) ) {
			element = new InventoryEntry { ItemType = newItemType, Count = 0 };
			return true;
		}
		
		element = default;
		return false;
	}

	protected override bool DrawElement( ref InventoryEntry element, EditorMember member, int index ) {
		
		// if ( element != null ) {
		// 	var count = element.Count;
		// 	if ( ImGui.DragInt( "Count", ref count ) ) {
		// 		element.SetCount( count );
		// 		return true;
		// 	}
		// 	ImGui.SameLine();
		// 	var itemType = element.ItemType;
		// 	if ( CustomComponent.DrawMemberForTarget( ref element, nameof( InventoryEntry.ItemType ) ) ) {
		// 		element.ItemType = itemType;
		// 		return true;
		// 	}
		// }
		// else {
			if ( DrawValue( member.CreateFrom( typeof( InventoryEntry ), "Value", element: default ), element, out InventoryEntry modifiedValue ) ) {
				element = modifiedValue;
				return true;
			}
		// }
		
		
		return false;
	}

}


[CustomFieldOf( typeof( ImmutableArray< ReadOnlyInventoryEntry > ) )]
internal class ImmutableArrayReadOnlyInventoryEntryField : ImmutableArrayField< ReadOnlyInventoryEntry > {
	
	protected override bool Add( in EditorMember member, [NotNullWhen( true )] out ReadOnlyInventoryEntry element ) {

		if ( SearchBox.Search( "sInventoryEntry_add", false, "Select a ItemType", ItemTypeServices.ItemTypesLookup, SearchBoxFlags.None, out var newItemType ) ) {
			element = new ReadOnlyInventoryEntry { ItemType = newItemType, Count = 0 };
			return true;
		}
		
		element = default;
		return false;
	}
	
	protected override bool DrawElement( ref ReadOnlyInventoryEntry element, EditorMember member, int index ) {
		
		if ( DrawValue( member.CreateFrom( typeof( ReadOnlyInventoryEntry ), "Value", element: default ), element, out ReadOnlyInventoryEntry modifiedValue ) ) {
			element = modifiedValue;
			return true;
		}
		
		return false;
	}

}


[CustomFieldOf( typeof( InventoryEntry[] ) )]
internal class ArrayInventoryEntryField : ArrayGenericField< InventoryEntry > {

	protected override bool Add( in EditorMember member, out InventoryEntry element ) {
		if ( SearchBox.Search( "sInventoryEntry_add", false, "Select a ItemType", ItemTypeServices.ItemTypesLookup, SearchBoxFlags.None, out var newItemType ) ) {
			element = new InventoryEntry { ItemType = newItemType, Count = 0 };
			return true;
		}
		
		element = default;
		return false;
	}
}


[CustomFieldOf( typeof( ItemType ) )]
public class ItemTypeField : CustomField {
	
	public override (bool modified, object result) ProcessInput( EditorMember member, object fieldValue ) {
		bool modified = false;
		var itemType = fieldValue as ItemType;
		
		if ( SearchBox.Search( "sItemType", itemType != null, itemType != null ? itemType.Id : "Select a ItemType", ItemTypeServices.ItemTypesLookup, SearchBoxFlags.None, out var newItemType ) ) {
			itemType = newItemType;
			return ( true, itemType );
		}
		
		if ( itemType is not null ) {
			// Readonly!
			CustomComponent.DrawMemberForTarget( ref itemType, nameof( ItemType.Id ) );
		}
		
		return ( modified, itemType );
	}
	
}
