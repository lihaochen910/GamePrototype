using ImGuiNET;
using Murder;
using Murder.Editor;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Pixpil.Components;


namespace Pixpil.GamePrototype.Editor.CustomComponents;

[CustomComponentOf( typeof( HFSMAgentComponent ) )]
public class HFSMAgentComponentEditor : CustomComponent {

	protected override bool DrawAllMembersWithTable( ref object target ) {

		var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
		if ( playingInEditor ) {
			var hfsmAgentComponent = ( HFSMAgentComponent )target;
			if ( hfsmAgentComponent.StateMachine is not null ) {
				if ( hfsmAgentComponent.StateMachine.ActiveState is not null ) {
					ImGui.Text( "State:" );
					ImGui.SameLine();
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
					ImGui.Text( $"{hfsmAgentComponent.StateMachine.ActiveState.GetActiveHierarchyPath()}" );
					ImGui.PopStyleColor();
				}
				else {
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
					ImGui.Text( "ActiveState is null." );
					ImGui.PopStyleColor();
				}
			}
		}

		return base.DrawAllMembersWithTable( ref target );
	}
	
}
