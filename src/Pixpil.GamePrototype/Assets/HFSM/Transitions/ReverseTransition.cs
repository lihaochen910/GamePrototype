namespace Pixpil.AI.HFSM
{
	/// <summary>
	/// A ReverseTransition wraps another transition, but reverses it. The "from"
	/// and "to" states are swapped. Only when the condition of the wrapped transition
	/// is false does it transition.
	/// The BeforeTransition and AfterTransition callbacks of the the wrapped transition
	/// are also swapped.
	/// </summary>
	public class ReverseTransition<TStateId> : TransitionBase<TStateId>
	{
		public readonly TransitionBase<TStateId> WrappedTransition;
		private readonly bool _shouldInitWrappedTransition;

		public ReverseTransition(
				TransitionBase<TStateId> wrappedTransition,
				bool shouldInitWrappedTransition = true)
			: base(
				from: wrappedTransition.To,
				to: wrappedTransition.From,
				forceInstantly: wrappedTransition.ForceInstantly)
		{
			WrappedTransition = wrappedTransition;
			_shouldInitWrappedTransition = shouldInitWrappedTransition;
		}

		public override void Init()
		{
			if (_shouldInitWrappedTransition)
			{
				WrappedTransition.Fsm = Fsm;
				WrappedTransition.Init();
			}
		}

		public override void OnEnter()
		{
			WrappedTransition.OnEnter();
		}

		public override bool ShouldTransition()
		{
			return !WrappedTransition.ShouldTransition();
		}

		public override void BeforeTransition()
		{
			WrappedTransition.AfterTransition();
		}

		public override void AfterTransition()
		{
			WrappedTransition.BeforeTransition();
		}
	}

	/// <inheritdoc />
	public class ReverseTransition : ReverseTransition<string>
	{
		/// <inheritdoc />
		public ReverseTransition(
			TransitionBase<string> wrappedTransition,
			bool shouldInitWrappedTransition = true)
			: base(wrappedTransition, shouldInitWrappedTransition)
		{
		}
	}
}
