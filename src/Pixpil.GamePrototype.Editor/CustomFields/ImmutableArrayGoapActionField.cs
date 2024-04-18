using System;
using System.Collections.Generic;
using Murder.Core.Geometry;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using ImGuiNET;
using Murder;
using Murder.Editor;
using Murder.Editor.CustomFields;
using Murder.Editor.Utilities;
using Murder.Utilities;
using Pixpil.AI;


namespace Pixpil.Editor.CustomFields;

[CustomFieldOf( typeof( ImmutableArray< GoapAction > ) )]
internal class ImmutableArrayGoapActionField : ImmutableArrayField< GoapAction > {
	
	private Lazy< Dictionary< string, Type > > _actionImplTypes = new( () => {
		var actionImplTypes = ReflectionHelper.GetAllImplementationsOf< GoapAction >();
		return CollectionHelper.ToStringDictionary( actionImplTypes, a => a.Name, a => a );
	} );
	
	protected override bool Add( in EditorMember member, [NotNullWhen( true )] out GoapAction element ) {

		if ( SearchBox.Search( "sGoapAction_", false, "Select a type of GoapAction", _actionImplTypes, SearchBoxFlags.None, out var newActionImplType ) ) {
			element = Activator.CreateInstance( newActionImplType, null ) as GoapAction;
			return true;
		}
		
		element = default;
		return false;
	}

	protected override bool DrawElement( ref GoapAction? element, EditorMember member, int index ) {
		
		if ( element != null ) {
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, element.GetType().Name );
			var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
			if ( playingInEditor ) {
				ImGui.Text( $"ElapsedTime: {element.ElapsedTime:0.00}" );
			}
		}
		
		if ( DrawValue( member.CreateFrom( typeof( GoapAction ), "Value", element: default ), element, out GoapAction? modifiedValue ) ) {
			element = modifiedValue;
			return true;
		}
		
		return false;
	}

}
