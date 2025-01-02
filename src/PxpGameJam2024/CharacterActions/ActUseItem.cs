using Spectre.Console;


namespace PxpGameJam2024;

public class ActUseItem : CharacterAction {

	public override string Name => "物品";

	public override bool CheckAvailable( Character character ) {
		if ( character.Items.Count < 1 ) {
			return false;
		}
		
		return true;
	}

	public override async Task Do( Character character ) {
		
		var avlItems = character.ListItems();
		if ( avlItems.IsDefaultOrEmpty ) {
			AnsiConsole.Clear();
			AnsiConsole.WriteLine( "当前没有任何物品可以使用" );
			await Game.Delay( 1f );
			return;
		}
		
		var prompt = new SelectionPrompt< Item >()
					 .Title( "当前有这些道具，该怎么办?" )
					 .PageSize( 7 );
		prompt.Converter = item => {
			if ( item is null ) {
				return "[cyan]返回[/]";
			}
			if ( item.CanUse( Game.PlayerCharacter ) ) {
				return item.Name;
			}
			
			return $"{item.Name} (当前不可用)";
		};
		prompt.AddChoice( null );
		prompt.AddChoices( avlItems.ToArray() );
				
		var choice = AnsiConsole.Prompt( prompt );
		if ( choice is not null ) {
			var consume = await choice.Use( character );
			if ( consume &&
				 choice is Item and not SpecialItem and not Weapon ) {
				character.PopItem( choice );
			}
			
			Game.AnyButtonToContinue();
		}
		
	}
	
}
