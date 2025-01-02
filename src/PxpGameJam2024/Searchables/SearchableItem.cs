

using Spectre.Console;


namespace PxpGameJam2024;

public class SearchableItem : Searchable {

	public override string Name => $"道具：{Item.Name}";
	private Item Item { get; set; }
	public SearchableItem( Item item ) {
		Item = item;
	}

	public override async Task Collect( Character character ) {

		var propCategory = "道具";
		if ( Item is Weapon ) {
			propCategory = "武器";
		}
		else if ( Item is SpecialItem ) {
			propCategory = "特殊道具";
		}

		var itemName = Item.Name;
		if ( Item is SpecialItemGoodLuck or SpecialItemBadSign ) {
			itemName = "灵签";
		}
		
		var choice = AnsiConsole.Prompt(
			new SelectionPrompt< string >()
				.Title( $"发现{propCategory}：[green]{itemName}[/]，该怎么办？" )
				.PageSize( 10 )
				.AddChoices( [ "收集", "放着不管" ] ) );

		if ( choice is "收集" ) {
			character.PushItem( Item );
			AnsiConsole.MarkupLine( $"收集了{propCategory}：[green]{Item.Name}[/]" );
			await Game.Delay( 0.5f );

			if ( Item is WeaponShotgun ) {
				character.PushItem( new ItemShotgunBullets { BulletCount = 2 } );
				AnsiConsole.MarkupLine( "在武器中发现了一些弹药" );
				await Game.Delay( 0.5f );
			}

			if ( Item is Weapon &&
				 Item != character.EquipedWeapon ) {
				choice = AnsiConsole.Prompt(
					new SelectionPrompt< string >()
						.Title( $"要装备[green]{Item.Name}[/]吗？" )
						.AddChoices( [ "好", "放着不管" ] ) );
				if ( choice is "好" ) {
					await Item.Use( character );
				}
			}
		}
	}
	
}
