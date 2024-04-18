using System;
using Murder;
using Murder.Diagnostics;
using Murder.Editor;
using Pixpil.Editor.Data;


namespace Pixpil;

public static class Program {

    [STAThread]
    static void Main() {
        try {
            var gamePrototype = new GamePrototypeMurderGame();
            using var game = new Murder.Game( gamePrototype, new PixpilDataManager( gamePrototype ) );
            game.Run();
        }
        catch ( Exception ex ) when ( GameLogger.CaptureCrash( ex ) ) { }
    }

}
