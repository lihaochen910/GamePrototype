using System;
using System.Collections.Generic;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomFields;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using Murder.Editor.Utilities;
using Murder.Utilities;
using Pixpil.AI;


namespace Pixpil.Editor.CustomFields; 

[CustomFieldOf( typeof( GoapCondition ) )]
public class GoapConditionField : CustomField {
	
	private static readonly Lazy< Dictionary< string, Type > > ConditionImplTypes = new(() => {
		var conditionImplTypes = ReflectionHelper.GetAllImplementationsOf< GoapCondition >();
		return CollectionHelper.ToStringDictionary( conditionImplTypes, a => a.Name, a => a );
	} );

	public override (bool modified, object result) ProcessInput( EditorMember member, object fieldValue ) {
		SearchBox.SearchBoxSettings< Type > settings = new ( "Select a type of GoapCondition" );
		if ( fieldValue != null ) {
			settings.InitialSelected = new SearchBox.InitialSelectedValue< Type >( fieldValue.GetType().Name, fieldValue.GetType() );
		}
		
		bool modified = false;
		if ( fieldValue is not null ) {
			modified |= CustomComponent.DrawAllMembers( fieldValue );
			if ( SearchBox.Search( $"sConditionImplTypes_{fieldValue.GetHashCode()}", settings, ConditionImplTypes, SearchBoxFlags.None, out var conditionImplType ) ) {
				if ( conditionImplType is null ) {
					return ( true, null );
				}
				fieldValue = Activator.CreateInstance( conditionImplType, null ) as GoapCondition;
			}
			return ( modified, fieldValue );
		}
		else {
			SearchBox.SearchBoxSettings< Type > settings2 = new ( "Select a type of GoapCondition" );
			
			if ( SearchBox.Search( $"sConditionImplTypes_{member.Name}", settings2, ConditionImplTypes, SearchBoxFlags.None, out var newConditionImplType ) ) {
				if ( newConditionImplType is not null ) {
					fieldValue = Activator.CreateInstance( newConditionImplType, null ) as GoapCondition;
					modified = true;
				}
			}
		}
		
		return ( modified, fieldValue );
	}
	
}
