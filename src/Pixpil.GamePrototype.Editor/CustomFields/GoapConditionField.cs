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
		bool modified = false;
		if ( fieldValue is not null ) {
			modified |= CustomComponent.DrawAllMembers( fieldValue );
			if ( SearchBox.Search( $"sConditionImplTypes_{fieldValue.GetHashCode()}", fieldValue != null, fieldValue != null ? fieldValue.GetType().Name : "Select a type of GoapCondition", ConditionImplTypes, SearchBoxFlags.None, out var conditionImplType ) ) {
				if ( conditionImplType is null ) {
					return ( true, null );
				}
				fieldValue = Activator.CreateInstance( conditionImplType, null ) as GoapCondition;
			}
			return ( modified, fieldValue );
		}
		else {
			if ( SearchBox.Search( $"sConditionImplTypes_{member.Name}", false, "Select a type of GoapCondition", ConditionImplTypes, SearchBoxFlags.None, out var newConditionImplType ) ) {
				if ( newConditionImplType is not null ) {
					fieldValue = Activator.CreateInstance( newConditionImplType, null ) as GoapCondition;
					modified = true;
				}
			}
		}
		
		return ( modified, fieldValue );
	}
	
}
