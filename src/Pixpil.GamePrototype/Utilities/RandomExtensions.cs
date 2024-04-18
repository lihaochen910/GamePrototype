using System;
using System.Numerics;
using Murder.Utilities;


namespace Pixpil.Utilities; 

public static class RandomExtensions {

	public static Vector2 RandomPointInUnitCircle( this Random r ) {
		return Vector2Helper.FromAngle( r.NextFloat( 0f, 360f ) ) * ( float )Math.Sqrt( r.NextFloat( 0.0f, 1f ) );
	}

}
