using System.Collections.Immutable;
using Bang.Components;
using Murder.Assets;
using Murder.Attributes;
using Murder.Core.Dialogs;
using Murder.Services;
using Murder.Utilities.Attributes;
using Pixpil.StateMachines;


namespace Pixpil.Components;

[RuntimeOnly]
[DoNotPersistOnSave]
public readonly struct MonologueComponent : IComponent {
	
	public readonly Line Line;

	public readonly InputType InputType = InputType.Time;

	public readonly ImmutableArray< string >? Choices = null;

	public MonologueComponent( Line line, InputType inputType ) {
		Line = line;
		InputType = inputType;
	}

	public MonologueComponent( string line, ImmutableArray< string > choices, InputType inputType ) {
		Line = new Line( new LocalizedString( line ) );
		Choices = choices;
		InputType = inputType;
	}
}
