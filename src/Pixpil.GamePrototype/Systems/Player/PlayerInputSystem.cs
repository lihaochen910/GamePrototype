using System.Numerics;
using Bang.Contexts;
using Bang.Entities;
using Bang.Interactions;
using Bang.StateMachines;
using Bang.Systems;
using Murder.Components;
using Murder.Diagnostics;
using Murder.Helpers;
using Murder.Utilities;
using Pixpil.Components;
using Pixpil.Core;
using Pixpil.Services;
using Pixpil.StateMachines;
using Game = Murder.Game;


namespace Pixpil.Systems;

/// <summary>
///     System intended to capture and relay player inputs to entities.
///     System is called during frame updates and fixed updates thanks to interfaces.<br/>
///     Targets only entities with <b>both</b> PlayerComponent and AgentComponent
///     Example usage:<br/>
///     1. Poll input system with: <br/>
///         Game.Input <see cref="Murder.Game.Input"/><br/>
///     2. Send entity messages or call extension functions in FixedUpdate within the foreach:<br/>
///         entity.SendMessage <see cref="Entity.SendMessage{T}()"/><br/>
///         entity.SetImpulse <see cref="MurderEntityExtensions.SetAgentImpulse(Entity)"/><br/>
/// </summary>
[Filter(kind: ContextAccessorKind.Read, typeof(PlayerComponent), typeof(AgentComponent))]
public class PlayerInputSystem : IUpdateSystem, IFixedUpdateSystem {
    
    private Vector2 _cachedInputAxis = Vector2.Zero;
    private int _cachedInputSkill = -1;
    
    private bool _previousCachedAttack = false;
    private int _cachedPlaceBuildingType = -1;

    private bool _interacted = false;
    
    
    /// <summary>
    /// Whether the player locked the movement and only wants to change the facing.
    /// </summary>
    private bool _lockMovement = false;
    private Vector2 _forcedInput = Vector2.Zero;
    private Vector2 _previousInput = Vector2.Zero;
    private float _nextSound = 0;


    /// <summary>
    ///     Called every fixed update.
    ///     We can apply input values to fixed updating components such as physics components.
    ///     For example the <see cref="AgentComponent"/>
    /// </summary>
    /// <param name="context"></param>
    public void FixedUpdate( Context context ) {
        
        foreach ( Entity entity in context.Entities ) {
            // Send entity messages or use entity extensions to update relevant entities
            
            PlayerComponent player = entity.GetComponent<PlayerComponent>();
            if ( player.CurrentState != PlayerStates.Normal ) {
                // Skip movement if the player is casting a spell
                continue;
            }

            bool moved = _cachedInputAxis.Manhattan() != 0;
            Vector2 impulse = Vector2.Zero;

            if ( _interacted ) {
                entity.SendMessage< InteractorMessage >();
            }

            Direction direction;
            if ( moved ) {
                _forcedInput = Vector2.Zero;
                direction = DirectionHelper.FromVector( _cachedInputAxis );
                impulse = _lockMovement ? Vector2.Zero : _cachedInputAxis;
                entity.SetAgentImpulse( impulse, direction );
            }
            else {
                // _forcedInput = save.SwayDirection;
                // _forcedInput = _forcedInput.ClampMagnitude(1);
                direction = DirectionHelper.FromVector( _forcedInput );

                if ( _forcedInput.Length() > 0.2f ) {
                    impulse = _lockMovement ? Vector2.Zero : _forcedInput;

                    // if (impulse.HasValue)
                    //     entity.SetAgentImpulse(impulse, direction);
                }
            }

            _interacted = false;

        }
    }

    /// <summary>
    ///     Called every frame
    ///     This is where we should poll our input system
    ///     We can optionally cache these values and use them in the <see cref="FixedUpdate(Context)"/>
    /// </summary>
    /// <param name="context"></param>
    public void Update( Context context ) {
        // Read from Game.Input
        _cachedInputAxis = Game.Input.GetAxis( InputAxis.Movement ).Value;
        _lockMovement = Game.Input.Down( InputButtons.LockMovement );

        // player place building
        _cachedPlaceBuildingType = -1;
        if ( Game.Input.Down( InputButtons.ShotcutSelectBuilding01 ) ) {
            _cachedPlaceBuildingType = 0;
        }
        else if ( Game.Input.Down( InputButtons.ShotcutSelectBuilding02 ) ) {
            _cachedPlaceBuildingType = 1;
        }
        else if ( Game.Input.Down( InputButtons.ShotcutSelectBuilding03 ) ) {
            _cachedPlaceBuildingType = 2;
        }
        else if ( Game.Input.Down( InputButtons.ShotcutSelectBuilding04 ) ) {
            _cachedPlaceBuildingType = 3;
        }

        if ( _cachedPlaceBuildingType >= 0 && context.Entity.GetPlayer().CurrentState is PlayerStates.Normal ) {
            if ( _cachedPlaceBuildingType is 0 ) {
                context.Entity.SetPlayerSelectedBuilding( LibraryServices.GetLibrary().Dormitry );
            }
            if ( _cachedPlaceBuildingType is 1 ) {
                context.Entity.SetPlayerSelectedBuilding( LibraryServices.GetLibrary().Quarry );
            }
            if ( _cachedPlaceBuildingType is 2 ) {
                context.Entity.SetPlayerSelectedBuilding( LibraryServices.GetLibrary().LightHouse );
            }
            if ( _cachedPlaceBuildingType is 3 ) {
                context.Entity.SetPlayerSelectedBuilding( LibraryServices.GetLibrary().SawMill );
            }
            context.Entity.SetStateMachine( new StateMachineComponent< PlayerPlacingBuildingStateMachine >() );
            // GameLogger.Log( $"_cachedPlaceBuildingType = {_cachedPlaceBuildingType}" );
        }

        if ( Game.Input.Pressed( InputButtons.Interact ) ) {
            _interacted = true;
        }

        // else if (Game.Input.Pressed(InputButtons.Pause) && 
        //          !context.World.IsPaused && context.World.GetEntitiesWith(typeof(FadeTransitionComponent)).Count() == 0)
        // {
        //     LibraryServices.GetPausePrefab().Create(context.World);
        //
        //     Game.Input.Consume(InputButtons.Pause);
        //     Game.Input.Consume(InputButtons.Cancel);
        //
        //     LDGameSoundPlayer.Instance.PlayEvent(LibraryServices.GetRoadLibrary().UiBack, isLoop: false);
        // }

        // if (_nextSound < Game.Now)
        // {
        //     if (_cachedInputAxis.X != 0 && _previousInput.X != _cachedInputAxis.X)
        //     {
        //
        //         if (Game.Random.TryWithChanceOf(0.1f))
        //         {
        //             LDGameSoundPlayer.Instance.PlayEvent(LibraryServices.GetRoadLibrary().CarTireSqueal, isLoop: false);
        //             _nextSound = Game.Now + 1.1f;
        //         }
        //     }
        // }

        _previousInput = _cachedInputAxis;
    }
}
