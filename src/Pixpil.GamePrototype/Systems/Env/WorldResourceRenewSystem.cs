using System;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DigitalRune.Linq;
using Murder;
using Pixpil.Components;
using Pixpil.Data;


namespace Pixpil.Systems; 

[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Write, typeof( WorldResourceRenewableComponent ), typeof( InventoryComponent ) )]
public class WorldResourceRenewSystem : IStartupSystem, IUpdateSystem {
	
	private GamePrototypePreferences _preferences;

	public void Start( Context context ) {
		_preferences = Game.Preferences as GamePrototypePreferences;
	}

	public void Update( Context context ) {
		context.Entities.ForEach( entity => {
			var renewableComponent = entity.GetWorldResourceRenewable();
			if ( renewableComponent.ItemType is null ) {
				return;
			}
			if ( !entity.HasWorldResourceRenewCountdown() ) {
				entity.SetWorldResourceRenewCountdown( renewableComponent.Speed );
			}

			var renewTime = entity.GetWorldResourceRenewCountdown().Time - Game.DeltaTime * _preferences.SpeedOfTime;
			if ( renewTime < 0f ) {
				var inventoryComponent = entity.GetInventory();
				if ( inventoryComponent.GetItemCount( renewableComponent.ItemType ) < renewableComponent.Max ) {
					var renewCount = Math.Clamp( renewableComponent.RenewCount, 0,
						renewableComponent.Max -
						inventoryComponent.GetItemCount( renewableComponent.ItemType ) );
					inventoryComponent.AddItem( renewableComponent.ItemType, renewCount, entity );
					entity.SetWorldResourceRenewCountdown( renewableComponent.Speed );
					entity.SetAgentDebugMessage( $"+{renewCount}", 3f );
				}
			}
			else {
				entity.SetWorldResourceRenewCountdown( renewTime );
			}
		} );
	}
	
}
