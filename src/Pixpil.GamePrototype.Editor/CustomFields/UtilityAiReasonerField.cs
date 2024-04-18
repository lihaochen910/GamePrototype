using System.Reflection;
using DigitalRune.Linq;
using ImGuiNET;
using Murder;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomFields;
using Murder.Editor.Reflection;
using Pixpil.AI;


namespace Pixpil.Editor.CustomFields; 

[CustomFieldOf( typeof( UtilityAiReasoner ) )]
public class UtilityAiReasonerField : CustomField {
	
	private readonly EditorMember _defaultConsiderationMember;
	private readonly EditorMember _considerationsMember;


	public UtilityAiReasonerField() {
		_defaultConsiderationMember = EditorMember.Create( typeof( UtilityAiReasoner ).GetProperty( nameof( UtilityAiReasoner.DefaultConsideration ), BindingFlags.Public | BindingFlags.Instance ) );
		_considerationsMember = EditorMember.Create( typeof( UtilityAiReasoner ).GetField( nameof( UtilityAiReasoner.Considerations ), BindingFlags.Public | BindingFlags.Instance ) );
	}
	
	public override (bool modified, object result) ProcessInput( EditorMember member, object fieldValue ) {
		bool modified = false;
		if ( fieldValue is not null ) {
			// modified |= CustomComponent.DrawAllMembers( fieldValue );
			
			var reasoner = ( UtilityAiReasoner )fieldValue;
			
			3.Times( _ => ImGui.Spacing() );
			ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Warning );
			ImGui.SeparatorText( $"{'\uf54c'}" );
			ImGui.PopStyleColor();
			modified |= CustomComponent.DrawMemberForTarget( ref reasoner, nameof( UtilityAiReasoner.DefaultConsideration ) );
			if ( reasoner.DefaultConsideration != null && ImGui.Button( "Clear" ) ) {
				reasoner.DefaultConsideration = null;
				modified = true;
			}
			ImGui.Separator();
			2.Times( _ => ImGui.Spacing() );
			
			ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Warning );
			ImGui.SeparatorText( $"{'\uf0cb'}" );
			ImGui.PopStyleColor();
			modified |= CustomComponent.DrawMemberForTarget( ref reasoner, nameof( UtilityAiReasoner.Considerations ) );
			ImGui.Separator();
			
			// if ( SearchBox.Search( $"sConditionImplTypes_{fieldValue.GetHashCode()}", fieldValue != null, fieldValue != null ? fieldValue.GetType().Name : "Select a type of GoapCondition", ConditionImplTypes, SearchBoxFlags.None, out var conditionImplType ) ) {
			// 	if ( conditionImplType is null ) {
			// 		return ( true, null );
			// 	}
			// 	fieldValue = Activator.CreateInstance( conditionImplType, null ) as GoapCondition;
			// }
			return ( modified, fieldValue );
		}
		// else {
		// 	if ( SearchBox.Search( $"sConditionImplTypes_{member.Name}", false, "Select a type of GoapCondition", ConditionImplTypes, SearchBoxFlags.None, out var newConditionImplType ) ) {
		// 		if ( newConditionImplType is not null ) {
		// 			fieldValue = Activator.CreateInstance( newConditionImplType, null ) as GoapCondition;
		// 			modified = true;
		// 		}
		// 	}
		// }
		
		return ( modified, fieldValue );
	}
	
}
