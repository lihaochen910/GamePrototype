using System.Collections.Generic;
using Murder.Editor.CustomFields;
using Murder.Editor.Reflection;
using Pixpil.AI;


namespace Pixpil.Editor.CustomFields; 

[CustomFieldOf( typeof( Stack< GoapScenarioAction > ) )]
public class StackGoapScenarioActionField : StackField< GoapScenarioAction > {
	
	protected override bool Push( in EditorMember member, out GoapScenarioAction element ) {
		element = default;
		return false;
	}
	
}
