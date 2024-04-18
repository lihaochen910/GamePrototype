using System;
using Bang.Components;
using Murder.Utilities.Attributes;
using Pixpil.Assets;


namespace Pixpil.Components;

[Unique]
[Story]
public readonly struct DayCycleComponent : IComponent {
	
	[Bang.Serialize]
	public readonly DayProgress DayCycle;

	public DayCycleComponent() { }

	public DayCycleComponent( DayProgress day ) => DayCycle = day;
}