using System;
using System.Collections.Immutable;
using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder;
using Murder.Assets;
using Pixpil.AI;
using Pixpil.Components;
using Pixpil.Services;


namespace Pixpil.Systems;


/// <summary>
/// 负责生成住房中的小人
/// </summary>
[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( DormitoryComponent ), typeof( DormitoryPersonnelSituationComponent ) )]
public class DormitoryWorkerSpawnSystem : IStartupSystem, IFixedUpdateSystem {
	
	public void Start( Context context ) {
		
	}

	public void FixedUpdate( Context context ) {
		foreach ( var dormitoryEntity in context.Entities ) {
			if ( dormitoryEntity.HasBuildingConstructionStatus() ) {
				continue;
			}
			var dormitoryPersonnelSituation = dormitoryEntity.GetDormitoryPersonnelSituation();
			if ( dormitoryPersonnelSituation.Totals < dormitoryEntity.GetDormitory().Capacity ) {
				var workerPrefab = Game.Data.TryGetAsset< PrefabAsset >( LibraryServices.GetLibrary().Worker );
				var workerEntity = workerPrefab.CreateAndFetch( context.World );
				workerEntity.SetPosition( dormitoryEntity.GetPosition() );
				workerEntity.SetWorkerBelongWhichDormitory( dormitoryEntity.EntityId );
				var utilityAiAgentComponent = new UtilityAiAgentComponent( LibraryServices.GetLibrary().WorkerAI_Idle, UtilityAiEvaluateMethod.Message, 1f );
				workerEntity.SetUtilityAiAgent( utilityAiAgentComponent );
				workerEntity.SendMessage< RequestUtilityAiAgentEvaluateMessage >();
			}
		}
	}
	
}


/// <summary>
/// 负责接到任务后, 调度人员去执行
/// </summary>
// [Obsolete]
// [Watch( typeof( BuildingConstructionStatusComponent ) )]
// [Filter( ContextAccessorFilter.AllOf, typeof( BuildingComponent ), typeof( BuildingWorkersInConstructingComponent ), typeof( BuildingConstructionStatusComponent ) )]
// [Filter( ContextAccessorFilter.NoneOf, typeof( IsPlacingBuildingComponent ) )]
// public class DormitoryPersonnelSchedulingSystem : IFixedUpdateSystem, IReactiveSystem, IMessagerSystem {
// 	
// 	public void FixedUpdate( Context context ) {
// 		DoSchedule( context.World, context.Entities );
// 	}
//
// 	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
// 		DoSchedule( world, entities );
// 	}
//
// 	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
// 		// 建造完成后遣散工人
// 		foreach ( var buildingEntity in entities ) {
//
// 			var buildingWorkersInConstructing = buildingEntity.GetBuildingWorkersInConstructing();
// 			if ( !buildingWorkersInConstructing.Workers.IsDefaultOrEmpty ) {
// 				foreach ( var workerId in buildingWorkersInConstructing.Workers ) {
// 					var worker = world.TryGetEntity( workerId );
// 					if ( worker is not null ) {
// 						worker.RemoveWorkerWorkConstruct();
// 					}
// 				}
// 				buildingEntity.SetBuildingWorkersInConstructing( buildingWorkersInConstructing.Capcity );
// 			}
// 			
// 		}
// 	}
//
// 	public void OnModified( World world, ImmutableArray< Entity > entities ) {
// 		DoSchedule( world, entities );
// 	}
//
// 	public void OnMessage( World world, Entity entity, IMessage message ) {}
//
// 	void DoSchedule( World world, ImmutableArray< Entity > entities ) {
// 		
// 		var workers = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( WorkerComponent ) );
//
// 		bool CheckWorkerEntityHasTaskAlready( Entity workerEntity ) {
// 			return workerEntity.HasWorkerWorkConstruct();
// 		}
//
// 		bool HasIdleWorker() {
// 			foreach ( var worker in workers ) {
// 				if ( !CheckWorkerEntityHasTaskAlready( worker ) ) {
// 					return true;
// 				}
// 			}
//
// 			return false;
// 		}
//
// 		Entity FetchIdleWorker() {
// 			foreach ( var worker in workers ) {
// 				if ( !CheckWorkerEntityHasTaskAlready( worker ) ) {
// 					return worker;
// 				}
// 			}
//
// 			return null;
// 		}
// 		
// 		// 分配建造任务
// 		foreach ( var buildingEntity in entities ) {
// 			var buildingWorkersInConstructing = buildingEntity.GetBuildingWorkersInConstructing();
// 			var needWorkersCount = buildingWorkersInConstructing.SpaceRemaining();
// 			if ( needWorkersCount > 0 ) {
// 				
// 				for ( var i = 0; i < needWorkersCount; i++ ) {
// 					var workerAvailable = FetchIdleWorker();
// 					if ( workerAvailable is null ) {
// 						break;
// 					}
//
// 					buildingWorkersInConstructing = buildingEntity.GetBuildingWorkersInConstructing();
// 					workerAvailable.SetWorkerWorkConstruct( buildingEntity.EntityId );
// 					buildingEntity.SetBuildingWorkersInConstructing( buildingWorkersInConstructing.Capcity, buildingWorkersInConstructing.Workers.Add( workerAvailable.EntityId ) );
// 				}
// 			}
// 			
// 		}
// 	}
//
// 	
// }


/// <summary>
/// 记录住房中的人口状态信息
/// </summary>
[Watch( typeof( WorkerComponent ), typeof( UtilityAiAgentComponent ) )]
[Filter( ContextAccessorFilter.AllOf, typeof( WorkerComponent ), typeof( UtilityAiAgentComponent ), typeof( WorkerBelongWhichDormitoryComponent ) )]
public class DormitoryPersonnelRecordingSystem : IReactiveSystem {

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		OnChanged( world );
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
		OnChanged( world );
	}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		OnChanged( world );
	}

	private void OnChanged( World world ) {
		var dormitorys = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( DormitoryComponent ) );
		foreach ( var dormitory in dormitorys ) {
			dormitory.SetDormitoryPersonnelSituation( ImmutableArray< int >.Empty, ImmutableArray< int >.Empty );
		}

		var entities = world.GetEntitiesWith( ContextAccessorFilter.AllOf, typeof( WorkerComponent ), typeof( UtilityAiAgentComponent ), typeof( WorkerBelongWhichDormitoryComponent ) );
		foreach ( var worker in entities ) {
			var dormitory = world.TryGetEntity( worker.GetWorkerBelongWhichDormitory().DormitoryEntityId );
			if ( dormitory is not null ) {
				var dormitoryPersonnelSituation = dormitory.GetDormitoryPersonnelSituation();
				var utilityAiAgent = worker.GetUtilityAiAgent();
				if ( utilityAiAgent.UtilityAiAsset == LibraryServices.GetLibrary().WorkerAI_Idle ) {
					dormitory.SetDormitoryPersonnelSituation( dormitoryPersonnelSituation.IdleStaff.Add( worker.EntityId ), dormitoryPersonnelSituation.BusyStaff );
				}
				else {
					dormitory.SetDormitoryPersonnelSituation( dormitoryPersonnelSituation.IdleStaff, dormitoryPersonnelSituation.BusyStaff.Add( worker.EntityId ) );
				}
			}
		}
	}
}
