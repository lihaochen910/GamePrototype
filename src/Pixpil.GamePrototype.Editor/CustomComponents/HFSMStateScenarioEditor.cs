using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Murder;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Utilities;
using Murder.Utilities;
using Pixpil.AI.HFSM;


namespace Pixpil.Editor.CustomComponents;

[CustomComponentOf(typeof(HFSMStateScenario))]
public class HFSMStateScenarioEditor : CustomComponent {
	
	private static readonly Lazy< Dictionary< string, Type > > ImplTypes = new(() => {
		var conditionImplTypes = ReflectionHelper.GetAllImplementationsOf< HFSMStateAction >();
		return CollectionHelper.ToStringDictionary( conditionImplTypes, a => a.Name, a => a );
	} );
	
	protected override bool DrawAllMembersWithTable( ref object target, bool sameLineFilter ) {
		var stateScenario = target as HFSMStateScenario;
		if ( stateScenario is null ) {
			return false;
		}
		
		bool modified = false;
		if ( target is not HFSMStateMachineScenario ) {
			ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Accent );
			ImGui.Text( $"State: {stateScenario.Name}" );
			ImGui.PopStyleColor();
		}
		
		ImGui.PushID( "HFSMStateScenario_Name" );
		var stateName = stateScenario.Name ?? "";
		if ( ImGui.InputText( string.Empty, ref stateName, 64, ImGuiInputTextFlags.AutoSelectAll ) ) {
			stateScenario.SetStateName( stateName );
			modified = true;
		}
		ImGui.PopID();

		ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.HighAccent );
		bool isGhostState = stateScenario.IsGhostState;
		if ( ImGui.Checkbox( nameof( HFSMStateScenario.IsGhostState ), ref isGhostState ) ) {
			stateScenario.SetGhostState( isGhostState );
		}
		ImGui.PopStyleColor();
		ImGuiHelpers.HelpTooltip( "it will instantly try all of its outbound transitions. If any one succeeds, it will instantly transition to the next state." );
		
		ImGui.Separator();
		ImGui.Dummy( new Vector2( -1f, 3f ) );

		// if ( target is HFSMStateMachineScenario ) {
		// 	ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Warning );
		// 	ImGui.TextWrapped( "Currently,\nadding actions to sub-state machine directly is not supported.\nBut you can add StateAction in sub-state." );
		// 	ImGui.PopStyleColor();
		// 	
		// 	ImGui.Separator();
		// 	ImGui.TextWrapped( "Double-click node to enter the sub-state machine." );
		// 	return modified;
		// }
		
		if ( stateScenario.Impl.IsDefaultOrEmpty ) {
			ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
			ImGui.Text( "no HFSMStateAction defined." );
			ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.HighAccent );
			ImGui.Text( "click 'Add' to add StateAction." );
			ImGui.PopStyleColor();
			ImGui.PopStyleColor();
		}
		else {
			for ( var i = 0; i < stateScenario.Impl.Length; i++ ) {
				var stateAction = stateScenario.Impl[ i ];
				ImGui.PushID( $"#{i}_{stateAction.GetType().Name}" );
				var headerOpened = ImGui.CollapsingHeader( string.Empty,
					ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowOverlap );
				if ( ImGui.IsItemHovered() ) {
					ImGui.OpenPopupOnItemClick( $"{i}_{stateAction.GetType().Name}_context", ImGuiPopupFlags.MouseButtonRight );
				}
				if ( headerOpened ) {
					ImGui.SameLine();
					ImGui.Checkbox( "##Checkbox", ref stateAction.IsActived );
					ImGui.SameLine();
					ImGui.Text( $"{stateAction.GetType().Name}" );
					
					modified |= CustomComponent.ShowEditorOf( ref stateAction, CustomComponentsFlags.SkipSameLineForFilterField );
				}
				else {
					ImGui.SameLine();
					ImGui.Checkbox( "##Checkbox", ref stateAction.IsActived );
					ImGui.SameLine();
					ImGui.Text( $"{stateAction.GetType().Name}" );
				}
                ImGui.Dummy( new Vector2( -1f, 6f ) );
				
				if ( ImGui.BeginPopup( $"{i}_{stateAction.GetType().Name}_context" ) ) {
					if ( i != 0 ) {
						if ( ImGui.MenuItem( "move up" ) ) {
							stateScenario.Impl = stateScenario.Impl.RemoveAt( i ).Insert( i - 1, stateAction );
                        	ImGui.CloseCurrentPopup();
                        }
					}
					if ( i != stateScenario.Impl.Length - 1 ) {
						if ( ImGui.MenuItem( "move down" ) ) {
							stateScenario.Impl = stateScenario.Impl.RemoveAt( i ).Insert( i + 1, stateAction );
							ImGui.CloseCurrentPopup();
						}
					}
					if ( ImGui.MenuItem( "remove" ) ) {
						stateScenario.RemoveStateAction( stateAction );
						ImGui.CloseCurrentPopup();
					}
					ImGui.EndPopup();
				}
				
				ImGui.PopID();
			}
		}
		
		ImGui.Separator();
		
		SearchBox.SearchBoxSettings< Type > settings = new ( initialText: "Add StateAction" );
		
		if ( SearchBox.Search( "sHFSMStateAction_", settings, ImplTypes, SearchBoxFlags.None, out var newConditionImplType ) ) {
			var newStateAction = Activator.CreateInstance( newConditionImplType, null ) as HFSMStateAction;
			stateScenario.AddStateAction( newStateAction );
			modified = true;
		}
		
		return modified;
	}
	
}


[CustomComponentOf(typeof(HFSMStateMachineScenario))]
public class HFSMStateMachineScenarioEditor : HFSMStateScenarioEditor {
	
	protected override bool DrawAllMembersWithTable( ref object target, bool sameLineFilter ) {
		var stateMachineScenario = target as HFSMStateMachineScenario;
		if ( stateMachineScenario is null ) {
			return false;
		}
		
		bool modified = false;
		
		ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Accent );
		ImGui.Text( $"StateMachine: {stateMachineScenario.Name}" );
		ImGui.PopStyleColor();
		
		// ImGui.PushID( "HFSMStateMachineScenario_Name" );
		// var stateMachineName = stateMachineScenario.Name ?? "";
		// if ( ImGui.InputText( string.Empty, ref stateMachineName, 64, ImGuiInputTextFlags.AutoSelectAll ) ) {
		// 	stateMachineScenario.SetStateMachineName( stateMachineName );
		// 	modified = true;
		// }
		// ImGui.PopID();
		
		ImGui.Separator();
		ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
		ImGui.Text( $"StateCount: {stateMachineScenario.States.Length}" );
		ImGui.Text( $"Nested Sub-Fsm Count: {stateMachineScenario.ChildrenStateMachine.Length}" );
		ImGui.PopStyleColor();
		
		ImGui.Separator();
        
		return base.DrawAllMembersWithTable( ref target, sameLineFilter );
	}

}
