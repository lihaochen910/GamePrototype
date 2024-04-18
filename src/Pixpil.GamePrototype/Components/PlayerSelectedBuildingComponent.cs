using System;
using Bang.Components;
using Murder.Assets;
using Murder.Attributes;

namespace Pixpil.Components;

[Requires(typeof(PlayerComponent))]
public readonly struct PlayerSelectedBuildingComponent : IComponent {

	[GameAssetId<PrefabAsset>]
	public readonly Guid SelectedBuilding;

	public PlayerSelectedBuildingComponent( Guid selected ) {
		SelectedBuilding = selected;
	}

}