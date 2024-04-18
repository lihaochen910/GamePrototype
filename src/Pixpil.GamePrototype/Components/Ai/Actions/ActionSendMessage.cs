using Bang;
using Bang.Components;
using Bang.Entities;


namespace Pixpil.AI.Actions; 

public class ActionSendMessage : AiAction {
	
	public readonly IMessage Msg;
	public readonly AiActionPhase Phase;

	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
		if ( Phase.HasFlag( GoapActionPhase.OnPre ) ) {
			entity.SendMessage( Msg );
			return AiActionExecuteStatus.Success;
		}
		
		return base.OnPreExecute( world, entity );
	}

	public override void OnPostExecute( World world, Entity entity ) {
		if ( Phase.HasFlag( GoapActionPhase.OnPost ) ) {
			entity.SendMessage( Msg );
		}
	}
	
}
