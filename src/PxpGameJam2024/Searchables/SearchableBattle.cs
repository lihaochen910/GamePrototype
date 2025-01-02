using Spectre.Console;


namespace PxpGameJam2024;

public class SearchableBattle : Searchable {

	private const string CmdFight = "战斗";
	private const string CmdSneakAttack = "偷袭";
	private const string CmdObserve = "观察";
	private const string CmdItem = "使用道具";
	private const string CmdEscape = "逃跑";

	public override string Name => "遭遇敌人";

	private readonly Character _target;
	public SearchableBattle( Character target ) {
		_target = target;
	}
	
	public override async Task Collect( Character character ) {
		AnsiConsole.MarkupLine( $"遭遇了敌人：{_target.Name}" );
		await Game.Delay( 1f );

		var playerCharacter = Game.PlayerCharacter;
		var sneakAttackRemain = 1;

		bool IsPlayerDead() => playerCharacter.Hp.StatValueCurrent <= 0;
		
		AnsiConsole.MarkupLine( $"当前生命值：[green]{playerCharacter.Hp.StatValueCurrent}[/]" );
		var currentWeaponName = character.EquipedWeapon is not null ? character.EquipedWeapon.Name : "徒手";
		AnsiConsole.MarkupLine( $"当前装备：[green]{currentWeaponName}[/]" );
		AnsiConsole.MarkupLine( $"当前攻击力：[green]{playerCharacter.Attack.StatValue}[/]" );
		AnsiConsole.WriteLine();
		
BattleLoop:
		var prompt = new SelectionPrompt< string >()
					 .Title( "该怎么办？" )
					 .PageSize( 5 );
		prompt.AddChoices( CmdFight );
		if ( character.HasItem< SpecialItemInvisibleCoin >() &&
			 sneakAttackRemain > 0 ) {
			prompt.AddChoices( CmdSneakAttack );
		}
		prompt.AddChoices( [ CmdObserve, CmdEscape ] );
		var choice = AnsiConsole.Prompt( prompt );

		if ( choice is CmdFight ) {
			var success = false;
			int damageApplyToPlayer = 0;
			if ( playerCharacter.Attack.StatValue >= _target.Attack.StatValue ) {
				damageApplyToPlayer = ( int )Math.Abs( _target.Attack.StatValue );
				success = true;
			}

			if ( playerCharacter.EquipedWeapon is WeaponGrenade weaponGrenade ) {
				if ( Game.Chance( 25 ) ) {
					var grenadeDamage = ( int )weaponGrenade.AttrMod.Value;
					playerCharacter.Hp.StatValueCurrent -= grenadeDamage;
					AnsiConsole.MarkupLine( "古董手榴弹突然在手中爆炸！" );
					await Game.Delay( 1f );
					AnsiConsole.MarkupLine( $"你受到{grenadeDamage}点伤害" );
					await Game.Delay( 0.5f );
					if ( IsPlayerDead() ) {
						AnsiConsole.MarkupLine( "你死掉了！" );
						Game.PushPlayerCost( 1 );
						return;
					}
				}
			}

			if ( playerCharacter.EquipedWeapon is WeaponShotgun ) {
				if ( playerCharacter.TryGetItem< ItemShotgunBullets >() is {} bullets ) {
					bullets.BulletCount--;
					if ( bullets.BulletCount <= 0 ) {
						playerCharacter.PopItem( bullets );
						AnsiConsole.MarkupLine( "猎枪的弹药用光了！" );
						await Game.Delay( 0.5f );
					}
				}
				else {
					AnsiConsole.MarkupLine( "掏出猎枪，射击！" );
					await Game.Delay( 1f );
					AnsiConsole.MarkupLine( "但是尴尬的是，没有子弹！" );
					await Game.Delay( 1f );
					goto BattleLoop;
				}
			}

			// 有概率受到对面的伤害
			if ( Game.Chance( 50 ) ) {
				playerCharacter.Hp.StatValueCurrent -= damageApplyToPlayer;
				AnsiConsole.MarkupLine( $"你受到{damageApplyToPlayer}点伤害" );
				await Game.Delay( 0.5f );
				if ( IsPlayerDead() ) {
					AnsiConsole.MarkupLine( "你死掉了！" );
					Game.PushPlayerCost( 1 );
					return;
				}
			}
			
			if ( success ) {
				await BattleResultSuccess( character );
			}
			else {
				AnsiConsole.MarkupLine( $"没能击败NPC: {_target.Name}！" );
				await Game.Delay( 0.5f );
				// goto BattleLoop;
			}
		}
		else if ( choice is CmdSneakAttack ) {
			if ( Game.Chance( 50 ) ) {
				AnsiConsole.MarkupLine( "[green]偷袭[/]成功！" );
				await Game.Delay( 0.5f );
				await BattleResultSuccess( character );
			}
			else {
				sneakAttackRemain--;
				AnsiConsole.MarkupLine( "[green]偷袭[/]失败！" );
				await Game.Delay( 0.5f );
				goto BattleLoop;
			}
		}
		else if ( choice is CmdObserve ) {
			AnsiConsole.MarkupLine( "仔细的观察了对方" );
			await Game.Delay( 0.5f );
			if ( _target.EquipedWeapon is not null ) {
				AnsiConsole.MarkupLine( $"对方好像装备了[green]{_target.EquipedWeapon.Name}[/]！" );
			}
			else {
				AnsiConsole.MarkupLine( "对方好像没有任何武器！" );
			}
			await Game.Delay( 1f );
			goto BattleLoop;
		}
		else if ( choice is CmdEscape ) {
			if ( Game.Chance( 50 ) ) {
				AnsiConsole.MarkupLine( "逃跑成功！" );
				await Game.Delay( 0.5f );
			}
			else {
				AnsiConsole.MarkupLine( "逃跑失败！" );
				await Game.Delay( 0.5f );

				if ( Game.Chance( 50 ) ) {
					var targetAtk = _target.Attack.StatValue;
					var damage = ( int )Math.Ceiling( targetAtk / 2f );
					playerCharacter.Hp.StatValueCurrent -= damage;
					AnsiConsole.MarkupLine( $"受到伤害{targetAtk}点" );
					await Game.Delay( 0.5f );
					if ( IsPlayerDead() ) {
						AnsiConsole.MarkupLine( "你死掉了！" );
						Game.PushPlayerCost( 1 );
						return;
					}
				}
				goto BattleLoop;
			}
		}
	}


