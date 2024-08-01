using System;

namespace Pixpil.AI.HFSM
{
	/// <summary>
	/// A class used to determine whether the state machine should transition to another state.
	/// </summary>
	public class Transition<TStateId> : TransitionBase<TStateId>
	{
		public readonly Func<Transition<TStateId>, bool> Condition;
		public readonly Action<Transition<TStateId>> BeforeTransitionAction;
		public readonly Action<Transition<TStateId>> AfterTransitionAction;

		/// <summary>
		/// Initialises a new instance of the Transition class.
		/// </summary>
		/// <param name="condition">A function that returns true if the state machine
		/// 	should transition to the <c>to</c> state.</param>
		/// <param name="onTransition">Callback function that is called just before the transition happens.</param>
		/// <param name="afterTransitionAction">Callback function that is called just after the transition happens.</param>
		/// <inheritdoc cref="TransitionBase{TStateId}(TStateId, TStateId, bool)" />
		public Transition(
				TStateId from,
				TStateId to,
				Func<Transition<TStateId>, bool> condition = null,
				Action<Transition<TStateId>> onTransition = null,
				Action<Transition<TStateId>> afterTransitionAction = null,
				bool forceInstantly = false) : base(from, to, forceInstantly)
		{
			Condition = condition;
			BeforeTransitionAction = onTransition;
			AfterTransitionAction = afterTransitionAction;
		}

		public override bool ShouldTransition() {
			if ( Condition == null ) {
				return true;
			}

			return Condition( this );
		}

		public override void BeforeTransition() => BeforeTransitionAction?.Invoke( this );
		public override void AfterTransition() => AfterTransitionAction?.Invoke( this );
	}

	/// <inheritdoc />
	public class Transition : Transition<string>
	{
		/// <inheritdoc />
		public Transition(
			string @from,
			string to,
			Func<Transition<string>, bool> condition = null,
			Action<Transition<string>> onTransition = null,
			Action<Transition<string>> afterTransitionAction = null,
			bool forceInstantly = false) : base(
				@from,
				to,
				condition,
				onTransition: onTransition,
				afterTransitionAction: afterTransitionAction,
				forceInstantly: forceInstantly)
		{
		}
	}
}
