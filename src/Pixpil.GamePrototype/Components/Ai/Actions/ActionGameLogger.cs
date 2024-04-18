using Bang;
using Bang.Entities;
using Murder.Diagnostics;


namespace Pixpil.AI.Actions; 

public class ActionGameLogger : AiAction {

	public enum GameLoggerMsgType : byte {
		Log,
		Warning,
		Error
	}


	public readonly GameLoggerMsgType MsgType;
	public readonly string Msg;
	public readonly AiActionExecuteStatus Result;

	public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
		switch ( MsgType ) {
			case GameLoggerMsgType.Log: GameLogger.Log( Msg ); break;
			case GameLoggerMsgType.Warning: GameLogger.Warning( Msg ); break;
			case GameLoggerMsgType.Error: GameLogger.Error( Msg ); break;
		}
		return Result;
	}
}
