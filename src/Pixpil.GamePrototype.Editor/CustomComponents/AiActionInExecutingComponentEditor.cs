using System.Numerics;
using ImGuiNET;
using Murder;
using Murder.Editor;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Pixpil.AI;


namespace Pixpil.GamePrototype.Editor.CustomComponents; 

[CustomComponentOf( typeof( AiActionInExecutingComponent ) )]
public class AiActionInExecutingComponentEditor : CustomComponent {

	private static Vector4 GetStatusColor( AiActionExecuteStatus status ) {
		switch ( status ) {
			case AiActionExecuteStatus.Success: return Game.Profile.Theme.Green;
			case AiActionExecuteStatus.Failure: return Game.Profile.Theme.Red;
			case AiActionExecuteStatus.Running: return Game.Profile.Theme.Yellow;
		}
		
		return Game.Profile.Theme.White;
	}
	
	protected override bool DrawAllMembersWithTable( ref object target ) {

		var playingInEditor = Architect.Instance != null && Architect.Instance.IsPlayingGame;
		if ( playingInEditor ) {
			var aiActionInExecutingComponent = ( AiActionInExecutingComponent )target;
			if ( aiActionInExecutingComponent.Action is null ) {
				return base.DrawAllMembersWithTable( ref target );
			}

			for ( var i = 0; i < aiActionInExecutingComponent.ActionCloned.Length; i++ ) {
				var action = aiActionInExecutingComponent.ActionCloned[ i ];
				ImGui.TextColored( Game.Profile.Theme.HighAccent, $"{action.GetType().Name}:" );
				ImGui.SameLine();
				ImGui.TextColored( Game.Profile.Theme.Faded, $" {action.ElapsedTime:0.00}" );
				
				ImGui.Indent( 12 );
				ImGui.Text( "Pre: " );
				ImGui.SameLine();
				ImGui.TextColored( GetStatusColor( action.PreExecuteResult ), $"{action.PreExecuteResult}" );
				
				ImGui.Text( "Execute: " );
				ImGui.SameLine();
				ImGui.TextColored( GetStatusColor( action.ExecuteResult ), $"{action.ExecuteResult}" );
				ImGui.Unindent( 12 );
			}

			// foreach ( var action in aiActionInExecutingComponent.ActionCloned ) {
			// 	ImGui.TextColored( Game.Profile.Theme.HighAccent, $"{action.GetType().Name}: " );
			// 	// ImGui.SameLine();
			// 	
			// 	ImGui.Indent( 12 );
			// 	ImGui.TextColored( Game.Profile.Theme.Faded, $"{action.ElapsedTime:0.00}: " );
			// 	ImGui.TextColored( Game.Profile.Theme.Yellow, $"PreExecuteResult: {action.PreExecuteResult}" );
			// 	ImGui.TextColored( Game.Profile.Theme.Yellow, $"ExecuteResult: {action.ExecuteResult}" );
			// 	ImGui.Unindent( 12 );
			// }

			return false;
		}

		return base.DrawAllMembersWithTable( ref target );
	}
	
}
