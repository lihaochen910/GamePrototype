using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder;
using Murder.Components;
using Murder.Core;
using Murder.Core.Geometry;
using Murder.Core.Graphics;
using Murder.Core.Physics;
using Murder.Diagnostics;
using Murder.Editor.Attributes;
using Murder.Editor.Components;
using Murder.Services;
using Murder.Utilities;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.Systems; 

[Filter( ContextAccessorFilter.AllOf, typeof( MapComponent ) )]
public class DarkFogInitializerSystem : IStartupSystem {

	public void Start( Context context ) {
		
		Map? map = context.World.TryGetUnique<MapComponent>()?.Map;
		if ( map is null ) {
			return;
		}

		var safeAreas = context.World.GetEntitiesWith( typeof( DarkFogSafeAreaComponent ) );

		bool CollidesWithSafeArea( Rectangle tileRect ) {
			foreach ( var safeArea in safeAreas ) {
				var safeAreaCollider = safeArea.GetCollider();
				foreach ( var shape in safeAreaCollider.Shapes ) {
					if ( shape is BoxShape boxShape ) {
						if ( boxShape.Rectangle.Touches( tileRect ) ) {
							return true;
						}
					}
				}
			}

			return false;
		}

		// GameLogger.Log( "found MapComponent!" );
		
		for ( var y = 0; y < map.Height; y++ ) {

			for ( var x = 0; x < map.Width; x++ ) {

				var pos = new Point( x, y );
				Rectangle tileRect = XnaExtensions.ToRectangle( x * Grid.CellSize - Grid.HalfCellSize,
					y * Grid.CellSize - Grid.HalfCellSize, Grid.CellSize, Grid.CellSize );
				var collidesWithSafeArea = CollidesWithSafeArea( tileRect );
				
				if ( map.IsObstacle( pos ) || !collidesWithSafeArea ) {
					var tilesetComponent = context.World.GetUnique< TilesetComponent >();
					var tileGridComponent =
						context.World.GetEntitiesWith( typeof( TileGridComponent ) ).FirstOrDefault();

					var idx = tilesetComponent.Tilesets.IndexOf( LibraryServices.GetLibrary().DarkFogTileset ).ToMask();
					if ( idx >= 0 ) {
						tileGridComponent.GetTileGrid().Grid.SetGridPosition( pos, idx );
						// GameLogger.Log( $"SetGridPosition: {pos} {idx}" );
					}
				}
				
			}
		}
	}
	
}


[Watch( typeof( PositionComponent ) )]
[Filter( ContextAccessorFilter.AllOf, typeof( DarkFogAgentComponent ), typeof( PositionComponent ) )]
public class DarkFogUpdateSystem : IStartupSystem, IReactiveSystem {

	public void Start( Context context ) {
		UpdateMapDarkFog( context.World, ImmutableArray< Entity >.Empty, true );
	}
	
	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		UpdateMapDarkFog( world, entities );
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		UpdateMapDarkFog( world, entities );
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		UpdateMapDarkFog( world, entities );
	}

	public static void UpdateMapDarkFog( World world, ImmutableArray< Entity > entities, bool force = false ) {
		if ( force ) {
			goto DoUpdate;
		}
		
		bool hasActive = false;
		bool needUpdate = false;
		foreach ( var entity in entities ) {
			var child = entity.TryFetchChildWithComponent< DispelTheDarkFogComponent >();
			if ( child is not null ) {
				needUpdate = true;
				if ( child.GetDispelTheDarkFog().Active ) {
					hasActive = true;
					break;
				}
			}
		}

		if ( !needUpdate || !hasActive ) {
			return;
		}
		
DoUpdate:
		Map? map = world.TryGetUnique<MapComponent>()?.Map;
		if ( map is null ) {
			return;
		}
		
		var tilesetComponent = world.GetUnique< TilesetComponent >();
		var tileGridComponent = world.GetEntitiesWith( typeof( TileGridComponent ) ).FirstOrDefault();
		if ( tileGridComponent is null ) {
			return;
		}

		var safeAreas = world.GetEntitiesWith( typeof( DarkFogSafeAreaComponent ) );
		// var lightHouses = entities.Select( entity => {
		// 	foreach ( var childId in entity.Children ) {
		// 		var child = world.TryGetEntity( childId );
		// 		if ( child is not null && child.HasDispelTheDarkFog() && child.IsActive ) {
		// 			return child;
		// 		}
		// 	}
		//
		// 	return null;
		// } );
		var lightHouses = world.GetEntitiesWith( typeof( DispelTheDarkFogComponent ) );

		bool CollidesWithSafeArea( Rectangle tileRect ) {
			foreach ( var safeArea in safeAreas ) {
				var safeAreaCollider = safeArea.GetCollider();
				foreach ( var shape in safeAreaCollider.Shapes ) {
					if ( shape is BoxShape boxShape ) {
						if ( boxShape.Rectangle.Touches( tileRect ) ) {
							return true;
						}
					}
				}
			}

			return false;
		}
		
		bool CollidesWithLightHouses( Rectangle tileRect, int mapX, int mapY ) {
			foreach ( var area in lightHouses ) {
				var collider = area.GetCollider();
				foreach ( var shape in collider.Shapes ) {
					if ( shape is BoxShape boxShape ) {
						if ( boxShape.Rectangle.Touches( tileRect ) ) {
							return true;
						}
					}
					if ( shape is CircleShape circleShape ) {
						var tileCenter = new Vector2( mapX * Grid.CellSize - Grid.HalfCellSize, mapY * Grid.CellSize - Grid.HalfCellSize );
						var entityCircle = new Circle( circleShape.Circle.X - circleShape.Offset.X + area.GetGlobalTransform().X, circleShape.Circle.Y - circleShape.Offset.Y + area.GetGlobalTransform().Y, circleShape.Radius );
						if ( entityCircle.Contains( tileCenter ) ) {
							return true;
						}
					}
				}
			}

			return false;
		}
		
		var darkFogMask = tilesetComponent.Tilesets.IndexOf( LibraryServices.GetLibrary().DarkFogTileset ).ToMask();
		if ( darkFogMask < 0 ) {
			return;
		}
		
		for ( var y = 0; y < map.Height; y++ ) {

			for ( var x = 0; x < map.Width; x++ ) {

				var pos = new Point( x, y );
				Rectangle tileRect = XnaExtensions.ToRectangle(
					x * Grid.CellSize - Grid.HalfCellSize,
					y * Grid.CellSize - Grid.HalfCellSize, Grid.CellSize, Grid.CellSize );
				
				var collidesWithLightHouse = CollidesWithLightHouses( tileRect, x, y );
				if ( collidesWithLightHouse ) {
					map.UnSetOccupiedAsStatic( x, y, CollisionLayersBase.SOLID );
					map.UnSetOccupiedAsStatic( x, y, CollisionLayersBase.BLOCK_VISION );
					tileGridComponent.GetTileGrid().Grid.UnsetGridPosition( pos, darkFogMask );
				}
				else {
					var collidesWithSafeArea = CollidesWithSafeArea( tileRect );
					if ( map.IsObstacle( pos ) || !collidesWithSafeArea ) {
						map.SetOccupiedAsStatic( x, y, CollisionLayersBase.SOLID | CollisionLayersBase.BLOCK_VISION );
						tileGridComponent.GetTileGrid().Grid.SetGridPosition( pos, darkFogMask );
					}
				}
				
			}
		}
	}
	
}


