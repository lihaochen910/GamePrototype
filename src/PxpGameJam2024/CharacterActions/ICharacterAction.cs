namespace PxpGameJam2024;

public interface ICharacterAction {

	virtual bool CheckAvailable( Character character ) => true;
	Task Do( Character character );

}


public abstract class CharacterAction : ICharacterAction {

	public abstract string Name { get; }
	public virtual string Description => string.Empty;
	// public abstract int Cost { get; } // 消耗回合数

	// public bool IsNoConsumption => Cost < 1;

	public virtual bool CheckAvailable( Character character ) => true;
	
	public abstract Task Do( Character character );

}
