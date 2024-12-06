using System.Numerics;
using Murder;
using Murder.Core.Graphics;
using Murder.Services;


namespace Pixpil.AI.HFSM.Test;

public enum ActionDelayTriggerTarget : byte {
	Owner,
	Parent,
	Root
}

public class ActionDelayTrigger : HFSMStateAction {

	public ActionDelayTriggerTarget TriggerTarget;
	public string Trigger;
	public float Delay;

	private float _timer;
	private bool _triggered;

	public override void OnEnter() {
		_timer = 0f;
		_triggered = false;
		if ( Delay <= 0f ) {
			ProcessTrigger();
			_triggered = true;
		}
	}

	public override void OnLogic() {
		if ( !_triggered ) {
			_timer += Game.DeltaTime;
			if ( _timer > Delay ) {
				ProcessTrigger();
				_triggered = true;
			}
		}
	}

#if MURDER
	public override void OnMurderDraw( RenderContext render ) {
		render.UiBatch.DrawText( MurderFonts.PixelFont, $"[ActionDelayTrigger]: {_timer:0.0}/{Delay}", new Vector2( 0, 0 ) );
	}
#endif

	private void ProcessTrigger() {
		switch ( TriggerTarget ) {
			case ActionDelayTriggerTarget.Owner:
				Trigger( Trigger );
				break;
			case ActionDelayTriggerTarget.Parent:
				if ( Fsm.ParentFsm is ITriggerable< string > triggerable ) {
					triggerable.Trigger( Trigger );
				}
				break;
			case ActionDelayTriggerTarget.Root:
				RootFsm.Trigger( Trigger );
				break;
		}
	}
}