[Filter(typeof(ColliderComponent), typeof(ClearDarkFogOnTouchComponent))]
[Watch(typeof(PositionComponent))]
public class ClearDarkFogWhenTouchSystem : IReactiveSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		Map? map = world.TryGetUnique<MapComponent>()?.Map;
		if ( map is null ) {
			return;
		}
		
		var tilesetComponent = world.GetUnique< TilesetComponent >();
		var tileGridEntity = world.GetEntitiesWith( typeof( TileGridComponent ) ).FirstOrDefault();
		var darkFogMask = tilesetComponent.Tilesets.IndexOf( LibraryServices.GetLibrary().DarkFogTileset ).ToMask();
		
		for ( var y = 0; y < map.Height; y++ ) {
			for ( var x = 0; x < map.Width; x++ ) {
				
				foreach ( var entity in entities ) {
					if ( entity.IsDeactivated ) {
						continue;
					}

					foreach ( var shape in entity.GetCollider().Shapes ) {
						if ( shape is CircleShape circleShape ) {

							var tileCenter = new Vector2( x * Grid.CellSize - Grid.HalfCellSize, y * Grid.CellSize - Grid.HalfCellSize );
							var entityCircle = new Circle( circleShape.Circle.X + entity.GetGlobalTransform().X, circleShape.Circle.Y + entity.GetGlobalTransform().Y, circleShape.Radius );
							if ( entityCircle.Contains( tileCenter ) ) {
								
								if ( tileGridEntity.GetTileGrid().Grid.HasFlagAt( x, y, darkFogMask ) ) {
									tileGridEntity.GetTileGrid().Grid.UnsetGridPosition( new Point( x, y ), darkFogMask );
									GameLogger.Log($"UnsetGridPosition {x}, {y} ({darkFogMask})");
								}
							}

						}
					}
				}
			}
		}
		
	}
}


[Filter(typeof(ColliderComponent), typeof(PositionComponent))]
[Watch(typeof(ClearDarkFogOnAttachComponent))]
public class ClearDarkFogWhenAttachSystem : IReactiveSystem {
	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		Map? map = world.TryGetUnique<MapComponent>()?.Map;
		if ( map is null ) {
			return;
		}
		
		var tilesetComponent = world.GetUnique< TilesetComponent >();
		var tileGridEntity = world.GetEntitiesWith( typeof( TileGridComponent ) ).FirstOrDefault();
		var darkFogMask = tilesetComponent.Tilesets.IndexOf( LibraryServices.GetLibrary().DarkFogTileset ).ToMask();
		
		for ( var y = 0; y < map.Height; y++ ) {
			for ( var x = 0; x < map.Width; x++ ) {
				
				foreach ( var entity in entities ) {
					if ( entity.IsDeactivated ) {
						continue;
					}

					foreach ( var shape in entity.GetCollider().Shapes ) {
						if ( shape is CircleShape circleShape ) {

							var tileCenter = new Vector2( x * Grid.CellSize - Grid.HalfCellSize, y * Grid.CellSize - Grid.HalfCellSize );
							var entityCircle = new Circle( circleShape.Circle.X + entity.GetGlobalTransform().X, circleShape.Circle.Y + entity.GetGlobalTransform().Y, circleShape.Radius );
							if ( entityCircle.Contains( tileCenter ) ) {
								
								if ( tileGridEntity.GetTileGrid().Grid.HasFlagAt( x, y, darkFogMask ) ) {
									tileGridEntity.GetTileGrid().Grid.UnsetGridPosition( new Point( x, y ), darkFogMask );
									GameLogger.Log($"UnsetGridPosition {x}, {y} ({darkFogMask})");
								}
							}

						}
					}
				}
			}
		}
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {}
}


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
