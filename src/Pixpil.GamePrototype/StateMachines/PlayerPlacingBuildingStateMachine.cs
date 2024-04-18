using System;
using System.Collections.Generic;
using System.Numerics;
using Bang.Entities;
using Bang.StateMachines;
using Murder;
using Murder.Assets;
using Murder.Core.Geometry;
using Murder.Core.Graphics;
using Murder.Diagnostics;
using Pixpil.Components;
using Pixpil.Messages;
using Pixpil.Services;


namespace Pixpil.StateMachines;

public class PlayerPlacingBuildingStateMachine : StateMachine {

    private Guid _selectedBuilding;

    public PlayerPlacingBuildingStateMachine() {
        State( Main );
    }

    protected override void OnStart() {
        base.OnStart();
        // Entity.SetCustomDraw( DrawMessage );

        // World.DeactivateSystem< PlayerInputSystem >();
    }

    public override void OnDestroyed() {
        // World.ActivateSystem< PlayerInputSystem >();
        Entity.SetPlayer( PlayerStates.Normal );
        base.OnDestroyed();
    }

    private IEnumerator<Wait> Main() {
        var selectedBuilding = Entity.TryGetPlayerSelectedBuilding();
        if ( selectedBuilding is null || selectedBuilding.Value.SelectedBuilding == Guid.Empty ) {
            GameLogger.Error( "Invalid PlayerSelectedBuilding." );
            yield return Wait.Stop;
        }

        _selectedBuilding = selectedBuilding.Value.SelectedBuilding;
        Entity.SetPlayer( PlayerStates.PlaceBuilding );
        yield return GoTo( SelectPlacingLocation );
    }
    
    private IEnumerator<Wait> WaitCommandStartPlacing() {
        yield return Wait.ForMessage< StartPlacingBuildingMessage >();
        yield return GoTo( SelectPlacingLocation );
    }

    private IEnumerator< Wait > SelectPlacingLocation() {
        var buildingPrefab = Game.Data.TryGetAsset< PrefabAsset >( _selectedBuilding );
        if ( buildingPrefab is null ) {
            yield return Wait.Stop;
        }

        var instance = buildingPrefab.CreateAndFetch( World );
        instance.SetIsPlacingBuilding( Entity );
        var status = new BuildingConstructionStatusComponent( BuildingConstructionStatus.Building );
        instance.SetBuildingConstructionStatus( status );
        Entity.SendMessage< StartPlacingBuildingMessage >();
        yield return Wait.ForMessage< FinishedPlacingBuildingMessage >();

        // trigger react system
        instance.ReplaceComponent( status, typeof( BuildingConstructionStatusComponent ), forceReplace: true );
        
        // GameLogger.Log( "Finished SelectPlacingLocation." );
        yield return Wait.Stop;
        
    }
    
    private void DrawMessage( RenderContext render ) {
        Point cameraSize = render.Camera.Size;
        render.DebugBatch.DrawText( PPFonts.FusionPixel, $"S: {Name}",
            new Vector2( 0, 10 ), new DrawInfo( Color.White, 0.1f ) );

    }

}
