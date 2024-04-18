using Bang;
using Bang.Entities;


namespace Pixpil.AI;

public class CheckPPBlackboardBooleanVariable : GoapCondition {
	
	public readonly string VarName;
	public readonly bool CompareTo;

	public override bool OnCheck( World world, Entity entity ) {
		var blackboardComponent = entity.TryGetBlackboard();
		if ( blackboardComponent is null ) {
			return false;
		}

		var blackboard = blackboardComponent.Value;
		if ( !blackboard.HasVariable( VarName ) ) {
			return false;
		}
		
		return blackboard.GetValue< bool >( VarName ) == CompareTo;
	}
}


public class CheckPPBlackboardIntVariable : GoapCondition {
	
	public readonly string VarName;
	public readonly CompareMethod Method;
	public readonly int CompareTo;

	public override bool OnCheck( World world, Entity entity ) {
		var blackboardComponent = entity.TryGetBlackboard();
		if ( blackboardComponent is null ) {
			return false;
		}

		var blackboard = blackboardComponent.Value;
		if ( !blackboard.HasVariable( VarName ) ) {
			return false;
		}
		
		return NumericCompareHelper.Compare( blackboard.GetValue< int >( VarName ), Method, CompareTo );
	}
}


public class CheckPPBlackboardFloatVariable : GoapCondition {
	
	public readonly string VarName;
	public readonly CompareMethod Method;
	public readonly float CompareTo;

	public override bool OnCheck( World world, Entity entity ) {
		var blackboardComponent = entity.TryGetBlackboard();
		if ( blackboardComponent is null ) {
			return false;
		}

		var blackboard = blackboardComponent.Value;
		if ( !blackboard.HasVariable( VarName ) ) {
			return false;
		}
		
		return NumericCompareHelper.Compare( blackboard.GetValue< float >( VarName ), Method, CompareTo );
	}
}


public class CheckPPBlackboardStringVariable : GoapCondition {
	
	public readonly string VarName;
	public readonly CompareMethod Method;
	public readonly string CompareTo;

	public override bool OnCheck( World world, Entity entity ) {
		var blackboardComponent = entity.TryGetBlackboard();
		if ( blackboardComponent is null ) {
			return false;
		}

		var blackboard = blackboardComponent.Value;
		if ( !blackboard.HasVariable( VarName ) ) {
			return false;
		}

		var bbValue = blackboard.GetValue< string >( VarName );
		if ( Method is CompareMethod.EqualTo ) {
			return bbValue == CompareTo;
		}
		return bbValue != CompareTo;
	}
}


public class CheckPPBlackboardStringIsNullOrEmptyVariable : GoapCondition {
	
	public readonly string VarName;
	public readonly bool IsNullOrEmpty;

	public override bool OnCheck( World world, Entity entity ) {
		var blackboardComponent = entity.TryGetBlackboard();
		if ( blackboardComponent is null ) {
			return false;
		}

		var blackboard = blackboardComponent.Value;
		if ( !blackboard.HasVariable( VarName ) ) {
			return false;
		}
		
		return IsNullOrEmpty ? string.IsNullOrEmpty( blackboard.GetValue< string >( VarName ) ) : !string.IsNullOrEmpty( blackboard.GetValue< string >( VarName ) );
	}
}
