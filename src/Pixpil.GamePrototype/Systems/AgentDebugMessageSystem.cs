using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.StateMachines;
using Bang.Systems;
using Murder.Core.Graphics;
using Murder.Services;
using Murder.Utilities;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.Systems;

[Watch( typeof( AgentDebugMessageComponent ) )]
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( AgentDebugMessageComponent ) )]
internal class AgentDebugMessageSystem : IReactiveSystem, IMurderRenderSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			var debugMsgComp = entity.GetAgentDebugMessage();
			if ( debugMsgComp.DestroyDelay >= 0f ) {
				world.RunCoroutine( ClearComponentAfter( debugMsgComp.DestroyDelay, entity ) );
				// entity.SetDestroyAfterSeconds( new DestroyAfterSecondsComponent { Lifespan = debugMsgComp.DestroyDelay } );
			}
		}
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		
	}

	public void Draw( RenderContext render, Context context ) {
		foreach ( var entity in context.Entities ) {
			if ( entity.TryGetInCamera() is null ) {
				continue;
			}

			var agentDebugMessage = entity.GetAgentDebugMessage();
			render.UiBatch.DrawText( PPFonts.FusionPixel, $"{agentDebugMessage.Msg}", render.Camera.WorldToScreenPosition( entity.GetPosition().ToVector2() ), new DrawInfo( 0.2f ) {
				Scale = Vector2.One * agentDebugMessage.Size,
				Color = Color.Green
			} );
		}
	}


	private IEnumerator< Wait > ClearComponentAfter( float seconds, Entity entity ) {
		yield return Wait.ForSeconds( seconds );
		entity.RemoveAgentDebugMessage();
	}
}
