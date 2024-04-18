using Bang;
using Bang.Entities;
using Pixpil.Core;
using Pixpil.Services;


namespace Pixpil.AI;

public enum BlackboardValueType : byte {
	Int,
	Float,
	Bool,
	String
}


public class CheckGameplayBlackboardValue : GoapCondition {

	public readonly BlackboardValueType ValueType;
	public readonly string FieldName;
	public readonly CompareMethod Method;
	public readonly int CompareToInt;
	public readonly float CompareToFloat;
	public readonly bool CompareToBool;
	public readonly string CompareToString;

	public override bool OnCheck( World world, Entity entity ) {
		var save = SaveServices.GetOrCreateSave();
		
		switch ( ValueType ) {
			case BlackboardValueType.Int:
				var intValue = save.BlackboardTracker.GetInt( GameplayBlackboard.Name, FieldName );
				return NumericCompareHelper.Compare( intValue, Method, CompareToInt );
			case BlackboardValueType.Float:
				var floatValue = save.BlackboardTracker.GetFloat( GameplayBlackboard.Name, FieldName );
				return NumericCompareHelper.Compare( floatValue, Method, CompareToFloat );
			case BlackboardValueType.Bool:
				var boolValue = save.BlackboardTracker.GetBool( GameplayBlackboard.Name, FieldName );
				return boolValue == CompareToBool;
			case BlackboardValueType.String:
				var strValue = save.BlackboardTracker.GetString( GameplayBlackboard.Name, FieldName );
				return strValue == CompareToString;
		}

		return false;
	}
}
