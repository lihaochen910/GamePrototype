using System;
using Bang.Entities;
using Murder;
using Murder.Attributes;
using Murder.Diagnostics;
using Pixpil.Assets;


namespace Pixpil.AI.Actions;

public class ActionSetGoapAgentComponent : GoapAction {
	
	public readonly GoapAgentComponent GoapAgentComponent;
	
	public readonly GoapActionPhase Phase;

	public override GoapActionExecuteStatus OnPreExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnPre ) ) {
			TrySet();

			if ( Phase.HasFlag( GoapActionPhase.OnExecute ) ) {
				return GoapActionExecuteStatus.Running;
			}
		}

		return GoapActionExecuteStatus.Success;
	}

	public override GoapActionExecuteStatus OnExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnExecute ) ) {
			var result = TrySet();
			return result ? GoapActionExecuteStatus.Success : GoapActionExecuteStatus.Running;
		}
		
		return GoapActionExecuteStatus.Success;
	}
	
	public override void OnPostExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnPost ) ) {
			TrySet();
		}
	}

	private bool TrySet() {
		if ( Game.Data.TryGetAsset< GoapScenarioAsset >( GoapAgentComponent.GoapScenarioAsset ) is not null ) {
			if ( Entity.TryGetGoapAgent() is {} goapAgentComponent ) {
				Entity.SetGoapAgent( GoapAgentComponent.GoapScenarioAsset, GoapAgentComponent.Goal, goapAgentComponent.EvaluateFrequency, goapAgentComponent.EvaluateInterval );
			}
			else {
				Entity.SetGoapAgent( new GoapAgentComponent( GoapAgentComponent.GoapScenarioAsset, GoapAgentComponent.Goal, GoapAgentComponent.EvaluateFrequency, GoapAgentComponent.EvaluateInterval ) );
			}
			Entity.SendMessage< RequestGoapAgentEvaluateMessage >();
			return true;
		}
		
		return false;
	}
}


public class ActionSetGoapAgentComponentConditionally : GoapAction {
	
	[GameAssetId< GoapScenarioAsset >]
	public readonly Guid GoapScenarioAsset;

	public readonly string Goal;
	
	public readonly string Condition;
	public readonly bool ConditionValue = true;
	public readonly GoapActionPhase Phase;

	public override GoapActionExecuteStatus OnPreExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnPre ) ) {
			var result = TrySet();
			if ( Phase.HasFlag( GoapActionPhase.OnExecute ) ) {
				return result ? GoapActionExecuteStatus.Success : GoapActionExecuteStatus.Running;
			}
			
			return GoapActionExecuteStatus.Success;
		}

		if ( Phase.HasFlag( GoapActionPhase.OnExecute ) ) {
			return base.OnPreExecute();
		}

		return GoapActionExecuteStatus.Success;
	}

	public override GoapActionExecuteStatus OnExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnExecute ) ) {
			var result = TrySet();
			return result ? GoapActionExecuteStatus.Success : GoapActionExecuteStatus.Running;
		}
		
		return GoapActionExecuteStatus.Success;
	}

	public override void OnPostExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnPost ) ) {
			TrySet();
		}
	}

	private bool TrySet() {
		if ( Entity.TryGetGoapAgent() is {} goapAgentComponent ) {

			if ( !goapAgentComponent.TryGetScenario().HasCondition( Condition ) ) {
				GameLogger.Warning( $"GoapAgent has no condition: {Condition}" );
				return false;
			}
			
			if ( goapAgentComponent.CheckCondition( Condition, World, Entity ) == ConditionValue ) {
				Entity.SetGoapAgent( new GoapAgentComponent( GoapScenarioAsset, Goal ) );
				Entity.SendMessage< RequestGoapAgentEvaluateMessage >();
				return true;
			}
			
		}
		
		return false;
	}
}
