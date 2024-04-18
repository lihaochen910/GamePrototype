using System;
using System.Collections.Generic;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using DigitalRune.Linq;
using ImGuiNET;
using Murder;
using Murder.Diagnostics;
using Murder.Editor;
using Murder.Editor.CustomFields;
using Murder.Editor.Utilities;
using Murder.Utilities;
using Pixpil.AI;


namespace Pixpil.Editor.CustomFields; 

[CustomFieldOf( typeof( ImmutableArray< IAppraisal > ) )]
internal class ImmutableArrayUtilityAiIAppraisalField : ImmutableArrayField< IAppraisal > {
	
	internal static readonly Lazy< Dictionary< string, Type > > UtilityAiIAppraisalTypes = new(() => {
		var types = ReflectionHelper.GetAllImplementationsOf< IAppraisal >();
		return CollectionHelper.ToStringDictionary( types, a => a.Name, a => a );
	} );
	
	protected override bool Add( in EditorMember member, [NotNullWhen( true )] out IAppraisal element ) {

		if ( SearchBox.Search( "sUtilityAiIAppraisal_", false, "Add UtilityAiIAppraisal", UtilityAiIAppraisalTypes, SearchBoxFlags.None, out var type ) ) {
			try {
				element = Activator.CreateInstance( type, null ) as IAppraisal;
				return true;
			}
			catch ( MissingMethodException _ ) {
				GameLogger.Error( $"can't create instance of {type.Name}, no default ctor." );
			}
		}
		
		element = default;
		return false;
	}

	protected override bool DrawElement( ref IAppraisal? element, EditorMember member, int index ) {
		
		if ( element != null ) {
			// ImGui.SameLine();
			ImGui.TextColored( Game.Profile.Theme.HighAccent, Prettify.FormatName( element.GetType().Name ) );
			// var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
			// if ( playingInEditor ) {
			// 	ImGui.Text( $"ElapsedTime: {element.ElapsedTime:0.00}" );
			// }
			// ImGui.SameLine();
		}
		else {
			ImGui.TextColored( Game.Profile.Theme.Red, $"{'\uf071'} nullptr" );
		}
		
		// 5.Times( _ => { ImGui.Spacing(); ImGui.SameLine(); } );
		// ImGui.SameLine();
		ImGui.Indent( 12 );
		
		if ( DrawValue( member.CreateFrom( typeof( IAppraisal ), "Value", element: default ), element, out IAppraisal? modifiedValue ) ) {
			element = modifiedValue;
			return true;
		}
		
		ImGui.Unindent( 12 );
		
		return false;
	}
}
