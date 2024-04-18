using System.Linq;
using Bang.Contexts;
using Bang.Entities;
using Murder;
using Pixpil.Components;
using Pixpil.Data;


namespace Pixpil.AI.Actions; 

public class BuildingDoBuildingActionSelf : GoapAction {
	
	public override GoapActionExecuteStatus OnPreExecute() {
		if ( !Entity.HasBuildingSelfConstructionAbility() ) {
			return GoapActionExecuteStatus.Failure;
		}
		if ( Entity.HasBuildingConstructRequireResources() ) {
			return GoapActionExecuteStatus.Success;
		}

		var playerEntity = World.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( PlayerComponent ),
			typeof( InventoryComponent ) ).FirstOrDefault();
		if ( playerEntity is null ) {
			return GoapActionExecuteStatus.Failure;
		}

		if ( !Entity.GetBuildingConstructRequireResources().CheckInventoryResourceEnough( playerEntity ) ) {
			return GoapActionExecuteStatus.Failure;
		}
		
		return GoapActionExecuteStatus.Running;
	}

	public override GoapActionExecuteStatus OnExecute() {
		
		var speedOfTime = ( Game.Preferences as GamePrototypePreferences ).SpeedOfTime;
		var deltaTime = Game.DeltaTime * speedOfTime;
		
		void DoSubmit( int submitCount ) {
			var playerEntity = World.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( PlayerComponent ),
				typeof( InventoryComponent ) ).FirstOrDefault();
			Entity.GetBuildingConstructRequireResources().SubmitResourceCount( submitCount, playerEntity, Entity );
		}
		
		var buildingSelfConstructionAbility = Entity.GetBuildingSelfConstructionAbility();
		if ( !Entity.HasBuildingSelfBuildingBuildingSubmitTimer() ) {
			Entity.SetBuildingSelfBuildingBuildingSubmitTimer( buildingSelfConstructionAbility.SubmitTime );
		}
		
		var time = Entity.GetBuildingSelfBuildingBuildingSubmitTimer().SubmitTime - deltaTime;
		if ( time < 0f ) {
			DoSubmit( buildingSelfConstructionAbility.SubmitCount );
			time = buildingSelfConstructionAbility.SubmitTime;
			Entity.SetBuildingSelfBuildingBuildingSubmitTimer( time );
			return GoapActionExecuteStatus.Success;
		}
		
		Entity.SetBuildingSelfBuildingBuildingSubmitTimer( time );
		return GoapActionExecuteStatus.Running;
	}

	public override void OnPostExecute() {
		if ( Entity.HasBuildingSelfBuildingBuildingSubmitTimer() ) {
			Entity.RemoveBuildingSelfBuildingBuildingSubmitTimer();
		}
	}
}
