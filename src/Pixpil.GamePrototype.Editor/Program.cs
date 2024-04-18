using System;
using Murder.Editor;
using Pixpil.Editor.Data;


namespace Pixpil.Editor;

public static class Program {
    
    [STAThread]
    static void Main() {
        var iMurderGame = new GamePrototypeArchitect();
        using var editor = new PixpilArchitect( iMurderGame, new PixpilEditorDataManager( iMurderGame ) );
        editor.Run();
    }
    
}
