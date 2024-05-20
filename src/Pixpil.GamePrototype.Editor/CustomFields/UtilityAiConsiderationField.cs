using System;
using System.Collections.Generic;
using System.Reflection;
using DigitalRune.Linq;
using DigitalRune.Mathematics;
using ImGuiNET;
using Murder;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomFields;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using Murder.Editor.Utilities;
using Murder.Utilities;
using Pixpil.AI;


namespace Pixpil.Editor.CustomFields; 

[CustomFieldOf( typeof( UtilityAiConsideration ) )]
public class UtilityAiConsiderationField : CustomField {
	
	internal static readonly Lazy< Dictionary< string, Type > > UtilityAiConsiderationTypes = new(() => {
		var types = ReflectionHelper.GetAllImplementationsOf< UtilityAiConsideration >();
		return CollectionHelper.ToStringDictionary( types, a => a.Name, a => a );
	} );
	
	
	public override (bool modified, object result) ProcessInput( EditorMember member, object fieldValue ) {
		bool modified = false;
		if ( fieldValue is not null ) {
			
			ImGui.Indent( 24 );
			UtilityAiConsideration consideration = ( UtilityAiConsideration )fieldValue;
			
			2.Times( _ => ImGui.Spacing() );
			ImGui.Indent( 12 );
			ImGui.Separator();
			ImGui.Spacing();
			ImGui.TextColored( Game.Profile.Theme.Faded, $"{'\uf0eb'} {consideration.Description}" );
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			DrawConsiderationPredictedScore( consideration );
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Unindent( 12 );
			
			3.Times( _ => ImGui.Spacing() );

			bool hasValidAction = consideration.Action != null;
			if ( hasValidAction ) {
				ImGui.SeparatorText( $"{'\uf6e2'} Action" );
			}
			else {
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Red );
				ImGui.SeparatorText( $"{'\uf071'} Assign Action For The Consideration First!" );
				ImGui.PopStyleColor();
			}
            // ImGui.Text( "Action:" );
			if ( consideration.FuncGetOwnerUtilityAiAsset is not null ) {
				var utilityAiAsset = consideration.FuncGetOwnerUtilityAiAsset.Invoke();
				SearchBox.SearchBoxSettings< UtilityAiAction > settings = new ( "Select a Variable Type##BlackboardSource" );
				if ( consideration.Action != null ) {
					settings.InitialSelected = new SearchBox.InitialSelectedValue< UtilityAiAction >( consideration.Action.Name, consideration.Action );
				}
				Lazy< Dictionary< string, UtilityAiAction > > candidateActionDefine = new(() => {
					return CollectionHelper.ToStringDictionary( utilityAiAsset.Actions, a => a.Name, a => a );
				} );
				
				// ImGui.SameLine();
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
				if ( SearchBox.Search( "sUtilityAiConsideration_Action", settings, candidateActionDefine, SearchBoxFlags.None, out var action ) ) {
					consideration.Action = action;
					modified = true;
				}
				ImGui.PopStyleColor();
			}
			else {
				// ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Red, "nullptr" );
			}

			if ( hasValidAction ) {
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
				ImGui.SeparatorText( $"{'\uf00c'}" );
				ImGui.PopStyleColor();
			}
			else {
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Red );
				ImGui.SeparatorText( $"{'\uf00d'}" );
				ImGui.PopStyleColor();
			}
			2.Times( _ => ImGui.Spacing() );
			ImGui.Unindent( 24 );
			
			modified |= CustomComponent.DrawAllMembers( fieldValue, [ nameof( UtilityAiConsideration.Name ), nameof( UtilityAiConsideration.Action ) ] );
			
			var ( replaced, newConsideration ) = DrawReplaceUtilityAiConsiderationSearchBox( consideration );
			if ( replaced ) {
				return ( replaced, newConsideration );
			}
			return ( modified, consideration );
		}
		else {
			SearchBox.SearchBoxSettings< Type > settings = new ( "Select a type of UtilityAiConsideration" );
			if ( SearchBox.Search( $"sUtilityAiConsiderationTypes_{member.Name}", settings, UtilityAiConsiderationTypes, SearchBoxFlags.None, out var type ) ) {
				if ( type is not null ) {
					fieldValue = Activator.CreateInstance( type, null ) as UtilityAiConsideration;
					modified = true;
				}
			}
		}
		
		return ( modified, fieldValue );
	}


	public static void DrawConsiderationPredictedScore( UtilityAiConsideration consideration ) {
		if ( consideration is not null ) {
			( var predictedScoreMin, var predictedScoreMax ) = consideration.GetPreictedScores();
			if ( !Numeric.AreEqual( predictedScoreMin, predictedScoreMax ) ) {
				if ( predictedScoreMin < predictedScoreMax ) {
					ImGui.TextColored( Game.Profile.Theme.Yellow, $"{'\uf0eb'} Predicted Value Range: {predictedScoreMin:0.00} ~ {predictedScoreMax:0.00}" );
				}
				else {
					ImGui.TextColored( Game.Profile.Theme.Yellow, $"{'\uf0eb'} Predicted Value Range: {predictedScoreMax:0.00} ~ {predictedScoreMin:0.00}" );
				}
			}
			else {
				ImGui.TextColored( Game.Profile.Theme.Yellow, $"{'\uf0eb'} Predicted Value: {predictedScoreMin:0.00}" );
			}
		}
	}


	public static (bool modified, UtilityAiConsideration result) DrawReplaceUtilityAiConsiderationSearchBox( UtilityAiConsideration consideration ) {
		SearchBox.SearchBoxSettings< Type > settings = new ( "Replace with UtilityAiConsideration" );
		if ( SearchBox.Search( $"sUtilityAiConsiderationTypes_{consideration.GetHashCode()}", settings, UtilityAiConsiderationTypes, SearchBoxFlags.None, out var type ) ) {
			if ( type is null ) {
				return ( true, null );
			}

			if ( type == consideration.GetType() ) {
				return ( false, consideration );
			}
			
			var oldConsideration = consideration;
			var func = consideration.FuncGetOwnerUtilityAiAsset;
			consideration = Activator.CreateInstance( type, null ) as UtilityAiConsideration;
			consideration.Name = oldConsideration.Name;
			consideration.Action = oldConsideration.Action;
			consideration.FuncGetOwnerUtilityAiAsset = func;
				
			// try copy old field to new.
			var thresholdFieldOld = oldConsideration.GetType().GetField( "Threshold", BindingFlags.Public | BindingFlags.Instance );
			var thresholdField = consideration.GetType().GetField( "Threshold", BindingFlags.Public | BindingFlags.Instance );
			if ( thresholdFieldOld != null && thresholdField != null && thresholdFieldOld.FieldType.IsAssignableTo( thresholdField.FieldType ) ) {
				thresholdField.SetValue( consideration, thresholdFieldOld.GetValue( oldConsideration ) );
			}
				
			var appraisalsFieldOld = oldConsideration.GetType().GetField( "Appraisals", BindingFlags.Public | BindingFlags.Instance );
			var appraisalsField = consideration.GetType().GetField( "Appraisals", BindingFlags.Public | BindingFlags.Instance );
			if ( appraisalsFieldOld != null && appraisalsField != null && appraisalsFieldOld.FieldType.IsAssignableTo( appraisalsField.FieldType ) ) {
				appraisalsField.SetValue( consideration, appraisalsFieldOld.GetValue( oldConsideration ) );
			}

			return ( true, consideration );
		}

		return ( false, consideration );
	}
	
}
