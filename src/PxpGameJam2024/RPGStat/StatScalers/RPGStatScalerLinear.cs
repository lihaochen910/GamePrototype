using System;


namespace Pixpil.RPGStatSystem {

	public class RPGStatScalerLinear : RPGStatScaler {
		
		private float _slope = 0f;
		public float _offset = 0f;

		public RPGStatScalerLinear( float slope, float offset ) {
			_slope = slope;
			_offset = offset;
		}

		public override float GetValue( int level ) {
			return ( int )Math.Round( _slope * ( level - 1 ) + _offset );
		}

	}
	
}
