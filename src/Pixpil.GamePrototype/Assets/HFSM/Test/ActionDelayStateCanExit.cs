using System.Collections.Generic;
using Bang.StateMachines;


namespace Pixpil.AI.HFSM.Test;

public class ActionDelayStateCanExit : HFSMStateActionWithCo {
	
	public float Delay;

	public override IEnumerator< Wait > OnCoroutine() {
		yield return Wait.ForSeconds( Delay );
		Fsm.StateCanExit();
	}

}
