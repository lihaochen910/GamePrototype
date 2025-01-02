namespace PxpGameJam2024;

public abstract class Item {

	public abstract string Name { get; }
	public virtual string Description => string.Empty;

	public virtual bool CanUse( Character character ) => true;
	public abstract Task< bool > Use( Character character );

}
