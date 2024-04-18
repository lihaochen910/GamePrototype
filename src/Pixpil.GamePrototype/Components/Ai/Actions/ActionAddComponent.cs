using Bang.Components;
using Murder.Attributes;


namespace Pixpil.AI.Actions; 

public class ActionAddComponent : GoapAction {
	
	[NoLabel]
	public readonly IComponent Component;

	public override GoapActionExecuteStatus OnPreExecute() {
		if ( Component is null ) {
			return GoapActionExecuteStatus.Failure;
		}
		
		Entity.AddOrReplaceComponent( Component );
		return GoapActionExecuteStatus.Success;
	}
	
}
