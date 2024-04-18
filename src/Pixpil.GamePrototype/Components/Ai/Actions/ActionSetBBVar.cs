using Bang.Entities;


namespace Pixpil.AI.Actions; 

public class ActionSetBooleanBBVar : GoapAction {
	
	public readonly string VarName;
	public readonly bool Value;

	public override GoapActionExecuteStatus OnPreExecute() {
		if ( Entity.TryGetBlackboard() is BlackboardComponent blackboardComponent ) {
			Entity.SetBlackboard( blackboardComponent.SetValue( VarName, Value ) );
			return GoapActionExecuteStatus.Success;
		}
		
		return GoapActionExecuteStatus.Failure;
	}
}


public class ActionSetBooleanBBVarOnPost : GoapAction {
	
	public readonly string VarName;
	public readonly bool Value;

	public override GoapActionExecuteStatus OnPreExecute() => GoapActionExecuteStatus.Success;

	public override void OnPostExecute() {
		if ( Entity.TryGetBlackboard() is BlackboardComponent blackboardComponent ) {
			Entity.SetBlackboard( blackboardComponent.SetValue( VarName, Value ) );
		}
	}
}


public class ActionSetIntBBVar : GoapAction {
	
	public readonly string VarName;
	public readonly int Value;
	
	public override GoapActionExecuteStatus OnPreExecute() {
		if ( Entity.TryGetBlackboard() is BlackboardComponent blackboardComponent ) {
			Entity.SetBlackboard( blackboardComponent.SetValue( VarName, Value ) );
			return GoapActionExecuteStatus.Success;
		}
		
		return GoapActionExecuteStatus.Failure;
	}
}


public class ActionSetIntBBVarOnPost : GoapAction {
	
	public readonly string VarName;
	public readonly int Value;

	public override GoapActionExecuteStatus OnPreExecute() => GoapActionExecuteStatus.Success;

	public override void OnPostExecute() {
		if ( Entity.TryGetBlackboard() is BlackboardComponent blackboardComponent ) {
			Entity.SetBlackboard( blackboardComponent.SetValue( VarName, Value ) );
		}
	}
}


public class ActionSetFloatBBVar : GoapAction {
	
	public readonly string VarName;
	public readonly float Value;
	
	public override GoapActionExecuteStatus OnPreExecute() {
		if ( Entity.TryGetBlackboard() is BlackboardComponent blackboardComponent ) {
			Entity.SetBlackboard( blackboardComponent.SetValue( VarName, Value ) );
			return GoapActionExecuteStatus.Success;
		}
		
		return GoapActionExecuteStatus.Failure;
	}
}


public class ActionSetStringBBVar : GoapAction {
	
	public readonly string VarName;
	public readonly string Value;
	
	public override GoapActionExecuteStatus OnPreExecute() {
		if ( Entity.TryGetBlackboard() is BlackboardComponent blackboardComponent ) {
			Entity.SetBlackboard( blackboardComponent.SetValue( VarName, Value ) );
			return GoapActionExecuteStatus.Success;
		}
		
		return GoapActionExecuteStatus.Failure;
	}
}
