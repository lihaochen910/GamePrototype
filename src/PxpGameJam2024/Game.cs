using System.Reflection;
using System.Text;
using Spectre.Console;


namespace PxpGameJam2024;

public interface IGame {

	Task GameSplash();
	Task Setup();
	Task MainLoop();

}


public enum GameMapLocation {
	Loc_A,
	Loc_B,
	Loc_C,
	Loc_D,
	Loc_E,
	Loc_F,
	Loc_G,
	Loc_H
}


public enum GameEndReason : byte {
	PlayerDead,
	TheLastOfUs,
	DeveloperTerminationSignal
}


public class Game : IGame {

	public const int NPCActOnEveryRound = 3;
	public const int HostDestroyGameMapLocationWarningAfterRounds = 4;
	public const int HostDestroyGameMapLocationAfterRounds = 6;

	public static PlayerCharacter PlayerCharacter { get; private set; }
	public static List< Character > NonPlayerControledCharacters { get; private set; }
	public static Dictionary< GameMapLocation, bool > LocationDestroyStatus { get; private set; }
	public static int CurrentRound { get; private set; }
	public static int PlayerCostPending { get; private set; }
	public static int NPCActRemainRounds { get; private set; }
	public static int HostDestroyLocationWarningRemainRounds { get; private set; }
	public static int HostDestroyLocationRemainRounds { get; private set; }
	public static GameMapLocation? HostDestroyLocationPending { get; private set; }
	public static SearchEvent? NextSearchEventPending { get; internal set; }
	public static bool FinalBossDefeated { get; internal set; }

	public static bool DevelopmentMode { get; set; }
	public static bool DeveloperTerminationSignal { get; private set; }
	public static bool GameResetRequested { get; private set; }
	public static bool NoTaskDelay { get; internal set; } = false;

	public Game() {}
	
	public async Task GameSplash() {
#if !DEBUG
		AnsiConsole.Clear();
		AnsiConsole.WriteLine();
		await AvgDialog( "今天是2024年的最后一天，你的Gamejam作品在公司里大受好评。" );
		await AvgDialog( "背起电脑，你走在街头的冷风中像是多了一分勇气和自信。" );
		await AvgDialog( "夜晚吴中路依然车如流水，你在路口站定，准备在红灯的时间里，叫个外卖。" );
		await AvgDialog( "这时，一辆等在路口的出租车突然启动，向你冲来。" );
		
		AnsiConsole.Clear();
		Console.WriteLine( HeadArt );
		AnsiConsole.MarkupLine( "\n[grey]按任意键继续\u21b5[/]" );
		Console.ReadKey( true );
		
		await AvgDialog( "不知过去了多久" );
		await AvgDialog( "海浪的声音把你唤醒，你站起身来发现自己置身于一片海滩。\n朦胧的月光在云中忽隐忽现。" );
		await AvgDialog( "摸索口袋，里面只有一张地图。" );
		
		AnsiConsole.Clear();
		PrintGameMap();
		AnsiConsole.MarkupLine( "\n[grey]按任意键继续\u21b5[/]" );
		Console.ReadKey( true );
		
		AnsiConsole.Clear();
		await AvgDialog( "地图的背面写着三个血字：", waitForAnyButton: false );
		await Delay( 1f );
		AnsiConsole.MarkupLine( "\n" );
		
		await Delay( 1f );
		AnsiConsole.Markup( "「 [red]大[/]" );
		await Delay( 1f );
		AnsiConsole.Markup( " [red]逃[/]" );
		await Delay( 1f );
		AnsiConsole.MarkupLine( " [red]杀[/] 」" );
		await Delay( 1.5f );
		
		async Task PrintTypingEffectAsync( string text ) {
			string[] lines = text.Split( new[] { Environment.NewLine, "\n" },
				StringSplitOptions.RemoveEmptyEntries );

			foreach ( string line in lines ) {
				foreach ( char c in line ) {
					Console.Write( c );
					await Task.Delay( 1 ); // 每个字符延迟50毫秒
				}

				Console.WriteLine();
				await Task.Delay( 5 ); // 每行结束后延迟500毫秒
			}
		}

		// Console.Write( GameLogoArt );
		await PrintTypingEffectAsync( GameLogoArt );
		AnsiConsole.Write( "\n" );
		await Delay( 1f );
		
		AnsiConsole.MarkupLine( "\n\n[grey]按任意键继续\u21b5[/]" );
		Console.ReadKey( true );
		AnsiConsole.Clear();

#endif
		
#if !DEBUG
		await Task.Delay( 500 );
#endif
	}

	public async Task Setup() {
		
		CurrentRound = 0;
		PlayerCostPending = 0;
		LocationDestroyStatus = new Dictionary< GameMapLocation, bool >();
		foreach ( var gameMapLocation in AllGameMapLocations.Value ) {
			LocationDestroyStatus[ gameMapLocation ] = false;
		}
		PlayerCharacter = new PlayerCharacter();
		var npcCount = Range( 49, 52 );
		NonPlayerControledCharacters = new ( npcCount );
		for ( var i = 0; i < npcCount; i++ ) {
			NonPlayerControledCharacters.Add( new NPCCharacter() );
		}
		
		// Chara actions
		PlayerCharacter.PushAction( new ActSeach() );
		PlayerCharacter.PushAction( new ActUseItem() );
		PlayerCharacter.PushAction( new ActMove() );
		PlayerCharacter.PushAction( new ActCheckPlayerStrength() );
		PlayerCharacter.PushAction( new ActViewGameMap() );
// #if DEBUG
		if ( DevelopmentMode ) {
			PlayerCharacter.PushAction( new ActDebugSkipRound() );
			PlayerCharacter.PushAction( new ActDebugAddAllItems() );
			PlayerCharacter.PushAction( new ActDebugKillPlayer() );
			PlayerCharacter.PushAction( new ActDebugKillAllNPCCharacter() );
			PlayerCharacter.PushAction( new ActDebugTerminateGame() );
		}
// #endif

		// Init Anything
		PlayerCharacter.Location = GameMapLocation.Loc_A;
		PlayerCharacter.Hp.StatBaseValue = Range( 80, 120 );
		PlayerCharacter.Hp.SetCurrentValueToMax();
		PlayerCharacter.Attack.StatBaseValue = Range( 2, 4 );
		PlayerCharacter.BattleEncounteringRate.StatBaseValue = Range( 0.25f, 0.4f );
		PlayerCharacter.DiscoverPropsRate.StatBaseValue = 0.6f;
		foreach ( var npcCharacter in NonPlayerControledCharacters ) {
			npcCharacter.Name = Names.GetRandomName();
			npcCharacter.Location = RandomGameMapLocation();
			npcCharacter.Hp.StatBaseValue = Range( 80, 100 );
			npcCharacter.Hp.SetCurrentValueToMax();
			npcCharacter.Attack.StatBaseValue = Range( 1, 3 );
			// 有概率装备一个随机武器
			if ( Chance( 0.6f ) ) {
				npcCharacter.EquipedWeapon = GenerateARandomWeapon();
			}
		}
		
		// TODO: Init GameMapData
		
		
		
#if !DEBUG && MY
		var name = AnsiConsole.Ask< string >( "你的[green]名字[/]是?" );
		AnsiConsole.Prompt(
			new SelectionPrompt< string >()
				.Title( $"[green]{name}[/] 是这样吗?" )
				.PageSize( 10 )
				.AddChoices( [ "是的", "不是" ] ) );
		AnsiConsole.WriteLine( "好的" );
		await Delay( 1f );
		AnsiConsole.WriteLine( "不过，" );
		await Delay( 0.5f );
		AnsiConsole.WriteLine( "我并没有多余的内存用来保存你的名字。。" );
		await Delay( 1f );

		AnsiConsole.Prompt(
			new TextPrompt< int >( "你的角色的[green]年龄[/]将是：" )
				.Validate( age => {
					return age switch {
						<= 0  => ValidationResult.Error( "[red]Age must be positive[/]" ),
						> 120 => ValidationResult.Error( "[red]Age must be realistic[/]" ),
						_     => ValidationResult.Success()
					};
				} ) );
		AnsiConsole.WriteLine( "好的" );
		await Delay( 0.5f );
		AnsiConsole.WriteLine( "但是这并不重要。" );
		await Delay( 2f );
		
		AnsiConsole.Clear();
#endif
		
		// 随机出生地点
		// AnsiConsole.MarkupLine( "系统正在选择出生地点..." );
#if !DEBUG
		await Task.Delay( 1000 );
#endif
		
		// Debug
		// DebugAddAllItemToPlayer();
		// await HostDestroyGameMapLocation();
		// Console.WriteLine( GameEnd_03 );
	}
	
