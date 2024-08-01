using System;
using System.Collections.Generic;

namespace Pixpil.AI.HFSM
{
	/// <summary>
	/// A state that can run multiple states in parallel.
	/// </summary>
	/// <remarks>
	/// If needsExitTime is set to true, it will exit when *any* one of the child states calls StateCanExit()
	/// on this class. Note that having multiple child states that all do not need exit time and hence don't
	/// call the StateCanExit() method, will mean that this state will never exit.
	/// This behaviour can be overridden by specifying a canExit function that determines when this state may exit.
	/// This will ignore the needsExitTime and StateCanExit() calls of the child states. It works the same as the
	/// canExit feature of the State class.
	/// </remarks>
	public class ParallelStates<TOwnId, TStateId, TEvent> : StateBase<TOwnId>, IActionable<TEvent>, IStateMachine
	{
		private readonly List<StateBase<TStateId>> _states = new ();

		// When the states are passed in via the constructor, they are not assigned names / identifiers.
		// This means that the active hierarchy path cannot include them (which would be used for debugging purposes).
		private bool _areStatesNameless = false;

		// This variable keeps track whether this state is currently active. It is used to prevent
		// StateCanExit() calls from the child states to be passed on to the parent state machine
		// when this state is no longer active, which would result in unwanted behaviour
		// (e.g. two transitions).
		private bool _isActive;

		private Func<ParallelStates<TOwnId, TStateId, TEvent>, bool> _canExit;

		public bool HasPendingTransition => Fsm.HasPendingTransition;
		public IStateMachine ParentFsm => Fsm;

		/// <inheritdoc cref="ParallelStates{TOwnId,TStateId,TEvent}"/>
		public ParallelStates(
			Func<ParallelStates<TOwnId, TStateId, TEvent>, bool> canExit = null,
			bool needsExitTime = false,
			bool isGhostState = false) : base(needsExitTime, isGhostState)
		{
			_canExit = canExit;
		}

		/// <inheritdoc cref="ParallelStates{TOwnId,TStateId,TEvent}"/>
		public ParallelStates(params StateBase<TStateId>[] states)
			: this(null, false, false, states) { }

		/// <inheritdoc cref="ParallelStates{TOwnId,TStateId,TEvent}"/>
		public ParallelStates(bool needsExitTime, params StateBase<TStateId>[] states)
			: this(null, needsExitTime, false, states) { }

		/// <inheritdoc cref="ParallelStates{TOwnId,TStateId,TEvent}"/>
		public ParallelStates(
			Func<ParallelStates<TOwnId, TStateId, TEvent>, bool> canExit,
			bool needsExitTime,
			params StateBase<TStateId>[] states) : this(canExit, needsExitTime, false, states) { }

		/// <summary>
		///	Initialises a new instance of the ParallelStates class.
		/// </summary>
		/// <param name="canExit">(Only if needsExitTime is true):
		/// 	Function that determines if the state is ready to exit (true) or not (false).
		/// 	It is called OnExitRequest and on each logic step when a transition is pending.</param>
		/// <param name="states">States to run in parallel. Note that they are not assigned names / identifiers
		/// 	and will therefore not be included in the active hierarchy path. If this is unwanted,
		/// 	add the states using AddState() instead.</param>
		/// <inheritdoc cref="StateBase{T}(bool, bool)"/>
		public ParallelStates(
			Func<ParallelStates<TOwnId, TStateId, TEvent>, bool> canExit,
			bool needsExitTime,
			bool isGhostState,
			params StateBase<TStateId>[] states) : base(needsExitTime, isGhostState)
		{
			_canExit = canExit;
			_areStatesNameless = true;

			foreach (var state in states)
			{
				AddState(default, state);
			}
		}

		/// <summary>
		/// Adds a new state that is run in parallel while this state is active.
		/// </summary>
		/// <param name="id">Name / identifier of the state. This is only used for debugging purposes.</param>
		/// <param name="state">State to add.</param>
		/// <returns>Itself to allow for a fluent interface.</returns>
		public ParallelStates<TOwnId, TStateId, TEvent> AddState(TStateId id, StateBase<TStateId> state)
		{
			state.Fsm = this;
			state.Name = id;
			state.Init();

			_states.Add(state);

			// Fluent interface.
			return this;
		}

		public override void Init()
		{
			foreach (var state in _states)
			{
				state.Fsm = this;
			}
		}

		public override void OnEnter()
		{
			_isActive = true;

			foreach (var state in _states)
			{
				state.OnEnter();
			}
		}

		public override void OnLogic()
		{
			foreach (var state in _states)
			{
				state.OnLogic();
			}

			if (NeedsExitTime && _canExit != null && Fsm.HasPendingTransition && _canExit(this))
			{
				Fsm.StateCanExit();
			}
		}

		public override void OnExit()
		{
			_isActive = false;

			foreach (var state in _states)
			{
				state.OnExit();
			}
		}

		public override void OnExitRequest()
		{
			// When this state machine is requested to exit, check each child state to see if any one is
			// ready to exit. This behaviour can be overridden by providing a canExit function.
			if (_canExit == null)
			{
				foreach (var state in _states)
				{
					state.OnExitRequest();
				}
			}
			else
			{
				if (Fsm.HasPendingTransition && _canExit(this))
				{
					Fsm.StateCanExit();
				}
			}
		}

