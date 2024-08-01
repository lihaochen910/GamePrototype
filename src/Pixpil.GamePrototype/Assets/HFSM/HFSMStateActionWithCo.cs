using System.Collections.Generic;
using System.Diagnostics;
using Bang.Components;
using Bang.Entities;
using Bang.StateMachines;
using Murder;
using Murder.Attributes;


namespace Pixpil.AI.HFSM;

public abstract class HFSMStateActionWithCo : HFSMStateAction {
	
	/// <summary>
	/// The routine the entity is currently executing.
	/// </summary>
	[HideInEditor]
	public IEnumerator< Wait >? Routine { get; protected set; }


	#region Routine Field

	/// <summary>
	/// Track any wait time before calling the next Tick.
	/// </summary>
	private float? _waitTime = null;

	/// <summary>
	/// Track any amount of frames before calling the next Tick.
	/// </summary>
	private int? _waitFrames = null;

	/// <summary>
	/// Routine which we might be currently waiting on, before resuming to <see cref="Routine"/>.
	/// </summary>
	private readonly Stack< IEnumerator< Wait > > _routinesOnWait = new ();

	/// <summary>
	/// Track the message we are waiting for.
	/// </summary>
	private int? _waitForMessage = null;

	/// <summary>
	/// Target entity for <see cref="_waitForMessage"/>.
	/// </summary>
	private Entity? _waitForMessageTarget = null;

	/// <summary>
	/// Tracks whether a message which was waited has been received.
	/// </summary>
	private bool _isMessageReceived = false;

	#endregion


	/// <inheritdoc/>
	public override void OnEnter() {
		Entity.OnMessage += OnMessageSent;
		Routine = OnCoroutine();
	}

	public override void OnLogic() {
		if ( Routine is not null ) {
			Tick( Game.DeltaTime * 1000 );
		}
	}

	/// <inheritdoc/>
	public override void OnExit() {
		if ( Routine is not null ) {
			Finish();
		}
		Entity.OnMessage -= OnMessageSent;
	}

	
	/// <summary>
	/// like Bang Coroutine, invoke after OnEnter call.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerator< Wait > OnCoroutine() {
		yield return Wait.Stop;
	}
	
	
	/// <summary>
	/// Implemented by state machine implementations that want to listen to message
	/// notifications from outer systems.
	/// </summary>
	protected virtual void OnMessage( IMessage message ) {}
	
	/// <summary>
    /// Tick an update.
    /// Should only be called by the state machine component, see <see cref="StateMachineComponent{T}"/>.
    /// </summary>
    internal bool Tick(float dt)
    {
#if DEBUG
        Debug.Assert(World is not null && Entity is not null, "Why are we ticking before starting first?");
#endif

        if (_waitTime is not null)
        {
            _waitTime -= dt;

            if (_waitTime > 0)
            {
                return true;
            }

            _waitTime = null;
        }

        if (_waitFrames is not null)
        {
            if (--_waitFrames > 0)
            {
                return true;
            }

            _waitFrames = null;
        }

        if (_waitForMessage is not null)
        {
            if (!_isMessageReceived)
            {
                return true;
            }

            _waitForMessage = null;
            _isMessageReceived = false;
        }

        Wait r = Tick();
        switch (r.Kind)
        {
            case WaitKind.Stop:
                Finish();
                return false;

            case WaitKind.Ms:
                _waitTime = r.Value!.Value;
                return true;

            case WaitKind.Frames:
                _waitFrames = r.Value!.Value;
                return true;

            case WaitKind.Message:
                Entity target = r.Target ?? Entity;
                int messageId = World.ComponentsLookup.Id(r.Component!);

                if (target.HasMessage(messageId))
                {
                    // The entity might already have the message within the frame.
                    // If that is the case, skip the wait and resume in the next frame.
                    _waitFrames = 1;
                    return true;
                }

                _waitForMessage = messageId;

                // Do extra setup on custom targets.
                if (r.Target is not null)
                {
                    _waitForMessageTarget = r.Target;
                    target.OnMessage += OnMessageSent;
                }
                else
                {
                    _waitForMessageTarget = null;
                }

                return true;

            case WaitKind.Routine:
                _routinesOnWait.Push(r.Routine!);

                // When we wait for a routine, immediately run it first.
                return Tick(dt);
        }

        return true;
    }

    private Wait Tick()
    {
        if (Routine is null)
        {
            Debug.Assert(Routine is not null, "Have you called State() before ticking this state machine?");

            // Instead of embarrassingly crashing, send a stop wait message.
            return Wait.Stop;
        }

        // if (_isFirstTick)
        // {
        //     OnStart();
        //     _isFirstTick = false;
        // }

        // If there is a wait routine, go for that instead.
        while (_routinesOnWait.Count != 0)
        {
            if (_routinesOnWait.Peek().MoveNext())
            {
                return _routinesOnWait.Peek().Current ?? Wait.Stop;
            }
            else
            {
                _routinesOnWait.Pop();
            }
        }

        if (!Routine.MoveNext())
        {
            return Wait.Stop;
        }

        return Routine?.Current ?? Wait.Stop;
    }

	internal void Finish() {
		Routine?.Dispose();
		Routine = null;
	}

	private void OnMessageSent( Entity e, int index, IMessage message ) {
		if ( e.EntityId == Entity.EntityId ) {
			OnMessage( message );
		}

		if ( _waitForMessage is null ||
			 ( _waitForMessageTarget is not null && e.EntityId != _waitForMessageTarget.EntityId ) ) {
			return;
		}

		if ( index != _waitForMessage.Value ) {
			return;
		}

		_isMessageReceived = true;

		if ( _waitForMessageTarget is not null ) {
			_waitForMessageTarget.OnMessage -= OnMessageSent;
			_waitForMessageTarget = null;
		}
	}

}
