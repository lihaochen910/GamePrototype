using System;
using System.Collections.Generic;
using Murder.Core.Geometry;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using ImGuiNET;
using Murder;
using Murder.Editor.CustomFields;
using Murder.Editor.Utilities;
using Murder.Utilities;
using Pixpil.AI;


namespace Pixpil.Editor.CustomFields;

[CustomFieldOf( typeof( ImmutableArray< GoapCondition > ) )]
internal class ImmutableArrayGoapConditionField : ImmutableArrayField< GoapCondition > {
	
	private Lazy< Dictionary< string, Type > > _conditionImplTypes = new( () => {
		var conditionImplTypes = ReflectionHelper.GetAllImplementationsOf< GoapCondition >();
		return CollectionHelper.ToStringDictionary( conditionImplTypes, a => a.Name, a => a );
	} );
	
	protected override bool Add( in EditorMember member, [NotNullWhen( true )] out GoapCondition element ) {

		if ( SearchBox.Search( "sGoapCondition_", false, "Select a type of GoapCondition", _conditionImplTypes, SearchBoxFlags.None, out var newConditionImplType ) ) {
			element = Activator.CreateInstance( newConditionImplType, null ) as GoapCondition;
			return true;
		}
		
		element = default;
		// if ( ImGuiHelpers.IconButton( '', $"{member.Name}_add", Game.Data.GameProfile.Theme.Accent ) ) {
		// 	return true;
		// }

		return false;
	}

	protected override bool DrawElement( ref GoapCondition? element, EditorMember member, int index ) {
		
		if ( element != null ) {
			ImGui.SameLine();
			ImGui.Text( element.GetType().Name );
		}
		
		if ( DrawValue( member.CreateFrom( typeof( GoapCondition ), "Value", element: default ), element, out GoapCondition? modifiedValue ) ) {
			element = modifiedValue;
			return true;
		}
		
		return false;
	}

}
