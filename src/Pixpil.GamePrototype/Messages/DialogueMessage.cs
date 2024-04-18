using Bang.Components;
using Pixpil.Components;


namespace Pixpil.Messages;

public readonly struct DialogueMessage : IMessage {
	
	public readonly MonologueComponent Monologue { get; init; } = new();

	public readonly bool Clear { get; init; } = false;

	public DialogueMessage( MonologueComponent monologue ) => Monologue = monologue;

	public static DialogueMessage CreateClear() => new() { Clear = true };
}
