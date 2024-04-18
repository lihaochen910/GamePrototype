using Bang;
using Bang.Entities;
using Murder;


namespace Pixpil.AI.Actions; 

public class ActionWaitForSecond : AiAction {
	
	public readonly float Time;
	public readonly bool UseUnscaledDeltaTime;
	public readonly AiActionExecuteStatus StatusWhenFinish;

	private float _timer;

	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
		_timer = Time;
		return base.OnPreExecute( world, entity );
	}

	public override AiActionExecuteStatus OnExecute( World world, Entity entity ) {
		_timer -= UseUnscaledDeltaTime ? Game.UnscaledDeltaTime : Game.DeltaTime;
		if ( _timer < 0f ) {
			return StatusWhenFinish;
		}
		return AiActionExecuteStatus.Running;
	}
}
