using Spectre.Console;


namespace PxpGameJam2024;

public class ActMove : CharacterAction {

	public override string Name => "移动";
	
	public override async Task Do( Character character ) {
		
		AnsiConsole.Clear();
		AnsiConsole.MarkupLine( "正在打开地图..." );
		await Game.Delay( 0.5f );
		Game.PrintGameMap();
		await Game.Delay( 0.5f );
		AnsiConsole.Markup( "正在获取当前位置：" );
		await Game.Delay( 0.5f );
		AnsiConsole.MarkupLine( $"[green]{Game.TranslateGameMapLocation( character.Location )}[/]" );
		await Game.Delay( 0.3f );

		var locations = ( GameMapLocation[] )Enum.GetValues( typeof( GameMapLocation ) );

		var prompt = new SelectionPrompt< GameMapLocation >()
					 .Title( "移动到哪个[green]区域[/]?" )
					 .PageSize( 10 )
					 .AddChoices( locations );
		prompt.Converter = location => {
			if ( location != character.Location ) {
				if ( !Game.LocationDestroyStatus[ location ] ) {
					return Game.TranslateGameMapLocation( location );
				}
				
				return $"{Game.TranslateGameMapLocation( location )} ([red]已被摧毁[/])";
			}

			return $"{Game.TranslateGameMapLocation( location )} ([green]当前位置[/])";
		};
		var choice = AnsiConsole.Prompt( prompt );

		if ( character.Location != choice ) {
			if ( Game.LocationDestroyStatus[ choice ] ) {
				AnsiConsole.MarkupLine( $"{Game.TranslateGameMapLocation( choice )} 已被摧毁" );
				await Game.Delay( 0.5f );
				AnsiConsole.MarkupLine( "无法移动到目标位置。" );
			}
			else {
				character.Location = choice;
				Game.PushPlayerCost( 1 );
				AnsiConsole.MarkupLine( $"移动到位置：[green]{Game.TranslateGameMapLocation( choice )}[/]" );
			}
		}
		else {
			AnsiConsole.MarkupLine( "当前位置没发生改变。" );
		}
		await Game.Delay( 1f );
	}
}