using System;
using Murder.Assets;
using Murder.Assets.Graphics;
using Murder.Attributes;
using System.Numerics;


namespace Pixpil.Assets;

public class UiSkinAsset : GameAsset {

	public override string EditorFolder => "#\uf86dUi";

	public override char Icon => '\uf86d';

	public override Vector4 EditorColor => new ( 1f, .8f, .25f, 1f );

	[GameAssetId< SpriteAsset >]
	public Guid BoxBasic = Guid.Empty;

}
