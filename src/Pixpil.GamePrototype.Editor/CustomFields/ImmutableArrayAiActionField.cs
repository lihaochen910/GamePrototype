using System;
using System.Collections.Generic;
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

[CustomFieldOf( typeof( ImmutableArray< AiAction > ) )]
internal class ImmutableArrayAiActionField : ImmutableArrayField< AiAction > {
	
	private Lazy< Dictionary< string, Type > > _actionImplTypes = new( () => {
		var actionImplTypes = ReflectionHelper.GetAllImplementationsOf< AiAction >();
		return CollectionHelper.ToStringDictionary( actionImplTypes, a => a.Name, a => a );
	} );
	
	protected override bool Add( in EditorMember member, [NotNullWhen( true )] out AiAction element ) {

		if ( SearchBox.Search( "sAiAction_", false, "Add AiAction", _actionImplTypes, SearchBoxFlags.None, out var newActionImplType ) ) {
			element = Activator.CreateInstance( newActionImplType, null ) as AiAction;
			return true;
		}
		
		element = default;
		return false;
	}

	protected override bool DrawElement( ref AiAction? element, EditorMember member, int index ) {
		
		if ( element != null ) {
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, element.GetType().Name );
			ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.Faded, $" #{element.GetHashCode():x8}" );
			var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
			if ( playingInEditor ) {
				ImGui.Text( $"ElapsedTime: {element.ElapsedTime:0.00}" );
				ImGui.Text( $"PreExecuteResult: {element.PreExecuteResult}" );
				ImGui.Text( $"ExecuteResult: {element.ExecuteResult}" );
			}
		}
		
		if ( DrawValue( member.CreateFrom( typeof( AiAction ), "Value", element: default ), element, out AiAction? modifiedValue ) ) {
			element = modifiedValue;
			return true;
		}
		
		return false;
	}

}
