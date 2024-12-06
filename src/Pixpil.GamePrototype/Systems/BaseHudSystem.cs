using System;
using System.Numerics;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder;
using Murder.Core.Graphics;
using Murder.Services;
using Pixpil.Components;
using Pixpil.Data;
using Pixpil.Services;


namespace Pixpil.Systems;


[Filter( ContextAccessorFilter.AllOf, typeof( PlayerComponent ) )]
internal class BaseHudSystem : IStartupSystem, IMurderRenderSystem {

	private ItemType _foodItemType;
	private ItemType _woodItemType;
	private ItemType _stoneItemType;
	
	public void Start( Context context ) {
		_foodItemType = ItemTypeServices.GetItemType( "food" );
		_woodItemType = ItemTypeServices.GetItemType( "wood" );
		_stoneItemType = ItemTypeServices.GetItemType( "stone" );
	}
	
	public void Draw( RenderContext render, Context context ) {
		
		// render.UiBatch.DrawText( PPFonts.PixelFont, $"Now: {Game.Now.ToString("0.00")}", new Vector2( 5, 10 ), new DrawInfo( 0.984f ) {
		// 	Scale = Vector2.One * 1f,
		// 	Color = Color.Green,
		// 	// BlendMode = BlendStyle.Color,
		// 	Debug = true
		// } );

		var gamePlayBlackboard = SaveServices.GetGameplay();

		var playerEntity = context.World.TryGetUniqueEntity< PlayerComponent >();
		if ( playerEntity != null ) {
			var inventory = playerEntity.GetInventory();

			var drawInfo = new DrawInfo( Color.Green, 0.2f ) { Scale = Vector2.One * 1f };
			const int offsetX = -120;
			render.UiBatch.DrawText( PPFonts.FusionPixel, $"食物: {inventory.GetItemCount( _foodItemType )}", new Vector2( render.Camera.Width + offsetX, 0 ), drawInfo );
			render.UiBatch.DrawText( PPFonts.FusionPixel, $"木: {inventory.GetItemCount( _woodItemType )}", new Vector2( render.Camera.Width + offsetX, 12 ), drawInfo );
			render.UiBatch.DrawText( PPFonts.FusionPixel, $"石: {inventory.GetItemCount( _stoneItemType )}", new Vector2( render.Camera.Width + offsetX, 24 ), drawInfo );
			render.UiBatch.DrawText( PPFonts.FusionPixel, $"Popula: {gamePlayBlackboard.Population}", new Vector2( render.Camera.Width + offsetX, 36 ), drawInfo );
		}
	}

	
}
