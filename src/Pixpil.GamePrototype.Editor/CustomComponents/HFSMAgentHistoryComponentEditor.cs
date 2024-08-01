using ImGuiNET;
using Murder;
using Murder.Editor;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Murder.Editor.ImGuiExtended;
using Pixpil.Components;


namespace Pixpil.GamePrototype.Editor.CustomComponents;

[CustomComponentOf( typeof( HFSMAgentHistoryComponent ) )]
public class HFSMAgentHistoryComponentEditor : CustomComponent {

	protected override bool DrawAllMembersWithTable( ref object target, bool sameLineFilter ) {

		var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
		if ( playingInEditor ) {
			var hfsmAgentHistoryComponent = ( HFSMAgentHistoryComponent )target;
			if ( hfsmAgentHistoryComponent.Deque is null ) {
				return base.DrawAllMembersWithTable( ref target, sameLineFilter );
			}

			if ( ImGui.BeginTable( "HFSMScenarioAsset Table", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingFixedFit ) ) {

				ImGui.TableSetupColumn( "transition_detail", ImGuiTableColumnFlags.WidthStretch, -1, 0 );
				ImGui.TableSetupColumn( "frame", ImGuiTableColumnFlags.WidthFixed, 50, 1 );
				ImGui.TableHeadersRow();
				
				foreach ( var entry in hfsmAgentHistoryComponent.Deque ) {
					ImGui.TableNextRow();
					ImGui.TableNextColumn();
					
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Green );
					ImGui.Text( $"{entry.To.Name}" );
					ImGui.PopStyleColor();
					
					if ( ImGui.IsItemHovered() ) {
						if ( !string.IsNullOrEmpty( entry.StackTrace ) ) {
							ImGuiHelpers.HelpTooltip( $"{entry.StackTrace}" );
						}
					}
					
					ImGui.SameLine();
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Warning );
					ImGui.Text( " <- " );
					ImGui.PopStyleColor();

					ImGui.SameLine();
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
					ImGui.Text( $"{entry.From.Name}" );
					ImGui.PopStyleColor();

					ImGui.TableNextColumn();
					ImGui.Text( $"{entry.Time:0.00}" );
				}
				
				ImGui.EndTable();
			}
			
			return false;
		}

		return base.DrawAllMembersWithTable( ref target, sameLineFilter );
	}
	
}
