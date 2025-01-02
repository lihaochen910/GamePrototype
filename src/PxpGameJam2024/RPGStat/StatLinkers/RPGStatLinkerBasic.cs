namespace Pixpil.RPGStatSystem {

	/// <summary>
	/// Basic implementation of a RPGStatLinker. Returns a percentage 
	/// of the Linked Stat
	/// </summary>
	public class RPGStatLinkerBasic : RPGStatLinker {
		
		/// <summary>
		/// The Ratio of the linked stat to use
		/// </summary>
		private readonly float _ratio;

		/// <summary>
		/// Gets the Ratio value. Read Only
		/// </summary>
		public float Ratio => _ratio;

		/// <summary>
		/// returns the ratio of the linked stat as the linker's value
		/// </summary>
		public override float GetValue() => LinkedStat.StatValue * _ratio;


		public RPGStatLinkerBasic( float ratio ) {
			_ratio = ratio;
		}

		public RPGStatLinkerBasic( float ratio, RPGStat linkedStat ) : base( linkedStat ) {
			_ratio = ratio;
		}
	}
	
}
