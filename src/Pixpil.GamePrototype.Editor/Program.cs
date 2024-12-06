using System;
using Murder.Editor;
using Pixpil.Editor.Data;


namespace Pixpil.Editor;

public static class Program {
    
    [STAThread]
    static void Main() {
        // Environment.SetEnvironmentVariable( "FNA3D_FORCE_DRIVER", "Vulkan" );
        
        var iMurderGame = new GamePrototypeArchitect();
        using var editor = new PixpilArchitect( iMurderGame, new PixpilEditorDataManager( iMurderGame ) );
        editor.Run();
    }
    
}
