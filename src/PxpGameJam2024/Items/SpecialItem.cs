namespace PxpGameJam2024;

public abstract class SpecialItem : Item {
	
	public virtual void OnCollect( Character character ) {}
	public virtual void OnThowAway( Character character ) {}
	
}