	public async Task MainLoop() {
		CurrentRound++;
		NPCActRemainRounds = NPCActOnEveryRound;
		HostDestroyLocationWarningRemainRounds = HostDestroyGameMapLocationWarningAfterRounds;
		HostDestroyLocationRemainRounds = HostDestroyGameMapLocationAfterRounds;
		HostDestroyLocationPending = null;
		FinalBossDefeated = false;
		
		GameEndReason? endReason = null;
		while ( endReason is null ) {

			if ( CurrentRound > 1 ) {
				AnsiConsole.Clear();
			}
			AnsiConsole.MarkupLine( $"第 [cyan]{CurrentRound}[/] 回合" );
			await Delay( 500 );
			AnsiConsole.MarkupLine( $"岛上剩余幸存者：[#fc8403]{NonPlayerControledCharacters.Count + 1}[/]人" );
			await Delay( 100 );
			AnsiConsole.MarkupLine( $" 当前区域：[green]{TranslateGameMapLocation( PlayerCharacter.Location )}[/]" );
			await Delay( 500 );
			AnsiConsole.MarkupLine( $"  [#828282]{GetGameMapLocationDescription( PlayerCharacter.Location )}[/]" );
			if ( HostDestroyLocationPending is not null ) {
				await Delay( 500 );
				AnsiConsole.WriteLine();
				AnsiConsole.MarkupLine( $" [#ffff05]{GetGameMapLocationWarningDescription( HostDestroyLocationPending.Value )}[/]" );
			}
			AnsiConsole.WriteLine();
			
			// Player Options
			while ( PlayerCostPending < 1 && !DeveloperTerminationSignal ) {
				var avlActions = PlayerCharacter.ListAvailableActions();
				var prompt = new SelectionPrompt< CharacterAction >()
							 .Title( "接下来该干些什么?" )
							 .PageSize( 20 );
				prompt.Converter = action => {
					if ( action.CheckAvailable( PlayerCharacter ) ) {
						return action.Name;
					}
					
					return $"{action.Name} (当前不可用)";
				};
				// prompt.AddChoiceGroup( new ActGroupBattle(), [ new ActNamed( "B1" ), new ActNamed( "B2" ) ] );
				// prompt.AddChoiceGroup( new ActNamed( "----------" ) );
				prompt.AddChoices( avlActions.ToArray() );
				
				var choice = AnsiConsole.Prompt( prompt );
				await choice.Do( PlayerCharacter );
				
			}
			
			// Round relat counter
			CurrentRound++;
			PlayerCostPending = Math.Clamp( PlayerCostPending - 1, 0, 30 );
			NPCActRemainRounds = Math.Clamp( NPCActRemainRounds - 1, 0, NPCActOnEveryRound );
			HostDestroyLocationWarningRemainRounds = Math.Clamp( HostDestroyLocationWarningRemainRounds - 1, 0, HostDestroyGameMapLocationWarningAfterRounds );
			HostDestroyLocationRemainRounds = Math.Clamp( HostDestroyLocationRemainRounds - 1, 0, HostDestroyGameMapLocationAfterRounds );

			if ( NPCActRemainRounds is 0 ) {
				NPCActRemainRounds = NPCActOnEveryRound;
				await DoNPCCharactersAction();
			}
			
			if ( HostDestroyLocationWarningRemainRounds is 0 ) {
				HostDestroyLocationWarningRemainRounds = HostDestroyGameMapLocationWarningAfterRounds;
				HostDestroyLocationPending = GetNextGameMapLocationToDestroy();
			}
			
			if ( HostDestroyLocationRemainRounds is 0 ) {
				HostDestroyLocationRemainRounds = HostDestroyGameMapLocationAfterRounds;
				await HostDestroyGameMapLocation();
			}
			
			// End Game Judgement
			endReason = GetGameEndReason();
		}

		if ( endReason is not GameEndReason.DeveloperTerminationSignal ) {
			if ( endReason is GameEndReason.PlayerDead ) {
				Console.WriteLine( GameOverLogoArt );
			}
			else {
				if ( !FinalBossDefeated ) {
					if ( PlayerCharacter.HasItem< SpecialItemWildLily >() ) {
						Console.WriteLine( GameEnd_02 );
					}
					else {
						Console.WriteLine( GameEnd_01 );
					}
				}
				else {
					Console.WriteLine( GameEnd_03 );
				}
			}
			AnsiConsole.MarkupLine( $"游戏结束：{TranslateGameEndReason( endReason.Value )}" );
			var choice = AnsiConsole.Prompt(
				new SelectionPrompt< string >()
					.Title( "是否要重开游戏?" )
					.PageSize( 10 )
					.AddChoices( [ "是的", "不是" ] ) );
			if ( choice is "是的" ) {
				Game.GameResetRequested = true;
			}
			else {
				Game.GameResetRequested = false;
				AnsiConsole.MarkupLine( "感谢您的游玩！" );
			}
		}
	}
	
	private async Task DoNPCCharactersAction() {
		AnsiConsole.Clear();
		AnsiConsole.MarkupLine( "[cyan]NPC行动[/]" );
		await Delay( 1f );

		if ( NonPlayerControledCharacters.Count > 7 ) {
			AnsiConsole.MarkupLine( "其他[cyan]幸存者[/]相互残杀。" );
			await Delay( 1f );

			var deadNpcCount = Range( 3, 5 );
			while ( deadNpcCount > 0 && NonPlayerControledCharacters.Count > 7 ) {
				var npcCharacter = RandomListElement( NonPlayerControledCharacters );
				deadNpcCount--;
				if ( npcCharacter != null ) {
					var npcName = npcCharacter.Name is null ? npcCharacter.GetHashCode().ToString() : npcCharacter.Name;
					NonPlayerControledCharacters.Remove( npcCharacter );
					AnsiConsole.MarkupLine( $"[red]x[/] {npcName} 死掉了。" );
					await Delay( 0.25f );
				}
			}
			await Delay( 1f );
		}
	}
	
	private async Task HostDestroyGameMapLocation() {
		var locationToDestroy = HostDestroyLocationPending;
		if ( locationToDestroy is GameMapLocation gameMapLocation ) {
			HostDestroyLocationPending = null;
			LocationDestroyStatus[ gameMapLocation ] = true;
			AnsiConsole.Clear();
				
			var currentBackground = AnsiConsole.Background;
			AnsiConsole.Background = Color.White;
			await Delay( 300 );
			AnsiConsole.Background = currentBackground;
			await Delay( 0.5f );

			var desc = GetGameMapLocationDestroyDescription( gameMapLocation );
			AnsiConsole.MarkupLine( desc );
			await Delay( 2.5f );

			var npcToDestroy = new List< Character >();
			foreach ( var npc in NonPlayerControledCharacters ) {
				if ( npc.Location == gameMapLocation ) {
					npcToDestroy.Add( npc );
				}
			}
			
			foreach ( var npc in npcToDestroy ) {
				AnsiConsole.MarkupLine( $"[red]x[/] {npc.Name} 没能幸免" );
				await Delay( 25 );
			}

			if ( PlayerCharacter.Location == gameMapLocation ) {
				AnsiConsole.WriteLine();
				AnsiConsole.WriteLine();
				await Delay( 0.5f );
				PlayerCharacter.Hp.StatValueCurrent = 0f;
				AnsiConsole.MarkupLine( $"你没能逃离[yellow]{TranslateGameMapLocation( gameMapLocation )}[/], 死掉了" );
				await Delay( 3f );
			}
			else {
				await Delay( 1f );
				AnsiConsole.WriteLine();
				AnsiConsole.MarkupLine( $"你不在[yellow]{TranslateGameMapLocation( gameMapLocation )}[/]，逃过一劫！" );
				await Delay( 2f );
			}
		}
	}

	
	private GameEndReason? GetGameEndReason() {

		if ( DeveloperTerminationSignal ) {
			return GameEndReason.DeveloperTerminationSignal;
		}

		if ( FinalBossDefeated ) {
			return GameEndReason.TheLastOfUs;
		}
		
		// survivor check
		var playerLeft = PlayerCharacter.Alive;
		var anyNPCSurvivorLeft = false;
		foreach ( var npcCharacter in NonPlayerControledCharacters ) {
			if ( npcCharacter.Alive ) {
				anyNPCSurvivorLeft = true;
				break;
			}
		}

		if ( !anyNPCSurvivorLeft && playerLeft ) {
			return GameEndReason.TheLastOfUs;
		}

		if ( !playerLeft ) {
			return GameEndReason.PlayerDead;
		}
		
		return null;
	}
	
	
	#region Static

	public static async Task Delay( int ms ) {
		if ( !NoTaskDelay ) {
			await Task.Delay( ms );
		}
	}
	
	public static async Task Delay( float s ) {
		if ( !NoTaskDelay ) {
			await Task.Delay( TimeSpan.FromSeconds( s ) );
		}
	}

	public static void PushPlayerCost( int cost ) {
		PlayerCostPending += cost;
	}

	public static void PrintGameMap() {
		Console.Write( GameMapArt );
		AnsiConsole.Write( "\n" );
	}

	public static void AnyButtonToContinue() {
		var spinner = Spinner.Known.Dots;
		AnsiConsole.Status()
				   .Spinner( spinner )
				   .SpinnerStyle( Style.Parse( "yellow" ) )
				   .Start( "[cyan]任意键返回...[/]", _ => {
					   Console.ReadKey( true );
				   } );
	}

	public static async Task AvgDialog( string text, bool waitForAnyButton = true ) {
		AnsiConsole.Clear();

		// // 创建一个面板来显示对话
		// var panel = new Panel( $"[yellow]{text}[/]" ) { Border = BoxBorder.Rounded, Padding = new Padding( 1, 1 ) };
		//
		// // 添加角色名称
		// if ( character != null ) {
		// 	panel.Header = new PanelHeader( $"[blue]{character}[/]" );
		// }
		//
		// // 显示对话面板
		// AnsiConsole.Write( panel );
		//
		// // 显示提示
		// AnsiConsole.MarkupLine( "\n[grey]按任意键继续\u21b5[/]" );
		// Console.ReadKey( true );
		
		await AnsiConsole.Live( new Panel( "" ) )
						 .StartAsync( async ctx => {
							 var progress = new StringBuilder();
							 var index = 0;

							 // 模拟启动过程
							 for ( var i = 0; i <= text.Length; i++ ) {
								 // 更新进度条
								 progress.Clear();
								 
								 for ( var j = 0; j < index; j++ ) {
									 progress.Append( text[ j ] );
								 }
								 
								 // 创建面板内容
								 var panel = new Panel( new Text( $"{progress}" ) )
											 .Border( BoxBorder.Rounded )
											 .Padding( 1, 1 )
											 .BorderStyle( Style.Parse( "blue" ) );
								 
								 ctx.UpdateTarget( panel );
								 await Task.Delay( 75 );

								 index++;
								 if ( index > text.Length ) {
									 break;
								 }
							 }
							 
						 } );

		if ( waitForAnyButton ) {
			AnsiConsole.MarkupLine( "\n[grey]按任意键继续\u21b5[/]" );
			Console.ReadKey( true );
		}
	}

	public static Character? KillNextNPCCharacter() {
		Character? characterKilled = null;
		foreach ( var character in NonPlayerControledCharacters ) {
			if ( character.Alive ) {
				character.Hp.StatValueCurrent = 0;
				characterKilled = character;
				break;
			}
		}

		if ( characterKilled is not null ) {
			NonPlayerControledCharacters.Remove( characterKilled );
		}

		return characterKilled;
	}
	
	private static Lazy< Type[] > AllItemType = new ( () => {
		// 获取当前程序集
		var assembly = Assembly.GetExecutingAssembly();

		// 查找符合条件的类型
		return assembly.GetTypes()
					   .Where( type =>
						   // 不是抽象类
						   !type.IsAbstract &&
						   // 继承自Item
						   typeof( Item ).IsAssignableFrom( type ) &&
						   // 不是Item本身
						   type != typeof( Item )
					   )
					   .ToArray();
	} );
	private static Lazy< Type[] > AllWeaponItemType = new ( () => {
		// 获取当前程序集
		var assembly = Assembly.GetExecutingAssembly();

		// 查找符合条件的类型
		return assembly.GetTypes()
					   .Where( type =>
						   // 不是抽象类
						   !type.IsAbstract &&
						   // 继承自Item
						   typeof( Weapon ).IsAssignableFrom( type )
					   )
					   .ToArray();
	} );
	public static void DebugAddAllItemToPlayer() {
		foreach ( var itemType in AllItemType.Value ) {
			try {
				var instance = Activator.CreateInstance( itemType ) as Item;
				PlayerCharacter.PushItem( instance );
			}
			catch ( Exception ) {
				// Console.WriteLine( e );
			}
		}
	}

