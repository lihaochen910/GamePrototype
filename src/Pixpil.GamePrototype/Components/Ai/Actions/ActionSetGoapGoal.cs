using System;
using Bang.Entities;
using Murder.Diagnostics;


namespace Pixpil.AI.Actions;

[Flags]
public enum GoapActionPhase : byte {
	None = 0,
	OnPre = 1,
	OnExecute = 1 << 2,
	OnPost = 1 << 3
}


public class ActionSetGoapGoal : GoapAction {
	
	public readonly string Goal;
	public readonly GoapActionPhase Phase;
	
	public override GoapActionExecuteStatus OnPreExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnPre ) ) {
			if ( Entity.TryGetGoapAgent() is {} goapAgentComponent ) {
				if ( goapAgentComponent.TryGetScenario() is {} asset ) {
					var goapGoal = asset.FindGoal( Goal );
					if ( goapGoal != null ) {
						if ( goapAgentComponent.Goal != Goal ) {
							Entity.SetGoapAgent( new GoapAgentComponent( goapAgentComponent.GoapScenarioAsset, Goal ) );
						}

						return GoapActionExecuteStatus.Success;
					}
				}
			}

			return GoapActionExecuteStatus.Failure;
		}

		if ( Phase.HasFlag( GoapActionPhase.OnExecute ) ) {
			return base.OnPreExecute();
		}

		return GoapActionExecuteStatus.Success;
	}

	public override void OnPostExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnPost ) ) {
			if ( Entity.TryGetGoapAgent() is {} goapAgentComponent ) {
				if ( goapAgentComponent.TryGetScenario() is {} asset ) {
					var goapGoal = asset.FindGoal( Goal );
					if ( goapGoal != null ) {
						if ( goapAgentComponent.Goal != Goal ) {
							Entity.SetGoapAgent( new GoapAgentComponent( goapAgentComponent.GoapScenarioAsset, Goal ) );
						}
					}
				}
			}
		}
	}
}


public class ActionSetGoapGoalConditionally : GoapAction {
	
	public readonly string Goal;
	public readonly string Condition;
	public readonly bool ConditionValue = true;
	public readonly GoapActionPhase Phase;
	
	public override GoapActionExecuteStatus OnPreExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnPre ) ) {
			if ( Entity.TryGetGoapAgent() is {} goapAgentComponent ) {
				if ( goapAgentComponent.TryGetScenario() is {} asset ) {
					if ( !asset.HasCondition( Condition ) ) {
						GameLogger.Warning( $"GoapAgent has no condition: {Condition}" );
						return GoapActionExecuteStatus.Failure;
					}

					if ( goapAgentComponent.CheckCondition( Condition, World, Entity ) == ConditionValue ) {
						var goapGoal = asset.FindGoal( Goal );
						if ( goapGoal != null ) {
							if ( goapAgentComponent.Goal != Goal ) {
								Entity.SetGoapAgent( new GoapAgentComponent( goapAgentComponent.GoapScenarioAsset, Goal ) );
							}
						}
					}
					
					return GoapActionExecuteStatus.Success;
				}
			}

			return GoapActionExecuteStatus.Failure;
		}

		if ( Phase.HasFlag( GoapActionPhase.OnExecute ) ) {
			return GoapActionExecuteStatus.Running;
		}

		return GoapActionExecuteStatus.Success;
	}

	public override GoapActionExecuteStatus OnExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnExecute ) ) {
			if ( Entity.TryGetGoapAgent() is {} goapAgentComponent ) {
				if ( goapAgentComponent.TryGetScenario() is {} asset ) {
					
					if ( !asset.HasCondition( Condition ) ) {
						GameLogger.Warning( $"GoapAgent has no condition: {Condition}" );
						return GoapActionExecuteStatus.Failure;
					}

					if ( goapAgentComponent.CheckCondition( Condition, World, Entity ) == ConditionValue ) {
						var goapGoal = asset.FindGoal( Goal );
						if ( goapGoal != null ) {
							if ( goapAgentComponent.Goal != Goal ) {
								Entity.SetGoapAgent( new GoapAgentComponent( goapAgentComponent.GoapScenarioAsset, Goal ) );
							}
							
							return GoapActionExecuteStatus.Success;
						}
					}
					
					return GoapActionExecuteStatus.Success;
				}
			}
		}
		
		return GoapActionExecuteStatus.Success;
	}

	public override void OnPostExecute() {
		if ( Phase.HasFlag( GoapActionPhase.OnPost ) ) {
			if ( Entity.TryGetGoapAgent() is {} goapAgentComponent ) {
				if ( goapAgentComponent.TryGetScenario() is {} asset ) {
                    
                    if ( !asset.HasCondition( Condition ) ) {
                    	GameLogger.Warning( $"GoapAgent has no condition: {Condition}" );
                    	return;
                    }

					if ( goapAgentComponent.CheckCondition( Condition, World, Entity ) == ConditionValue ) {
						var goapGoal = asset.FindGoal( Goal );
						if ( goapGoal != null ) {
							if ( goapAgentComponent.Goal != Goal ) {
								Entity.SetGoapAgent( new GoapAgentComponent( goapAgentComponent.GoapScenarioAsset, Goal ) );
							}
						}
					}
				}
			}
		}
	}
}
