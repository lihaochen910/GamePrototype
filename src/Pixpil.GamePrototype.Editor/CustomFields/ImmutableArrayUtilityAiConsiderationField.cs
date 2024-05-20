using System;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DigitalRune.Linq;
using ImGuiNET;
using Murder;
using Murder.Editor;
using Murder.Editor.CustomFields;
using Murder.Editor.Utilities;
using Pixpil.AI;


namespace Pixpil.Editor.CustomFields; 

[CustomFieldOf( typeof( ImmutableArray< UtilityAiConsideration > ) )]
internal class ImmutableArrayUtilityAiConsiderationField : ImmutableArrayField< UtilityAiConsideration > {
	
	protected override bool Add( in EditorMember member, [NotNullWhen( true )] out UtilityAiConsideration element ) {
		
		SearchBox.SearchBoxSettings< Type > settings = new ( "Add UtilityAiConsideration" );

		ImGui.Separator();
		if ( SearchBox.Search( "sUtilityAiConsideration_", settings, UtilityAiConsiderationField.UtilityAiConsiderationTypes, SearchBoxFlags.None, out var type ) ) {
			element = Activator.CreateInstance( type, null ) as UtilityAiConsideration;
			return true;
		}
		ImGui.Separator();
		
		element = default;
		return false;
	}

	protected override bool DrawElement( ref UtilityAiConsideration? element, EditorMember member, int index ) {

		bool modified = false;
		
		if ( element != null ) {
			ImGui.SameLine();
			if ( element.Action is null ) {
				ImGui.Text( $"{index}." );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Accent, Prettify.FormatName( element.GetType().Name ) );
				ImGui.SameLine();
				ImGui.Text( " -> " );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Red, "nullptr" );
			}
			else {
				ImGui.Text( $"{index}." );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Accent, Prettify.FormatName( element.GetType().Name ) );
				ImGui.SameLine();
				ImGui.Text( "->" );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Green, element.Action.Name );
			}
			
			ImGui.SameLine();
			2.Times( _ => { ImGui.Spacing(); ImGui.SameLine(); } );
			ImGui.SetNextItemWidth( ImGui.GetContentRegionAvail().X );
			ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Yellow );
			var considerationName = element.Name;
			if ( ImGui.InputTextWithHint( string.Empty, "Add some tips for this consideration!", ref considerationName, 0xFF ) ) {
				element.Name = considerationName;
				modified = true;
			}
			ImGui.PopStyleColor();
			
			// var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
			// if ( playingInEditor ) {
			// 	ImGui.Text( $"ElapsedTime: {element.ElapsedTime:0.00}" );
			// }
		}
		
		ImGui.Indent( 24 );
		
		if ( DrawValue( member.CreateFrom( typeof( UtilityAiConsideration ), "Value", element: default ), element, out UtilityAiConsideration? modifiedValue ) ) {
			element = modifiedValue;
			return true;
		}
		
		ImGui.Unindent( 24 );
		
		5.Times( _ => ImGui.Spacing() );
		
		return modified;
	}
}
