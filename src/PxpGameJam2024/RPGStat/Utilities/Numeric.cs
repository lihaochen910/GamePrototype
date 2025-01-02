namespace Pixpil.RPGStatSystem;

public static class Numeric {
	
	// Epsilon values from other projects: EpsilonF = 1e-7; EpsilonD = 9e-16;
	// According to our unit tests the double epsilon is to small. 
	// Following epsilon values were appropriate for typical game applications and 3D simulations.
	private static float _epsilonF = 1e-5f;
	private static float _epsilonFSquared = 1e-5f * 1e-5f;
	private static double _epsilonD = 1e-12;
	private static double _epsilonDSquared = 1e-12 * 1e-12;

	public static bool AreEqual( float value1, float value2 ) {
		// Infinity values have to be handled carefully because the check with the epsilon tolerance
		// does not work there. Check for equality in case they are infinite:
		if ( value1 == value2 ) return true;

		// Scale epsilon proportional the given values.
		float epsilon = _epsilonF * ( Math.Abs( value1 ) + Math.Abs( value2 ) + 1.0f );
		float delta = value1 - value2;
		return -epsilon < delta && delta < epsilon;

		// We could also use ... Abs(v1 - v2) <= _epsilonF * Max(Abs(v1), Abs(v2), 1)
	}
	
	
	public static bool IsZero(float value)
	{
		return (-_epsilonF < value) && (value < _epsilonF);
	}

}
