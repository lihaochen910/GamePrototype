using System;
using Murder.Assets;
using Murder.Attributes;

namespace Pixpil.Assets;

public class GamePrototypeProfile : GameProfile {
	
	[GameAssetId( typeof( LibraryAsset ) )]
	public readonly Guid Library;

	//
	// [FmodId(FmodIdKind.Bus)]
	// [Tooltip("This is the bus in fmod that translates to the music setting.")]
	// public readonly SoundEventId MusicBus;
	//
	// [FmodId(FmodIdKind.Bus)]
	// [Tooltip("This is the bus in fmod that translates to the sound setting.")]
	// public readonly SoundEventId SoundBus;
}
