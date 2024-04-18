using System;
using System.Numerics;
using Bang.Entities;
using Murder.Core.Ai;
using Murder.Utilities;


namespace Pixpil.AI.Actions; 

public class ActionGotoEntityViaBBVar : GoapAction {

	public readonly string BBVar;
	public readonly PathfindAlgorithmKind Algorithm;
	public readonly float MinDistance = 2f;

	public override GoapActionExecuteStatus OnPreExecute() {
		
		var myBlackboard = Entity.TryGetBlackboard();
		if ( myBlackboard is null ) {
			return GoapActionExecuteStatus.Failure;
		}

		if ( !myBlackboard.Value.HasVariableWithType( BBVar, typeof( int ) ) ) {
			return GoapActionExecuteStatus.Failure;
		}
		
		var entityId = myBlackboard.Value.GetValue< int >( BBVar );
		if ( entityId < 0 ) {
			return GoapActionExecuteStatus.Failure;
		}

		var entity = World.TryGetEntity( entityId );
		if ( entity is null ) {
			return GoapActionExecuteStatus.Failure;
		}

		if ( !entity.HasPosition() ) {
			return GoapActionExecuteStatus.Failure;
		}
		
		Vector2 delta = entity.GetPosition().ToVector2() - Entity.GetPosition().ToVector2();
		var distanceSq = delta.LengthSquared();
		if ( distanceSq < MathF.Pow( MinDistance, 2 ) ) {
			// No impulse, I'm too close
			return GoapActionExecuteStatus.Success;
		}

		Entity.SetPathfind( entity.GetPosition().ToVector2(), Algorithm );
		return base.OnPreExecute();
	}

	public override GoapActionExecuteStatus OnExecute() {
		if ( Entity.HasMoveTo() ) {
			return GoapActionExecuteStatus.Running;
		}
		
		return GoapActionExecuteStatus.Success;
	}

	public override void OnPostExecute() {
		if ( Entity.HasMoveTo() ) {
			Entity.RemoveMoveTo();
		}
		if ( Entity.HasPathfind() ) {
			Entity.RemovePathfind();
		}
	}

}