	private static readonly Lazy< Type[] > RandomItemTypePool = new ( () => {
		// 获取当前程序集
		var assembly = Assembly.GetExecutingAssembly();

		// 查找符合条件的类型
		return assembly.GetTypes()
					   .Where( type =>
						   // 不是抽象类
						   !type.IsAbstract &&
						   // 继承自Item
						   typeof( Item ).IsAssignableFrom( type ) &&
						   // 不是Item本身
						   type != typeof( Item ) &&
						   // 不是SpecialItem或其子类
						   !typeof( SpecialItem ).IsAssignableFrom( type )
					   )
					   .ToArray();
	} );
	public static Item GenerateARandomItem() {
		var itemType = RandomArrayElement( RandomItemTypePool.Value );
		return Activator.CreateInstance( itemType ) as Item;
	}
	
	public static Weapon GenerateARandomWeapon() {
		var itemType = RandomArrayElement( AllWeaponItemType.Value );
		return Activator.CreateInstance( itemType ) as Weapon;
	}

	public static Character GetARandomNpcCharacter() {
		var npcCharacter = RandomListElement( NonPlayerControledCharacters );
		return npcCharacter;
	}

	public const string FinalBossName = "外星老爹";
	public static Character GenerateFinalBossCharacter() {
		var boss = new NPCCharacter();
		boss.Location = GameMapLocation.Loc_E;
		boss.Name = FinalBossName;
		boss.Hp.StatBaseValue = Range( 120, 200 );
		boss.Hp.SetCurrentValueToMax();
		boss.Attack.StatBaseValue = Range( 5, 10 );
		boss.EquipedWeapon = Chance( 50 ) ? new WeaponSledge() : new WeaponGrenade();
		return boss;
	}

	private static Lazy< GameMapLocation[] > AllGameMapLocations = new ( () => {
		return Enum.GetValues( typeof( GameMapLocation ) ).Cast< GameMapLocation >().ToArray();
	} );
	public static GameMapLocation RandomGameMapLocation() {
		return RandomArrayElement( AllGameMapLocations.Value );
	}
	
	private static Lazy< SearchEvent[] > AllSearchEvents = new ( () => Enum.GetValues( typeof( SearchEvent ) ).Cast< SearchEvent >().ToArray() );
	internal static SearchEvent GetNextSearchEvent() {
		if ( NextSearchEventPending is not null ) {
			var next = NextSearchEventPending.Value;
			NextSearchEventPending = null;
			return next;
		}
		
		if ( Chance( PlayerCharacter.BattleEncounteringRate.StatValue ) ) {
			return SearchEvent.EnemyEncounter;
		}
		
		if ( Chance( PlayerCharacter.DiscoverPropsRate.StatValue ) ) {
			return SearchEvent.ItemFound;
		}

		return SearchEvent.Nothing;
	}

	private static GameMapLocation? GetNextGameMapLocationToDestroy() {
		var aliveLocationCount = 0;
		foreach ( var kv in LocationDestroyStatus ) {
			// 特殊
			if ( kv.Key is GameMapLocation.Loc_E ) {
				continue;
			}
			
			if ( kv.Value is false ) {
				aliveLocationCount++;
			}
		}

		if ( aliveLocationCount > 1 ) {
			GameMapLocation? locationToDestroy = null;
			var locations = LocationDestroyStatus.Keys.Where( loc => LocationDestroyStatus[ loc ] is false ).ToList();
			locations.Remove( GameMapLocation.Loc_E );
			ListShuffle( locations );
			locationToDestroy = RandomListElement( locations );
			return locationToDestroy;
		}

		return null;
	}

	public static bool IsLocationPropCollected( Character character, GameMapLocation location ) {
		switch ( location ) {
			case GameMapLocation.Loc_A: return character.HasItem< SpecialItemPremonitionFishingRod >();
			case GameMapLocation.Loc_B: return character.HasItem< SpecialItemInvisibleCoin >();
			case GameMapLocation.Loc_C: return character.HasItem< SpecialItemCursedAnchor >();
			case GameMapLocation.Loc_D: return character.HasItem< SpecialItemBearBell >();
			case GameMapLocation.Loc_E: return character.HasItem< SpecialItemGoodLuck >() && character.HasItem< SpecialItemBadSign >();
			case GameMapLocation.Loc_F: return character.HasItem< SpecialItemLuckyWhaleOil >();
			case GameMapLocation.Loc_G: return character.HasItem< SpecialItemWildLily >();
			case GameMapLocation.Loc_H: return character.HasItem< SpecialItemDivineBody >();
		}

		return false;
	}

	public static SpecialItem GenerateLocationProp( GameMapLocation location ) {
		switch ( location ) {
			case GameMapLocation.Loc_A: return new SpecialItemPremonitionFishingRod();
			case GameMapLocation.Loc_B: return new SpecialItemInvisibleCoin();
			case GameMapLocation.Loc_C: return new SpecialItemCursedAnchor();
			case GameMapLocation.Loc_D: return new SpecialItemBearBell();
			case GameMapLocation.Loc_E: return Chance( 50 ) ? new SpecialItemGoodLuck() : new SpecialItemBadSign();
			case GameMapLocation.Loc_F: return new SpecialItemLuckyWhaleOil();
			case GameMapLocation.Loc_G: return new SpecialItemWildLily();
			case GameMapLocation.Loc_H: return new SpecialItemDivineBody();
		}

		return null;
	}
	
	public static string TranslateGameMapLocation( GameMapLocation location ) {
		switch ( location ) {
			case GameMapLocation.Loc_A: return "\u2460  - 码头";
			case GameMapLocation.Loc_B: return "\u2461  - 聚落";
			case GameMapLocation.Loc_C: return "\u2462  - 废弃船厂";
			case GameMapLocation.Loc_D: return "\u2463  - 山道";
			case GameMapLocation.Loc_E: return "\u2464  - 山顶庙宇";
			case GameMapLocation.Loc_F: return "\u2465  - 灯塔";
			case GameMapLocation.Loc_G: return "\u2466  - 墓地";
			case GameMapLocation.Loc_H: return "\u2467  - 离岛";
		}

		return null;
	}

	public static string GetGameMapLocationDescription( GameMapLocation location ) {
		switch ( location ) {
			case GameMapLocation.Loc_A: return "破旧的码头上，没有活人的气息。仅有的一艘帆船搁浅在礁石中间。";
			case GameMapLocation.Loc_B: return "这曾经是一个小渔村，不过明显不在有人居住。海风拉扯着屋外破洞的渔网。一只野狗从围墙后面探出脑袋，警觉地看着你。";
			case GameMapLocation.Loc_C: return "低矮的围墙里堆满了杂物，废弃的渔船像怪兽一样趴在干涸的坑里。";
			case GameMapLocation.Loc_D: return "山道上的树木浓密，月光根本透不进来，像走进一条看不到头的隧道。";
			case GameMapLocation.Loc_E: return "山顶出乎意料的宁静，只有一座破庙，半边的屋檐已经坍塌。殿中金刚像在月光中狞笑。";
			case GameMapLocation.Loc_F: return "这是一根漆黑的石塔。如果没有塔顶的灯光，它也许只能是一块诡异的礁石。";
			case GameMapLocation.Loc_G: return "墓碑像野草一样插在松软的土地上，上面的不是文字，而是一堆人类无法理解的线条。";
			case GameMapLocation.Loc_H: return "远处看来碧绿的地面，居然是浓密的海草。到处是粘稠的汁液，一不小心就会滑落到海中。";
			default: return "*区域描述占位符*";
		}
	}
	
	public static string GetGameMapLocationWarningDescription( GameMapLocation location ) {
		switch ( location ) {
			case GameMapLocation.Loc_A: return "远处的海平面上一堵水墙，正在向码头靠近。";
			case GameMapLocation.Loc_B: return "奇怪的气味在聚落中蔓延。";
			case GameMapLocation.Loc_C: return "废弃的船舱里传出了轻微的蜂鸣声，声音越来越大。";
			case GameMapLocation.Loc_D: return "山道上到处都是野兽奔逃的声音，鸟们也从树林中飞出。";
			case GameMapLocation.Loc_E: return null;
			case GameMapLocation.Loc_F: return "灯塔的灯光不安地闪烁起来。";
			case GameMapLocation.Loc_G: return "墓碑下面传来低沉的呜咽声。";
			case GameMapLocation.Loc_H: return "海水开始上涨，离岛正在一肉眼可见的速度缩小。";
			default: return "*区域警报占位符*";
		}
	}
	
	public static string GetGameMapLocationDestroyDescription( GameMapLocation location ) {
		switch ( location ) {
			case GameMapLocation.Loc_A: return "巨浪袭来摧毁了码头。";
			case GameMapLocation.Loc_B: return "大量毒气从水井中涌出，杀死了聚落中的一切活物。";
			case GameMapLocation.Loc_C: return "废船上的炸药被引爆，这个船厂化为了一片火海。";
			case GameMapLocation.Loc_D: return "山中传来巨响，山体滑坡，山道被彻底摧毁。";
			case GameMapLocation.Loc_E: return null;
			case GameMapLocation.Loc_F: return "灯光熄灭，紧接着的是脚底传来的震动。在站稳之前，灯塔已经彻底从视野中消失。";
			case GameMapLocation.Loc_G: return "封土中爬出了有什么东西，战斗的人一个个倒下，最后只剩下一些蠕动的黑影。";
			case GameMapLocation.Loc_H: return "离岛被海水吞没了。原本是海岛的地方，只留下海面上白色的泡沫。";
			default:                    return $"老爹主机摧毁了地点：{TranslateGameMapLocation( location )}";
		}
	}

	public static string TranslateGameEndReason( GameEndReason gameEndReason ) {
		switch ( gameEndReason ) {
			case GameEndReason.PlayerDead: return "你死掉了";
			case GameEndReason.TheLastOfUs: return FinalBossDefeated ? $"你击败了{FinalBossName}" : "你是最后的生还者";
		}

		return null;
	}


