using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using ImGuiNET;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomFields;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using Pixpil.AI;


namespace Pixpil.GamePrototype.Editor.CustomComponents; 

[CustomComponentOf(typeof(BlackboardComponent))]
public class BlackboardComponentEditor : CustomComponent {
	
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

	private EditorMember _variablesMember;
	private string _newVariableName = string.Empty;
	private Type _newVariableType;

	public BlackboardComponentEditor() {
		_variablesMember = EditorMember.Create( typeof( BlackboardComponent ).GetField( nameof( BlackboardComponent.Variables ), BindingFlags.Public | BindingFlags.Instance ) );
	}

	protected override bool DrawAllMembersWithTable( ref object target ) {
		var blackboardComponent = ( BlackboardComponent )target;
		bool changed = false;

		if ( blackboardComponent.Variables is null ) {
			target = blackboardComponent = new BlackboardComponent( ImmutableDictionary< string, Variable >.Empty );
			changed = true;
		}

		ImGui.PushID( "BlackboardComponent_Variables" );
		( changed, object? boxedResult ) = CustomField.DrawValue( _variablesMember, _variablesMember.GetValue( target ) );
		if ( changed ) {
			target = new BlackboardComponent( ( ImmutableDictionary< string, Variable > )boxedResult );
			return true;
		}
		ImGui.PopID();
		// CustomField.DrawValue( ref blackboardComponent, nameof( BlackboardComponent.Variables ) )
		// if ( CustomComponent.DrawMemberForTarget( ref blackboardComponent, nameof( BlackboardComponent.Variables ) ) ) {
		// 	target = blackboardComponent;
		// 	return true;
		// }
		
		if ( ImGui.Button( "Add Variable" ) ) {
			_newVariableName = "New Variable";
			ImGui.OpenPopup( "Add Variable##BlackboardSource" );
		}

		if ( ImGui.BeginPopup( "Add Variable##BlackboardSource", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoNav ) ) {
			
			ImGui.InputTextWithHint( "Variable Name", "Key", ref _newVariableName, 0xFF );
			ImGui.Text( $"Variable Type: {_newVariableType}" );
				
			// ImGui.PushID( "Search Variable Type##BlackboardSource" );

			SearchBox.PushItemWidth( 350 );
			SearchBox.SearchBoxSettings< Type > settings = new ( "Select a Variable Type##BlackboardSource" );
			// if ( SearchBox.Search( "s_VariableT", _newVariableType != null, _newVariableType != null ? _newVariableType.Name : "Select a Variable Type##BlackboardSource", SupportedVariableTypes, SearchBoxFlags.None, out Type? newVariableType ) ) {
			if ( SearchBox.Search( "s_VariableT", settings, SupportedVariableTypes, SearchBoxFlags.None, out Type? newVariableType ) ) {
				_newVariableType = newVariableType;
			}
			SearchBox.PopItemWidth();
				
			// ImGui.PopID();

			if ( !string.IsNullOrEmpty( _newVariableName ) && _newVariableType != null ) {
				var hasDuplicate = blackboardComponent.HasVariable( _newVariableName );
				if ( ImGui.Button( !hasDuplicate ? "Add" : "Replace" ) ) {

					if ( hasDuplicate ) {
						target = blackboardComponent = blackboardComponent.RemoveVariable( _newVariableName );
					}

					target = blackboardComponent = blackboardComponent.AddVariable( _newVariableName, _newVariableType, _newVariableType.IsValueType ? Activator.CreateInstance( _newVariableType ) : null );
					changed = true;
						
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
		
		return changed;	
	}
	
}


// [CustomFieldOf( typeof( Variable ) )]
// public class BlackboardVariableField : CustomField {
//
// 	protected virtual bool Create( in EditorMember member, [NotNullWhen( true )] out Variable? element ) {
// 		element = default;
// 		return false;
// 	}
//
// 	
// 	public override (bool modified, object? result) ProcessInput( EditorMember member, object? fieldValue ) {
// 		bool modified = false;
// 		// Variable variable = ( Variable )fieldValue!;
//
// 		// ImGui.PushID( $"Add ${member.Member.ReflectedType}" );
// 		//
// 		// if ( Create( member, out T? element ) ) {
// 		// 	variable ??= new Variable< T >();
// 		// 	variable.SetValue( element );
// 		// 	modified = true;
// 		// }
// 		//
// 		// ImGui.PopID();
//
// 		// var variableValue = variable.Value;
// 		// if ( CustomComponent.ShowEditorOf( ref variableValue ) ) {
// 		// 	return ( true, variableValue );
// 		// }
// 		modified = DrawValue( ref fieldValue, nameof( Variable.Value ) );
// 		return ( modified, fieldValue );
// 	}
//
//
// 	// protected virtual bool DrawVariable( EditorMember member, ref Variable variable ) {
// 	// 	return false;
// 	// }
//
// }
