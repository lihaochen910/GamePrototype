using System;

namespace Pixpil.AI.HFSM
{
	/// <summary>
	/// A class used to determine whether the state machine should transition to another state
	/// depending on a delay and an optional condition.
	/// </summary>
	public class TransitionAfter<TStateId> : TransitionBase<TStateId>
	{
		public readonly float Delay;
		public readonly ITimer Timer;

		public readonly Func<TransitionAfter<TStateId>, bool> Condition;

		public readonly Action<TransitionAfter<TStateId>> BeforeTransitionAction;
		public readonly Action<TransitionAfter<TStateId>> AfterTransitionAction;

		/// <summary>
		/// Initialises a new instance of the TransitionAfter class.
		/// </summary>
		/// <param name="delay">The delay that must elapse before the transition can occur</param>
		/// <param name="condition">A function that returns true if the state machine
		/// 	should transition to the <c>to</c> state.
		/// 	It is only called after the delay has elapsed and is optional.</param>
		/// <inheritdoc cref="Transition{TStateId}
		/// 	Action{Transition{TStateId}}, Action{Transition{TStateId}}, bool)" />
		public TransitionAfter(
				TStateId from,
				TStateId to,
				float delay,
				Func<TransitionAfter<TStateId>, bool> condition = null,
				Action<TransitionAfter<TStateId>> onTransitionAction = null,
				Action<TransitionAfter<TStateId>> afterTransitionAction = null,
				bool forceInstantly = false) : base(from, to, forceInstantly)
		{
			Delay = delay;
			Condition = condition;
			BeforeTransitionAction = onTransitionAction;
			AfterTransitionAction = afterTransitionAction;
			Timer = new Timer();
		}

		public override void OnEnter()
		{
			Timer.Reset();
		}

		public override bool ShouldTransition() {
			if ( Timer.Elapsed < Delay ) {
				return false;
			}

			if ( Condition is null ) {
				return true;
			}

			return Condition( this );
		}

		public override void BeforeTransition() => BeforeTransitionAction?.Invoke( this );
		public override void AfterTransition() => AfterTransitionAction?.Invoke( this );
	}

	/// <inheritdoc />
	public class TransitionAfter : TransitionAfter<string>
	{
		/// <inheritdoc />
		public TransitionAfter(
			string @from,
			string to,
			float delay,
			Func<TransitionAfter<string>, bool> condition = null,
			Action<TransitionAfter<string>> onTransitionAction = null,
			Action<TransitionAfter<string>> afterTransitionAction = null,
			bool forceInstantly = false) : base(
				@from,
				to,
				delay,
				condition,
				onTransitionAction: onTransitionAction,
				afterTransitionAction: afterTransitionAction,
				forceInstantly: forceInstantly)
		{
		}
	}
}
