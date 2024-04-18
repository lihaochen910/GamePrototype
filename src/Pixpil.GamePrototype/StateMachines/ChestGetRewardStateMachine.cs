using System.Collections.Generic;
using Bang.Entities;
using Bang.StateMachines;
using Murder;
using Murder.Diagnostics;
using Murder.Messages;
using Murder.Services;
using Pixpil.Data;
using Pixpil.Services;


namespace Pixpil.StateMachines; 

public class ChestGetRewardStateMachine : StateMachine {
	
	public ChestGetRewardStateMachine() {
		State( Open );
	}

	private IEnumerator< Wait > Open() {

		var interacted = Entity;
		EntityServices.PlaySpriteAnimationOnce( interacted, "open" );
		interacted.SetAnimationStarted( Game.Now );
		yield return Wait.ForMessage< AnimationCompleteMessage >();
		yield return GoTo( Reward );
		
	}

	private IEnumerator< Wait > Reward() {

		var chestGetRewardInteractionData = Entity.GetChestGetRewardInteractionData();
		var interactor = World.GetEntity( chestGetRewardInteractionData.InteractorId );
		var interacted = Entity;
		
		ItemType[] rewardTypes = [ ItemTypeServices.GetItemType( "food" ), ItemTypeServices.GetItemType( "wood" ), ItemTypeServices.GetItemType( "stone" ) ];
		var selected = rewardTypes[ Game.Random.Next( 0, rewardTypes.Length - 1 ) ];
		interacted.GetInventory().MoveItemToOther( selected, interacted, interactor );
		interacted.SetAgentDebugMessage( $"{selected.Id}++", 3f );

		yield return Wait.ForSeconds( 1f );
		yield return GoTo( Close );
	}


	private IEnumerator< Wait > Close() {
		EntityServices.PlaySpriteAnimationOnce( Entity, "close" );
		Entity.SetAnimationStarted( Game.Now );
		yield return Wait.ForMessage< AnimationCompleteMessage >();
		
		Entity.Destroy();
		yield return Wait.Stop;
	}
	
}
