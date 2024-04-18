using Bang.Entities;


namespace Pixpil.AI.Actions; 

public class ActionCheckGoapCondition : GoapAction {

	public readonly string Condition;
	public readonly GoapActionExecuteStatus StatusWhileTrue;
	public readonly GoapActionExecuteStatus StatusWhileFalse;

	public override GoapActionExecuteStatus OnExecute() {
		var goapAgentComponent = Entity.GetGoapAgent();
		if ( goapAgentComponent.CheckCondition( Condition, World, Entity ) ) {
			return StatusWhileTrue;
		}

		return StatusWhileFalse;
	}

}
