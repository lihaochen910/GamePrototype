using System.Collections.Generic;
using System.Numerics;
using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.StateMachines;
using Bang.Systems;
using Murder;
using Murder.Components;
using Murder.Core.Geometry;
using Murder.Core.Graphics;
using Murder.Services;
using Pixpil.Assets;
using Pixpil.Components;
using Pixpil.Data;
using Pixpil.Messages;
using Pixpil.Services;


namespace Pixpil.Systems;

[Filter( typeof( DayCycleComponent ) )]
internal class DayCycleSystem : IStartupSystem, IUpdateSystem {

	private GPSaveData _saveData;
	private GamePrototypePreferences _preferences;

	public void Start( Context context ) {
		var save = SaveServices.GetOrCreateSave();
		var entity = context.World.AddEntity( new DayCycleComponent() );
		entity.SetDayCycle( save.DayProgress );

		_preferences = Game.Preferences as GamePrototypePreferences;
		_saveData = save;
	}

	public void Update( Context context ) {
		var entity = context.World.TryGetUniqueEntity< DayCycleComponent >();
		
		var dayProgress = entity.GetDayCycle().DayCycle;
		dayProgress.DayPercentile += Game.DeltaTime * _preferences.SpeedOfTime;
		entity.SetDayCycle( dayProgress );

		if ( dayProgress.DayPercentile > GamePrototypePreferences.TheDuskTime && !_saveData.TheDayNearDuskMessageFired ) {
			BroadcastNearDusk( context );
			_saveData.TheDayNearDuskMessageFired = true;
		}

		if ( dayProgress.DayPercentile >= 1f ) {
			dayProgress.Day++;
			dayProgress.DayPercentile = 0;
			entity.SetDayCycle( dayProgress );

			DoDaySummary( context );
			BroadcastDayPassed( context );
			SaveServices.QuickSave();
		}
	}

	
	private void BroadcastDayPassed( Context context ) {
		var entity = context.World.TryGetUniqueEntity< DayCycleComponent >();
		var msg = new OneDayPassedMessage( entity.GetDayCycle().DayCycle );
		entity.SendMessage( msg );
		foreach ( var agentEntity in context.World.GetEntitiesWith( ContextAccessorFilter.AnyOf, typeof( DayMessageListenerComponent ) ) ) {
			agentEntity.SendMessage( msg );
		}
	}
	
	
	private void BroadcastNearDusk( Context context ) {
		var entity = context.World.TryGetUniqueEntity< DayCycleComponent >();
		var msg = new TheDayNearDuskMessage( entity.GetDayCycle().DayCycle );
		entity.SendMessage( msg );
		foreach ( var agentEntity in context.World.GetEntitiesWith( ContextAccessorFilter.AnyOf, typeof( DayMessageListenerComponent ) ) ) {
			agentEntity.SendMessage( msg );
		}
	}


	private void DoDaySummary( Context context ) {
		var playerEntity = context.World.TryGetUniqueEntity< PlayerComponent >();
		if ( playerEntity is null ) {
			return;
		}

		var playerInventory = playerEntity.TryGetInventory();
		if ( playerInventory is null ) {
			return;
		}
		
		var gameplayBlackboard = SaveServices.GetGameplay();

		// 食物扣除
		var foodConsume = gameplayBlackboard.Population * _preferences.PopulationFoodConsume;
		playerInventory.Value.RemoveItem( ItemTypeServices.GetItemType( "food" ), foodConsume, playerEntity );

		// 人口增减
		// TODO: 
		
	}
	
}


[Filter(typeof(DayCycleComponent))]
[Messager(typeof(OneDayPassedMessage))]
internal class DayCycleTipsSystem : IMessagerSystem, IMurderRenderSystem, IUpdateSystem {

	private bool _showTips;
	private DayProgress _dayProgress;

	public void OnMessage( World world, Entity entity, IMessage message ) {
		_dayProgress = entity.GetDayCycle().DayCycle;
		entity.RunCoroutine( Main() );
	}
	
	public void Update( Context context ) {
		_dayProgress = context.World.GetUnique< DayCycleComponent >().DayCycle;
	}

	public void Draw( RenderContext render, Context context ) {
		// if ( _showTips ) {
			render.UiBatch.DrawText( MurderFonts.PixelFont, $"Day {_dayProgress.Day}", new Vector2( 0, 0 ), new DrawInfo( Color.Cyan, 1f ) );
		// }

		const float size = 15f;
		var frameRect = new Rectangle( new Vector2( 35f, 1f ), new Vector2( size, 5f ) );
		var progressRect = new Rectangle( new Vector2( 35f, 1f ), new Vector2( size * _dayProgress.DayPercentile, 5f ) );
		render.UiBatch.DrawRectangle( progressRect, Color.Green, 0.9f );
		render.UiBatch.DrawRectangleOutline( frameRect, Color.White, 1 );

	}
	
	private IEnumerator< Wait > Main() {
		_showTips = true;
		yield return Wait.ForSeconds( 3f );
		_showTips = false;
	}
	
}


// public class DayCycleSummarySystem : IMessagerSystem {
// 	
// }