	private static readonly System.Random _rng = new ( Environment.TickCount );
	
	public static int Range( int min, int max ) => _rng.Next( min, max );
	
	public static float Range( float min, float max ) {
		return min + NextFloat( max - min );
	}
	
	public static float NextFloat() {
		return ( float )_rng.NextDouble();
	}
	
	public static float NextFloat( float max ) {
		return ( float )_rng.NextDouble() * max;
	}
	
	public static int NextInt( int max ) {
		return _rng.Next( max );
	}
	
	public static bool Chance( float percent ) {
		return NextFloat() < percent;
	}
	
	public static bool Chance( int value ) {
		return NextInt( 100 ) < value;
	}

	
	public static int RandomArrayIndex< T >( T[] array ) {
		if ( array == null || array.Length == 0 )
			return - 1;

		return Range( 0, array.Length );
	}
	
	public static int RandomArrayIndex( int startIndex, int maxIndex ) {
		return Range( startIndex, maxIndex + 1 );
	}
	
	public static T RandomArrayElement< T >( T[] array ) {
		if ( array == null || array.Length == 0 )
			return default ( T );

		return array[ RandomArrayIndex( array ) ];
	}
	
	public static T RandomListElement< T >( System.Collections.Generic.List< T > list ) {
		if ( list == null || list.Count == 0 )
			return default ( T );

		return list[ RandomArrayIndex( 0, list.Count - 1 ) ];
	}
	
	public static void ListSwap< T >( System.Collections.Generic.IList< T > list, int i, int j ) {
		var temp = list[ i ];
		list[ i ] = list[ j ];
		list[ j ] = temp;
	}
	
	public static void ListShuffle< T >( System.Collections.Generic.IList< T > list ) {
		for ( var i = 0; i < list.Count - 1; i++ ) {
			ListSwap( list, i, _rng.Next( i, list.Count ) );
		}
	}

	#endregion


	#region Art

