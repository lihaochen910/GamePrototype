using Bang;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder.Components;
using Murder.Core.Graphics;
using Murder.Core.Input;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Murder;
using Murder.Core;
using Murder.Core.Geometry;
using Murder.Core.Physics;
using Murder.Services;
using Murder.Utilities;
using Pixpil.Components;
using Pixpil.Core;
using Pixpil.Messages;
using Pixpil.Services;


namespace Pixpil.Systems;

/// <summary>
/// System that places an entity within the map.
/// </summary>
[Watch( typeof( IsPlacingBuildingComponent ) )]
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( IsPlacingBuildingComponent ) )]
internal class PlacerBuildingSystem : IUpdateSystem, IReactiveSystem, IMurderRenderSystem {
    
    public void Update( Context context ) {

        if ( context.World is not MonoWorld monoWorld ) {
            return;
        }
        
        Map? map = context.World.TryGetUnique<MapComponent>()?.Map;
        if ( map is null ) {
            return;
        }

        var cursorPositionWorld = monoWorld.Camera.GetCursorWorldPosition( Point.Zero, new Point(
            Game.GraphicsDevice.Viewport.Width,
            Game.GraphicsDevice.Viewport.Height ) );
        // var cursorPositionWorld = monoWorld.Camera.GetCursorWorldPosition( Point.Zero, monoWorld.Camera.Size );
        
        
        // If user has selected to destroy entities.
        bool destroy = Game.Input.Pressed( InputButtons.Cancel );

        bool clicked = Game.Input.Pressed( MurderInputButtons.LeftClick );
        // bool doCopy = Game.Input.Down( MurderInputButtons.Shift );


        foreach ( var e in context.Entities ) {
            e.SetTransform( new PositionComponent( cursorPositionWorld ) );
            
            if ( e.HasCollider() ) {
                
                if ( PhysicsServices.CollidesAtTile( map, e.GetCollider(), cursorPositionWorld.ToVector2(), CollisionLayersBase.SOLID ) ) {
                    e.SetPlacingBuildingHasObstacle();
                }
                else {
                    e.RemovePlacingBuildingHasObstacle();
                }
                
            }

            var canPlaceHere = !e.HasPlacingBuildingHasObstacle();
            
            if ( destroy ) {
                var commander = e.GetIsPlacingBuilding().Commander;
                e.Destroy();
                commander.SendMessage( new FinishedPlacingBuildingMessage( null ) );
            }

            if ( clicked && canPlaceHere ) {
                
                var commander = e.GetIsPlacingBuilding().Commander;
                e.RemoveComponent( typeof( IsPlacingBuildingComponent ) );
                commander.SendMessage( new FinishedPlacingBuildingMessage( e ) );
                
                // if ( doCopy ) {
                //     e.AddComponent( new IsPlacingBuildingComponent( e ), typeof( IsPlacingBuildingComponent ) );
                // }
                // else {
                //     // Only destroy if we are no longer interested in creating this entity.
                //     e.Destroy();
                // }
            }
        }
    }
    
    public void OnAdded( World world, ImmutableArray< Entity > entities ) {
        var target = entities.Last();
        // target.SetAlpha( AlphaSources.Alpha, 0.5f );

        // Remove all other entities which also have the placing component.
        foreach ( Entity e in world.GetEntitiesWith( typeof( IsPlacingBuildingComponent ) ) ) {
            if ( e.EntityId != target.EntityId ) {
                e.Destroy();
            }
        }
    }

    public void OnModified( World world, ImmutableArray< Entity > entities ) { }

    public void OnRemoved( World world, ImmutableArray< Entity > entities ) { }
    
    public void Draw( RenderContext render, Context context ) {
        foreach ( var entity in context.Entities ) {
            if ( entity.TryGetInCamera() is null ) {
                continue;
            }
            // render.DebugBatch.DrawText( PPFonts.PixelFont, $"{entity.GetPosition().ToVector2()}", render.Camera.WorldToScreenPosition( entity.GetPosition().ToVector2() ), new DrawInfo( 0.2f ) {
            render.DebugBatch.DrawText( PPFonts.FusionPixel, $"{entity.GetPosition().ToVector2()}", entity.GetPosition().ToVector2() + new Vector2( 2, -2 ), new DrawInfo( 0.2f ) {
                Scale = Vector2.One * 1f,
                Color = Color.Cyan
            } );
            if ( entity.HasPlacingBuildingHasObstacle() ) {
                render.DebugBatch.DrawText( PPFonts.FusionPixel, $"cannot place here!", entity.GetPosition().ToVector2() + new Vector2( 2, 5 ), new DrawInfo( 0.21f ) {
                    Scale = Vector2.One * 1f,
                    Color = Color.Red * .8f
                } );
            }
        }
    }
}
