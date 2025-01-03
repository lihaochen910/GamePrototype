namespace Pixpil.RPGStatSystem {

	/// <summary>
	/// Modifier that takes a percentage of the stat's value
	/// </summary>
	[System.Serializable]
	public class RPGStatModBasePercent : RPGStatModifier {
		
		/// <summary>
		/// The order in which the modifier is applied to the stat
		/// </summary>
		public override int Order => 1;

		/// <summary>
		/// Calculates the amount to apply to the stat based off the 
		/// sum of all the stat modifier's value and the current value of
		/// the stat.
		/// </summary>
		public override float ApplyModifier( float statValue, float modValue ) => statValue * modValue;
		
		public RPGStatModBasePercent() : base( 1f ) {}

		/// <summary>
		/// Constructor that sets the Value and sets Stacks to true
		/// </summary>
		public RPGStatModBasePercent( float value ) : base( value ) {}

		/// <summary>
		/// Constructor that sets the Value and Stacks
		/// </summary>
		public RPGStatModBasePercent( float value, bool stacks ) : base( value, stacks ) {}
	}
	
}