		public void OnAction(TEvent trigger)
		{
			foreach (var state in _states)
			{
				(state as IActionable<TEvent>)?.OnAction(trigger);
			}
		}

		public void OnAction<TData>(TEvent trigger, TData data)
		{
			foreach (var state in _states)
			{
				(state as IActionable<TEvent>)?.OnAction(trigger, data);
			}
		}

		public void StateCanExit()
		{
			// Try to exit as soon as any one of the child states can exit, unless the exit behaviour
			// is overridden by canExit.
			if (_isActive && _canExit == null)
			{
				Fsm.StateCanExit();
			}
		}

		public override string GetActiveHierarchyPath()
		{
			// The name could be null when ParallelStates is used at the top level.
			string stringName = this.Name?.ToString() ?? "";

			if (_areStatesNameless || _states.Count == 0)
			{
				// Example path: "Parallel"
				return stringName;
			}

			if (_states.Count == 1)
			{
				// Example path: "Parallel/Move"
				return stringName + "/" + _states[0].GetActiveHierarchyPath();
			}

			// Example path: "Parallel/(Move & Attack/Shoot)"
			string path = stringName + "/(";

			for (int i = 0; i < _states.Count; i++)
			{
				path += _states[i].GetActiveHierarchyPath();
				if (i < _states.Count - 1)
				{
					path += " & ";
				}
			}

			return path + ")";
		}
	}

	/// <inheritdoc />
	public class ParallelStates<TStateId, TEvent> : ParallelStates<TStateId, TStateId, TEvent>
	{
		/// <inheritdoc />
		public ParallelStates(
			Func<ParallelStates<TStateId, TStateId, TEvent>, bool> canExit = null,
			bool needsExitTime = false,
			bool isGhostState = false) : base(canExit, needsExitTime, isGhostState) { }

		/// <inheritdoc />
		public ParallelStates(params StateBase<TStateId>[] states)
			: base(null, false, false, states) { }

		/// <inheritdoc />
		public ParallelStates(bool needsExitTime, params StateBase<TStateId>[] states)
			: base(null, needsExitTime, false, states) { }

		/// <inheritdoc />
		public ParallelStates(
			Func<ParallelStates<TStateId, TStateId, TEvent>, bool> canExit,
			bool needsExitTime,
			params StateBase<TStateId>[] states) : base(canExit, needsExitTime, false, states) { }

		/// <inheritdoc />
		public ParallelStates(
			Func<ParallelStates<TStateId, TStateId, TEvent>, bool> canExit,
			bool needsExitTime,
			bool isGhostState,
			params StateBase<TStateId>[] states) : base(canExit, needsExitTime, isGhostState, states) { }
	}

	public class ParallelStates<TStateId> : ParallelStates<TStateId, TStateId, string>
	{
		/// <inheritdoc />
		public ParallelStates(
			Func<ParallelStates<TStateId, TStateId, string>, bool> canExit = null,
			bool needsExitTime = false,
			bool isGhostState = false) : base(canExit, needsExitTime, isGhostState) { }

		/// <inheritdoc />
		public ParallelStates(params StateBase<TStateId>[] states)
			: base(null, false, false, states) { }

		/// <inheritdoc />
		public ParallelStates(bool needsExitTime, params StateBase<TStateId>[] states)
			: base(null, needsExitTime, false, states) { }

		/// <inheritdoc />
		public ParallelStates(
			Func<ParallelStates<TStateId, TStateId, string>, bool> canExit,
			bool needsExitTime,
			params StateBase<TStateId>[] states) : base(canExit, needsExitTime, false, states) { }

		/// <inheritdoc />
		public ParallelStates(
			Func<ParallelStates<TStateId, TStateId, string>, bool> canExit,
			bool needsExitTime,
			bool isGhostState,
			params StateBase<TStateId>[] states) : base(canExit, needsExitTime, isGhostState, states) { }
	}

	public class ParallelStates : ParallelStates<string, string, string>
	{
		/// <inheritdoc />
		public ParallelStates(
			Func<ParallelStates<string, string, string>, bool> canExit = null,
			bool needsExitTime = false,
			bool isGhostState = false) : base(canExit, needsExitTime, isGhostState) { }

		/// <inheritdoc />
		public ParallelStates(params StateBase<string>[] states)
			: this(null, false, false, states) { }

		/// <inheritdoc />
		public ParallelStates(bool needsExitTime, params StateBase<string>[] states)
			: this(null, needsExitTime, false, states) { }

		/// <inheritdoc />
		public ParallelStates(
			Func<ParallelStates<string, string, string>, bool> canExit,
			bool needsExitTime,
			params StateBase<string>[] states) : this(canExit, needsExitTime, false, states) { }

		/// <param name="states">States to run in parallel. They are implicitly assigned names
		/// 	based on their indices (e.g. the first state has the name "0", ...) which is
		/// 	useful for debugging.</param>
		/// <inheritdoc />
		public ParallelStates(
			Func<ParallelStates<string, string, string>, bool> canExit,
			bool needsExitTime,
			bool isGhostState,
			params StateBase<string>[] states) : base(canExit, needsExitTime, isGhostState)
		{
			for (int i = 0; i < states.Length; i++)
			{
				AddState(i.ToString(), states[i]);
			}
		}
	}
}