	private const string GameLogoArt = "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMHMMMMMMMMMMWMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMYMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMM]   .MMMMMN   .JMMM3?MMMMMMMMMMMMMHMMMMMMF   (TN._7\"M3  7MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMM]   MM#^TMN   JMM\"    (MMMMMMMMMMMMNa,  !   .NMM!  -M}  .dMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMM]   Y5....d   (! ..(gM#MMMMMMMMMMMMMMM@     .TMM`  JM}  .MMFMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMM]   MMMMMM#   JMMMMMM#`MMMMMMMMMMMMM@! ..N,   .M  .MM}  .MF MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMM]   MMMM\"9\"   (MMMM\"=  7MMMMMMMMMM\"..Jb`?7\"N,..F  dMM!       MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMN~``       ..jMN            .MMMMMMMMNMMMMM#   dM#HM!.MMMMN......MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMN.   ..&MMM`   ?Ngg++++gggMMMMMMMMMMMMMMMM@   d^   TMMMMMMD  7MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMNMMM,?\"YM#  .MMMMM#! ?MMMMMMMMMMMMMMNNMMF    dMMMMMM,MMMM'    dMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMM[   gNNNNNNNNN]    (MMMMMMMMMMMMMMMM`    . ?TMMM|(MMF   .MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMM[   MMMMMMMMMMF   JMMMMMMMMMMMMMMMM\\     db   WMb ,9   .MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMM[   JJJJJJJJJJ,   (MMMMMMMMMMMMMMM% .g   dN, .dMMp    .MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMM}   MMMMMMMMMMF   .MMMMMMMMMMMMMM^.+M#   dMMMMMM#'     .7TMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMM!   MMMMMMMMMMD   .MMMMMMMMMMMM#.MMMM#   JMMMM@' ..N,     ..MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMM.  .MMMMMMMMMMF  ..MMMMMMMMMMMMMMMMMM@   JM91..gMMMMMMN...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM=7MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMF7TMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMF dMMMMM`.MMMMM# .MMMMM]```````(MM  ....... .MMMMM#\"\"\"\"\"\"  \"\"\"\"\"\"\"4MMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMF _!!!!!  !!!!!! (MMMMM] MMMMM .MM .MMMMMMM .MMMMM# .MMMMMMMMMMM# (MMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM#74MMMMMMM`.MMMMMMM@74MMM] MMMMM .MM .MMMMMMM .MMMMM#  ............ (MMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMF JMMMMMMM`.MMMMMMMF JMMM] \"\"\"\"\" .MM          .MMMMM# .MMMMMMMMMMM@ (MMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMb....................JMMM] &&&&& .MM .MMMMMMM .MMMMM# ..............(MMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM#\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"MMMMMM] MMMMM .MM .MMMMMMM .MMMMM# .\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"\"WMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMNNNNNNNNNNNNNNNNNMMMMMM] MMMMM .M#  ??????? .MMMMM# .MMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM@                      JMM]       .M] dMMMMMMM .MMMMM#.... ............ JMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM@?TMMM` MMM\"?WMMMMMMM] MMMMMMM@ .MMMMMMMM .MMM# (MMM# .MMMM .MMMM] dMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\"`.(MMMM` MMMMN, (YMMMMMMMMMMMMF .MMMMMMMMM .MMM# (\"\"\"5 .\"\"\"\" .MMMM!.MMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN...MMM#\"\"\"`.MMMMMMMm..MMMMMMMMMB! .MMMMMN7777 .MMM# .gggggggggggm\"\"\"= .MMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMNggNMMMMMMMMMMMMMMMMMMMMMNMMMMMMMMNNNNMMMMMMMMMMMMMMMMMMMMNgggMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM\nMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMBY Kanbaru&oRanMMMMMM";
	private const string GameMapArt = ".,7777,.\n ... ..                                                        ,\"^        (777(.\n,     ,+                                                    .?^                 ?7i...\n(,      6.  .,74.J(,,                                     .=                         .3..\n j      .h7^    ~  .`=iJ<,   `  `  `  `  `  `          `.?`                               71,.  `    `    `    `\n.x4W                       ?7i.,               `  `  .27              `                       _7,                 `  `\n(v!  `                            ?=...    .J??<7777^!            `        `    `                ?7..\n t       `   `                         ``\u2462                                                         ?(.  `  `\n .l         `   `  `  `                                        `     `  `     `\u2464  `  `  `             ?i,     `\n   4.   `               `  `  `  `  `        `                    `                         `             7,      `\n    1.                                                 `   `               `                    `          .o        `\n     4,     `   ` \u2466`                     `     `  `          `                `  `      `         `         5.\n       4,              `  `  `  `  `  `      `                   `   `  `            `                        .t  `\n        O  `  `  `   `                 `              `   `                 `               `  `               ,&\n   `     =.        `       `  `     `    `  `                `               \u2463   `     `            `           ??7.,\n     `    .4,  `       `  `     `             `  `              `  `           `                  `     `            .T.\n            .i                                      `  `              `  `           `     `               `     \u2465    j\n  `           6.(gt...(??77771(.. `  ` `  `  `            `                 `                 `               `        ,\n     `  `                        .?i.                        `    `              `      `        `   `                (`\n           `        `               (71..   `   `  `                   `            `      `            `        ` ..=\n                       `                 .7i.         `  `     `          `  `                             `  ` .J=\n  `  `  `    `  `  `     `  ` `  `            7.        \u2461  `      `            `      `      `  `  `          ,^\n                      `            `  `        `7..                   `            `                  ...     ,`\n         `  `  `  `       `    `         `  `     .6.   `                    `            `         .:  ` ?=\"`\n   `  `                                             ,i.      `  `        `            `      `     `.=               `\n                      `                         \u2460     7-           `           `               .77`\n                                                         ?-                  `      ` ..((((J7?^\n            `   `  `                                       ?,          `            .,^\n       `                                       `             L   `        `       .?`\n    `                                             `           G                `.=\n          `           `                                        .1,   `       `.=\n               `   `                           `                  4,         .}           `   .}h,./1....<+.\n\u2460  码头                                              `   `         ,+   `     ]              =(JHJ!        +\n\u2461  聚落     `                                                       ,,    ..c?`               \"4]          .6\n\u2462  废弃船厂                                     `            `      .J' .jVYjR`         `       b       \u2467     7,\n\u2463  山道         `    `                             `           `  .)  J /hJ?`                  77i.            6\n\u2464  山顶庙宇                                              `           .:?L Jt         `     `       ,(..         .\\\n\u2465  灯塔      `     `                           `                .J' `   ,\",,                         ,i       J`\n\u2466  墓地`                                             `     ` ..?`         .%                             7(..?'\n\u2467  离岛   `                                                  t           .:      `     `\n..............................................................JUw&&+J(vI-...............................................";

	private const string HeadArt = "}       M!  J%       ..W8UwJHZO?1=1z6CTTWM$`                           .........zUMMHMMMMMMMMMMMMMMMMMM\"!.\"WMMMMMMMMMMMY\n)       .OH\"5a,    (7kWkzzzjMkwI1?<<;;:___ `           ...J~~  __<-(.-.JQJ<(Gz777TTTTTWNMMMMM#zJMMMM#=       ?WMMMMMMD`\n)       `H,..J^       (TAz1101ZC<<<:_~__`    ``..<Z^-.((+qV7U1zjd#?CzBP7c=<<!!~``(MMMn?TMNHMMMMNd#=             ?HMB!\n)         -\"!?4,         ,5<0;::_~~___.     `.(gT1+<~?6d~?~``(. l...(JSv^(`-``. ..JMMMR--?TNHHMMMMN,              `\n\\  `      Tn.(#.      .Jkkkwkwvz+<<<_ `    `.MM=.~! ` ?dc.`    -.........--.-(JgUdl?MMMK.-,1WNH.....?+, `  `         `\na(J&g.`    H! `J\\ `  .MMMHX1zz<(<`_      ` (M@~  `.' `.d#(-,~__-.-.(..+x&.,((NrBsM8b(NNMe,~ .dMNk, ..._~-.        `  .\nMMMHM    ``(WHMN7' ` `(HUOI<1+<(---     `.(M3``` ``-.-(MM+ORuNmdP7_?WXHD+J5IdNLWE?3GdMNNMp-...?MNHMM%`` .m,       ...\nMMMNP`     .JT8HY\"!   ,#VVZ?wzz<<(-__   .d#V_.-; ,._ tdMMddNnJ4!_-_..(.-JMfiMM#D!`(MMMMMMMMMNm&-dMm7HHMMMM)Q..`    `\nMHkV$     `.y4+JZY\"=   (sswyXdkkXw&&/ `.dMK .dMHdF,YT&(-(7\"JTNMMt?%`-MMMM5+MM#B>..NMMHMMMMMNHNM#HWTOY99wg+JvT\"5,`\nMMHu]  `                (QWHHXX3....(k.dM#!`.dXMZ?=?>!_`<..(JWMNqkagmQMM8V9Tz17YT7TU0V7<~$[>:QgkY\"!` _(CdZvUMMMMNJ.    .\nNkzv]    `               ?KXW0XNgg-(.W5MMP.gMM5...+f0wmvTYY\"9=!<+._.-(z_~!` _.(b..-(Je._((k#=``.. `` `..jl7O&wQdMH#JjdMM\nNW++]`..Y4,        ...  ` jywXXHMMMMM#X3<?<<?1i<.-(7?HYX_1.udJ.-7?<787D~` ``(_!!  -.._7^._~_-.`   `` .-~   ~_<jsX#dMmgAW\nNWzOhvkvkVz.   R,  (31z7UUGHXwXXV6OzWHZT?/\"9ZGJ-.......V\"!.``` `` (_~__   . `.. ``-,``` !_ `+?<` ```` ` _. `` ?-?CXA++vT\n#UCC<I<<v<(v, ,<(v&.C+=zzOwQW93icCJ~< , ` ` `.! ` .a,6J_``    ` - <(.` `  ``````  ` ``` ` _ . ~.`` ```      `  `___1wzVU\n@1z6+u&OZOz&dJSwTCzjJXvQgM91JvCx?</!(H= ..``^`.x,_ !` ` ` ``   <_( ` ``````  ``` `!`` `````.`  `````  `  ``` ``. ` ~-<CT\nNWWWV4RXHWXXWV1&dM#SgMM9GJ01(zQf(H{ ,! (3i_ (7\"=Y`  `  `~  `  _<~  .``````` `````  ``` ` ````` ` `````` `  `  `   `` _ j\nNHgHWHHHMqHkWHHHmdMM9ndZ=+VIq#3vF_ _'. `_` ` `` ` `  . `` ` `   _.` . `````````` ``  ` `..````  !` ```` ``  ``  ``` `` _\nMHWMHMNMMMMHh+MMW9&XZ=(d6wqH=ji#; `  ` ``  `  ``      ``   `..`   ``. `` ``. ``` . `` ` ```` ``` `       !   ``` `` ` ``\nMMNMMMNMMgMMMY4JUf3(d@uZ=.Y-+W@7!`   `   `   `  .` `    `  .!!``` -   `.` `` ` ```  `` ` `` ~.~.  ` ` `` ` ```` `` `\nMMMMMNgMNMBAdHZ7(gMHH=`.d=(j\\`` .`   `  `   `    ```  ```  ?_  ` ` ``   `. `  `  ``  ` ```` `. ._ `` ` ` ! `  ``  ..:- .\n#HgMMM#YqdHY=+d#MMY7`..ND(%_ `  ``   ` ``` `` ``    `` `` <_  ``._ ```. ``  `   `  ``  ```` ` ` `` `  ` `` `` `   ` ~`(+\nMMMH4J9WTC(gMMMB=  .x(MM68r~` `   `` ` ``` .``` `` ```` .````` ``` `` .```  . ``` ` `` ` `````  `.{```` ````   ..Jv\"7!\n@uuWXV:(dMMM#=  ..IdWMMHJg]. ``   ` ` `  ,|~  _-.``  ``(I . (- ` ``````` +A,` ` ``` ``````` ` ` .(``` `...gY\"\"!\n#sY>(dNMMMD` `.J+WD(X#dMNdN/.  ```  ., _`.    `.3`   ``. `-`_< ._` ` ` ``-= `  ` ` ``  `` `` ``.t.....J9!      .\nD+gMWMMY=   .XdVCzJXHB1VKHHD^````` `(%   `` ``   `.(``       ``  _`` `` ```````........(+ggNMMMMMMMB\"d'\nNMMNB\"   (VsX9ZuqHXW0>~dy ``~??\"\"\"\"\"\"4kOagJ(-...``,).`  ...........JJ+gggNNMMMMMMMMMMM\"B\"\"77<-(..(gsu!    `         ...?\nMMD!   .jdH61jWMHHWI(_`-6.                    ?OZWMMMMMMMMMMMMMH\"\"\"9\"\"\"\"=7<<~-((((JJgggNMMMMMMH\"B\"!-J       ...  ` ``` `\nY'  .-uWK6<uWMMH5<JJ?_..(         `-            (--(((.....JJJJ&ggggNNNMMMMMMMMMH\"\"\"77<(((J+ggNMMM@(b.~<~` ```  ` .``\n) .zdMXC1+MMHHD((jXMh.JK<J.                      ,/7\"\"\"\"\"\"\"7777<<<(((((((J+&ggNNMMMMMMHB\"\"\"<!`.JVT7=! _````    ...-__\nKGdH$JidWNMM8(+jZ<(> TUXW$=......... ... `  `  ` `,PTMMMMMMMMMMMMMMH\"\"\"\"\"\"7<!~__  ...``  ` ` `` `   ``  ` . -!!?C `````-\nMZ5CudMMMMB1v1J3;jZ.``````````````````   ``` `- __??7\"\"\"!!`   ` ``.  ..` .__`` `  .` ` _.` ..  ..-.~!~`.....```` ...(.NM\n#z+XNMMMB+zOv^-(JOWp....... ``.``` ``` _.``` ````` `` `  .`   ``   ``._  _ - `.-._--(~mv `.j? ..(J+ggMMMMMNHNgkNmHMMHQgM\nNMMMMMB1vzu3~(dHfzdNje-.... __~!~??<~<_(-.r.   .--_. .  .:.j+__~(~!~~Z'._``  ...-- _d_!_`...JdMMMMMMMMMMNMMMMMMNNMMMMMMM\nMMMMB1d$QY<<idNXc<1MMNNWU3((....-. -`.  .!<..gggggggggNNMNmJ: `.z;  -!_` ...((.((g+gHk9YT\"4MMMMMMMMMMMMMNNMMMMMMNMMMMNNM\nMM@+d8uZ<(jMMWNZ<:<WMMMMMMMMMMMNMMMMNm&+NMMNmMMMMMMMMMMMMMMN--qdMWYYB\"\"\"77?!``     .`. _  -MMNMMMMMNMMMMMN?MMMMMMMTMMHgN\n@+uBwXI>jdMMMW3(Ji?~?WMNMMMMNMMMMMMMMMMNMHMNMKSMMMMMMMMMMMM#MNM-                       ~  (MMMMMMMMMMMMMMMjMMMMMNMMMMMMM\nHBGk51dgMMMMK<+vJ>(C` OMMNNMMMMMMNNMNNNNMMMMMMM#MMMMMMMMNMMMNNNL`                      .-`JMMMMMMMMMM#MMM#dMMMMMDTSagMMN\nKX9J+gMMMMM6+z1z<j3 ..X7MMMMMMMMMMMMMMMMMMMMMMMJMMMMMMMMMMMMMMMN    . .              ._   dMMMMMMMMMMMMMM@MMMMMMMHQNNMMM\n#vOQMMMNM#ZuYj3.Z=  (W>J~?MMMMMMNNNggggJMMMMMMMkMMMMMMMMMMMMMMMM;  .( `` ` ` ........(J+gNMMMMMMMMMMMMMMHVMNQgNMMMMMMMM9\ntudMMNMM8wd6v!j9'` J9~J``  7MgHMMMMMMMMNVMMMMMMNHMMMMMMMMMMMMMMMN++gNNNNMMMMMMMMMMMMMMMMMMMMMMB9\"\"7<z><b(_!dMMMMMMB\"<.JM\nHHMMMM#0AKjY<X$` .dC_>_` ` (MM5ggXWHWMMMMNMMMMMMKMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMBY9\"=7!~-(! :`.` - `(``v``.dMB\"!((gMMMMM\nMMNMM9wd8J=Jk=` .wC.>   `.(KOMbHMMMMMMMMNNNggggJA(-~!<<<<<<<<<<!<<<___~._.__. -.`_._..((z_`` ...JLf~` ` `` .(gMMMMMMMHky\nM#MV1GU1K-J8!``.XC(> ``` (HOWWNaJTWMMMMMMMMMMMMb.?!..u-(< .-.`.. .j;_ .._... _.``...--Jd@TWWQNNMNMN(dWQ+NMMMMMMMVn_(MMWm\nMD3zJVJ9.X9~`..w>.! ```.(d6XW4MMMNaJ...-?77TWMMHBO-..gMNNkWWf?zz<jd;-.--..+.(((JJ+gggNMMMMMMMMMMMMMMMMMMHHHRzk,(4+6-_WMH\nK>jK6d^(WC` .d0^(!.` ` _dSWWC.dTHMMMMMMMMMMNNNNgggmmNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMH\"\"\"\"\"^!~`_-7\"\"T{(<  4XWXW,-Tej/-?M\n$+f+Y~jH^ `-yZ!(!  `.V-d6XH6_J>(_ ` _Z0WYMHMMMMMMMMMMMM#\"7777777????!!~`_               ````    _  ..j;_> `jXHyWx-?h?l-(\nH3d=(W9^ ..Xv_J_ ``` _J$dWS!({(! ` ((Owr(} .`   _``` `                                     ``!`  _`   j,(>  ?XWXWo_?WJU-";
	
	private const string GameOverLogoArt = "`` ````` ``````` ``````````` ```````````````````````````` ``` ````````````````````` ``` ``` ``` ``` ``` ``` ``` ``` ```\n` ` ` ` ` `  ` `` `  `  ` ` `` `  `  `  `  `  `  `  `   `  ` `   `  `  `  `  `  ` `` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `\n ``` ` ``` `` `` ` ... `` `` ``` ````    `` `` ``` ` ```````  ``  ... ` ` ``` `` `` ````````` ````````` `` `` ``` ``` ``\n` ` ``` ` ```` `.M#\"\"THN,`` ` .MN, ```JMN.``` `(MN` (M\"\"\"\"\"\"5``.M#\"\"THN,` Mb`````.M^.M#\"\"\"\"\"5`.M#\"\"\"\"MN,` `` ` ` ` ` ``\n`` ` `` `` ` ``(M'`````,M{`` .M^dN.` `J#Wb ` `.MdN``(M_` `` ``(M'`` ``,Mp ,M| ` .MF`.M]`` `` `.M]```` d#` ` `` `` ` ` ``\n `` ` `` `` ` .MF` `.....,``.MF` Mb`` J#.M[ `.Mt(N` (M+(((((..MF` ` ` `JN `dN.``J#` .Mm(((((,`.ML....(M3`` `` ` `` `` `\n` `` `` ` ````.M]`` (\"\"\"MF` dN&&&uMp``J#`,M, dF`(N``(M~`````  Mb ``` ` J#   Mb`.M'``.M]`````  .MF7777TN, `` `` ` `` `` `\n` `` ``` ` ` ``?M,```` .MF`.M`````?M,`J#``?N.#  (N` (M_ ` ` ` ?M,   ` .Mt`` ,M;MF ` .M]`` `` `.M]`` ` MF` ` ` `` ` ` ``\n`` ` ` `` `` `` .TMNgNM\"WF.M%`  ` `WN.J#` `WM'``(M  (MHHHHHHb `,TMNgNM9! `` `dM@` ``.MMHHHHHN`.M]` ` `dN.` `` ``` `` ``\n `` ` `` `` `` ``` `` `` `` ```` ```` `` `````    `````````` ```````````   ` `` ``  `` `` `` `` ` ``` `` `` ``  `` `` ``\n` `` `` ` `` ``` `` `` `` `` ` ``` ` ` `   `` ` ``` `  ` `  `  `` `  ` ````  ` ` ` ` `` `` `` `` `` ` ` `` ` `` ` ` ` `\n` ` `` `` `` `` ` `` `` `` `` ` ` ` ``` ``     ` `` ` ` `` ```` `  `` ` ` ``  `  `` ` `` ` ` ` `` ` `` `` `` ``` ``` ```\n`` `` `` ` `` `` `` `` `` `` ``` ```    ` ` ` `    ` `   ``.In.``` ````` ` ``````   `   ``` `` ``` ` `` ` ` `  `` ` ` `\n `` ` `` `` `` `` ` `` `` `` ````   ```` ` ` ` `` `  `` ` J-.1d-` `` `  ``` `` `````````  `` ``    `` `` ``` `` ` `` ` `\n` `` `` `` `` `` `` ``` `` `   ````` `  ` `   `  ` `  `  `(JIOJ`  ` ` ``` `` ` ```````````_`  ` `    `` ` ` `` `` `` ```\n` ` `` `` `` `` `` `` `` `` ```` `  `  `  ` ` `   ` `  ` ` n+u\\` `  ```` `` `` `` ``````` ```` ```` `` `` `` `` `` `` `\n`` ` `` ` `` `` `` ``` `` ``_`` `` ` `` `` `   ` ` ` `` ` .$1zt` `  ` `` `` `` ` `````````_ ` `` ` `` `` ` `` ` ` ` ` ``\n `` ` `` ` `` `` `  ``` ``  ````` `  `  `  ` `` ` `  `  ``,C(zZ ` ``  ....((,`` `` ````````  ` ` `` ` ` `` ` ` `` `` ` `\n` `` `` `` `` `` ```` -  ` `  `` ``      `  `  `  ` ` `  `.wzZf` .-3(Jz1z&+dX ``` ``````````_`` ` `` `` ` ``` `` ` `` `\n` `` `` `` ``` `` ` ` ```  ```` ` ``_` `` ` ` ` `  ` ` `  j+yWXwv7TTOUwl `zkV+. ````` ``````   `` ` `` `` ` `` `` ` ````\n`` `` `` `` ` `` `` `` `  ``` `` `  ` `  ` ` ` ` ``JCz.((.Ji+zv((((JCI_?!` ?i-(Jn.``  ```````_   ``` ` ` `` ` ` `` ` ` `\n ` ` `` ` `` `` ` `      ``` ``` ``  `` ` ` ` `  ` <zv77<x+.oG+zZ774zv!``````jXS(....J. ````` ``` _   ``` `` `` `` `` `\n` `` ``` ` `` ``` ````  `  ````` ``` ` ` `  `  `  ` ` ` .)`.?~(-r`````` `  ``` ?77Z4J?1ZSa\\`` ````````  ` `` ` ` `` `` `\n`` `` ` `` `` ``  ```` `````` ```  `  ` ` ` ` ` `  ` `   [.,lI  r` `` ```` ` ```````?dWXY^```.  `````` ``` `` `` ` ` ``\n `` ` `` `` `` ` ```   _ ``` `` ````` ` `` ````` `` ` `  [.,{I  [`` `` ` ```` ` ``````````````  ```````_  ` `` `` `` ``\n` `` `` ` `` ```-```   `   ```` ```` ``````` ` ````` ``` r-.{I ?;` `` .++,`` ``````````````````  `` _```_` ` ` ` `` ` ``\n` ` `` `` `` `` ```  `    _````` ``````` `````` ` `` ````t{.{I .}`` `(J6dh. ```````````````````  ``` ```  ``` `` ` `` `\n`` ` `` `` ```  `` `` `` ` -`````` ` ``   `` ````` `` ` `I!,{I .{`````` ``!..,```````````````` -`````````  `` ` `` ` ```\n `` ` `` ` `  ``` ` `  ` `?. ```````` ``````` `` ````````O ,{I`-:` `` ```````>``````` ``````  ``` `````````_`` `` `` ` `\n` `` `` ``` `  ````````    `_<. .````````..``` ```` `` ``j,,lI ,~````````...,`````````````` `````` ```````  ` `` `` `` `\n` `` `` ` `````  ``  ``` ````` _?<+<.. .wC+O,```` `` ````j ,2I.J````````-!```````````.```` `````````````  `` `` ` ` ` `\n`` `` `` ` ` ```  `````` `` ```  ``````` ???T_```````````( ,lI`J ```````` `````````  `    ````````````` ``` `` ` ``` ```\n ` ` `` `` `` ```  ``   ````` ``  ```````````````````````(_(lI dJ..```````````` .````````````````````` `` `` `` ` ` `` `\n` `` `` ` ``` ` ``  ``   ````````    ... ````````` ..vTf7d+JI0(dZlvTG.-... _ ``````````````.```_`````  `` ` ` `` ` ` ` `\n`` `` `` ` ` ``` ``  ```````````````````     . _ .J6J4J?G-__7C~::::::<dS   `````` ``..J7TZK!``  ````  ` `` `` `` `` ```\n `` ` ``` ``` ``  `` _ ```````````````  _`_. ``.vv=<(+Jd$++Z6uJ+<:<++WVW_``  ._`. <!``(77=````````   ` ` `` `` `` `` ` `\n` `` ` ` `` `` `` ```  `` ` ``  `. ` .``_`    (nJVTTXZYT7<<<?7TO&zzldIld)_`   ``.``_   `  ``` `     ``` ` `` ` ` ` `` ``\n` ` ``` `` ` `` ``  ` ```` `  `` ``_ ```.``_.3~~~?yz$<~~~__~:::::+4zS===Zo. `.``_    ``  `   `` ````` _` ` `` ` `` ` ``\n`` ` ``` `` `` ` ` ```  ` ` ````````   `I(.Z::__(:?24_~~_::,<::<;>j0dzl=??dn,``  `    ` ` ````````````  `` ` ``` `` ` ``\n `` ` ` ` ``` ``` ``` `     ` `   `````.V=<S<::::::D?0+<:++hkv4VWZ0XdRzzz??X+h`   ``````  ``````` `````. `` ` ` ` `` ` `\n` `` ``` ` ` ``   ```` ` `` ```` ````.Z7C++?vUx:::+$j1uZ=<~:+ZwY::::::::<?17TUJr,```````````` ````````` _` ``` `` `` ``\n` `` ` `` ``` ``   ``````````````.O-.3~~::(4Ow3;;jduXY<~~~(v1JC:::::+<:::::::;>?6-.``Jw``` ````````````  ` ` `` `` `` ``\n`` `` `` ` `` `` `_````` ``` ````.$(1<_(:::_?AXXdUZC++++++$jIX+u+++Z1vn+++Jz&+&&uZOVuI[ ````` ```````` `` `` ` ` ` ` ` `\n ` ` `` `` ``.... .-............lhXOgjo+++++bwd6(+1+=(((((+z&&&zzz&&Rdzzzz&zzzzzzzzwzyhyJ.............-......`` ` ``` `\n` `` `` ` `` ````` ` `` `` ` ` ``````` `````````````````````````````````````````````````` ` ``` ````````````` ``` ` ````\n`` `` `` `` ``` ``` `` `` `` `` ` ` ``` `  ` `` ` ` ` `` `` ` ` ` ` ` ` ` `  `  ` ` `  ` ` `  ``  `  `  ` ` `  ` ``  ` `";
	private const string GameEnd_01 = "????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????\n????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????\n????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????\n????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????\n????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????\n??????????????????????????1gggggx???gg??????gg??gggggggx??gg??????gg?gg?ggz?????ugcdgggggggx????????????????????????????\n?????????????????????????uMBz?1TMN??dN??????M#??M#CCCOTMNzdMR????uME?dM?JMK????1M#?dMVCCCCCC????????????????????????????\n?????????????????????????dMmz????T1?dN??????M#??M#?????dMC?HNz??1M#??dM??dM2???dM1?dMz??????????????????????????????????\n????????>???>???>???>??????HMMMNgx??dN??????M#??MNNNNNMM$??zMb??dM3??dM???MN??dM5??dMMMMMMMI?????>???>???>???>???>??????\n?????????????????????????gx????1dMR?dN??????M#??M#????zMN???dMsjM8???dM????MR1M@???dMz??????????????????????????????????\n?????????????????????????dMez??1gM8?dMmz??1gM5??M#?????dMz???HNd#????dM????dMd#1???dMz??????????????????????????????????\n?????>????????????????????vTMMMMBC???zTMMMMB3???ME?????dMD???zMMI????dM?????HM$????dMMMMMMME??>?>??>??>??>??>??>??>?>???\n?????????????????>??>?>??????????????????????????????????????????????????????????????????????????????????????????????>??\n??>??>?>>?>>?>>?>??>???>??>????????>?????????>?>??????>?>?>??>??>??>???>?>?>??>?>>??????????>??>??>?>?>?>?>?>?>?>?>???>?\n?>????????????????>??>??>??>??>?>???>??>??>>??>?>>>>?>?>?>?>?>?>?>?>>?>?>?>?>?>??>?>>?>>?>>??>?>?>?>?>?>?>?>?>?>?>?>>??>\n>?>>>>>>>>>?>>>>>>?>>?>>?>??>>?>??>??>>??>??>??>????>???>??>??>?>?>??>?>?>?>?>?>>?>?>>?>?>>?>>?>>?>?>?>?>?>?>?>?>?>?>>?>\n?>??>??>??>?>??>???>?>?>??>?>>?>>>>>>>?>>>>>>>>>>>>>>>>>>>>>>>>>?>>>>>>?>>>?>>>?>>?>?>>?>?>>?>>?>>?>>>?>>>?>>>?>>>?>?>>?\n>>>?>>?>>?>>?>>?>>>?>>>?>>>>?>>?>??>?>>?>??>?>??>?>???>??>??>??>>??>??>>?>?>?>?>?>?>>?>>?>?>?>?>?>>?>>>>?>>?>?>?>?>>?>?>\n>?>>?>>?>>?>>?>>?>>?>?>>??>?>?>>?>?>?>?>>?>>>>>>?>>>>>>>>>>>>>>?>>>>>>>>>?>>>>>>>>>>>?>>>>>>>>>>>?>??>?>>?>>>>>>>>>>>>>>\n>>?>>>?>?>>?>>>>>?>>>>?>>>>>>>?>>>>>>>>>?>>>?>>>>>?>?>>?>?>>?>>>?>>?>?>?>>>?>?>?>?>?>>?>?>?>>?>?>>>>>>>?>>?>?>?>?>?>?>>?\n>>>?>>>>>?>>?>?>>>?>?>>>?>?>?>>?>>?>>?>>>>?>>??>?>>>>?>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n>?>>?>?>>>>>>>>?>>>?>>?>>>>?>>>>?>>?>>?>?>>?>>>>>?>>?>>?>?>?>?>?>?>?>>>>?>>>>>>>?>>>?>>>>>>?>>?>>?>?>>?>>>>>>>>?>>>?>>>>\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n>>>>>>>>>>>?>>>>>>>>>>>>>?>>>>>>>>>>>>>>>>>>>>>>>>><<!~` ````  `~?<<>>?>>>>?>?>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><! ``  `` `  ` ``` ```_!<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><``` `  `  ` ` `  `  `  ` `` !<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>! `  `` `` `` ` ` `` ` ``  `  ```_<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><`` ``  `  ` ` ` ` `  ` ` `` ` `   ``_;>>;>>;>>;>>>>>>>>>>>>>;>>;>>;>>;>>;>>;>>;\n;>;>;>;;>;>;>;;>;;>;;>;;>;>;>;>>>>>>>>;!` `  `` ` ` ` ` ` ` `` `  ` ` ` ` `  ` <>>;>>;>>;>;;>;>;>;>>;>>;>>;>>;>>;>>;>>;>\n>;>;>;>>;>>>;>;>;>>;>;>>;>>>;>;;>;>;;<`` ` `  ` `` ` ` ` ` `  `` ` ` ` ` `` ` ``(;;>;>;>;>>;;>;>;;;>;>;>;>;>;>;>;>;>;>;>\n;>;>>;>;>;>>>>>>>>;>>>>>>;>>>>>>;>>>>`` ` `` ` `  ` ` ` ` ` ` ` ` ` ` ` `  ` `  `<;>;;>;>;>>>;>>>>>;>;>;>;>;>;>;>;>;>;>;\n>;>;>>;>;>;;;;>;;>;>;;;;>;;;;;>;;;;>!` ` `  `` ` ` ` ` ` ` ` ` ` ` ` ` ` `` ` ` ` <>;>;>;;;;;>;;;;;>;>;;>;;>;;>;;>;;>;;>\n;;>;;;>;;>;>;;;>;;>;>;>;;>>;>;;>;;><` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `  ` ` ` `.;>;;>;>>;>;;>;>;;;;>;;>;;;>;;;>;;;>;;\n;>;>;;;;>;;;>;>;;;;;;>;;;;;>;;>;;;;!` ` `` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `` ` ` `  ;;;;;;;;>;;>;;;>;>;;>;;>;>;;>;;>;;;;+\nm+;;;>;;;;;;;;;;;>;;;;;;>;;;;;;;;;;``  `  ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `  ` ` ` ``(;;;;>;;;;;;;;;;;;;;;;;;;;;;;;;;;+jd@\ngg@@Hma+;;;;;;;;;;;;;;;;;;;;;;;;;;;``` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `` ` ` `  (;;;;;;;;;;;;;;;;;;;;;;;;;;;j&QH@gggg\ng@gggg@@@mx+;;;;;;>;;;;;;;;;;;;;;;;  ` `` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `  ` ` ` ``(;;;;;;;;;;;;;;;;;;;;;;;;+jW@@gggg@gg\ngggg@gggggg@gmQAyOrrrrtOz++<;;;;;;;_` `  ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `` ` ` ` `(;;;;;;;;;;;;;;;;;;;<j+gg@ggggg@ggg@g\ng@gg@gggg@ggggggg@gHmQAOrrtrtz+<;;;<` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `  ` ` ` `.;;:;<+&+++++++++udgg@ggggg@gg@gg@gggg\ng@gg@gg@g@gg@g@ggggggggg@HmyrtrtO++;. `` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `` ` ` ``(<++zttrttrtrAQWgg@ggg@gg@gg@gg@gg@g@g\ngg@gg@ggg@ggg@gg@gg@ggggggg@HmyrtrtrO-.` `` ` ` ` ` ` ` ` ``  ` ` ` ` ` ` `` ``..zttttrtrrtwQWg@ggggg@ggg@gg@gggggggg@gg\ng@gg@ggg@gg@ggg@gg@gg@@gg@ggggg@myrtrrwO+-...` ` ` ` ` `  .a,` ` ` ` `....(J+zAQWHsrrrwAQggg@gggg@g@gg@ggg@gg@g@g@g@ggg@\ng@gg@gg@gg@gg@ggg@ggggg@gg@gg@gggggg@g@gHmAyrOz+(.` ` ` `.@gH`` `  ..ztAQmgQH@gg@gg@gg@gg@g@gg@ggg@ggg@g@ggggggg@ggg@ggg\nggg@g@ggg@gggg@ggg@g@ggg@gg@g@g@g@gggggg@g@@gHmmAOtz(...``J@\\..+OwAAQkgg@ggggg@ggggg@ggg@gg@g@g@@ggg@gggg@g@g@gggg@gg@gg\n77777777777777777777777777777777777<<<<<<<<<<<<<<<<~~~~(JWg@@HJ<<?<?<?<??77777777777777777777777777777777777777777777777\n?????????????????????????????>????????>>>>>>>><<;:::::+@@g@gg@gR>>>>>????????????=?=?===?======?=====================l=l\n?????????????????????????????????==?????<<<:::::::::::d@g@g@g@@gc::<<>??????=?==?============l=llllllllllllllllllttltltt\n=============================ll=<<;;:;;:::::::~:~::~:(@g@g@g@gg@b:~::~(<<<<<<1==l=l===llllltllttltttttttttttrtrrrrrrrrrt\nlllllllllllllltlltltlllllttlzz<;>;;;;<::~::~::~::~~:~J@g@g@g@g@g@:~~::~::::~::~<?1=lllllllllOrrrrrrrrrrvvvvvvvvvvrvvrvvv\nttrtrtrtrtrtttttrtttrrrrtv1>>>>>><;::~~:~:~:~~:~:~:::~Hg@@g@g@g@$::~:~:~~:~::~::~::<<1zttttttttOwvvzzzzzzzzuzzuzuuzuuuuu\nrrrrrrrrrrrvvrrvrrrOC11>?>??><<::::~~~~~~~~~~~~~~~~~~~v@gg@g@g@#~~~~~~~~~~~~~~~~~~~(:::<?11<<<<<<?1CVXVVVVVVXXuZZZZZuXXX\nzzzzzzzzzvvvwOCz1??????<<<<::::::::~~~~~~~~~~~~~~~~~~~~4@g@g@@#<~~~~~~~~~~~~~~~~~~~~:::::::::::::::::::<<1zlllllllllllll\nrrvrrrrrrv<::::::::::::::::::::::::_~~.~~~~~~~~.~~.~~..(@@gHg@].~..~.~.~.~..~..~..~:::::::::::::::::::::::::<1tttttttttr\nvvvOOC7<:::::::::::::::::::::::::::_..................._H@KJ@g]..................._::::::::::::::::::::::::::::?zOrrrrrr\nC<::::::::::::::::::::::::::::::::::_......`............W@P,g@%...................(:::::::::::::::::::::::::::::::::<?Or\n::::::::::::::::::::::::::::::::::::<```````````````````X@%.@@;``````````````````.:::::::::::::::::::::::::::::::::::::?\n:::::::::::::::::::::::::::::::::::::_``````````````````d@L,gH~`````````````````.:::::::::::::::::::::::::::::::::::::::\n::::::::::::::::::::::::::::::::::::::_ ````````````````Jg].g#`````````````````.::::::~::~::~:::::::::::::::::::::::::::\n:::::::::::::::::::::::::::::::::::::::_.  `      `    `.@P.@%              ` .:::::::::::::::(J::~::~::~::::~::::::::::\n::+++++++<:::::::<+++ggmgaJ++(::::::(+g<:_.`` .....JJ....g].@[..... ``````` .::++WgggHa+++++gHgggH&<::++++++++(::::~::::\ngg@gg@gggggggHHggg@ggg@g@gg@gggggHkkgg@HkH@gg@@g@@@@@@@g@@g@g@@@g@@@@gggggggHHg@@@@g@ggg@gg@@g@@g@@gg@g@gggggggggggggggg\n@g@@g@@@@@g@gg@g@@g@@g@@g@@g@ggggg@g@@g@@@g@ggg@gg@ggg@gg@@g@g@g@ggg@gg@gg@g@g@gg@g@g@@@g@g@gg@g@gg@g@@g@@g@@@@g@g@gggg@\n@gg@gg@gg@@g@@g@g@g@g@gg@gg@g@@@@@g@g@gg@g@g@@g@g@g@@g@@gg@gg@g@g@@g@@@g@@g@g@g@gg@g@gg@g@g@g@g@g@@g@gg@g@g@gg@@g@g@@@g@\n@g@g@gg@gg@g@g@g@g@g@g@g@@g@g@gg@g@g@g@@gg@g@g@g@g@g@gg@g@g@@g@g@g@gg@g@g@g@g@g@@g@g@g@g@g@g@g@g@gg@g@g@gg@g@gg@g@g@g@gg\n@g@@g@@g@@g@g@g@g@g@g@g@g@@g@g@gg@g@g@g@g@g@g@g@g@g@g@g@@g@gg@g@g@g@gg@g@g@g@g@gg@g@g@g@g@g@g@g@g@g@g@@g@@g@g@@g@g@g@g@@\n@gg@g@g@gg@g@g@g@g@g@g@ggg@g@g@@g@g@g@gg@g@g@g@g@g@g@g@gg@g@g@g@g@g@@g@g@g@g@g@g@g@g@g@g@g@g@g@g@g@g@gg@g@g@g@g@g@g@g@gg\n@g@g@g@g@g@g@g@g@g@g@g@@g@g@g@gg@g@g@g@@g@g@g@g@g@g@g@g@g@g@@g@g@g@gg@g@g@g@g@g@g@g@g@g@g@g@g@g@g@g@g@g@gg@g@g@g@g@g@g@@";
	private const string GameEnd_02 = "`\n    `     ,M|    `.M!,M}  ..M#\"WMN, ?MMMMMMMM8  .MH\"\"MN,   dMMMMMMNJ ,Mb     .M3\n           UN.   .M$ ,M} .MF     ,M[    d#    .MD     .WN  d#     ,M] .WN.  (M'\n       `    Mb   dF  ,M} J#             d#    (M       .M] dN.....(M`   TN,d@\n  `         ,M, .M`  ,M} dN       ..    d#    dN       .M] dM\"\"\"\"\"Ne     JMF       `\n     `       qN.M%   ,M} ,Mp     .M$    d#    .Mp      J#` d#     -M.    .M[\n        `     MMF    ,M}  ,WNg..gM=     d#     .TNg(.gMD   d#     .M[    .M[\n                                                                               `       `\n                                                                                 `  `  ` ` ``` `\n   `  `  `                                                           `  `  `  `       ` .(,\n            `                                            `  `  `  `               `  `.>(`,-`\n                 `       `                   `                        `   `  `     ` .^   `(     ` ` `\n       `             `      `    `    `                  `   `  `  `           `   `J` . . .  ..(<<~-(.\n   `      `                                `         `      `          `  ` `   `  .!` / ``u?` `  ._~_/\n             `   `                                        `      `  `   ``  .......,__+\\  .> `  ` ` J!\n      `                  `    `    `               `           `     `` .?^`  `  `  _4xh(-v(+/`  ..^`\n   `    `           `                   `      `        `   `      ` .?-._```    ..._.(THsdz=` _ .'\n           `     `         `                                    `   ``_<<_-.....  .JXOudXKdt_(.(z(..  `  ` `  `\n                                                         `   `              ?1.....(GJUXbHHY>...   ?=. `       ` ` `\n      `      `         `         `           `       `     `     `        `..?!__~jwXWHmQHMC~?!_..   `?.   `        `\n   `    `        `        `   `       `                       `     `     ,'  ._~ .(7<+OV74v?i..   !.   1.`           `\n           `         `                     `             `             ` (`    `  .! !-` ` Jm, ` `   ``  1    `  `\n                                   `               `       `   `  `   `  l(    `.?%  `     { ?WmJ..    `_(~  `     `\n      `        `                                              `          O`    ,'`>  .   `,    (HW[ ?77=i\\  `     `   `\n   `    `   `      `     `  `                  `       `  `        `    `( c..!   (,.>   `(.   `.H\\    `..-(J-<(,`\n                                 `      `                    `  `    `   .<7!     `(+r`   .}     ,\\` `.dHHHHm,_<.:`\n                 `     `                     `                                     `.4,  `(r` ` `,\\ .JWXWWHWY71-_?,` `\n      `  `                            `              `   `  `    `  `       ``..     `(i.` (-`   ,).WXXXUXV!  `.<<(,\n   `        `       `      `      `                           `       `  `    `_7Ww+...` ~..--  `,lWXXXXXf      ` ?O,`\n                 `            `                    `     `        `              `?XwwzOG,`  `  `.HHkHWf^`         ` `\n        `                                  `   `           `   `     `    `         ?y=zz1v+     .M@HX^`\n    `      `           `  `             `                          `    `    `  `  ` (w++z+10,`  .H@t       `  `\n               `                  `                    `  `  `  `                      7x1zOOzh.`.@t`            `  `\n       `           `                                                `  `  `  `   `    `  7uwdWWW/.D    `  `   `       `\n   `      `                `          `              `     `     `              `   `     `.74WH@e$`              `\n             `   `     `      `   `          `           `    `      `     `            `       7HR`    `  `   `     `\n      `                                            `             `     `  `   `    `           `  4   `      `\n   `    `           `     `                               `  `     `            `     `     `    `,.            `  `\n           `     `                      `    `                 `      `    `            `         .{`   `  `        `\n                                  `                    `   `      `     `    `   `  `     `  `  `  t         `  `     `\n      `      `         `   `                              `   `      `                `           `(.`   `         `\n   `    `        `            `       `            `             `        `   `   `      `    `   ` {`      `    `";
	private const string GameEnd_03 = "`  ` `  ` ` `  `  ` ` ` `  ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` ` `  `    `    `    `    `    `    `\n   `    `      `  `        `                                                      `    `    `    `    `    `    `    `\n             `       `  `     `  `  `  `  `  `  `  `  `  `  `  `  `  `  `  `  `\n `  `  `   `     `    `   `    `     `     `     `     `     `     `     `     `  `  `   `   `  `  `   `  `  `   `  `\n   `    `     `   `        `      `     `     `     `     `     `     `     `    `         `                          `\n           `   `    `  `     `  `   `  `  `  `  `  `  `  `  `  `  `  `  `  `  `       `        `    `   `     `   `\n `  `  `    `    `    `  `    `    `                  ......                    `  `    `   `     `    `   `    `    `\n   `    `     `           `      `    `  `  `  `  .J\"^`   _??\"Ya,`  `  `  `  `    `       `\n          `      `  `  `   `  `     `   `    `  .Y`             .4+   `  `    `      `        `  `  `   `   `  `  `\n    `                                         `.$     .J\"7YN      .W,                   `                            `\n       `   `  `    `    `       `  `          .F     `d:d9@J:.,     W.           `\n   `                      `            `  ` `.F       ,h.WuF ('     ,b      `              `         `  `  `   `\n          `      `    `      `   `  `       `H    `    .NJD  -       J[`  `         `  `      `  `                 `\n                   `     `              ` ..gb.. `  `..(PJJ?\"`   ` .JT\"\"\"S+,.  `\n   `  `    `  `                 `   ` .JT\"`    ?T9SQug..mJ....-gV\"\"`    ..  ?TWJ,  `      `         `       `   `    `\n                  `     `  `    ` ..#\"`.-T\"8&                        .#\"!?4,    ?4a,         `         `\n        `                    ` .d\"!    d`   ,]    .JaJ,     .-Z9WJ.  d;    d~      .Te          `         `\n   `      `        `     `   (N.       ?a. .#`  `.^  `/b`  `d`    W-  ThJJT^   ` ..d\"     `        `          `   `\n      `      `  `     `        ?\"Q..`    7\"'     J,   .#    Jh.  .#          ..JT=                    `              `\n                          `        ?Th...         ?\"\"\"^       ?\"\"!   ` ..JY\"4@               `             `\n   `      `       `     `             ,N/7\"SaJ......        ......JVT\"\"! ..J\"                   `              `\n             `       `                  (\"Q,..` _????77777??!``     ..JY\"^              `          `  `           `\n      `   `        `      `     `            ?\"\"9BWQ&(.....JJzZTY\"T#!                      `             `           `\n   `            `            `    `                d!              4,              `          `             `\n           `            `           `  `          .t                N                 `          `    `        `\n      `           `      `      `                .F                 ,b         `         `                        `\n   `     `    `      `     `       `    `       .M`                  X,     `     `         `      `     `           `\n           `            `                  `   `J^      `  `          W. `           `         `      `     `\n      `          `           `  `   `  `       .F         .., `  `    .N                `                      `\n   `      `        `     `        `          `.#     `  .MMMMN,        ,b      `           `      `               `\n              `       `    `             `   `J^         TMMM#`     `   ?p        `           `         `  `         `\n           `      `             `      `    `.F         .gMMN.          `Zc  `       `               `        `\n   `  `                 `         `         .#      ` .MMMMMMN.          `W,            `        `               `\n         `         `      `  `      `    `  d!       ,9qMMMMMM]   `        H.     `        `  `                     `\n           `   `                `      `  `J^         .MMMM#,M!      `     .N. `                     `   `  `\n   `              `   `  `         `      .$        `.MMMMM%                ,b`      `                         `\n      `   `                  `          `.F      `  .M#^dMF                  (p         `       `  `              `  `\n              `    `      `     `   `   .@           ` .MD       `  `  `     `4,   `       `           `    `\n   `       `           `          `     d`                                `    H.             `\n      `          `         `          `(\\     `              `                 .b       `                `     `\n         `         `    `      `   `  .F         `        `     `      `        ,b   `           `  `       `     `\n   `       `   `                     .@             `  `           `             Jp        `           `             `\n      `              `   `  `   `    d'  `  `                             `       ?[          `\n          `       `               `            `            `  `      `      `         `            `         `  `\n   `          `         `  `           `          `      `        `                       `      `       `          `\n           `       `           `   `     `           `                   `     `   `         `              `\n      `           `      `       `          `                        `      `          `              `\n   `     `            `               `        `        `   `  `  `                             `        `     `  `  `\n           `   `          `  `  `  `    `         `  `                  `       `  `      `  `     `\n      `           `     `                  `                               `          `               `     `\n   `      `          `              `    `     `         `   `    ..  `       `\n              `    `      `    `  `    `            ``...    .S,.($`    .JY9a.           `     `  `      `     `  `\n           `                                         M= ?N     .W}     .#JJdY`   `  `       `                        `\n   `  `          `      `  `    `   `       `   `    JgKT8x     J)     ,b              `             `      `\n         `                             `             .@  .@     ,^      /h&V`                           `        `\n           `   `   `  `  `   `   `       `           `7\"\"!                    `    `          `  `\n   `                               `        `  `  `                  `                 `  `         `      `  `     `\n      `   `       `       `     `     `                                 `  `    `                      `\n              `        `     `      `   `                      `  `                `         `                   `\n   `       `       `       `      `        `             `                   `                  `  `      `         `";

	#endregion
	
}