	private async Task BattleResultSuccess( Character character ) {
		_target.Hp.StatValueCurrent = 0f;
		Game.NonPlayerControledCharacters.Remove( _target );
		AnsiConsole.MarkupLine( $"[red]x[/] 击败了NPC: {_target.Name}" );

		if ( _target.Name == Game.FinalBossName ) {
			Game.FinalBossDefeated = true;
		}

		string choice = null;
		if ( _target.EquipedWeapon is Weapon targetWeapon ) {
			await Game.Delay( 0.5f );
			choice = AnsiConsole.Prompt(
				new SelectionPrompt< string >()
					.Title( $"在NPC身上发现了装备[green]{targetWeapon.Name}[/]，怎么办？" )
					.AddChoices( [ "拿走", "放着不管" ] ) );
			if ( choice is "拿走" ) {
				Game.PlayerCharacter.PushItem( targetWeapon );
						
				choice = AnsiConsole.Prompt(
					new SelectionPrompt< string >()
						.Title( $"要装备[green]{targetWeapon.Name}[/]吗？" )
						.AddChoices( [ "好", "不用" ] ) );
				if ( choice is "好" ) {
					await targetWeapon.Use( character );
				}
			}
		}

		if ( Game.Chance( 50 ) ) {
			await Game.Delay( 0.5f );

			var randomProp = Game.GenerateARandomItem();
			choice = AnsiConsole.Prompt(
				new SelectionPrompt< string >()
					.Title( $"NPC尸体附近有道具[green]{randomProp.Name}[/]掉落，怎么办？" )
					.AddChoices( [ "拿走", "放着不管" ] ) );
			if ( choice is "拿走" ) {
				Game.PlayerCharacter.PushItem( randomProp );
			}
		}
	}
	
}
