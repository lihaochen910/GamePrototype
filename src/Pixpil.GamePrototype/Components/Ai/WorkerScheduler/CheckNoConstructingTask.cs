using Bang;
using Bang.Contexts;
using Bang.Entities;
using Pixpil.Components;


namespace Pixpil.AI.WorkerScheduler; 

public class CheckNoConstructingTask : GoapCondition {

	public override bool OnCheck( World world, Entity entity ) {
		var buildings = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( BuildingComponent ), typeof( BuildingWorkersInConstructingComponent ), typeof( BuildingConstructionStatusComponent ) );
		return !buildings.IsDefaultOrEmpty;
	}
}
