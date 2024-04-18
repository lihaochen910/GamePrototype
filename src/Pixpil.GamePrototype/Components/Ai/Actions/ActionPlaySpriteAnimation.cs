using System.Collections.Immutable;
using System.Linq;
using Bang;
using Bang.Entities;
using DigitalRune.Linq;


namespace Pixpil.AI.Actions; 

public class ActionPlaySpriteAnimation : AiAction {

	public readonly ImmutableArray< string > Animations = ImmutableArray< string >.Empty;
	public readonly bool FindSpriteInChild;
	
	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
		var spriteComponent = entity.TryGetSprite();
		if ( spriteComponent is null && FindSpriteInChild ) {
			var children = TreeHelper.GetDescendants( entity, entity => {
				return entity.Children.Select( childId => world.GetEntity( childId ) );
			} );
			foreach ( var child in children ) {
				if ( child.HasSprite() ) {
					spriteComponent = child.GetSprite();
					break;
				}
			}
		}

		if ( spriteComponent is null ) {
			return AiActionExecuteStatus.Failure;
		}
		
		spriteComponent.Value.Play( Animations );
		return base.OnPreExecute( world, entity );
	}
	
}
