using System.Text;
using Spectre.Console;

namespace PxpGameJam2024;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program {
	
	static async Task Main( string[] args ) {
		AppDomain.CurrentDomain.FirstChanceException += (_, e) => {
			StringBuilder message = new ();
			message.Append( $"Exception was thrown! {e.Exception.Message}" );

			// Ignore stacks for Newtonsoft.
			if ( e.Exception.Source is not string ) {
				message.Append( $"\n{e.Exception.StackTrace}" );
			}
			
			AnsiConsole.Write( new Rule( "[red]老爹系统崩溃了*_*[/]" ).RuleStyle( "red" ).LeftJustified() );
			AnsiConsole.WriteLine( message.ToString() );
			AnsiConsole.WriteLine( "请联系软件制作商获取帮助" );
		};
		
		AnsiConsole.Clear();
		// AnsiConsole.Write(new Rule("[blue]Hello, World![/]").RuleStyle("blue").LeftJustified());
		// await Task.Delay( 1000 );
		
#if !DEBUG
		AnsiConsole.Write(new Rule("[blue]老爹系统[/]").RuleStyle("blue").LeftJustified());
		AnsiConsole.WriteLine();
		AnsiConsole.WriteLine();

		await AnsiConsole.Live( new Panel( "" ) )
						 .StartAsync( async ctx => {
							 var progress = new StringBuilder();
							 var maxBlocks = 25; // 进度条的总块数

							 // 模拟启动过程
							 for ( var i = 0; i <= maxBlocks; i++ ) {
								 // 更新进度条
								 progress.Clear();
								 progress.Append( '[' );

								 // 添加已完成的块
								 for ( int j = 0; j < i; j++ ) progress.Append( '■' );

								 // 添加未完成的块
								 for ( int j = i; j < maxBlocks; j++ ) progress.Append( '□' );

								 progress.Append( ']' );

								 // 创建面板内容
								 var panel = new Panel( new Text( $"{progress}\n\n系统初始化..." ) )
											 .Border( BoxBorder.Rounded )
											 .BorderStyle( Style.Parse( "blue" ) );

								 // 更新显示
								 ctx.UpdateTarget( panel );

								 // 随机延迟，模拟不同组件的加载时间
								 await Task.Delay( Random.Shared.Next( 50, 100 ) );
							 }

							await Task.Delay( 1000 );

							 // 完成加载后显示最终消息
							 var finalPanel = new Panel( new Text( "系统初始化完成!" ) )
											  .Border( BoxBorder.Rounded )
											  .BorderStyle( Style.Parse( "blue" ) );

							 ctx.UpdateTarget( finalPanel );
							 await Task.Delay( 1000 );
						 } );

		AnsiConsole.MarkupLine("[bold green]加载游戏镜像...[/]");
		await Task.Delay( 2500 );

		AnsiConsole.MarkupLine( "\n[white]按任意键开始游戏\u21b5[/]" );
		Console.ReadKey( true );
		AnsiConsole.Clear();
#endif
		
		// Task.Run(async () =>
		// {
		// 	await new Game().MainLoop();
		// }).GetAwaiter().GetResult();

		foreach ( var arg in args ) {
			if ( arg is "--dev" ) {
				Game.DevelopmentMode = true;
			}
		}
		
#if DEBUG
		Game.DevelopmentMode = true;
#endif

		var game = new Game();
		await game.GameSplash();
ResetGame:
		await game.Setup();
		await game.MainLoop();

		if ( Game.GameResetRequested ) {
			AnsiConsole.Clear();
			goto ResetGame;
		}
		
#if !DEBUG
		AnsiConsole.MarkupLine("[blue]老爹系统终止~[/]");
#endif
	}


	private static void SampleUserInput() {
		// 获取用户输入
		var name = AnsiConsole.Ask<string>("What's your [green]name[/]?");
		AnsiConsole.MarkupLine($"Hello, [bold yellow]{name}[/]!");
	}


	private static void SampleUserSelection() {
		// 单选菜单
		var choice = AnsiConsole.Prompt(
			new SelectionPrompt< string >()
				.Title("What's your [green]favorite fruit[/]?")
				.PageSize( 10 )
				.AddChoices( [ "Apple", "Banana", "Orange", "Strawberry", "Grape" ] ) );

		AnsiConsole.MarkupLine($"You selected: [bold cyan]{choice}[/]");
	}


	private static void SampleUserMultiSelection() {
		var choices = AnsiConsole.Prompt(
			new MultiSelectionPrompt<string>()
				.Title("What are your [green]favorite fruits[/]?")
				.PageSize(10)
				.AddChoices(new[] {
					"Apple", "Banana", "Orange",
					"Strawberry", "Grape"
				}));
		
		foreach ( var choice in choices ) {
			AnsiConsole.MarkupLine($"\tselected: [bold cyan]{choice}[/]");
		}
	}


	private static void SampleInputWithPrompt() {
		// 文本输入
		var name = AnsiConsole.Ask<string>("What's your [green]name[/]?");

		// 密码输入
		var password = AnsiConsole.Prompt(
			new TextPrompt<string>("Enter [green]password[/]:")
				.Secret());

		// 带验证的输入
		var age = AnsiConsole.Prompt(
			new TextPrompt<int>("Enter your [green]age[/]:")
				.Validate(age =>
				{
					return age switch
					{
						<= 0  => ValidationResult.Error("[red]Age must be positive[/]"),
						> 120 => ValidationResult.Error("[red]Age must be realistic[/]"),
						_     => ValidationResult.Success()
					};
				}));
	}


	private static async Task SampleLoadingStatus() {
		// 状态指示器
		// await AnsiConsole.Status()
		// 				 .Start("Loading...", async ctx =>
		// 				 {
		// 					 // 模拟一些工作
		// 					 await Task.Delay(1000);
		// 					 ctx.Status("Processing...");
		// 					 await Task.Delay(2000);
		// 					 ctx.Status("Finishing...");
		// 					 await Task.Delay(1000);
		// 				 });

		// 加载动画
		await AnsiConsole.Live(Text.Empty)
						 .StartAsync(async ctx =>
						 {
							 for (var i = 0; i < 100; i++)
							 {
								 ctx.UpdateTarget(new Text($"Progress: {i}%"));
								 await Task.Delay(50);
							 }
						 });
	}


	private static void SamplePanel() {
		// 创建面板
		var panel = new Panel("Hello World!")
					.Header("Header")
					// .Footer("Footer")
					.BorderColor(Color.Green);

		AnsiConsole.Write(panel);
	}

	private static void SampleChart() {
		// 条形图
		var chart = new BarChart()
					.Width(60)
					.Label("Sales")
					.AddItem("2021", 123)
					.AddItem("2022", 456)
					.AddItem("2023", 789);

		AnsiConsole.Write(chart);
	}
	
	
	
}
