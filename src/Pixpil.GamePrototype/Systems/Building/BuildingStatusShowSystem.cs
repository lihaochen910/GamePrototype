using System.Collections.Immutable;
using System.Numerics;
using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder.Components;
using Murder.Core.Geometry;
using Murder.Core.Graphics;
using Murder.Services;
using Murder.Utilities;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.Systems.Building; 

[Watch( typeof( BuildingConstructionStatusComponent ) )]
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( BuildingConstructionStatusComponent ) )]
internal class BuildingStatusShowSystem : IReactiveSystem {
	
	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			var compBuildingConstructionStatus = entity.TryGetBuildingConstructionStatus();
			if ( compBuildingConstructionStatus is null ) {
				entity.RemoveAlpha();
			}
			else {
				if ( compBuildingConstructionStatus.Value.Status is BuildingConstructionStatus.Building ) {
					entity.SetAlpha( AlphaSources.Alpha, 0.5f );
				}
				else {
					entity.RemoveAlpha();
				}
			}
		}
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			entity.RemoveAlpha();
		}
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			var compBuildingConstructionStatus = entity.TryGetBuildingConstructionStatus();
			if ( compBuildingConstructionStatus?.Status is BuildingConstructionStatus.Building ) {
				entity.SetAlpha( AlphaSources.Alpha, 0.5f );
			}
			else {
				entity.RemoveAlpha();
			}
		}
	}

	public void Draw( RenderContext render, Context context ) {
		foreach ( var entity in context.Entities ) {
			if ( entity.TryGetInCamera() is null ) {
				continue;
			}
			render.UiBatch.DrawText( PPFonts.FusionPixel, $"{entity.GetBuildingConstructionStatus().Status}", render.Camera.WorldToScreenPosition( entity.GetPosition().ToVector2() ), new DrawInfo( 0.984f ) {
				Scale = Vector2.One * 0.5f,
				Color = Color.Green
			} );
		}
	}
}


[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( BuildingConstructionStatusComponent ), typeof( BuildingConstructRequireResourcesComponent ), typeof( InventoryComponent ) )]
[Filter( ContextAccessorFilter.NoneOf, typeof( IsPlacingBuildingComponent ) )]
internal class BuildingStatusProgressShowSystem : IMurderRenderSystem {
	public void Draw( RenderContext render, Context context ) {
		foreach ( var entity in context.Entities ) {
			if ( entity.GetBuildingConstructionStatus().Status != BuildingConstructionStatus.Building ) {
				continue;
			}
			
			if ( entity.TryGetInCamera() is null ) {
				continue;
			}

			int totalCount = 0;
			int currentCount = 0;
			foreach ( var entry in entity.GetBuildingConstructRequireResources().Requires ) {
				totalCount += entry.Count;
				currentCount += entity.GetInventory().GetItemCount( entry.ItemType );
			}

			float progress = ( float )currentCount / totalCount;
			
			const float size = 10f;
			var screenPos = render.Camera.WorldToScreenPosition( entity.GetPosition().ToVector2() );
			var frameRect = new Rectangle( screenPos, new Vector2( size, 5f ) );
			var progressRect = new Rectangle( screenPos, new Vector2( size * progress, 5f ) );
			render.UiBatch.DrawRectangle( progressRect, Color.Green, 0.9f );
			render.UiBatch.DrawRectangleOutline( frameRect, Color.White, 1 );
			
			render.UiBatch.DrawText( MurderFonts.PixelFont, $"{currentCount} / {totalCount}", render.Camera.WorldToScreenPosition( entity.GetPosition().ToVector2() ) + new Vector2( size + 2, 0 ), new DrawInfo( 0.984f ) {
				Scale = Vector2.One * 1f,
				Color = Color.Green
			} );

			if ( entity.HasBuildingSelfBuildingBuildingSubmitTimer() && entity.HasBuildingSelfConstructionAbility() ) {
				const float lineSize = 10f;
				var screenPosDown = screenPos + new Vector2( 0, 8f );
				// var progressRect2 = new Rectangle( screenPosDown, new Vector2( size * ( entity.GetBuildingSelfBuildingBuildingSubmitTimer().SubmitTime / entity.GetBuildingSelfConstructionAbility().SubmitTime ), 2f ) );
				render.UiBatch.DrawLine( screenPosDown, screenPosDown + new Vector2( lineSize * ( entity.GetBuildingSelfBuildingBuildingSubmitTimer().SubmitTime / entity.GetBuildingSelfConstructionAbility().SubmitTime ), 0 ), Color.Green );
			}
		}
	}
}
