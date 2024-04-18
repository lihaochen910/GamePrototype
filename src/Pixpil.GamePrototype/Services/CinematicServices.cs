using System;
using Bang.Entities;
using Bang;
using Murder.Components;
using Murder;
using Murder.Systems.Physics;
using Pixpil.Systems;


namespace Pixpil.Services;

/// <summary>
/// This will enable a cinematic border within the world.
/// This will not pause or freeze the game.
/// </summary>
public struct PartiallyPauseGame : IDisposable {
	
	private readonly World _world;
	private Entity? _freezeWorldEntity;

	public PartiallyPauseGame( World world ) {
		_world = world;

		Freeze();
	}

	public void Dispose() {
		Unfreeze();
	}

	private void Freeze() {
		if ( _world.TryGetUnique< FreezeWorldComponent >() is FreezeWorldComponent ) {
			return;
		}

		_freezeWorldEntity = _world.AddEntity( new FreezeWorldComponent() );

		_world.DeactivateSystem< PlayerInputSystem >();
		_world.DeactivateSystem< SATPhysicsSystem >();

		_world.DeactivateSystem< DayCycleSystem >();

		Game.Instance.Pause();
	}

	private void Unfreeze() {
		if ( _freezeWorldEntity is { IsDestroyed: false } ) {
			_freezeWorldEntity.Destroy();
		}
		else {
			// Nothing to do.
			return;
		}

		_world.ActivateSystem< PlayerInputSystem >();
		_world.ActivateSystem< SATPhysicsSystem >();

		_world.ActivateSystem< DayCycleSystem >();

		Game.Instance.Resume();
	}

	public static PartiallyPauseGame Create( World world ) {
		return new ( world );
	}
}
