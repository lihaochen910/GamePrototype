using System.Collections.Immutable;
using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder.Components;
using Murder.Core.Graphics;
using Murder.Services;
using Murder.Utilities;
using Pixpil.Components;


namespace Pixpil.Systems.Building; 

[Watch( typeof( BuildingComponent ) )]
public class LightHouseSystem : IReactiveSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		OnCheck( world, entities );
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		OnCheck( world, entities );
	}

	void OnCheck( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			if ( !entity.HasBuilding() ) {
				continue;
			}

			if ( entity.GetBuilding().Type is not BuildingType.LightHouse ) {
				continue;
			}

			if ( entity.HasBuildingConstructionStatus() ) {
				continue;
			}

			var child = entity.TryFetchChildWithComponent< DispelTheDarkFogComponent >();
			child?.SetDispelTheDarkFog( true );

			// trigger DarkFog update
			var positionComponent = entity.GetPosition();
			entity.ReplaceComponent( positionComponent, typeof( PositionComponent ), true );
		}
	}
}

[Watch( typeof( IsPlacingBuildingComponent ) )]
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( DarkFogAgentComponent ) )]
public class AttachShowRangeLightHouseRangeDuringPlacingSystem : IReactiveSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			var child = entity.TryFetchChildWithComponent< DispelTheDarkFogComponent >();
			child?.SetShowRangeDispelTheDarkFog();
		}
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			var child = entity.TryFetchChildWithComponent< DispelTheDarkFogComponent >();
			child?.RemoveShowRangeDispelTheDarkFog();
		}
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {}
}


[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( DispelTheDarkFogComponent ), typeof( ShowRangeDispelTheDarkFogComponent ) )]
public class DrawLightHouseRangeSystem : IMurderRenderSystem {

	public void Draw( RenderContext render, Context context ) {
		foreach ( Entity e in context.Entities ) {
			ColliderComponent collider = e.GetCollider();
			IMurderTransformComponent globalPosition = e.GetGlobalTransform();

			Color color = collider.DebugColor * .6f;
			foreach ( var colliderShape in collider.Shapes ) {
				colliderShape.GetPolygon().Polygon.Draw( render.GameUiBatch, globalPosition.Vector2, false, color );
			}

			// render.DebugBatch.DrawText( MurderFonts.PixelFont, $"{globalPosition.Vector2}", globalPosition.Vector2 );
		}
	}
}
