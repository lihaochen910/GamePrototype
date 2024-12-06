using Bang;
using Bang.Entities;


namespace Pixpil.AI; 

public class GoapConditionAppraisal : IAppraisal {

	public readonly GoapCondition GoapCondition;
	
	// [Slider(-999, 999)]
	public readonly float ScoreWhenTrue = 1f;
	
	// [Slider(-999, 999)]
	public readonly float ScoreWhenFalse = 0f;

	public float GetScore( World world, Entity entity ) {
		if ( GoapCondition is null ) {
			return 0;
		}
		return GoapCondition.OnCheck( world, entity ) ? ScoreWhenTrue : ScoreWhenFalse;
	}
	
	public (float, float) GetPredictedScores() => ScoreWhenTrue > ScoreWhenFalse ? ( ScoreWhenFalse, ScoreWhenTrue ) : ( ScoreWhenTrue, ScoreWhenFalse );

    
}
