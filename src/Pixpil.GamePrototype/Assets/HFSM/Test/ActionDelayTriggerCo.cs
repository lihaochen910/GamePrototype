using System.Collections.Generic;
using Bang.StateMachines;


namespace Pixpil.AI.HFSM.Test;

public class ActionDelayTriggerCo : HFSMStateActionWithCo {
	
	public string Trigger;
	public float Delay;

	public override IEnumerator< Wait > OnCoroutine() {
		yield return Wait.ForSeconds( Delay );
		Trigger( Trigger );
	}

}
