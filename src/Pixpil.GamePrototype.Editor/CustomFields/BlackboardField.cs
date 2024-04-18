/*
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ImGuiNET;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomFields;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using Pixpil.AI;


namespace Pixpil.Editor.CustomFields; 

[CustomFieldOf( typeof( BlackboardSource ) )]
public class BlackboardSourceField : CustomField {
	
	private static readonly Lazy< Dictionary< string, Type > > SupportedVariableTypes = new(() => {
		var dict = new Dictionary< string, Type > {
			{ "boolean", typeof( bool ) },
			{ "int", typeof( int ) },
			{ "float", typeof( float ) },
			{ "string", typeof( string ) },
			{ "Sys::Vector2", typeof( System.Numerics.Vector2 ) },
			{ "Sys::Vector3", typeof( System.Numerics.Vector3 ) },
			{ "Xna::Vector2", typeof( Microsoft.Xna.Framework.Vector2 ) },
			{ "Xna::Vector3", typeof( Microsoft.Xna.Framework.Vector3 ) },
			{ "Xna::Rectangle", typeof( Microsoft.Xna.Framework.Rectangle ) },
			{ "Xna::Color", typeof( Microsoft.Xna.Framework.Color ) },
			{ "Murder::Point", typeof( Murder.Core.Geometry.Point ) },
			{ "Murder::Rectangle", typeof( Murder.Core.Geometry.Rectangle ) },
			{ "Murder::Color", typeof( Murder.Core.Graphics.Color ) },
			// { "Guid", typeof( Guid ) },
		};
		return dict;
	} );

	private string _newVariableName = string.Empty;
	private Type _newVariableType;
	public override (bool modified, object? result) ProcessInput( EditorMember member, object fieldValue ) {
		bool modified = false;
		BlackboardSource blackboard = ( BlackboardSource )fieldValue!;
		if ( blackboard is null ) {
			if ( ImGui.Button( "Create" ) ) {
				blackboard = new ();
				modified = true;
			}
		}
		else {
			if ( CustomComponent.DrawAllMembers( blackboard ) ) {
				modified = true;
			}
			
			if ( ImGui.Button( "Add Variable" ) ) {
				_newVariableName = "New Variable";
				ImGui.OpenPopup( "Add Variable##BlackboardSource" );
			}

			if ( ImGui.BeginPopup( "Add Variable##BlackboardSource", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoNav ) ) {
			
				ImGui.InputTextWithHint( "Variable Name", "Key", ref _newVariableName, 0xFF );
				ImGui.Text( $"Variable Type: {_newVariableType}" );
				
				// ImGui.PushID( "Search Variable Type##BlackboardSource" );

				SearchBox.PushItemWidth( 350 );
				// if ( SearchBox.Search( "s_VariableT", _newVariableType != null, _newVariableType != null ? _newVariableType.Name : "Select a Variable Type##BlackboardSource", SupportedVariableTypes, SearchBoxFlags.None, out Type? newVariableType ) ) {
				if ( SearchBox.Search( "s_VariableT", false, "Select a Variable Type##BlackboardSource", SupportedVariableTypes, SearchBoxFlags.None, out Type? newVariableType ) ) {
					_newVariableType = newVariableType;
				}
				SearchBox.PopItemWidth();
				
				// ImGui.PopID();

				if ( !string.IsNullOrEmpty( _newVariableName ) && _newVariableType != null ) {
					var hasDuplicate = blackboard.GetVariable( _newVariableName ) != null;
					if ( ImGui.Button( !hasDuplicate ? "Add" : "Replace" ) ) {

						if ( hasDuplicate ) {
							blackboard.RemoveVariable( _newVariableName );
						}

						blackboard.AddVariable( _newVariableName, _newVariableType );
						modified = true;
						
						ImGui.CloseCurrentPopup();
					}
					ImGui.SameLine();
				}

				if ( ImGui.Button( "Cancel" ) ) {
					_newVariableName = string.Empty;
					_newVariableType = null;
					ImGui.CloseCurrentPopup();
				}
				
				ImGui.EndPopup();
			}
		}

		return ( modified, blackboard );
	}
	
}


[CustomFieldOf( typeof( Variable<> ) )]
public class BlackboardVariableField< T > : CustomField {

	protected virtual bool Create( in EditorMember member, [NotNullWhen( true )] out T? element ) {
		element = default;
		return false;
	}

	
	public override (bool modified, object? result) ProcessInput( EditorMember member, object? fieldValue ) {
		bool modified = false;
		Variable< T > variable = ( Variable< T > )fieldValue!;

		// ImGui.PushID( $"Add ${member.Member.ReflectedType}" );
		//
		// if ( Create( member, out T? element ) ) {
		// 	variable ??= new Variable< T >();
		// 	variable.SetValue( element );
		// 	modified = true;
		// }
		//
		// ImGui.PopID();
		
		if ( DrawVariable( ref variable ) ) {
			modified = true;
		}

		if ( modified ) {
			return ( modified, variable );
		}
		
		return ( modified, variable );
	}


	protected virtual bool DrawVariable( ref Variable< T >? variable ) {
		
		if ( DrawValue( ref variable, nameof( Variable< T >.Value ) ) ) {
			return true;
		}

		return false;
	}

}
*/