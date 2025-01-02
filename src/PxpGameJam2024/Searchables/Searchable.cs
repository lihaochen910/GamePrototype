namespace PxpGameJam2024;

public abstract class Searchable {

	public abstract string Name { get; }
	public virtual string Description => string.Empty;

	public abstract Task Collect( Character character );
	
}
