using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Systems;
using Murder.Components;
using Murder.Messages;
using Murder.Messages.Physics;
using Murder.Utilities;


namespace Pixpil.Systems.Interactions;

/// <summary>
/// Interactors tag hightlights and interacts with InteractorComponents
/// </summary>
[Filter( typeof( InteractOnCollisionComponent ) )]
[Messager( typeof( OnCollisionMessage ) )]
internal class InteractOnCollisionSystem : IMessagerSystem {
	
	public void OnMessage( World world, Entity entity, IMessage message ) {
		var msg = ( OnCollisionMessage )message;

		if ( world.TryGetEntity( msg.EntityId ) is not Entity interactorEntity ) {
			return;
		}

		if ( interactorEntity.IsDestroyed || !interactorEntity.HasInteractor() ) {
			return;
		}

		Entity interactiveEntity = entity;
		if ( interactiveEntity.IsDestroyed ) {
			return;
		}

		var interactOnCollision = interactiveEntity.GetInteractOnCollision();
		if ( interactOnCollision.PlayerOnly && !interactorEntity.HasPlayer() ) {
			return;
		}

		else if ( msg.Movement == CollisionDirection.Exit ) {
			foreach ( var interaction in interactOnCollision.CustomExitMessages ) {
				interaction.Interact( world, interactorEntity, interactiveEntity );
			}

			if ( !interactOnCollision.SendMessageOnExit ) {
				return;
			}
		}

		// After all these checks, I thinks it's ok to send that message!            
		// Trigger right away!

		interactiveEntity.SendMessage( new InteractMessage( interactorEntity ) );

		if ( interactOnCollision.OnlyOnce ) {
			interactiveEntity.RemoveInteractOnCollision();
		}

	}
	
}
