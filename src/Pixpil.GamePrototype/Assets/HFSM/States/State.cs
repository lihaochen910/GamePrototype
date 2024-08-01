using System;

namespace Pixpil.AI.HFSM
{
	/// <summary>
	/// The "normal" state class that can run code on Enter, on Logic and on Exit,
	/// while also handling the timing of the next state transition.
	/// </summary>
	public class State<TStateId, TEvent> : ActionState<TStateId, TEvent>
	{
		private readonly Action<State<TStateId, TEvent>> _onEnter;
		private readonly Action<State<TStateId, TEvent>> _onLogic;
		private readonly Action<State<TStateId, TEvent>> _onExit;
		private readonly Func<State<TStateId, TEvent>, bool> _canExit;

		public readonly ITimer Timer;

		/// <summary>
		/// Initialises a new instance of the State class.
		/// </summary>
		/// <param name="onEnter">A function that is called when the state machine enters this state.</param>
		/// <param name="onLogic">A function that is called by the logic function of the state machine if this
		/// 	state is active.</param>
		/// <param name="onExit">A function that is called when the state machine exits this state.</param>
		/// <param name="canExit">(Only if needsExitTime is true):
		/// 	Function that determines if the state is ready to exit (true) or not (false).
		/// 	It is called OnExitRequest and on each logic step when a transition is pending.</param>
		/// <param name="needsExitTime">Determines if the state is allowed to instantly
		/// 	exit on a transition (false), or if the state machine should wait until the state is ready for a
		/// 	state change (true).</param>
		public State(
				Action<State<TStateId, TEvent>> onEnter = null,
				Action<State<TStateId, TEvent>> onLogic = null,
				Action<State<TStateId, TEvent>> onExit = null,
				Func<State<TStateId, TEvent>, bool> canExit = null,
				bool needsExitTime = false,
				bool isGhostState = false) : base(needsExitTime, isGhostState)
		{
			_onEnter = onEnter;
			_onLogic = onLogic;
			_onExit = onExit;
			_canExit = canExit;

			Timer = new Timer();
		}

		public override void OnEnter()
		{
			Timer.Reset();

			_onEnter?.Invoke(this);
		}

		public override void OnLogic()
		{
			_onLogic?.Invoke(this);

			// Check whether the state is ready to exit after calling onLogic, as it may trigger a transition.
			// Calling onLogic beforehand would lead to invalid behaviour as it would be called, even though this state
			// is not active anymore.
			if (NeedsExitTime && _canExit != null && Fsm.HasPendingTransition && _canExit(this))
			{
				Fsm.StateCanExit();
			}
		}

		public override void OnExit()
		{
			_onExit?.Invoke(this);
		}

		public override void OnExitRequest()
		{
			if (_canExit != null && _canExit(this))
			{
				Fsm.StateCanExit();
			}
		}
	}

	/// <inheritdoc />
	public class State<TStateId> : State<TStateId, string>
	{
		/// <inheritdoc />
		public State(
			Action<State<TStateId, string>> onEnter = null,
			Action<State<TStateId, string>> onLogic = null,
			Action<State<TStateId, string>> onExit = null,
			Func<State<TStateId, string>, bool> canExit = null,
			bool needsExitTime = false,
			bool isGhostState = false)
			: base(
				onEnter,
				onLogic,
				onExit,
				canExit,
				needsExitTime: needsExitTime,
				isGhostState: isGhostState)
		{
		}
	}

	/// <inheritdoc />
	public class State : State<string, string>
	{
		/// <inheritdoc />
		public State(
			Action<State<string, string>> onEnter = null,
			Action<State<string, string>> onLogic = null,
			Action<State<string, string>> onExit = null,
			Func<State<string, string>, bool> canExit = null,
			bool needsExitTime = false,
			bool isGhostState = false)
			: base(
				onEnter,
				onLogic,
				onExit,
				canExit,
				needsExitTime: needsExitTime,
				isGhostState: isGhostState)
		{
		}
	}
}
