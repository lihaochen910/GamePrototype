using Bang.Components;


namespace Pixpil.Messages;

public readonly struct HFSMTriggerMessage : IMessage {

	public readonly string Trigger;
	
	public HFSMTriggerMessage( string trigger ) {
		Trigger = trigger;
	}

}
