namespace Pixpil.AI.HFSM;

/// <summary>
/// The base class of all transitions.
/// </summary>
public class TransitionBase<TStateId> : ITransitionListener {
	
	public readonly TStateId From;
	public readonly TStateId To;

	public readonly bool ForceInstantly;
	public bool IsExitTransition;

	public IStateMachine Fsm;

	/// <summary>
	/// Initialises a new instance of the TransitionBase class.
	/// </summary>
	/// <param name="from">The name / identifier of the active state.</param>
	/// <param name="to">The name / identifier of the next state.</param>
	/// <param name="forceInstantly">Ignores the needsExitTime of the active state if forceInstantly is true.
	/// 	=> Forces an instant transition</param>
	public TransitionBase( TStateId from, TStateId to, bool forceInstantly = false ) {
		From = from;
		To = to;
		ForceInstantly = forceInstantly;
		IsExitTransition = false;
	}

	/// <summary>
	/// Called to initialise the transition, after values like fsm have been set.
	/// </summary>
	public virtual void Init() {}

	/// <summary>
	/// Called when the state machine enters the "from" state.
	/// </summary>
	public virtual void OnEnter() {}

	/// <summary>
	/// Called to determine whether the state machine should transition to the <c>to</c> state.
	/// </summary>
	/// <returns>True if the state machine should change states / transition.</returns>
	public virtual bool ShouldTransition() => true;

	/// <summary>
	/// Callback method that is called just before the transition happens.
	/// </summary>
	public virtual void BeforeTransition() {}

	/// <summary>
	/// Callback method that is called just after the transition happens.
	/// </summary>
	public virtual void AfterTransition() {}

}


/// <inheritdoc />
public class TransitionBase : TransitionBase< string > {
	
	/// <inheritdoc />
	public TransitionBase( string @from, string to, bool forceInstantly = false ) : base( @from, to, forceInstantly ) {}

}

