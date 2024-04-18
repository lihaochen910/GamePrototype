using Bang.Components;
using Pixpil.Assets;


namespace Pixpil.Messages;

public readonly record struct TheDayNearDuskMessage : IMessage {
	
	public readonly DayProgress DayProgress;

	public TheDayNearDuskMessage( DayProgress dayProgress ) {
		DayProgress = dayProgress;
	}
	
}


public readonly record struct OneDayPassedMessage : IMessage {
	
	public readonly DayProgress DayProgress;

	public OneDayPassedMessage( DayProgress dayProgress ) {
		DayProgress = dayProgress;
	}
	
}
