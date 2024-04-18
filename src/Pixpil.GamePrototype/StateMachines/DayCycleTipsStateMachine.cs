using System.Collections.Generic;
using Bang.StateMachines;
using Pixpil.Messages;


namespace Pixpil.StateMachines; 

public class DayCycleTipsStateMachine : StateMachine {
	
	public DayCycleTipsStateMachine() {
		State( Main );
	}

	private IEnumerator< Wait > Main() {
		while ( true ) {
			yield return Wait.ForMessage< TheDayNearDuskMessage >();
		}
	}
}