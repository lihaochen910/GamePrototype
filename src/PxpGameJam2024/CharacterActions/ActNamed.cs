namespace PxpGameJam2024;

public class ActNamed : CharacterAction {

	public override string Name => _actName;

	private readonly string _actName;

	public ActNamed( string actName ) {
		_actName = actName;
	}
	
	public override async Task Do( Character character ) {
		
	}
	
}
