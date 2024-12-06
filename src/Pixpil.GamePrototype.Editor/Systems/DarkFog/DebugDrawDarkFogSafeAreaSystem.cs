using System.Collections.Immutable;
using System.Numerics;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder;
using Murder.Components;
using Murder.Core;
using Murder.Core.Geometry;
using Murder.Core.Graphics;
using Murder.Core.Physics;
using Murder.Editor.Attributes;
using Murder.Editor.Components;
using Murder.Services;
using Murder.Utilities;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.Systems;

[OnlyShowOnDebugView]
[WorldEditor(startActive: true)]
[Filter(kind: ContextAccessorKind.Read, typeof(MapComponent))]
public class DebugDrawDarkFogSafeAreaSystem : IMurderRenderSystem {

	public void Draw( RenderContext render, Context context ) {
		var editorHook = context.World.GetUnique< EditorComponent >().EditorHook;
		if ( !editorHook.DrawGrid ) {
			return;
		}

		Map? map = context.World.TryGetUnique< MapComponent >()?.Map;
		if ( map is null ) {
			return;
		}

		var safeAreas = context.World.GetEntitiesWith( typeof( DarkFogSafeAreaComponent ) );
		if ( safeAreas == ImmutableArray< Entity >.Empty || safeAreas.Length <= 0 ) {
			return;
		}

		Rectangle cameraRect = new(
			render.Camera.Position.X + render.Camera.HalfWidth - Game.Profile.GameWidth / 2f,
			render.Camera.Position.Y + render.Camera.Height / 2f - Game.Profile.GameHeight / 2f,
			Game.Profile.GameWidth, Game.Profile.GameHeight);

		// ( int minX, int maxX, int minY, int maxY ) = render.Camera.GetSafeGridBounds( map );
		Color safeAreaColor = Color.CreateFrom256( r: 255, g: 0, b: 0 ) * .2f;
		Color collideColor = Color.CreateFrom256( r: 0, g: 255, b: 0 ) * .5f;

		foreach ( var safeArea in safeAreas ) {

			render.GameUiBatch.DrawText( PPFonts.PixelFont, "你好！世界, '飞行器'" +
															  "常用中文字体测试: 方舟像素字体/Ark Pixel Font\n\n我们度过的每个平凡的日常，也许就是连续发生的奇迹。\nTHE QUICK BROWN FOX JUMPS OVER A LAZY DOG. the quick brown fox jumps over a lazy dog.",
				cameraRect.TopLeft - new Point( -4, 12 ),
				// safeArea.GetPosition().ToPoint() - new Point( 0, 0 ),
				new DrawInfo( 0.44f ) { Color = Game.Profile.Theme.Green, Scale = Vector2.One * 1f } );
			
			var safeAreaCollider = safeArea.GetCollider();
			foreach ( var shape in safeAreaCollider.Shapes ) {
				if ( shape is BoxShape boxShape ) {
					var shapeRect = boxShape.Rectangle;
					DrawSafeArea( render, shapeRect, safeAreaColor );
					
					for ( var y = 0; y < map.Height; y++ ) {
						for ( var x = 0; x < map.Width; x++ ) {

							Rectangle tileRectangle = XnaExtensions.ToRectangle( x * Grid.CellSize - Grid.HalfCellSize,
								y * Grid.CellSize - Grid.HalfCellSize, Grid.CellSize, Grid.CellSize );
							if ( shapeRect.Touches( tileRectangle ) ) {
								DrawSafeArea( render, tileRectangle, collideColor );
							}
						}
					}
				}
			}
		}
		
	}


	private void DrawSafeArea( RenderContext render, Rectangle rectangle, Color color ) {
		const float sorting = 1;
		// const int mask = CollisionLayersBase.SOLID;

		// if ( ( topLeft & mask ) != 0 ) {
			render.DebugBatch.DrawRectangle(
				new Rectangle( rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height ), color, sorting );
		// }
	}


	private void DrawTileCollisions( int topLeft, int topRight, int botLeft, int botRight,
									 RenderContext render, Rectangle rectangle, Color color ) {
		const float sorting = 1;
		const int mask = CollisionLayersBase.SOLID;

		if ( ( topLeft & mask ) != 0 ) {
			render.DebugBatch.DrawRectangle(
				new Rectangle( rectangle.X, rectangle.Y, Grid.HalfCellSize, Grid.HalfCellSize ), color, sorting );
		}

		if ( ( topRight & mask ) != 0 ) {
			render.DebugBatch.DrawRectangle(
				new Rectangle( rectangle.X + Grid.HalfCellSize, rectangle.Y, Grid.HalfCellSize, Grid.HalfCellSize ),
				color, sorting );
		}

		if ( ( botLeft & mask ) != 0 ) {
			render.DebugBatch.DrawRectangle(
				new Rectangle( rectangle.X, rectangle.Y + Grid.HalfCellSize, Grid.HalfCellSize, Grid.HalfCellSize ),
				color, sorting );
		}

		if ( ( botRight & mask ) != 0 ) {
			render.DebugBatch.DrawRectangle(
				new Rectangle( rectangle.X + Grid.HalfCellSize, rectangle.Y + Grid.HalfCellSize, Grid.HalfCellSize,
					Grid.HalfCellSize ), color, sorting );
		}
	}

}
