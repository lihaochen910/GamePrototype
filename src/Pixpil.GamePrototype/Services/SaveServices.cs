using Bang;
using Murder;
using Murder.Core;
using Murder.Core.Dialogs;
using Murder.Diagnostics;
using Pixpil.Assets;
using Pixpil.Core;


namespace Pixpil.Services;

internal static class SaveServices {
    
    public static GPSaveData GetOrCreateSave() {
#if DEBUG
        if ( Game.Instance.ActiveScene is not GameScene && Game.Data.TryGetActiveSaveData() is null ) {
            GameLogger.Warning( "Creating a save out of the game!" );
        }
#endif

        if ( Game.Data.TryGetActiveSaveData() is not GPSaveData save ) {
            // Right now, we are creating a new save if one is already not here.
            save = ( GPSaveData )Game.Data.CreateSave();
        }

        return save;
    }

    public static GPSaveData? TryGetSave() => Game.Data.TryGetActiveSaveData() as GPSaveData;

    public static GameplayBlackboard GetGameplay() {
        return GetOrCreateSave().GameplayBlackboard;
    }

    public static void SetGameplayValue( string fieldName, bool value ) {
        GetOrCreateSave().BlackboardTracker.SetBool( GameplayBlackboard.Name, fieldName, BlackboardActionKind.Set, value );
    }

    public static void SetGameplayValue( string fieldName, int value ) {
        GetOrCreateSave().BlackboardTracker
                         .SetInt( GameplayBlackboard.Name, fieldName, BlackboardActionKind.Set, value );
    }

    public static void SetGameplayValue( string fieldName, string value ) {
        GetOrCreateSave().BlackboardTracker.SetString( GameplayBlackboard.Name, fieldName, value );
    }

    internal static void AddGameplayValue( string fieldName, int value ) {
        GetOrCreateSave().BlackboardTracker.SetInt( GameplayBlackboard.Name, fieldName, BlackboardActionKind.Add, value );
    }

    public static void QuickSave() => Game.Data.QuickSave();

}
