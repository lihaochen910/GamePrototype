using Bang;
using Bang.Entities;
using Bang.StateMachines;
using Bang.Systems;
using Murder;
using Murder.Core.Dialogs;
using System.Collections.Immutable;
using Pixpil.Components;
using Pixpil.Core;
using Pixpil.Messages;
using Pixpil.StateMachines;


namespace Pixpil.Systems.Ui;

[DoNotPause]
[Filter( typeof( MonologueComponent ) )]
[Watch( typeof( MonologueComponent ) )]
public class MonologueUiSystem : IReactiveSystem {
	
	private Entity? _monologueEntity;

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		Game.Input.Consume( InputButtons.Submit );

		_monologueEntity ??= world.AddEntity();

		Entity e = entities[ 0 ];

		_monologueEntity.SetStateMachine( new StateMachineComponent< MonologueUiStateMachine >() );
		_monologueEntity.SendMessage< TargetEntityMessage >( new ( e ) );

		SendDialogue( e );
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		Game.Input.Consume( InputButtons.Submit );

		Entity e = entities[ 0 ];
		SendDialogue( e );
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		_monologueEntity?.SendMessage( DialogueMessage.CreateClear() );
	}

	private void SendDialogue( Entity e ) {
		// LibraryAsset library = LibraryServices.GetLibrary();

		// SpeakerKind speaker;
		Line line = e.GetMonologue().Line;

		// if ( line.Speaker == library.DriverSpeaker ) {
		// 	speaker = SpeakerKind.Driver;
		// }
		// else if ( line.Speaker == library.GrannySpeaker ) {
		// 	speaker = SpeakerKind.Granny;
		// }
		// else {
		// 	speaker = SpeakerKind.Passenger;
		// }

		_monologueEntity?.SendMessage( new DialogueMessage( e.GetMonologue() ) );
	}
}
