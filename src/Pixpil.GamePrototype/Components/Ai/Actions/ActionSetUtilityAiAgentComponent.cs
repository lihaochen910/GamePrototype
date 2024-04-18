using System;
using Bang;
using Bang.Entities;
using Murder;
using Pixpil.Assets;


namespace Pixpil.AI.Actions;

public class ActionSetUtilityAiAgentComponent : AiAction {
	
	public readonly UtilityAiAgentComponent UtilityAiAgentComponent;
	
	public readonly AiActionPhase Phase;

	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
		if ( Phase.HasFlag( AiActionPhase.OnPre ) ) {
			TrySet( entity );

			if ( Phase.HasFlag( AiActionPhase.OnExecute ) ) {
				return AiActionExecuteStatus.Running;
			}
		}

		return AiActionExecuteStatus.Success;
	}

	public override AiActionExecuteStatus OnExecute( World world, Entity entity ) {
		if ( Phase.HasFlag( AiActionPhase.OnExecute ) ) {
			var result = TrySet( entity );
			return result ? AiActionExecuteStatus.Success : AiActionExecuteStatus.Running;
		}
		
		return AiActionExecuteStatus.Success;
	}
	
	public override void OnPostExecute( World world, Entity entity ) {
		if ( Phase.HasFlag( AiActionPhase.OnPost ) ) {
			TrySet( entity );
		}
	}

	private bool TrySet( Entity entity ) {
		if ( Game.Data.TryGetAsset< UtilityAiAsset >( UtilityAiAgentComponent.UtilityAiAsset ) is not null ) {
			if ( entity.TryGetUtilityAiAgent() is {} utilityAiAgentComponent ) {
				entity.SetUtilityAiAgent( UtilityAiAgentComponent.UtilityAiAsset, utilityAiAgentComponent.EvaluateMethod, utilityAiAgentComponent.EvaluateInterval );
			}
			else {
				entity.SetUtilityAiAgent( new UtilityAiAgentComponent( UtilityAiAgentComponent.UtilityAiAsset, UtilityAiAgentComponent.EvaluateMethod, UtilityAiAgentComponent.EvaluateInterval ) );
			}
			entity.SendMessage< RequestUtilityAiAgentEvaluateMessage >();
			return true;
		}
		
		return false;
	}
}
