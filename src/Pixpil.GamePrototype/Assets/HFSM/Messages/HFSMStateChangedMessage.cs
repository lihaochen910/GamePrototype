using Bang.Components;


namespace Pixpil.Messages;

public readonly struct HFSMStateChangedMessage : IMessage {

	public readonly string State;
	
	public HFSMStateChangedMessage( string state ) {
		State = state;
	}

}
