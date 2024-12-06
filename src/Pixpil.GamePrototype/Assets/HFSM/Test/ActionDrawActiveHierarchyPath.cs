using System.Numerics;
using Murder.Core.Graphics;
using Murder.Services;


namespace Pixpil.AI.HFSM.Test;

public class ActionDrawActiveHierarchyPath : HFSMStateAction {

#if MURDER
	public override void OnMurderDraw( RenderContext render ) {
		render.UiBatch.DrawText( MurderFonts.PixelFont, RootFsm != null ? RootFsm.GetActiveHierarchyPath() : Fsm.GetActiveHierarchyPath(), new Vector2( 0, 20 ) );
	}
#endif
	
}
