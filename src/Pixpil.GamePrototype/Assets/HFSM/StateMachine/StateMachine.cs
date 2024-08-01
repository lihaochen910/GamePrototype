using System;
using System.Collections.Generic;
using Pixpil.AI.HFSM.Exceptions;


/**
 * Hierarchical finite state machine for Unity
 * by Inspiaaa
 *
 * Version: 2.1.0
 */

namespace Pixpil.AI.HFSM
{
	/// <summary>
	/// A finite state machine that can also be used as a state of a parent state machine to create
	/// a hierarchy (-> hierarchical state machine).
	/// </summary>
	public class StateMachine<TOwnId, TStateId, TEvent> :
		StateBase<TOwnId>,
		ITriggerable<TEvent>,
		IStateMachine,
		IActionable<TEvent>
	{
		/// <summary>
		/// A bundle of a state together with the outgoing transitions and trigger transitions.
		/// It's useful, as you only need to do one Dictionary lookup for these three items.
		/// => Much better performance
		/// </summary>
		internal class StateBundle
		{
			// By default, these fields are all null and only get a value when you need them.
			// => Lazy evaluation => Memory efficient, when you only need a subset of features
			public StateBase<TStateId> state;
			public List<TransitionBase<TStateId>> transitions;
			public Dictionary<TEvent, List<TransitionBase<TStateId>>> triggerToTransitions;

			public TransitionBase<TStateId> AddTransition(TransitionBase<TStateId> t)
			{
				transitions = transitions ?? new List<TransitionBase<TStateId>>();
				transitions.Add(t);
				return t;
			}

			public TransitionBase<TStateId> AddTriggerTransition(TEvent trigger, TransitionBase<TStateId> transition)
			{
				triggerToTransitions = triggerToTransitions
					?? new Dictionary<TEvent, List<TransitionBase<TStateId>>>();

				List<TransitionBase<TStateId>> transitionsOfTrigger;

				if (!triggerToTransitions.TryGetValue(trigger, out transitionsOfTrigger))
				{
					transitionsOfTrigger = new List<TransitionBase<TStateId>>();
					triggerToTransitions.Add(trigger, transitionsOfTrigger);
				}

				transitionsOfTrigger.Add(transition);
				return transition;
			}
		}

		private struct PendingTransition
		{
			public TStateId targetState;

			public bool isExitTransition;

			// Optional (may be null), used for callbacks when the transition succeeds.
			public ITransitionListener listener;

			// As this type is not nullable (it is a value type), an additional field is required
			// to see if the pending transition has been set yet.
			public bool isPending;

			public static PendingTransition CreateForExit(ITransitionListener listener = null)
				=> new PendingTransition {
					targetState = default,
					isExitTransition = true,
					listener = listener,
					isPending = true
				};

			public static PendingTransition CreateForState(TStateId target, ITransitionListener listener = null)
				=> new PendingTransition {
					targetState = target,
					isExitTransition = false,
					listener = listener,
					isPending = true
				};
		}

		// A cached empty list of transitions (For improved readability, less GC).
		private static readonly List<TransitionBase<TStateId>> noTransitions = new (0);
		private static readonly Dictionary<TEvent, List<TransitionBase<TStateId>>> noTriggerTransitions = new (0);

		/// <summary>
		/// Event that is raised when the active state changes.
		/// </summary>
		/// <remarks>
		/// It is triggered when the state machine enters its initial state, and after a transition is performed.
		/// Note that it is not called when the state machine exits.
		/// </remarks>
		public event Action<StateBase<TStateId>> StateChanged;
		
		// from, to, transition taken
		public event Action<StateBase<TStateId>, StateBase<TStateId>, TransitionBase<TStateId>> StateChangedYetAnother;

		private (TStateId state, bool hasState) _startState = (default, false);
		private PendingTransition _pendingTransition = default;
		private bool _rememberLastState = false;

		// Central storage of states.
		private Dictionary<TStateId, StateBundle> _stateBundlesByName = new ();
		internal Dictionary< TStateId, StateBundle > StateBundles => _stateBundlesByName;

		protected StateBase<TStateId> _activeState = null;
		private List<TransitionBase<TStateId>> _activeTransitions = noTransitions;
		private Dictionary<TEvent, List<TransitionBase<TStateId>>> _activeTriggerTransitions = noTriggerTransitions;

		private List<TransitionBase<TStateId>> _transitionsFromAny = new ();
		private Dictionary<TEvent, List<TransitionBase<TStateId>>> _triggerTransitionsFromAny = new ();

		public StateBase<TStateId> ActiveState
		{
			get
			{
				EnsureIsInitializedFor("Trying to get the active state");
				return _activeState;
			}
		}
		public TStateId ActiveStateName => ActiveState.Name;

		public IStateMachine ParentFsm => Fsm;

		private bool IsRootFsm => Fsm == null;

		public bool HasPendingTransition => _pendingTransition.isPending;

		/// <summary>
		/// Initialises a new instance of the StateMachine class.
		/// </summary>
		/// <param name="needsExitTime">(Only for hierarchical states):
		/// 	Determines whether the state machine as a state of a parent state machine is allowed to instantly
		/// 	exit on a transition (false), or if it should wait until an explicit exit transition occurs.</param>
		/// <param name="rememberLastState">(Only for hierarchical states):
		/// 	If true, the state machine will return to its last active state when it enters, instead
		/// 	of to its original start state.</param>
		/// <inheritdoc cref="StateBase{T}(bool, bool)"/>
		public StateMachine(bool needsExitTime = false, bool isGhostState = false, bool rememberLastState = false)
			: base(needsExitTime: needsExitTime, isGhostState: isGhostState)
		{
			this._rememberLastState = rememberLastState;
		}

		/// <summary>
		/// Throws an exception if the state machine is not initialised yet.
		/// </summary>
		/// <param name="context">String message for which action the fsm should be initialised for.</param>
		protected void EnsureIsInitializedFor(string context)
		{
			if (_activeState == null)
				throw Common.NotInitialized(context);
		}

		/// <summary>
		/// Notifies the state machine that the state can cleanly exit,
		/// and if a state change is pending, it will execute it.
		/// </summary>
		public void StateCanExit()
		{
			if (!_pendingTransition.isPending)
				return;

			ITransitionListener listener = _pendingTransition.listener;
			if (_pendingTransition.isExitTransition)
			{
				_pendingTransition = default;

				listener?.BeforeTransition();
				PerformVerticalTransition();
				listener?.AfterTransition();
			}
			else
			{
				TStateId state = _pendingTransition.targetState;

				// When the pending state is a ghost state, ChangeState() will have
				// to try all outgoing transitions, which may overwrite the pendingState.
				// That's why it is first cleared, and not afterwards, as that would overwrite
				// a new, valid pending state.
				_pendingTransition = default;
				ChangeState(state, listener);
			}
		}

		/// <summary>
		/// Instantly changes to the target state.
		/// </summary>
		/// <param name="name">The name / identifier of the active state.</param>
		/// <param name="listener">Optional object that receives callbacks before and after changing state.</param>
		private void ChangeState(TStateId name, ITransitionListener listener = null)
		{
			listener?.BeforeTransition();
			_activeState?.OnExit();

			if (!_stateBundlesByName.TryGetValue(name, out var bundle) || bundle.state == null)
			{
				throw Common.StateNotFound(name.ToString(), context: "Switching states");
			}

			_activeTransitions = bundle.transitions ?? noTransitions;
			_activeTriggerTransitions = bundle.triggerToTransitions ?? noTriggerTransitions;

			var previousState = _activeState;
			_activeState = bundle.state;
			_activeState.OnEnter();

			for (int i = 0, count = _activeTransitions.Count; i < count; i++)
			{
				_activeTransitions[i].OnEnter();
			}

			foreach (List<TransitionBase<TStateId>> transitions in _activeTriggerTransitions.Values)
			{
				for (int i = 0, count = transitions.Count; i < count; i++)
				{
					transitions[i].OnEnter();
				}
			}

			listener?.AfterTransition();

			StateChanged?.Invoke(_activeState);
			
			#if DEBUG
			StateChangedYetAnother?.Invoke(previousState, _activeState, listener as TransitionBase<TStateId>);
			#endif

			if (_activeState is { IsGhostState: true })
			{
				TryAllDirectTransitions();
			}
		}

		/// <summary>
		/// Signals to the parent fsm that this fsm can exit which allows the parent
		/// fsm to transition to the next state.
		/// </summary>
		private void PerformVerticalTransition()
		{
			Fsm?.StateCanExit();
		}

		/// <summary>
		/// Requests a state change, respecting the <c>needsExitTime</c> property of the active state.
		/// </summary>
		/// <param name="name">The name / identifier of the target state.</param>
		/// <param name="forceInstantly">Overrides the needsExitTime of the active state if true,
		/// 	therefore forcing an immediate state change.</param>
		/// <param name="listener">Optional object that receives callbacks before and after the transition.</param>
		public void RequestStateChange(
			TStateId name,
			bool forceInstantly = false,
			ITransitionListener listener = null)
		{
			if (!_activeState.NeedsExitTime || forceInstantly)
			{
				_pendingTransition = default;
				ChangeState(name, listener);
			}
			else
			{
				_pendingTransition = PendingTransition.CreateForState(name, listener);
				_activeState.OnExitRequest();
				// If it can exit, the activeState would call
				// -> state.fsm.StateCanExit() which in turn would call
				// -> fsm.ChangeState(...)
			}
		}

		/// <summary>
		/// Requests a "vertical transition", allowing the state machine to exit
		/// to allow the parent fsm to transition to the next state. It respects the
		/// needsExitTime property of the active state.
		/// </summary>
		/// <param name="forceInstantly">Overrides the needsExitTime of the active state if true,
		/// 	therefore forcing an immediate state change.</param>
		/// <param name="listener">Optional object that receives callbacks before and after the transition.</param>
		public void RequestExit(bool forceInstantly = false, ITransitionListener listener = null)
		{
			if (!_activeState.NeedsExitTime || forceInstantly)
			{
				_pendingTransition = default;
				listener?.BeforeTransition();
				PerformVerticalTransition();
				listener?.AfterTransition();
			}
			else
			{
				_pendingTransition = PendingTransition.CreateForExit(listener);
				_activeState.OnExitRequest();
			}
		}

		/// <summary>
		/// Checks if a transition can take place, and if this is the case, transition to the
		/// "to" state and return true. Otherwise it returns false.
		/// </summary>
		private bool TryTransition(TransitionBase<TStateId> transition)
		{
			if (transition.IsExitTransition)
			{
				if (Fsm == null || !Fsm.HasPendingTransition || !transition.ShouldTransition())
					return false;

				RequestExit(transition.ForceInstantly, transition as ITransitionListener);
				return true;
			}
			else
			{
				if (!transition.ShouldTransition())
					return false;

				RequestStateChange(transition.To, transition.ForceInstantly, transition as ITransitionListener);
				return true;
			}
		}

		/// <summary>
		/// Tries the "global" transitions that can transition from any state.
		/// </summary>
		/// <returns>Returns true if a transition occurred.</returns>
		protected bool TryAllGlobalTransitions()
		{
			for (int i = 0, count = _transitionsFromAny.Count; i < count; i++)
			{
				TransitionBase<TStateId> transition = _transitionsFromAny[i];

				// Don't transition to the "to" state, if that state is already the active state.
				if (EqualityComparer<TStateId>.Default.Equals(transition.To, _activeState.Name))
					continue;

				if (TryTransition(transition))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Tries the "normal" transitions that transition from one specific state to another.
		/// </summary>
		/// <returns>Returns true if a transition occurred.</returns>
		protected bool TryAllDirectTransitions()
		{
			for (int i = 0, count = _activeTransitions.Count; i < count; i++)
			{
				TransitionBase<TStateId> transition = _activeTransitions[i];

				if (TryTransition(transition))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Calls OnEnter if it is the root state machine, therefore initialising the state machine.
		/// </summary>
		public override void Init()
		{
			if (!IsRootFsm) return;

			OnEnter();
		}

		/// <summary>
		/// Initialises the state machine and must be called before OnLogic is called.
		/// It sets the activeState to the selected startState.
		/// </summary>
		public override void OnEnter()
		{
			if (!_startState.hasState)
			{
				throw Common.MissingStartState(context: "Running OnEnter of the state machine.");
			}

			// Clear any previous pending transition from the last run.
			_pendingTransition = default;

			ChangeState(_startState.state);

			for (int i = 0, count = _transitionsFromAny.Count; i < count; i++)
			{
				_transitionsFromAny[i].OnEnter();
			}

			foreach (List<TransitionBase<TStateId>> transitions in _triggerTransitionsFromAny.Values)
			{
				for (int i = 0, count = transitions.Count; i < count; i++)
				{
					transitions[i].OnEnter();
				}
			}
		}

		/// <summary>
		/// Runs one logic step. It does at most one transition itself and
		/// calls the active state's logic function (after the state transition, if
		/// one occurred).
		/// </summary>
		public override void OnLogic()
		{
			EnsureIsInitializedFor("Running OnLogic");

			if (TryAllGlobalTransitions())
				goto runOnLogic;

			if (TryAllDirectTransitions())
				goto runOnLogic;

			runOnLogic:
			_activeState?.OnLogic();
		}

		public override void OnExit()
		{
			if (_activeState == null)
				return;

			if (_rememberLastState)
			{
				_startState = (_activeState.Name, true);
			}

			_activeState.OnExit();
			// By setting the activeState to null, the state's onExit method won't be called
			// a second time when the state machine enters again (and changes to the start state).
			_activeState = null;
		}

		public override void OnExitRequest()
		{
			if (_activeState.NeedsExitTime)
				_activeState.OnExitRequest();
		}

		/// <summary>
		/// Defines the entry point of the state machine.
		/// </summary>
		/// <param name="name">The name / identifier of the start state.</param>
		public void SetStartState(TStateId name)
		{
			_startState = (name, true);
		}

		/// <summary>
		/// Gets the StateBundle belonging to the <c>name</c> state "slot" if it exists.
		/// Otherwise it will create a new StateBundle, that will be added to the Dictionary,
		/// and return the newly created instance.
		/// </summary>
		private StateBundle GetOrCreateStateBundle(TStateId name)
		{
			StateBundle bundle;

			if (!_stateBundlesByName.TryGetValue(name, out bundle))
			{
				bundle = new StateBundle();
				_stateBundlesByName.Add(name, bundle);
			}

			return bundle;
		}

		/// <summary>
		/// Adds a new node / state to the state machine.
		/// </summary>
		/// <param name="name">The name / identifier of the new state.</param>
		/// <param name="state">The new state instance, e.g. <c>State</c>, <c>CoState</c>, <c>StateMachine</c>.</param>
		public StateBase<TStateId> AddState(TStateId name, StateBase<TStateId> state)
		{
			state.Fsm = this;
			state.Name = name;
			state.Init();

			StateBundle bundle = GetOrCreateStateBundle(name);
			bundle.state = state;

			if (_stateBundlesByName.Count == 1 && !_startState.hasState)
			{
				SetStartState(name);
			}

			return state;
		}

		/// <summary>
		/// Initialises a transition, i.e. sets its fsm attribute, and then calls its Init method.
		/// </summary>
		/// <param name="transition"></param>
		private void InitTransition(TransitionBase<TStateId> transition)
		{
			transition.Fsm = this;
			transition.Init();
		}

		/// <summary>
		/// Adds a new transition between two states.
		/// </summary>
		/// <param name="transition">The transition instance.</param>
		public void AddTransition(TransitionBase<TStateId> transition)
		{
			InitTransition(transition);

			StateBundle bundle = GetOrCreateStateBundle(transition.From);
			bundle.AddTransition(transition);
		}

		/// <summary>
		/// Adds a new transition that can happen from any possible state.
		/// </summary>
		/// <param name="transition">The transition instance; The "from" field can be
		/// 	left empty, as it has no meaning in this context.</param>
		public void AddTransitionFromAny(TransitionBase<TStateId> transition)
		{
			InitTransition(transition);

			_transitionsFromAny.Add(transition);
		}

		/// <summary>
		/// Adds a new trigger transition between two states that is only checked
		/// when the specified trigger is activated.
		/// </summary>
		/// <param name="trigger">The name / identifier of the trigger.</param>
		/// <param name="transition">The transition instance, e.g. Transition, TransitionAfter, ...</param>
		public void AddTriggerTransition(TEvent trigger, TransitionBase<TStateId> transition)
		{
			InitTransition(transition);

			StateBundle bundle = GetOrCreateStateBundle(transition.From);
			bundle.AddTriggerTransition(trigger, transition);
		}

		/// <summary>
		/// Adds a new trigger transition that can happen from any possible state, but is only
		/// checked when the specified trigger is activated.
		/// </summary>
		/// <param name="trigger">The name / identifier of the trigger</param>
		/// <param name="transition">The transition instance; The "from" field can be
		/// 	left empty, as it has no meaning in this context.</param>
		public void AddTriggerTransitionFromAny(TEvent trigger, TransitionBase<TStateId> transition)
		{
			InitTransition(transition);

			List<TransitionBase<TStateId>> transitionsOfTrigger;

			if (!_triggerTransitionsFromAny.TryGetValue(trigger, out transitionsOfTrigger))
			{
				transitionsOfTrigger = new List<TransitionBase<TStateId>>();
				_triggerTransitionsFromAny.Add(trigger, transitionsOfTrigger);
			}

			transitionsOfTrigger.Add(transition);
		}

		/// <summary>
		/// Adds two transitions:
		/// If the condition of the transition instance is true, it transitions from the "from"
		/// state to the "to" state. Otherwise it performs a transition in the opposite direction,
		/// i.e. from "to" to "from".
		/// </summary>
		/// <remarks>
		/// Internally the same transition instance will be used for both transitions
		/// by wrapping it in a ReverseTransition.
		/// For the reverse transition the afterTransition callback is called before the transition
		/// and the onTransition callback afterwards. If this is not desired then replicate the behaviour
		/// of the two way transitions by creating two separate transitions.
		/// </remarks>
		public void AddTwoWayTransition(TransitionBase<TStateId> transition)
		{
			InitTransition(transition);
			AddTransition(transition);

			ReverseTransition<TStateId> reverse = new ReverseTransition<TStateId>(transition, false);
			InitTransition(reverse);
			AddTransition(reverse);
		}

		/// <summary>
		/// Adds two transitions that are only checked when the specified trigger is activated:
		/// If the condition of the transition instance is true, it transitions from the "from"
		/// state to the "to" state. Otherwise it performs a transition in the opposite direction,
		/// i.e. from "to" to "from".
		/// </summary>
		/// <remarks>
		/// Internally the same transition instance will be used for both transitions
		/// by wrapping it in a ReverseTransition.
		/// For the reverse transition the afterTransition callback is called before the transition
		/// and the onTransition callback afterwards. If this is not desired then replicate the behaviour
		/// of the two way transitions by creating two separate transitions.
		/// </remarks>
		public void AddTwoWayTriggerTransition(TEvent trigger, TransitionBase<TStateId> transition)
		{
			InitTransition(transition);
			AddTriggerTransition(trigger, transition);

			ReverseTransition<TStateId> reverse = new ReverseTransition<TStateId>(transition, false);
			InitTransition(reverse);
			AddTriggerTransition(trigger, reverse);
		}

		/// <summary>
		/// Adds a new exit transition from a state. It represents an exit point that
		/// allows the fsm to exit and the parent fsm to continue to the next state.
		/// It is only checked if the parent fsm has a pending transition.
		/// </summary>
		/// <param name="transition">The transition instance. The "to" field can be
		/// 	left empty, as it has no meaning in this context.</param>
		public void AddExitTransition(TransitionBase<TStateId> transition)
		{
			transition.IsExitTransition = true;
			AddTransition(transition);
		}

		/// <summary>
		/// Adds a new exit transition that can happen from any possible state.
		/// It represents an exit point that allows the fsm to exit and the parent fsm to continue
		/// to the next state. It is only checked if the parent fsm has a pending transition.
		/// </summary>
		/// <param name="transition">The transition instance. The "from" and "to" fields can be
		/// 	left empty, as they have no meaning in this context.</param>
		public void AddExitTransitionFromAny(TransitionBase<TStateId> transition)
		{
			transition.IsExitTransition = true;
			AddTransitionFromAny(transition);
		}

		/// <summary>
		/// Adds a new exit transition from a state that is only checked when the specified trigger
		/// is activated.
		/// It represents an exit point that allows the fsm to exit and the parent fsm to continue
		/// to the next state. It is only checked if the parent fsm has a pending transition.
		/// </summary>
		/// <param name="transition">The transition instance. The "to" field can be
		/// 	left empty, as it has no meaning in this context.</param>
		public void AddExitTriggerTransition(TEvent trigger, TransitionBase<TStateId> transition)
		{
			transition.IsExitTransition = true;
			AddTriggerTransition(trigger, transition);
		}

		/// <summary>
		/// Adds a new exit transition that can happen from any possible state and is only checked
		/// when the specified trigger is activated.
		/// It represents an exit point that allows the fsm to exit and the parent fsm to continue
		/// to the next state. It is only checked if the parent fsm has a pending transition.
		/// </summary>
		/// <param name="transition">The transition instance. The "from" and "to" fields can be
		/// 	left empty, as they have no meaning in this context.</param>
		public void AddExitTriggerTransitionFromAny(TEvent trigger, TransitionBase<TStateId> transition)
		{
			transition.IsExitTransition = true;
			AddTriggerTransitionFromAny(trigger, transition);
		}

		/// <summary>
		/// Activates the specified trigger, checking all targeted trigger transitions to see whether
		/// a transition should occur.
		/// </summary>
		/// <param name="trigger">The name / identifier of the trigger.</param>
		/// <returns>True when a transition occurred, otherwise false.</returns>
		private bool TryTrigger(TEvent trigger)
		{
			EnsureIsInitializedFor("Checking all trigger transitions of the active state");

			List<TransitionBase<TStateId>> triggerTransitions;

			if (_triggerTransitionsFromAny.TryGetValue(trigger, out triggerTransitions))
			{
				for (int i = 0, count = triggerTransitions.Count; i < count; i++)
				{
					TransitionBase<TStateId> transition = triggerTransitions[i];

					if (EqualityComparer<TStateId>.Default.Equals(transition.To, _activeState.Name))
						continue;

					if (TryTransition(transition))
						return true;
				}
			}

			if (_activeTriggerTransitions.TryGetValue(trigger, out triggerTransitions))
			{
				for (int i = 0, count = triggerTransitions.Count; i < count; i++)
				{
					TransitionBase<TStateId> transition = triggerTransitions[i];

					if (TryTransition(transition))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Activates the specified trigger in all active states of the hierarchy, checking all targeted
		/// trigger transitions to see whether a transition should occur.
		/// </summary>
		/// <param name="trigger">The name / identifier of the trigger.</param>
		public void Trigger( TEvent trigger ) {
			// If a transition occurs, then the trigger should not be activated
			// in the new active state, that the state machine just switched to.
			if ( TryTrigger( trigger ) ) {
				return;
			}

			( _activeState as ITriggerable< TEvent > )?.Trigger( trigger );
		}

		/// <summary>
		/// Only activates the specified trigger locally in this state machine.
		/// </summary>
		/// <param name="trigger">The name / identifier of the trigger.</param>
		public void TriggerLocally(TEvent trigger)
		{
			TryTrigger(trigger);
		}

		/// <summary>
		/// Runs an action on the currently active state.
		/// </summary>
		/// <param name="trigger">Name of the action.</param>
		public virtual void OnAction(TEvent trigger)
		{
			EnsureIsInitializedFor("Running OnAction of the active state");
			(_activeState as IActionable<TEvent>)?.OnAction(trigger);
		}

		/// <summary>
		/// Runs an action on the currently active state and lets you pass one data parameter.
		/// </summary>
		/// <param name="trigger">Name of the action.</param>
		/// <param name="data">Any custom data for the parameter.</param>
		/// <typeparam name="TData">Type of the data parameter.
		/// 	Should match the data type of the action that was added via AddAction<T>(...).</typeparam>
		public virtual void OnAction<TData>(TEvent trigger, TData data)
		{
			EnsureIsInitializedFor("Running OnAction of the active state");
			(_activeState as IActionable<TEvent>)?.OnAction<TData>(trigger, data);
		}

		public StateBase<TStateId> GetState(TStateId name)
		{

			if (!_stateBundlesByName.TryGetValue(name, out StateBundle bundle) || bundle.state == null)
			{
				throw Common.StateNotFound(name.ToString(), context: "Getting a state");
			}

			return bundle.state;
		}

		public StateMachine<string, string, string> this[TStateId name]
		{
			get
			{
				StateBase<TStateId> state = GetState(name);
				StateMachine<string, string, string> subFsm = state as StateMachine<string, string, string>;

				if (subFsm == null)
				{
					throw Common.QuickIndexerMisusedForGettingState(name.ToString());
				}

				return subFsm;
			}
		}

		public override string GetActiveHierarchyPath()
		{
			if (_activeState == null)
			{
				// When the state machine is not active, then the active hierarchy path
				// is empty.
				return "";
			}

			return $"{Name}/{_activeState.GetActiveHierarchyPath()}";
		}
	}

	// Overloaded classes to allow for an easier usage of the StateMachine for common cases.
	// E.g. new StateMachine() instead of new StateMachine<string, string, string>()

	public class StateMachine<TStateId, TEvent> : StateMachine<TStateId, TStateId, TEvent>
	{
		public StateMachine(bool needsExitTime = false, bool isGhostState = false, bool rememberLastState = false)
			: base(needsExitTime: needsExitTime, isGhostState: isGhostState, rememberLastState: rememberLastState)
		{
		}
	}

	public class StateMachine<TStateId> : StateMachine<TStateId, TStateId, string>
	{
		public StateMachine(bool needsExitTime = false, bool isGhostState = false, bool rememberLastState = false)
			: base(needsExitTime: needsExitTime, isGhostState: isGhostState, rememberLastState: rememberLastState)
		{
		}
	}

	public class StateMachine : StateMachine<string, string, string>
	{
		public StateMachine(bool needsExitTime = false, bool isGhostState = false, bool rememberLastState = false)
			: base(needsExitTime: needsExitTime, isGhostState: isGhostState, rememberLastState: rememberLastState)
		{
		}
	}
	
}
