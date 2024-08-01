using System;

namespace Pixpil.AI.HFSM
{
	/// <summary>
	/// A class that allows you to run additional functions (companion code)
	/// before and after the wrapped state's code.
	/// It does not interfere with the wrapped state's timing / needsExitTime / ... behaviour.
	/// </summary>
	public class StateWrapper<TStateId, TEvent>
	{
		public class WrappedState : StateBase<TStateId>, ITriggerable<TEvent>, IActionable<TEvent>
		{
			private Action<StateBase<TStateId>>
				_beforeOnEnter,
				_afterOnEnter,

				_beforeOnLogic,
				_afterOnLogic,

				_beforeOnExit,
				_afterOnExit;

			private StateBase<TStateId> _state;

			public WrappedState(
					StateBase<TStateId> state,

					Action<StateBase<TStateId>> beforeOnEnter = null,
					Action<StateBase<TStateId>> afterOnEnter = null,

					Action<StateBase<TStateId>> beforeOnLogic = null,
					Action<StateBase<TStateId>> afterOnLogic = null,

					Action<StateBase<TStateId>> beforeOnExit = null,
					Action<StateBase<TStateId>> afterOnExit = null) : base(state.NeedsExitTime, state.IsGhostState)
			{
				this._state = state;

				this._beforeOnEnter = beforeOnEnter;
				this._afterOnEnter = afterOnEnter;

				this._beforeOnLogic = beforeOnLogic;
				this._afterOnLogic = afterOnLogic;

				this._beforeOnExit = beforeOnExit;
				this._afterOnExit = afterOnExit;
			}

			public override void Init()
			{
				_state.Name = Name;
				_state.Fsm = Fsm;

				_state.Init();
			}

			public override void OnEnter()
			{
				_beforeOnEnter?.Invoke(this);
				_state.OnEnter();
				_afterOnEnter?.Invoke(this);
			}

			public override void OnLogic()
			{
				_beforeOnLogic?.Invoke(this);
				_state.OnLogic();
				_afterOnLogic?.Invoke(this);
			}

			public override void OnExit()
			{
				_beforeOnExit?.Invoke(this);
				_state.OnExit();
				_afterOnExit?.Invoke(this);
			}

			public override void OnExitRequest()
			{
				_state.OnExitRequest();
			}

			public void Trigger(TEvent trigger)
			{
				(_state as ITriggerable<TEvent>)?.Trigger(trigger);
			}

			public void OnAction(TEvent trigger)
			{
				(_state as IActionable<TEvent>)?.OnAction(trigger);
			}

			public void OnAction<TData>(TEvent trigger, TData data)
			{
				(_state as IActionable<TEvent>)?.OnAction<TData>(trigger, data);
			}

			public override string GetActiveHierarchyPath()
			{
				return _state.GetActiveHierarchyPath();
			}
		}

		private Action<StateBase<TStateId>>
			beforeOnEnter,
			afterOnEnter,

			beforeOnLogic,
			afterOnLogic,

			beforeOnExit,
			afterOnExit;

		/// <summary>
		/// Initialises a new instance of the StateWrapper class
		/// </summary>
		public StateWrapper(
				Action<StateBase<TStateId>> beforeOnEnter = null,
				Action<StateBase<TStateId>> afterOnEnter = null,

				Action<StateBase<TStateId>> beforeOnLogic = null,
				Action<StateBase<TStateId>> afterOnLogic = null,

				Action<StateBase<TStateId>> beforeOnExit = null,
				Action<StateBase<TStateId>> afterOnExit = null)
		{
			this.beforeOnEnter = beforeOnEnter;
			this.afterOnEnter = afterOnEnter;

			this.beforeOnLogic = beforeOnLogic;
			this.afterOnLogic = afterOnLogic;

			this.beforeOnExit = beforeOnExit;
			this.afterOnExit = afterOnExit;
		}

		public WrappedState Wrap(StateBase<TStateId> state)
		{
			return new WrappedState(
				state,
				beforeOnEnter,
				afterOnEnter,
				beforeOnLogic,
				afterOnLogic,
				beforeOnExit,
				afterOnExit
			);
		}
	}

	/// <inheritdoc />
	public class StateWrapper : StateWrapper<string, string>
	{
		public StateWrapper(
			Action<StateBase<string>> beforeOnEnter = null,
			Action<StateBase<string>> afterOnEnter = null,

			Action<StateBase<string>> beforeOnLogic = null,
			Action<StateBase<string>> afterOnLogic = null,

			Action<StateBase<string>> beforeOnExit = null,
			Action<StateBase<string>> afterOnExit = null) : base(
			beforeOnEnter, afterOnEnter,
			beforeOnLogic, afterOnLogic,
			beforeOnExit, afterOnExit)
		{
		}
	}
}
