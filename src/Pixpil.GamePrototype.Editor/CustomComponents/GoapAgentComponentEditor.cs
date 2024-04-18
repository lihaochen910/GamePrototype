using ImGuiNET;
using Murder;
using Murder.Editor;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Murder.Editor.ImGuiExtended;
using Pixpil.AI;


namespace Pixpil.GamePrototype.Editor.CustomComponents; 

[CustomComponentOf(typeof(GoapAgentComponent))]
public class GoapAgentComponentEditor : CustomComponent {

	protected override bool DrawAllMembersWithTable( ref object target, bool sameLineFilter ) {

		var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
		if ( playingInEditor ) {
#if DEBUG
			var goapAgentComponent = ( GoapAgentComponent )target;
			
			var asset = goapAgentComponent.TryGetScenario();
			if ( asset is null ) {
				return base.DrawAllMembersWithTable( ref target, sameLineFilter );
			}
			
			if ( goapAgentComponent.Planner is null ) {
				return base.DrawAllMembersWithTable( ref target, sameLineFilter );
			}
			
			var debugConditionData = goapAgentComponent.Planner.ConditionsDebugData;

			GoapScenarioAction topAction = default;
			
			ImGui.Separator();
			foreach ( var goal in asset.Goals ) {
				var activedGoal = goapAgentComponent.Goal == goal.Name;
				var goalAchieved = true;
				foreach ( var condKV in goal.Conditions ) {
					if ( !debugConditionData.ContainsKey( condKV.Key ) ) {
						goalAchieved = false;
						break;
					}
					
					var conditionResult = debugConditionData[ condKV.Key ];
					if ( conditionResult != condKV.Value ) {
						goalAchieved = false;
						break;
					}
				}

				var goalPrefix = activedGoal ? "*" : string.Empty;
				
				if ( goalAchieved ) {
					ImGui.TextColored( Game.Profile.Theme.White, $"{goalPrefix}" );
					ImGui.SameLine();
					ImGui.TextColored( Game.Profile.Theme.White, $"{goal.Name}" );
				}
				else {
					ImGui.TextColored( Game.Profile.Theme.White, $"{goalPrefix}" );
					ImGui.SameLine();
					ImGui.TextColored( Game.Profile.Theme.Faded, $"{goal.Name}" );
				}

				if ( activedGoal ) {
					ImGui.SameLine();
					ImGui.TextColored( Game.Profile.Theme.Accent, $" (Updated {Game.NowUnscaled - goapAgentComponent.Planner.PreviousPlanTime:0}s ago)" );
				}

				if ( activedGoal ) {
					ImGui.Indent( 36 );
					if ( goapAgentComponent.Planner.PreviousSelectedActions.Count > 0 ) {
						foreach ( var action in goapAgentComponent.Planner.PreviousSelectedActions ) {
							ImGui.TextColored( Game.Profile.Theme.Green, action.Name );

							topAction ??= action;
						}
					}
					else {
						ImGui.TextColored( Game.Profile.Theme.Red, "no plan." );
					}
					ImGui.Unindent();
				}
			}
			
			ImGui.Indent( 12 );
			// ImGui.Text( "Actions:" );
			// ImGui.Indent( 24 );
			ImGui.Separator();
			foreach ( var action in asset.Actions ) {
				var actionPreAllSuccess = true;
				foreach ( var preKV in action.Pre ) {
					if ( !debugConditionData.ContainsKey( preKV.Key ) ) {
						actionPreAllSuccess = false;
						break;
					}
					
					var conditionResult = debugConditionData[ preKV.Key ];
					if ( conditionResult != preKV.Value ) {
						actionPreAllSuccess = false;
						break;
					}
				}
				
				ImGui.TextColored( Game.Profile.Theme.HighAccent, $"{action.Name}: " );
				ImGui.SameLine();
				if ( actionPreAllSuccess ) {
					ImGui.TextColored( Game.Profile.Theme.Green, "ready!" );
				}
				else {
					ImGui.TextColored( Game.Profile.Theme.Red, "not ready." );
				}

				if ( topAction is not null && topAction == action ) {
					ImGui.SameLine();
					ImGui.TextColored( Game.Profile.Theme.Yellow, " <-" );
				}
				
				ImGui.Indent( 36 );
				
				foreach ( var preKV in action.Pre ) {
					if ( !debugConditionData.ContainsKey( preKV.Key ) ) {
						ImGui.TextColored( Game.Profile.Theme.Faded, $"{preKV.Key}" );
						ImGui.SameLine();
						ImGui.TextColored( Game.Profile.Theme.Red, "missing!" );
						continue;
					}
					
					var conditionResult = debugConditionData[ preKV.Key ];
					var matched = conditionResult == preKV.Value;

					if ( matched ) {
						ImGui.TextColored( Game.Profile.Theme.White, $"{preKV.Key}" );
					}
					else {
						ImGui.TextColored( Game.Profile.Theme.Faded, $"{preKV.Key}" );
					}
					
					// ImGui.TextColored( Game.Profile.Theme.Yellow, $"{preKV.Key}" );
					// ImGui.SameLine();
					// if ( matched ) {
					// 	ImGui.TextColored( Game.Profile.Theme.Green, $"{'\uf00c'}" );
					// }
					// else {
					// 	ImGui.TextColored( Game.Profile.Theme.Red, $"{'\uf00d'}" );
					// }
				}
				
				ImGui.Unindent( 36 );
			}
			// ImGui.Unindent( 24 );

			// ImGui.Text( "Conditions:" );
			// ImGui.Indent( 24 );
			ImGui.Separator();
			foreach ( var condition in asset.Conditions.Keys ) {
				if ( debugConditionData.ContainsKey( condition ) ) {
					ImGui.TextColored( Game.Profile.Theme.Yellow, $"{condition}: " );
					ImGui.SameLine();
					var conditionResult = debugConditionData[ condition ];
					if ( conditionResult ) {
						// ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
						ImGui.TextColored( Game.Profile.Theme.Green, $"{'\uf00c'}" );
						// ImGui.PopStyleColor();
						// ImGuiHelpers.IconButton( '\uf00c', $"GoapPlanner_Condition_{condition}",  );
					}
					else {
						ImGui.TextColored( Game.Profile.Theme.Red, $"{'\uf00d'}" );
						// ImGuiHelpers.IconButton( '\uf00d', $"GoapPlanner_Condition_{condition}", Game.Profile.Theme.Red );
					}
				}
				else {
					ImGui.TextColored( Game.Profile.Theme.Yellow, $"{condition}: " );
					ImGui.SameLine();
					ImGui.TextColored( Game.Profile.Theme.Red, "missing!" );
				}
			}
			// ImGui.Unindent( 24 );
			ImGui.Unindent( 12 );
#endif
			ImGui.Spacing();
			return base.DrawAllMembersWithTable( ref target, sameLineFilter );
		}
		
		return base.DrawAllMembersWithTable( ref target, sameLineFilter );
	}
}
