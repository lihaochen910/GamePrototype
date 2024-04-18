using System;
using System.Numerics;
using Murder.Assets;
using Murder.Assets.Graphics;
using Murder.Attributes;
using Murder.Utilities;


namespace Pixpil.Assets; 

public class LibraryAsset : GameAsset {
	
	public override string EditorFolder => "#\uf02dLibraries";

	public override char Icon => '\uf02d';

	public override Vector4 EditorColor => "#FA5276".ToVector4Color();

	[GameAssetId< UiSkinAsset >]
	public readonly Guid UiSkin = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid Player = Guid.Empty;
	
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid Worker = Guid.Empty;


	[GameAssetId< TilesetAsset >]
	public readonly Guid DarkFogTileset = Guid.Empty;


	#region Building
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid LightHouse = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid Quarry = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid Dormitry = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid SawMill = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid Farm = Guid.Empty;
	
	[GameAssetId< PrefabAsset >]
	public readonly Guid SpringCollector = Guid.Empty;
	
	#endregion


	#region Worker AI

	[GameAssetId< UtilityAiAsset >]
	public readonly Guid WorkerAI_Idle = Guid.Empty;
	
	[GameAssetId< UtilityAiAsset >]
	public readonly Guid WorkerAI_Constructor = Guid.Empty;
	
	[GameAssetId< UtilityAiAsset >]
	public readonly Guid WorkerAI_Quarry = Guid.Empty;

	#endregion


	#region Speaker

	[GameAssetId< SpeakerAsset >]
	public readonly Guid PlayerSpeaker = Guid.Empty;

	#endregion

}
