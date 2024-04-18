using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder;
using Murder.Assets;
using Pixpil.Components;
using Pixpil.Data;
using Pixpil.Services;


namespace Pixpil.Systems; 

[Filter( ContextAccessorFilter.AllOf, typeof( PlayerStartComponent ) )]
public class SpawnPlayerOnGameBegin : IStartupSystem {

	public void Start( Context context ) {
		var playerPrefab = Game.Data.TryGetAsset< PrefabAsset >( LibraryServices.GetLibrary().Player );
		var startPoint = context.World.TryGetUniqueEntity< PlayerStartComponent >();
		if ( startPoint is not null && playerPrefab is not null ) {
			var playerEntity = playerPrefab.CreateAndFetch( context.World );
			if ( startPoint.HasPosition() ) {
				playerEntity.SetPosition( startPoint.GetPosition() );
				// playerEntity.SetAgentDebugMessage( "刚复活的玩家", 3f );
			}

			if ( startPoint.HasInventory() && playerEntity.HasInventory() ) {
				// if ( playerEntity.GetInventory().Cells.IsDefault ) {
				// 	playerEntity.SetInventory( playerEntity.GetInventory().InitialSize, new Inventory( playerEntity.GetInventory().InitialSize ) );
				// }
				var startInventory = startPoint.GetInventory();
				startInventory.CopyAllToOther( playerEntity );
			}
		}
	}
	
}
