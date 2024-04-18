using System;
using System.Collections.Generic;
using Bang;
using Bang.Entities;
using Bang.StateMachines;
using Murder;
using Murder.Components;
using Murder.Core.Dialogs;
using Murder.Services;
using Pixpil.Components;
using Pixpil.StateMachines;


namespace Pixpil.Services; 

public static class DialogueServices {
	
	public static LineComponent CreateLine( Line line ) {
		return new ( line, Game.NowUnscaled );
	}


	public static void TriggerDialogue( World world, SituationComponent situation, InputType inputType,
										MessageType messageType,
										bool canInterrupt = true ) {
		world.RunCoroutine( DoTalkIntro( world, situation, inputType, messageType, canInterrupt ) );
	}

	private static IEnumerator< Wait > DoTalkIntro( World world, SituationComponent situation,
													InputType inputType,
													MessageType messageType,
													bool canInterrupt = true ) {
		if ( world.TryGetUniqueEntity< MonologueComponent >() is {} existingEntity &&
			 messageType == MessageType.Monologue ) {
			if ( canInterrupt ) {
				// uhhhhhhh i wonder what could possibly backfire here?
				existingEntity.Destroy();
			}
			else {
				// don't do anything, we are not interrupting it.
				yield break;
			}
		}

		Entity dialogEntity = world.AddEntity();

		dialogEntity.SetSituation( situation );
		dialogEntity.SetStateMachine( new StateMachineComponent< MonologueStateMachine >( new MonologueStateMachine( inputType, messageType ) ) );
		dialogEntity.SetDoNotPause();

		// if ( tracker is not null ) {
		// 	dialogEntity.SetTriggeredEventTracker( tracker.Value );
		// }

		yield return Wait.Stop;
	}
	
}
