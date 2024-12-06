using Murder.Diagnostics;


namespace Pixpil.AI.HFSM.Test;

public class ActionTestInitCall : HFSMStateAction {

	public override void Init() {
		GameLogger.Verify( World != null );
		GameLogger.Verify( Entity != null );
		GameLogger.Verify( Fsm != null );
		GameLogger.Verify( State != null );
		GameLogger.Log( $"finish test state: {StateName}" );
	}

}
