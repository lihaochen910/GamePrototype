using Murder.Diagnostics;


namespace Pixpil.AI.HFSM.Test;

public class ActionLog : HFSMStateAction {

	public string Msg;
	public bool LogOnInit;
	public bool LogOnEnter;
	public bool LogOnLogic;
	public bool LogOnExit;

	public override void Init() {
		if ( LogOnInit ) {
			GameLogger.Log( Msg );
		}
	}

	public override void OnEnter() {
		if ( LogOnEnter ) {
			GameLogger.Log( Msg );
		}
	}

	public override void OnLogic() {
		if ( LogOnLogic ) {
			GameLogger.Log( Msg );
		}
	}

	public override void OnExit() {
		if ( LogOnExit ) {
			GameLogger.Log( Msg );
		}
	}
	
}
