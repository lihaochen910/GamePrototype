using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomFields;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using Murder.Editor.Utilities;
using Murder.Utilities;
using Pixpil.AI;
using Pixpil.RPGStatSystem;


namespace Pixpil.Editor.CustomFields; 

[CustomFieldOf( typeof( RPGStat ) )]
internal class RPGStatField : CustomField {
    
	public override (bool modified, object? result) ProcessInput( EditorMember member, object? fieldValue ) {
		bool modified = false;
		RPGStat stat = ( RPGStat )fieldValue;

		if ( stat is not null ) {
			
			var statBaseValue = stat.StatBaseValue;
			ImGui.Text( "StatBaseValue:" );
			ImGui.SameLine();
			if ( ImGui.DragFloat( nameof( RPGStat.StatBaseValue ), ref statBaseValue, 0.1f ) ) {
				stat.StatBaseValue = statBaseValue;
				modified = true;
			}
			
			ImGui.Text( $"StatValue: {stat.StatValue}" );
			ImGui.Text( $"StatScaleValue: {stat.StatScaleValue}" );
		}
		else {
			if ( ImGui.Button( "Create" ) ) {
				stat = ( RPGStat )Activator.CreateInstance( member.Type, null );
				modified = true;
			}
		}
		
		return ( modified, stat );
	}
	
	
	public static void DisableNextWidget( float widgetCustomHeight = 0 ) {
		var origCursorPos = ImGui.GetCursorPos();
		var widgetSize = new Vector2( ImGui.GetContentRegionAvail().X,
			widgetCustomHeight > 0 ? widgetCustomHeight : GetDefaultWidgetHeight() );
		ImGui.InvisibleButton( "##disabled", widgetSize );
		ImGui.SetCursorPos( origCursorPos );
	}
	
	
	public static float GetDefaultWidgetHeight() => ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2f;
	
}


[CustomFieldOf( typeof( RPGStatModifier ) )]
internal class RPGStatModifierField : CustomField {
	
	private Lazy< Dictionary< string, Type > > _modifierImplTypes = new( () => {
		var actionImplTypes = ReflectionHelper.GetAllImplementationsOf< RPGStatModifier >();
		return CollectionHelper.ToStringDictionary( actionImplTypes, a => a.Name, a => a );
	} );

	public override (bool modified, object result) ProcessInput( EditorMember member, object fieldValue ) {
		SearchBox.SearchBoxSettings< Type > settings = new ( "Select a RPGStatModifier" );
		if ( fieldValue != null ) {
			settings.InitialSelected = new SearchBox.InitialSelectedValue< Type >( fieldValue.GetType().Name, fieldValue.GetType() );
		}
		
		bool modified = false;
		if ( fieldValue is not null ) {
			modified |= CustomComponent.DrawAllMembers( fieldValue );
			if ( SearchBox.Search( $"sRPGStatModifierTypes_{fieldValue.GetHashCode()}", settings, _modifierImplTypes, SearchBoxFlags.None, out var modifierImplType ) ) {
				if ( modifierImplType is null ) {
					return ( true, null );
				}
				fieldValue = Activator.CreateInstance( modifierImplType, new object[] { 0f, true } ) as RPGStatModifier;
			}
			return ( modified, fieldValue );
		}
		else {
			if ( SearchBox.Search( $"sConditionImplTypes_{member.Name}", settings, _modifierImplTypes, SearchBoxFlags.None, out var newConditionImplType ) ) {
				if ( newConditionImplType is not null ) {
					fieldValue = Activator.CreateInstance( newConditionImplType, new object[] { 0f, true } ) as RPGStatModifier;
					modified = true;
				}
			}
		}
		
		return ( modified, fieldValue );
	}
}
