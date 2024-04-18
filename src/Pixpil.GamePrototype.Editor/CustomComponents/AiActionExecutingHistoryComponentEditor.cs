using System.Numerics;
using ImGuiNET;
using Murder;
using Murder.Editor;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Pixpil.AI;


namespace Pixpil.GamePrototype.Editor.CustomComponents;

[CustomComponentOf( typeof( AiActionExecutingHistoryComponent ) )]
public class AiActionExecutingHistoryComponentEditor : CustomComponent {

	protected override bool DrawAllMembersWithTable( ref object target, bool sameLineFilter ) {

		var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
		if ( playingInEditor ) {
			var aiActionExecutingHistoryComponent = ( AiActionExecutingHistoryComponent )target;
			if ( aiActionExecutingHistoryComponent.Deque is null ) {
				return base.DrawAllMembersWithTable( ref target, sameLineFilter );
			}

			foreach ( var item in aiActionExecutingHistoryComponent.Deque ) {
				ImGui.TextColored( Game.Profile.Theme.HighAccent, $"{item.Action.Name}: " );
				ImGui.SameLine();
				
				Vector4 statusColor = default;
				switch ( item.ActionExecuteStatus ) {
					case AiActionExecuteStatus.Success: statusColor = Game.Profile.Theme.Green; break;
					case AiActionExecuteStatus.Failure: statusColor = Game.Profile.Theme.Red; break;
					case AiActionExecuteStatus.Running: statusColor = Game.Profile.Theme.Yellow; break;
				}
				ImGui.TextColored( statusColor, $"{item.ActionExecuteStatus}" );
				
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Faded, $" {item.ElapsedTime:0.0}s" );

				if ( item.IsInterrupted ) {
					ImGui.SameLine();
					ImGui.TextColored( Game.Profile.Theme.Faded, "\tInterrupted" );
				}
			}

			return false;
		}

		return base.DrawAllMembersWithTable( ref target, sameLineFilter );
	}
	
}
