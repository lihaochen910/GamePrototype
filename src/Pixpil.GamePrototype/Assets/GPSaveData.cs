using Murder.Assets;
using Pixpil.Core;


namespace Pixpil.Assets;

public class GPSaveData( int slot, float version ) : SaveData( slot, version ) {
	
	private GameplayBlackboard? _gameplayBlackboard;

	public DayProgress DayProgress;
	public bool TheDayNearDuskMessageFired;

	// /// <summary>
	// /// Next game level world which will be triggered from here.
	// /// </summary>
	// [GameAssetId<DayCycleAsset>]
	// public Guid? NextLevel = null;
	//
	// public List<Modifier> Modifiers = new();
	//
	// /// <summary>
	// /// Next level cutscenes that will be triggered, in order.
	// /// </summary>
	// public SituationComponent? NextLevelCutscene = null;
	//
	// public int Health = 8;
	//
	// public float TraveledDistance;
	// public int Day;
	//
	// /// <summary>
	// /// Tinder id matched so we can refresh our pool.
	// /// </summary>
	// public int TinderIdMatched = -1;
	// internal Vector2 SwayDirection;
	// internal bool HasSway;
	// internal bool Sleepy;
	// internal int MessagesSent = 0;

	/// <summary>
	/// Retrieves the gameplay blackboard.
	/// This should be used on a *readonly setting*.
	/// </summary>
	internal GameplayBlackboard GameplayBlackboard =>
		_gameplayBlackboard ??= ( GameplayBlackboard )BlackboardTracker.FindBlackboard( null, guid: null )!.Blackboard;

}
