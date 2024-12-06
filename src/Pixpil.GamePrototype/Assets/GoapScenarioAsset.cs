using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DigitalRune;
using DigitalRune.Mathematics;
using Murder;
using Murder.Assets;
using Murder.Attributes;
using Murder.Core.Graphics;
using Murder.Diagnostics;
using Murder.Services;
using Murder.Utilities;
using Murder.Utilities.Attributes;


namespace Pixpil.AI {
	
	using Pixpil.Assets;

	// public record GoapScenarioConditionItem {
	// 	// public int Id;
	// 	public string Name;
	// }


	public class GoapScenarioGoal {
		public string Name;
		public ImmutableDictionary< string, bool > Conditions = ImmutableDictionary< string, bool >.Empty;
		public bool IsDefault = false;
	}


	public class GoapScenarioAction {
		public string Name;
		public int Cost = 1;
		public bool IsActived = true;
		public GoapActionExecutePolicy ExecutePolicy;
		public ImmutableArray< GoapAction > Impl = ImmutableArray< GoapAction >.Empty;
		public ImmutableDictionary< string, bool > Pre = ImmutableDictionary< string, bool >.Empty;
		public ImmutableDictionary< string, bool > Post = ImmutableDictionary< string, bool >.Empty;

		public ImmutableArray< GoapAction > MakeClonedImpl() {
			var builder = ImmutableArray.CreateBuilder< GoapAction >( Impl.Length );
			foreach ( var action in Impl ) {
				builder.Add( ( GoapAction )action.Clone() );
			}
			return builder.ToImmutable();
		}
	}

	
	[Serializable]
	public abstract class GoapCondition {

		// [JsonIgnore]
		// public World World { get; private set; }
		//
		// [JsonIgnore]
		// public Entity Entity { get; private set; }
		//
		// public bool ContextSetted { get; private set; }

		public abstract bool OnCheck( World world, Entity entity );
		// public virtual void OnPreCheck() {}
		//
		// internal virtual void SetupContext( World world, Entity entity ) {
		// 	World = world;
		// 	Entity = entity;
		// 	ContextSetted = true;
		// }
		//
		// internal virtual void ClearContext() {
		// 	World = null;
		// 	Entity = null;
		// 	ContextSetted = false;
		// }

	}
	
	
	/// GoapAction Execution Status enumeration
	public enum GoapActionExecuteStatus : byte {
	
		/// The action has succeeded.
		Success = 0,
	
		/// The action has failed.
		Failure = 1,
	
		/// The action is still running.
		Running = 2
	}


	[Flags]
	public enum GoapActionExecutePolicy : int {
		None = 0,
		CantBeInterrupted = 1 << 1,
	}
	
	
	[Serializable]
	public abstract class GoapAction : ICloneable {
		
		[System.Text.Json.Serialization.JsonIgnore]
		public World World { get; private set; }
		
		[System.Text.Json.Serialization.JsonIgnore]
		public Entity Entity { get; private set; }

		internal GoapActionExecuteStatus PreExecuteResult;
		internal GoapActionExecuteStatus ExecuteResult;
		
		[ShowInEditor]
		public float ElapsedTime;

		public virtual GoapActionExecuteStatus OnPreExecute() => GoapActionExecuteStatus.Running;
		// public virtual IEnumerator< Wait > OnCoroutine() { yield return Wait.Stop; }
		public virtual GoapActionExecuteStatus OnExecute() => GoapActionExecuteStatus.Success;
		public virtual void OnPostExecute() {}
		
		internal virtual void SetupContext( World world, Entity entity ) {
			World = world;
			Entity = entity;
			PreExecuteResult = ExecuteResult = default;
		}


		internal virtual void ClearContext() {
			World = null;
			Entity = null;
		}

		public object Clone() {
			return MemberwiseClone();
		}
	}


	public sealed class GoapActionPauseEvaluateOnPreAndResumeOnPost : GoapAction {
		
		public override GoapActionExecuteStatus OnPreExecute() {
			Entity.SetGoapAgentPausedEvaluate();
			return GoapActionExecuteStatus.Success;
		}

		public override void OnPostExecute() {
			Entity.RemoveGoapAgentPausedEvaluate();
		}
	}
	
	
	public sealed class AlwaysTrueCondition : GoapCondition {
		public override bool OnCheck( World world, Entity entity ) => true;
	}


	public sealed class AlwaysFalseCondition : GoapCondition {
		public override bool OnCheck( World world, Entity entity ) => false;
	}


	public class GoapConditionCollection : GoapCondition {
		
		public ConditionsCheckMode CheckMode = ConditionsCheckMode.AllTrueRequired;
		public ImmutableArray< GoapCondition > Conditions = ImmutableArray< GoapCondition >.Empty;


		public enum ConditionsCheckMode : byte {
			AllTrueRequired,
			AnyTrueSuffice
		}


		// internal override void SetupContext( World world, Entity entity ) {
		// 	base.SetupContext( world, entity );
		// 	Conditions.ForEach( condition => condition?.SetupContext( world, entity ) );
		// }
		//
		// internal override void ClearContext() {
		// 	base.ClearContext();
		// 	Conditions.ForEach( condition => condition?.ClearContext() );
		// }

		public override bool OnCheck( World world, Entity entity ) {
			switch ( CheckMode ) {
				case ConditionsCheckMode.AllTrueRequired:
					foreach ( var condition in Conditions ) {
						if ( !condition.OnCheck( world, entity ) ) {
							return false;
						}
					}

					return true;
				
				case ConditionsCheckMode.AnyTrueSuffice:
					foreach ( var condition in Conditions ) {
						if ( condition.OnCheck( world, entity ) ) {
							return true;
						}
					}

					return false;
			}
			
			return false;
		}
	}


	public class InverseCondition : GoapCondition {
		
		public readonly GoapCondition Target;

		public override bool OnCheck( World world, Entity entity ) {
			if ( Target is null ) {
				return false;
			}

			return Target.OnCheck( world, entity );
		}

		// public override void OnPreCheck() {
		// 	base.OnPreCheck();
		// 	Target?.OnPreCheck();
		// }
		//
		// internal override void SetupContext( World world, Entity entity ) {
		// 	base.SetupContext( world, entity );
		// 	Target?.SetupContext( world, entity );
		// }
		//
		// internal override void ClearContext() {
		// 	base.ClearContext();
		// 	Target?.ClearContext();
		// }
	}


	public class CheckGoapScenarioCondition : GoapCondition {
		
		public readonly string Condition;
		public readonly bool EqualToValue;

		public override bool OnCheck( World world, Entity entity ) {
			if ( entity is null ) {
				return false;
			}

			var impl = entity.GetGoapAgent().TryGetScenario().GetConditionImpl( Condition );
			if ( impl is null ) {
				// TODO: Log Error
				return false;
			}

			// bool setContextHere = !impl.ContextSetted;
			// if ( setContextHere ) {
			// 	impl.SetupContext( World, Entity );
			// 	impl.OnPreCheck();
			// }
			
			var result = impl.OnCheck( world, entity );
			
			// if ( setContextHere ) {
			// 	impl.ClearContext();
			// }
			return result == EqualToValue;
		}
	}


	public class RandomProbabilityCondition : GoapCondition {
		
		[Slider(0, 1)]
		public float Probability;

		public override bool OnCheck( World world, Entity entity ) => Game.Random.TryWithChanceOf( Probability );
	}


	public class EntityHasSpecifyComponentCondition : GoapCondition {
		
		public readonly ImmutableArray< Type > ComponentTypes = ImmutableArray< Type >.Empty;
		
		public override bool OnCheck( World world, Entity entity ) {
			if ( !ComponentTypes.IsDefaultOrEmpty ) {
				foreach ( var type in ComponentTypes ) {
					if ( !entity.HasComponent( type ) ) {
						return false;
					}
				}

				return true;
			}

			return false;
		}
	}


	// public class TestGoapCondition : GoapCondition {
	// 	public int A;
	// 	public float B;
	// 	public bool C;
	// 	public GoapConditionCollection.ConditionsCheckMode D;
	// }


	public struct GoapWorldState : IEquatable< GoapWorldState > {

		/// <summary>
		/// we use a bitmask shifting on the condition index to flip bits
		/// </summary>
		public long Values;

		/// <summary>
		/// bitmask used to explicitly state false. We need a separate store for negatives because the absense of a value doesnt necessarily mean
		/// it is false.
		/// </summary>
		public long DontCare;

		/// <summary>
		/// required so that we can get the condition index from the string name
		/// </summary>
		internal readonly GoapActionPlanner Planner;


		public static GoapWorldState Create( GoapActionPlanner planner ) {
			return new GoapWorldState( planner, 0, -1 );
		}


		public GoapWorldState( GoapActionPlanner planner, long values, long dontcare ) {
			Planner = planner;
			Values = values;
			DontCare = dontcare;
		}


		public bool Set( string conditionName, bool value ) {
			return Set( Planner.FindConditionNameIndex( conditionName ), value );
		}


		internal bool Set( int conditionId, bool value ) {
			if ( conditionId is -1 ) {
				throw new ArgumentOutOfRangeException( $"conditionId: {conditionId}" );
			}
			Values = value ? ( Values | ( 1L << conditionId ) ) : ( Values & ~( 1L << conditionId ) );
			DontCare ^= ( 1L << conditionId );
			return true;
		}


		public bool Get( string conditionName ) {
			return Get( Planner.FindConditionNameIndex( conditionName ) );
		}


		internal bool Get( int conditionId ) {
			if ( conditionId is -1 ) {
				throw new ArgumentOutOfRangeException( $"conditionId: {conditionId}" );
			}
			return ( Values & ( 1L << conditionId ) ) == ( 1L << conditionId );
		}


		public bool Equals( GoapWorldState other ) {
			var care = DontCare ^ -1L;
			return ( Values & care ) == ( other.Values & care );
		}


		/// <summary>
		/// for debugging purposes. Provides a human readable string of all the preconditions.
		/// </summary>
		/// <param name="planner">Planner.</param>
		public string Describe( GoapActionPlanner planner ) {
			var sb = new StringBuilder();
			for ( var i = 0; i < GoapActionPlanner.MAX_CONDITIONS; i++ ) {
				if ( ( DontCare & ( 1L << i ) ) == 0 ) {
					var val = planner.ConditionNames[ i ];
					if ( val == null ) continue;

					bool set = ( ( Values & ( 1L << i ) ) != 0L );

					if ( sb.Length > 0 ) sb.Append( ", " );
					sb.Append( set ? val.ToUpper() : val );
				}
			}

			return sb.ToString();
		}
	}


	public class GoapActionPlanner {
		
		public const int MAX_CONDITIONS = 64;

		/// <summary>
		/// Names associated with all world state atoms
		/// </summary>
		public readonly string[] ConditionNames = new string[ MAX_CONDITIONS ];
		
		public event System.Action< Stack< GoapScenarioAction > > PlanUpdated;

		/// <summary>
		/// Preconditions for all actions
		/// </summary>
		private readonly GoapWorldState[] _preConditions = new GoapWorldState[ MAX_CONDITIONS ];

		/// <summary>
		/// Postconditions for all actions (action effects).
		/// </summary>
		private readonly GoapWorldState[] _postConditions = new GoapWorldState[ MAX_CONDITIONS ];

		/// <summary>
		/// Number of world state atoms.
		/// </summary>
		private int _numConditionNames;

		private GoapScenarioAsset _goapScenarioAsset;
		

		public GoapActionPlanner() {
			_numConditionNames = 0;
			for ( var i = 0; i < MAX_CONDITIONS; ++i ) {
				ConditionNames[ i ] = null;
				_preConditions[ i ] = GoapWorldState.Create( this );
				_postConditions[ i ] = GoapWorldState.Create( this );
			}
		}


		/// <summary>
		/// convenince method for fetching a WorldState object
		/// </summary>
		/// <returns>The world state.</returns>
		public GoapWorldState CreateWorldState() => GoapWorldState.Create( this );

		
		public void Setup( GoapScenarioAsset goapScenarioAsset ) {
			foreach ( var action in goapScenarioAsset.Actions ) {
				var actionId = goapScenarioAsset.Actions.IndexOf( action );
				// var actionId = FindActionIndex( action );
				// if ( actionId == -1 ) {
				// 	throw new KeyNotFoundException( "could not find or create Action" );
				// }

				foreach ( var preCondition in action.Pre ) {
					var conditionId = FindConditionNameIndex( preCondition.Key );
					if ( conditionId == -1 ) {
						throw new KeyNotFoundException( "could not find or create conditionName" );
					}

					_preConditions[ actionId ].Set( conditionId, preCondition.Value );
				}

				foreach ( var postCondition in action.Post ) {
					var conditionId = FindConditionNameIndex( postCondition.Key );
					if ( conditionId == -1 ) {
						throw new KeyNotFoundException( "could not find conditionName" );
					}

					_postConditions[ actionId ].Set( conditionId, postCondition.Value );
				}
			}

			_goapScenarioAsset = goapScenarioAsset;
		}


		public Stack< GoapScenarioAction > Plan( in GoapWorldState startState, in GoapWorldState goalState, in Stack< GoapScenarioAction > returnActions = null, List< AStarNode > selectedNodes = null ) {
			var result = AStar.Plan( this, startState, goalState, returnActions, selectedNodes );
			PlanUpdated?.Invoke( result );
			
#if DEBUG
			if ( result is not null ) {
				_previousSelectedActions.Clear();
				foreach ( var action in result ) {
					_previousSelectedActions.Add( action );
				}
			}

			PreviousPlanTime = Game.NowUnscaled;
#endif
			
			return result;
		}

		
#if DEBUG
		public Dictionary< string, bool > ConditionsDebugData => _cachedConditionsDebugData;
		private Dictionary< string, bool > _cachedConditionsDebugData = new ( 0xf );

		public void PushDebugConditionData( string condition, bool result ) => _cachedConditionsDebugData[ condition ] = result;

		public void ResetDebugConditionData() => _cachedConditionsDebugData.Clear();

		public List< GoapScenarioAction > PreviousSelectedActions => _previousSelectedActions;
		private List< GoapScenarioAction > _previousSelectedActions = new ( 0xf );

		public float PreviousPlanTime { get; private set; }
#endif

		/// <summary>
		/// Describe the action planner by listing all actions with pre and post conditions. For debugging purpose.
		/// </summary>
		public string Describe() {
			var sb = new StringBuilder();
			for ( var a = 0; a < _goapScenarioAsset.Actions.Length; ++a ) {
				sb.AppendLine( _goapScenarioAsset.Actions[ a ].Name );

				var pre = _preConditions[ a ];
				var pst = _postConditions[ a ];
				for ( var i = 0; i < MAX_CONDITIONS; ++i ) {
					if ( ( pre.DontCare & ( 1L << i ) ) == 0 ) {
						bool v = ( pre.Values & ( 1L << i ) ) != 0;
						sb.AppendFormat( "  {0}=={1}\n", ConditionNames[ i ], v ? 1 : 0 );
					}
				}

				for ( var i = 0; i < MAX_CONDITIONS; ++i ) {
					if ( ( pst.DontCare & ( 1L << i ) ) == 0 ) {
						bool v = ( pst.Values & ( 1L << i ) ) != 0;
						sb.AppendFormat( "  {0}:={1}\n", ConditionNames[ i ], v ? 1 : 0 );
					}
				}
			}

			return sb.ToString();
		}


		internal int FindConditionNameIndex( string conditionName ) {
			int idx;
			for ( idx = 0; idx < _numConditionNames; ++idx ) {
				if ( ConditionNames[ idx ] == conditionName )
					return idx;
			}

			if ( idx < MAX_CONDITIONS - 1 ) {
				ConditionNames[ idx ] = conditionName;
				_numConditionNames++;
				return idx;
			}

			return -1;
		}

		
		internal List< AStarNode > GetPossibleTransitions( in GoapWorldState fr ) {
			var result = AStar.AStarNodeListPool.Obtain();
			for ( var i = 0; i < _goapScenarioAsset.Actions.Length; ++i ) {
				// see if precondition is met
				var pre = _preConditions[ i ];
				var care = pre.DontCare ^ -1L;
				bool met = ( pre.Values & care ) == ( fr.Values & care );
				if ( met ) {
					var node = AStar.AStarNodePool.Obtain();
					node.Action = _goapScenarioAsset.Actions[ i ];
					node.CostSoFar = _goapScenarioAsset.Actions[ i ].Cost;
					node.WorldState = ApplyPostConditions( this, i, fr );
					result.Add( node );
				}
			}

			return result;
		}


		internal GoapWorldState ApplyPostConditions( GoapActionPlanner ap, int actionnr, GoapWorldState fr ) {
			var pst = ap._postConditions[ actionnr ];
			long unaffected = pst.DontCare;
			long affected = unaffected ^ -1L;
			fr.Values = ( fr.Values & unaffected ) | ( pst.Values & affected );
			fr.DontCare &= pst.DontCare;
			return fr;
		}
	}

	
	[Watch( typeof( GoapAgentComponent ) )]
	[Filter( ContextAccessorFilter.AnyOf, ContextAccessorKind.Read, typeof( GoapAgentComponent ) )]
	[Filter( ContextAccessorFilter.NoneOf, typeof( GoapAgentPausedEvaluateComponent ) )]
	[Messager(typeof( RequestGoapAgentEvaluateMessage ))]
	public class GoapComputeSystem : IFixedUpdateSystem, IReactiveSystem, IMessagerSystem {
		
		public void FixedUpdate( Context context ) {
			foreach ( var entity in context.Entities ) {
				var goapAgentComponent = entity.GetGoapAgent();
				if ( goapAgentComponent.EvaluateFrequency is not GoapAgentEvaluateFrequency.Interval ) {
					continue;
				}

				if ( !entity.HasGoapAgentEvaluateTimer() ) {
					entity.SetGoapAgentEvaluateTimer( 0f );
				}

				var time = entity.GetGoapAgentEvaluateTimer().Time;
				if ( time < 0f ) {
					// if ( entity.HasGoapPlan() ) {
					// 	var previousPlan = entity.GetGoapPlan();
					// 	previousPlan.Actions?.Clear();
					// }
					Evaluate( context.World, entity );
					entity.SetGoapAgentEvaluateTimer( goapAgentComponent.EvaluateInterval );
				}
				else {
					entity.SetGoapAgentEvaluateTimer( time - Game.FixedDeltaTime );
				}
			}
		}
		
		public void OnAdded( World world, ImmutableArray< Entity > entities ) {}

		public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

		public void OnModified( World world, ImmutableArray< Entity > entities ) {
			foreach ( var entity in entities ) {
				var goapAgentComponent = entity.GetGoapAgent();
				if ( goapAgentComponent.PlannerChangedOnly ) {
					continue;
				}
				
				entity.SetGoapAgent( new GoapAgentComponent(
					goapAgentComponent.GoapScenarioAsset,
					goapAgentComponent.Goal,
					goapAgentComponent.EvaluateFrequency,
					goapAgentComponent.EvaluateInterval
				) );
			}
		}

		public void OnMessage( World world, Entity entity, IMessage message ) {
			if ( message is RequestGoapAgentEvaluateMessage ) {
				Evaluate( world, entity );
			}
		}

		private void Evaluate( World world, Entity entity ) {
			var goapAgentComponent = entity.GetGoapAgent();
			var goapScenarioAsset = Game.Data.TryGetAsset< GoapScenarioAsset >( goapAgentComponent.GoapScenarioAsset );
			if ( goapScenarioAsset is null ) {
				return;
			}

			bool createPlannerThisFrame = false;
			GoapActionPlanner planner = goapAgentComponent.Planner;
			if ( planner is null ) {
				planner = new GoapActionPlanner();
				planner.Setup( goapScenarioAsset );
				createPlannerThisFrame = true;
			}
			
			var goapScenarioGoal = goapScenarioAsset.FindGoal( goapAgentComponent.Goal );
			if ( goapScenarioGoal is null ) {
				return;
			}

			if ( createPlannerThisFrame ) {
				entity.SetGoapAgent( new GoapAgentComponent(
					goapAgentComponent.GoapScenarioAsset,
					goapAgentComponent.Goal,
					goapAgentComponent.EvaluateFrequency,
					goapAgentComponent.EvaluateInterval,
					planner,
					true
				) );
			}

			// Setup condition context
			// foreach ( var condition in goapScenarioAsset.Conditions.Values ) {
			// 	if ( condition is null ) {
			// 		continue;
			// 	}
			// 	condition.SetupContextplanner( world, entity );
			// 	condition.OnPreCheck();
			// }
			
#if DEBUG
			goapAgentComponent.Planner.ResetDebugConditionData();
			
			foreach ( var condition in goapScenarioAsset.Conditions ) {
				if ( condition.Value != null ) {
					goapAgentComponent.Planner.PushDebugConditionData( condition.Key, condition.Value.OnCheck( world, entity ) );
				}
			}
#endif

			GoapWorldState GetWorldState() {
				var worldState = goapAgentComponent.Planner.CreateWorldState();
				
				foreach ( var condition in goapScenarioAsset.Conditions ) {
					if ( condition.Value != null ) {
						worldState.Set( condition.Key, condition.Value.OnCheck( world, entity ) );
					}
				}
		
				return worldState;
			}

			GoapWorldState GetGoalState() {
				var goalState = goapAgentComponent.Planner.CreateWorldState();
				var goalDefine = goapScenarioAsset.FindGoal( goapAgentComponent.Goal );
				if ( goapAgentComponent.Goal != null && goalDefine != null ) {
				
					foreach ( var condition in goalDefine.Conditions ) {
						goalState.Set( condition.Key, condition.Value );
					}
				}
			
				return goalState;
			}

			GoapPlanComponent goapPlanComponent = entity.HasGoapPlan() ? entity.GetGoapPlan() : new GoapPlanComponent( goapScenarioGoal, new Stack< GoapScenarioAction >( 3 ) );
			goapPlanComponent.Actions.Clear();
			
			var nodes = AStar.AStarNodeListPool.Obtain();
			goapAgentComponent.Planner.Plan( GetWorldState(), GetGoalState(), in goapPlanComponent.Actions, nodes );

			// if ( nodes is { Count: > 0 } ) {
			// 	GameLogger.Log( "---- ActionPlanner plan ----" );
			// 	GameLogger.Log( $"goal: {goapScenarioGoal.Name} plan cost = {nodes[ nodes.Count - 1 ].CostSoFar}\n" );
			// 	// GameLogger.Log( $"{"start".PadRight( 15 )}\t{GetWorldState().Describe( goapAgentComponent.Planner )}" );
			// 	// for ( var i = 0; i < nodes.Count; i++ ) {
			// 	// 	GameLogger.Log( $"{i}: {nodes[ i ].Action.Name.PadRight( 15 )}\t{nodes[ i ].WorldState.Describe( goapAgentComponent.Planner )}" );
			// 	// }
			// 	for ( var i = 0; i < nodes.Count; i++ ) {
			// 		var action = nodes[ i ].Action;
			// 		GameLogger.Log( $"{i}: {action.Name.PadRight( 15 )}" );
			// 		foreach ( var preKV in action.Pre ) {
			// 			GameLogger.Log( $"\t [{preKV.Key}] = {goapScenarioAsset.CheckCondition( preKV.Key )}" );
			// 		}
			// 	}
			// 	GameLogger.Log( "----------------------------" );
			// }
			
			AStar.AStarNodeListPool.Recycle( nodes );
			
			// Clear condition context
			// foreach ( var condition in goapScenarioAsset.Conditions.Values ) {
			// 	if ( condition is null ) {
			// 		continue;
			// 	}
			// 	condition.ClearContext();
			// }

			if ( goapPlanComponent.Actions is null || goapPlanComponent.Actions.Count < 1 ) {
				entity.RemoveGoapPlan();
				entity.SetGoapNoPlan( goapScenarioGoal );
			}
			else {
				entity.RemoveGoapNoPlan();
				if ( entity.HasGoapPlan() ) {
					entity.ReplaceComponent( goapPlanComponent, typeof( GoapPlanComponent ), true );
				}
				else {
					entity.SetGoapPlan( goapPlanComponent );
				}
			}
			
			entity.SendMessage< GoapAgentEvaluateFinishedMessage >();
		}


		public static bool EvaluateCondition( GoapScenarioAsset goapScenarioAsset, string condition, World world, Entity entity ) {
#if DEBUG
			if ( goapScenarioAsset is null )
				throw new ArgumentNullException( nameof( goapScenarioAsset ) );
			if ( world is null )
				throw new ArgumentNullException( nameof( world ) );
			if ( entity is null )
				throw new ArgumentNullException( nameof( entity ) );
#endif
			
			goapScenarioAsset.Conditions.TryGetValue( condition, out var goapCondition );
			if ( goapCondition is null )
				throw new KeyNotFoundException( $"condition: {condition}" );
			
			// goapCondition.SetupContext( world, entity );
			// goapCondition.OnPreCheck();
			var result = goapCondition.OnCheck( world, entity );
			// goapCondition.ClearContext();
			return result;
		}


		public static bool EvaluateGoalConditions( GoapScenarioAsset goapScenarioAsset, string goalName, World world, Entity entity ) {
#if DEBUG
			if ( goapScenarioAsset is null )
				throw new ArgumentNullException( nameof( goapScenarioAsset ) );
			if ( world is null )
				throw new ArgumentNullException( nameof( world ) );
			if ( entity is null )
				throw new ArgumentNullException( nameof( entity ) );
#endif

			var goal = goapScenarioAsset.Goals.FirstOrDefault( goal => goal.Name == goalName );
			if ( goal is null )
				throw new KeyNotFoundException( $"goal: {goalName}" );

			foreach ( var condKV in goal.Conditions ) {
				if ( goapScenarioAsset.Conditions.ContainsKey( condKV.Key ) ) {
					var goalCondition = goapScenarioAsset.Conditions[ condKV.Key ];
					// goalCondition.SetupContext( world, entity );
					// goalCondition.OnPreCheck();
					var result = goalCondition.OnCheck( world, entity );
					// goalCondition.ClearContext();
					if ( result != condKV.Value ) {
						return false;
					}
				}
				else {
					return false;
				}
			}

			return true;
		}
	}


	[Watch( typeof( GoapPlanComponent ) )]
	[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( GoapPlanExecutorComponent ), typeof( GoapPlanComponent ) )]
	[Messager(typeof( StopExecutingGoapPlanMessage ))]
	public class GoapPlanExecutingSystem : IUpdateSystem, IReactiveSystem, IMessagerSystem, IMurderRenderSystem {

		public void Update( Context context ) {

			// return true while processed.
			bool ProcessCantBeInterruptedAction( Entity entity, in GoapPlanInExecutingComponent goapPlanInExecuting ) {
				var goapPlanComponent = entity.GetGoapPlan();
				var topAction = goapPlanComponent.Action;
				if ( topAction != null && topAction != goapPlanInExecuting.Action && goapPlanInExecuting.Action.ExecutePolicy.HasFlag( GoapActionExecutePolicy.CantBeInterrupted ) ) {
					entity.RemoveGoapPlanInExecuting();
					OnEntityGoapPlanModified( context.World, entity );
					return true;
				}

				return false;
			}
			
			foreach ( var entity in context.Entities ) {
				if ( !entity.HasGoapPlanInExecuting() ) {
					continue;
				}
				
				var goapPlanInExecuting = entity.GetGoapPlanInExecuting();
				if ( goapPlanInExecuting.IsInterrupted || goapPlanInExecuting.ActionExecuteStatus is GoapActionExecuteStatus.Failure ) {

					bool planActionNonNull = goapPlanInExecuting.Action != null;
					if ( planActionNonNull ) {
						foreach ( var subTask in goapPlanInExecuting.ActionImplCloned ) {
							subTask.OnPostExecute();
						}
					}

					// check CantBeInterrupted action
					if ( planActionNonNull && ProcessCantBeInterruptedAction( entity, in goapPlanInExecuting ) ) {
						continue;
					}
					
					entity.SendMessage< RequestGoapAgentEvaluateMessage >();
					entity.RemoveGoapPlanInExecuting();
				}
				else {
					var allSuccessful = true;
					var anyRunning = false;
					foreach ( var subTask in goapPlanInExecuting.ActionImplCloned ) {
						var previousStatue = subTask.ExecuteResult;
						if ( previousStatue is not GoapActionExecuteStatus.Running ) {
							continue;
						}

						subTask.ElapsedTime += Game.DeltaTime;
						var result = subTask.OnExecute();
						switch ( result ) {
							case GoapActionExecuteStatus.Success:
								// do nothing
								break;
							case GoapActionExecuteStatus.Failure:
								allSuccessful = false;
								break;
							case GoapActionExecuteStatus.Running:
								anyRunning = true;
								break;
						}

						subTask.ExecuteResult = result;

						if ( !allSuccessful ) {
							break;
						}
					}

					// Successful and go next.
					if ( allSuccessful && !anyRunning ) {

						foreach ( var subTask in goapPlanInExecuting.ActionImplCloned ) {
							subTask.OnPostExecute();
						}
						
						GameLogger.Log( $"{goapPlanInExecuting.Action.Name}: Success." );

						if ( ProcessCantBeInterruptedAction( entity, in goapPlanInExecuting ) ) {
							continue;
						}
						
						entity.SendMessage< RequestGoapAgentEvaluateMessage >();
						entity.RemoveGoapPlanInExecuting();
						continue;
					}

					// Failure and go next.
					if ( !allSuccessful ) {
						
						foreach ( var subTask in goapPlanInExecuting.ActionImplCloned ) {
							subTask.OnPostExecute();
						}
						
						GameLogger.Log( $"{goapPlanInExecuting.Action.Name}: !allSuccessful, go next." );
						
						if ( ProcessCantBeInterruptedAction( entity, in goapPlanInExecuting ) ) {
							continue;
						}
						
						entity.SendMessage< RequestGoapAgentEvaluateMessage >();
						entity.RemoveGoapPlanInExecuting();
					}
				}
			}
		}

		public void OnAdded( World world, ImmutableArray< Entity > entities ) {
			foreach ( var entity in entities ) {
				SetupEntityForExecute( world, entity );
			}
		}

		public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
			foreach ( var entity in entities ) {
				if ( entity.TryGetGoapPlanInExecuting() is {} goapPlanInExecuting ) {
					if ( goapPlanInExecuting.Action != null ) {
						foreach ( var subTask in goapPlanInExecuting.ActionImplCloned ) {
							subTask.OnPostExecute();
						}
						
						entity.RemoveGoapPlanInExecuting();
					}
				}
			}
		}

		public void OnModified( World world, ImmutableArray< Entity > entities ) {
			foreach ( var entity in entities ) {
				OnEntityGoapPlanModified( world, entity );
			}
		}

		public void OnMessage( World world, Entity entity, IMessage message ) {
			if ( message is StopExecutingGoapPlanMessage ) {
				if ( entity.HasGoapPlanInExecuting() ) {
					var goapPlanInExecuting = entity.GetGoapPlanInExecuting();
					entity.SetGoapPlanInExecuting( goapPlanInExecuting.Action, goapPlanInExecuting.ActionImplCloned, goapPlanInExecuting.ActionExecuteStatus, true );
				}
			}
		}
		
		public void Draw( RenderContext render, Context context ) {

			// if ( context.World.TryGetUnique< EditorComponent >() is {} editorComponent && editorComponent.EditorHook != null && editorComponent.EditorHook.ShowStates ) {
			// 	var drawInfo = new DrawInfo( Color.White, 0.2f ) { Outline = Color.Black, Scale = Vector2.One, Offset = new Vector2( 0, 0 ) };
			// 	var statusDrawInfo = new DrawInfo( Color.Green, 0.2f ) { Scale = Vector2.One * 0.8f, Offset = new Vector2( 0, 0 ) };
			//
			// 	foreach ( var entity in context.Entities ) {
			// 		if ( entity.HasInCamera() && entity.TryGetGoapPlanInExecuting() is {} goapPlanInExecutingComponent ) {
			// 			var entityInScreen = entity.GetPosition().ToVector2() + new Vector2( 10f, 38f );
			// 			render.DebugBatch.DrawText( MurderFonts.PixelFont, $"{goapPlanInExecutingComponent.Action?.Name}", entityInScreen + new Vector2( 0, 0 ), drawInfo );
			// 			render.DebugBatch.DrawText( MurderFonts.PixelFont, $"{goapPlanInExecutingComponent.ActionExecuteStatus}", entityInScreen + new Vector2( 0, 8 ), statusDrawInfo );
			// 			// render.DebugBatch.DrawText( MurderFonts.PixelFont, $"{goapPlanInExecutingComponent.Action?.}", entityInScreen + new Vector2( 0, 16 ), statusDrawInfo );
			// 		}
			// 	}
			// }
		}
		
		private void SetupEntityForExecute( World world, Entity entity ) {
			if ( !entity.HasGoapPlan() ) {
				return;
			}
			
			var goapPlanComponent = entity.GetGoapPlan();
			var topAction = goapPlanComponent.Action;
			if ( topAction is { Impl.IsDefaultOrEmpty: false } ) {
				var anyPreFailed = false;

				var impl = topAction.MakeClonedImpl();
				foreach ( var action in impl ) {
					action.ElapsedTime = 0f;
					action.SetupContext( world, entity );
					action.PreExecuteResult = action.OnPreExecute();
					if ( action.PreExecuteResult is GoapActionExecuteStatus.Failure ) {
						anyPreFailed = true;
						GameLogger.Warning( $"{topAction.Name}: {action.GetType().Name}::OnPreExecute report Failure." );
					}
					action.ExecuteResult = GoapActionExecuteStatus.Running;
				}

				if ( anyPreFailed ) {
					entity.SetGoapPlanInExecuting( topAction, impl, GoapActionExecuteStatus.Failure, true );
				}
				else {
					entity.SetGoapPlanInExecuting( topAction, impl, GoapActionExecuteStatus.Running, false );
				}
					
			}
		}

		private void OnEntityGoapPlanModified( World world, Entity entity ) {
			if ( !entity.HasGoapPlanInExecuting() ) {
				SetupEntityForExecute( world, entity );
			}
			else {
				var goapPlanInExecutingComponent = entity.GetGoapPlanInExecuting();
				var goapPlanComponent = entity.GetGoapPlan();
				var topAction = goapPlanComponent.Action;
				if ( topAction != goapPlanInExecutingComponent.Action ) {
					if ( topAction is null ) {
						entity.SetGoapPlanInExecuting( null, ImmutableArray< GoapAction >.Empty, GoapActionExecuteStatus.Failure, true );
					}
					else {

						bool actionCanBeInterrupted = !goapPlanInExecutingComponent.Action.ExecutePolicy.HasFlag( GoapActionExecutePolicy.CantBeInterrupted );
						bool actionExecutingFinished = goapPlanInExecutingComponent.ActionExecuteStatus is not GoapActionExecuteStatus.Running;
						if ( actionCanBeInterrupted || actionExecutingFinished ) {
							entity.RemoveGoapPlanInExecuting();
							SetupEntityForExecute( world, entity );
						}
						else {
							// do nothing, execute current plan until no Running.
							
						}
					}
				}
				else {
					// if ( goapPlanComponent.Action is not null ) {
					// 	
					// }
				}
			}
		}
	}

	
	public enum GoapAgentEvaluateFrequency : byte {
		Interval,
		Message
	}


	/// <summary>
	/// Enumeration for comparisons
	/// </summary>
	public enum CompareMethod : byte {
		EqualTo,
		GreaterThan,
		LessThan,
		GreaterOrEqualTo,
		LessOrEqualTo,
		NotEqualTo
	}


	public enum BooleanCompareMethod : byte {
		EqualTo,
		NotEqualTo
	}
	
	
	public static class NumericCompareHelper {
		
		public static bool Compare( float value1, CompareMethod method, float value2 ) {
			switch ( method ) {
				case CompareMethod.EqualTo:          return Numeric.AreEqual( value1, value2 );
				case CompareMethod.GreaterThan:      return Numeric.IsGreater( value1, value2 );
				case CompareMethod.LessThan:         return Numeric.IsLess( value1, value2 );
				case CompareMethod.GreaterOrEqualTo: return Numeric.IsGreaterOrEqual( value1, value2 );
				case CompareMethod.LessOrEqualTo:    return Numeric.IsLessOrEqual( value1, value2 );
				case CompareMethod.NotEqualTo:       return !Numeric.AreEqual( value1, value2 );
			}

			throw new ArgumentException( nameof( method ) );
		}
		
		
		public static bool Compare( int value1, CompareMethod method, int value2 ) {
			switch ( method ) {
				case CompareMethod.EqualTo:          return value1 == value2;
				case CompareMethod.GreaterThan:      return value1 > value2;
				case CompareMethod.LessThan:         return value1 < value2;
				case CompareMethod.GreaterOrEqualTo: return value1 >= value2;
				case CompareMethod.LessOrEqualTo:    return value1 <= value2;
				case CompareMethod.NotEqualTo:       return value1 != value2;
			}

			throw new ArgumentException( nameof( method ) );
		}
		
	}


	/// <summary>
	/// Enumeration for Operations (Add, Subtract, Equality etc)
	/// </summary>
	public enum OperationMethod : byte {
		Set,
		Add,
		Subtract,
		Multiply,
		Divide
	}


	public readonly struct RequestGoapAgentEvaluateMessage : IMessage;


	public readonly struct GoapAgentEvaluateFinishedMessage : IMessage;
	
	
	public readonly struct StopExecutingGoapPlanMessage : IMessage;


	public readonly struct GoapAgentComponent : IModifiableComponent {

		[GameAssetId< GoapScenarioAsset >]
		public readonly Guid GoapScenarioAsset;
		
		public readonly string Goal;
		
		public readonly GoapAgentEvaluateFrequency EvaluateFrequency = GoapAgentEvaluateFrequency.Message; // Request Default.
		public readonly float EvaluateInterval = 1f; // 0 - EveryFrame, in seconds

		[JsonIgnore, HideInEditor]
		public readonly GoapActionPlanner Planner;

		[JsonIgnore, HideInEditor]
		public readonly bool PlannerChangedOnly;
		
		public GoapAgentComponent() {}


		public GoapAgentComponent( Guid goapScenarioAsset ) {
			GoapScenarioAsset = goapScenarioAsset;
		}
		
		
		public GoapAgentComponent( Guid goapScenarioAsset, string goal ) {
			GoapScenarioAsset = goapScenarioAsset;
			Goal = goal ?? GetDefaultGoal()?.Name;
		}
		
		
		public GoapAgentComponent( Guid goapScenarioAsset, GoapScenarioGoal goal ) {
			GoapScenarioAsset = goapScenarioAsset;
			Goal = goal?.Name;
		}
		
		
		public GoapAgentComponent( Guid goapScenarioAsset, string goal, GoapAgentEvaluateFrequency evaluateFrequency, float evaluateInterval = 1f ) {
			GoapScenarioAsset = goapScenarioAsset;
			Goal = goal ?? GetDefaultGoal()?.Name;
			EvaluateFrequency = evaluateFrequency;
			EvaluateInterval = evaluateInterval;
		}
		
		
		public GoapAgentComponent( Guid goapScenarioAsset, string goal, GoapAgentEvaluateFrequency evaluateFrequency, float evaluateInterval, GoapActionPlanner planner, bool plannerChangedOnly ) {
			GoapScenarioAsset = goapScenarioAsset;
			Goal = goal ?? GetDefaultGoal()?.Name;
			EvaluateFrequency = evaluateFrequency;
			EvaluateInterval = evaluateInterval;
			Planner = planner;
			PlannerChangedOnly = plannerChangedOnly;
		}


		public bool HasCondition( string condition ) {
			var scenarioAsset = Game.Data.TryGetAsset< GoapScenarioAsset >( GoapScenarioAsset );
			if ( scenarioAsset is not null ) {
				return scenarioAsset.HasCondition( condition );
			}

			GameLogger.Error( "GoapScenarioAsset not found for GoapAgentComponent." );
			return false;
		}

		
		public bool CheckCondition( string condition, World world, Entity entity ) {
			var scenarioAsset = Game.Data.TryGetAsset< GoapScenarioAsset >( GoapScenarioAsset );
			if ( scenarioAsset is null ) {
				GameLogger.Error( "GoapScenarioAsset not found for GoapAgentComponent." );
				return false;
			}
			return GoapComputeSystem.EvaluateCondition( scenarioAsset, condition, world, entity );
		}
		
		
		public bool CheckGoalConditions( string goalName, World world, Entity entity ) {
			var scenarioAsset = Game.Data.TryGetAsset< GoapScenarioAsset >( GoapScenarioAsset );
			if ( scenarioAsset is null ) {
				GameLogger.Error( "GoapScenarioAsset not found for GoapAgentComponent." );
				return false;
			}
			return GoapComputeSystem.EvaluateGoalConditions( scenarioAsset, goalName, world, entity );
		}
		
		
		public GoapScenarioAsset TryGetScenario() => Game.Data.TryGetAsset< GoapScenarioAsset >( GoapScenarioAsset );


		public GoapAgentComponent SetDefaultGoal() {
			if (  TryGetScenario() is {} goapScenarioAsset ) {
				foreach ( var goal in goapScenarioAsset.Goals ) {
					if ( goal.IsDefault ) {
						// TODO:
						// Goal = goal.Name;
						break;
					}
				}
			}
			
			return this;
		}


		public GoapScenarioGoal GetDefaultGoal() {
			foreach ( var goal in TryGetScenario().Goals ) {
				if ( goal.IsDefault ) {
					return goal;
				}
			}
			
			return null;
		}

		public void Subscribe( Action notification ) {}

		public void Unsubscribe( Action notification ) {}

        
		// public override bool Equals( object obj ) {
		// 	
		// 	// ignore Planner Changed for IReactSystem
		// 	if ( obj is GoapAgentComponent otherGoapAgentComponent ) {
		// 		return otherGoapAgentComponent.GoapScenarioAsset == GoapScenarioAsset &&
		// 			   otherGoapAgentComponent.Goal == Goal &&
		// 			   otherGoapAgentComponent.EvaluateFrequency == EvaluateFrequency &&
		// 			   Numeric.AreEqual( otherGoapAgentComponent.EvaluateInterval, EvaluateInterval );
		// 	}
		// 	
		// 	return base.Equals( obj );
		// }
	}


	[RuntimeOnly]
	public readonly struct GoapAgentEvaluateTimerComponent : IComponent {
		
		public readonly float Time;
		public GoapAgentEvaluateTimerComponent( float time ) => Time = time;
	}

	
	public readonly struct GoapAgentPausedEvaluateComponent : IComponent;


	[RuntimeOnly, DoNotPersistOnSave]
	public readonly struct GoapPlanComponent : IModifiableComponent {
		
		public readonly GoapScenarioGoal Goal;
		public readonly Stack< GoapScenarioAction > Actions;
		
		public GoapScenarioAction Action => Actions?.Count > 0 ? Actions.Peek() : null;
		
		public GoapPlanComponent( GoapScenarioGoal goal, Stack< GoapScenarioAction > actions ) {
			Goal = goal;
			Actions = actions;
		}
		
		public void Subscribe( Action notification ) {}

		public void Unsubscribe( Action notification ) {}
	}


	[RuntimeOnly, DoNotPersistOnSave]
	public readonly struct GoapNoPlanComponent : IComponent {
		
		public readonly GoapScenarioGoal ForGoal;
		
		public GoapNoPlanComponent( GoapScenarioGoal goal ) {
			ForGoal = goal;
		}
		
	}


	public readonly struct GoapPlanExecutorComponent : IComponent;

	
	[RuntimeOnly, DoNotPersistOnSave]
	public readonly struct GoapPlanInExecutingComponent : IComponent {
		public readonly GoapScenarioAction Action;
		public readonly ImmutableArray< GoapAction > ActionImplCloned = ImmutableArray< GoapAction >.Empty;
		public readonly GoapActionExecuteStatus ActionExecuteStatus;
		public readonly bool IsInterrupted;

		public GoapPlanInExecutingComponent( GoapScenarioAction action, ImmutableArray< GoapAction > actionImplCloned, GoapActionExecuteStatus status, bool isInterrupted = false ) {
			Action = action;
			ActionImplCloned = actionImplCloned;
			ActionExecuteStatus = status;
			IsInterrupted = isInterrupted;
		}
	}


	#region AStar
	
	public class AStarNode : IComparable<AStarNode>, IEquatable<AStarNode>, IRecyclable {
		
		/// <summary>
		/// The state of the world at this node.
		/// </summary>
		public GoapWorldState WorldState;

		/// <summary>
		/// The cost so far.
		/// </summary>
		public int CostSoFar;

		/// <summary>
		/// The heuristic for remaining cost (don't overestimate!)
		/// </summary>
		public int HeuristicCost;

		/// <summary>
		/// costSoFar + heuristicCost (g+h) combined.
		/// </summary>
		public int CostSoFarAndHeuristicCost;

		/// <summary>
		/// the Action associated with this node
		/// </summary>
		public GoapScenarioAction Action;

		// Where did we come from?
		public AStarNode Parent;
		public GoapWorldState ParentWorldState;
		public int Depth;


		#region IEquatable and IComparable

		public bool Equals( AStarNode other ) {
			long care = WorldState.DontCare ^ -1L;
			return ( WorldState.Values & care ) == ( other.WorldState.Values & care );
		}


		public int CompareTo( AStarNode other ) {
			return CostSoFarAndHeuristicCost.CompareTo( other.CostSoFarAndHeuristicCost );
		}

		#endregion


		public void Reset() {
			Action = null;
			Parent = null;
		}


		public AStarNode Clone() {
			return ( AStarNode )MemberwiseClone();
		}


		public override string ToString() {
			return string.Format( "[cost: {0} | heuristic: {1}]: {2}", CostSoFar, HeuristicCost, Action );
		}

		public void Recycle() {
			CostSoFar = 0;
			HeuristicCost = 0;
			CostSoFarAndHeuristicCost = 0;
			Parent = null;
			ParentWorldState = default;
			Depth = 0;
			Reset();
		}

	}


	public static class AStar {
		
		internal static readonly AStarStorage Storage = new ();

		internal static readonly ResourcePool< AStarNode > AStarNodePool = new (
			() => new AStarNode(), // Create
			null,                  // Initialize
			null                 // Uninitialize
		);
		
		internal static readonly ResourcePool< List< AStarNode > > AStarNodeListPool = new (
			() => new List< AStarNode >(), // Create
			null,                          // Initialize
			list => list.Clear()         // Uninitialize
		);

		/* from: http://theory.stanford.edu/~amitp/GameProgramming/ImplementationNotes.html
		OPEN = priority queue containing START
		CLOSED = empty set
		while lowest rank in OPEN is not the GOAL:
		  current = remove lowest rank item from OPEN
		  add current to CLOSED
		  for neighbors of current:
			cost = g(current) + movementcost(current, neighbor)
			if neighbor in OPEN and cost less than g(neighbor):
			  remove neighbor from OPEN, because new path is better
			if neighbor in CLOSED and cost less than g(neighbor): **
			  remove neighbor from CLOSED
			if neighbor not in OPEN and neighbor not in CLOSED:
			  set g(neighbor) to cost
			  add neighbor to OPEN
			  set priority queue rank to g(neighbor) + h(neighbor)
			  set neighbor's parent to current
		*/

		/// <summary>
		/// Make a plan of actions that will reach desired world state
		/// </summary>
		/// <param name="ap">Ap.</param>
		/// <param name="start">Start.</param>
		/// <param name="goal">Goal.</param>
		public static Stack< GoapScenarioAction > Plan( GoapActionPlanner ap, GoapWorldState start, GoapWorldState goal, in Stack< GoapScenarioAction > returnActions = null, List< AStarNode > selectedNodes = null ) {
			Storage.Clear();

			var currentNode = AStarNodePool.Obtain();
			currentNode.WorldState = start;
			currentNode.ParentWorldState = start;
			currentNode.CostSoFar = 0;                                                                 // g
			currentNode.HeuristicCost = CalculateHeuristic( start, goal );                         // h
			currentNode.CostSoFarAndHeuristicCost = currentNode.CostSoFar + currentNode.HeuristicCost; // f
			currentNode.Depth = 1;

			Storage.AddToOpenList( currentNode );

			while ( true ) {
				// nothing left open so we failed to find a path
				if ( !Storage.HasOpened() ) {
					Storage.Clear();
					return null;
				}

				currentNode = Storage.RemoveCheapestOpenNode();

				Storage.AddToClosedList( currentNode );

				// all done. we reached our goal
				if ( goal.Equals( currentNode.WorldState ) ) {
					var plan = ReconstructPlan( currentNode, returnActions, selectedNodes );
					Storage.Clear();
					return plan;
				}

				var neighbors = ap.GetPossibleTransitions( currentNode.WorldState );
				for ( var i = 0; i < neighbors.Count; i++ ) {
					var cur = neighbors[ i ];
					var opened = Storage.FindOpened( cur );
					var closed = Storage.FindClosed( cur );
					var cost = currentNode.CostSoFar + cur.CostSoFar;

					// if neighbor in OPEN and cost less than g(neighbor):
					if ( opened != null && cost < opened.CostSoFar ) {
						// remove neighbor from OPEN, because new path is better
						Storage.RemoveOpened( opened );
						opened = null;
					}

					// if neighbor in CLOSED and cost less than g(neighbor):
					if ( closed != null && cost < closed.CostSoFar ) {
						// remove neighbor from CLOSED
						Storage.RemoveClosed( closed );
					}

					// if neighbor not in OPEN and neighbor not in CLOSED:
					if ( opened == null && closed == null ) {
						var nb = AStarNodePool.Obtain();
						nb.WorldState = cur.WorldState;
						nb.CostSoFar = cost;
						nb.HeuristicCost = CalculateHeuristic( cur.WorldState, goal );
						nb.CostSoFarAndHeuristicCost = nb.CostSoFar + nb.HeuristicCost;
						nb.Action = cur.Action;
						nb.ParentWorldState = currentNode.WorldState;
						nb.Parent = currentNode;
						nb.Depth = currentNode.Depth + 1;
						Storage.AddToOpenList( nb );
					}
				}

				// done with neighbors so release it back to the pool
				AStarNodeListPool.Recycle( neighbors );
			}
		}


		/// <summary>
		/// internal function to reconstruct the plan by tracing from last node to initial node
		/// </summary>
		/// <returns>The plan.</returns>
		/// <param name="goalNode">Goalnode.</param>
		static Stack< GoapScenarioAction > ReconstructPlan( AStarNode goalNode, in Stack< GoapScenarioAction > returnActions = null, List< AStarNode > selectedNodes = null ) {
			var totalActionsInPlan = goalNode.Depth - 1;
			var plan = returnActions ?? new Stack< GoapScenarioAction >( totalActionsInPlan );
			
			var currentNode = goalNode;
			for ( var i = 0; i <= totalActionsInPlan - 1; i++ ) {
				// optionally add the node to the List if we have been passed one
				// selectedNodes.Add( currentNode.Clone() );
				selectedNodes?.Add( currentNode );
				plan.Push( currentNode.Action );
				currentNode = currentNode.Parent;
			}

			// our nodes went from the goal back to the start so reverse them
			selectedNodes?.Reverse();

			// foreach ( var node in selectedNodes ) {
			// 	plan.Push( node.Action );
			// }

			return plan;
		}


		/// <summary>
		/// This is our heuristic: estimate for remaining distance is the nr of mismatched atoms that matter.
		/// </summary>
		/// <returns>The heuristic.</returns>
		/// <param name="fr">Fr.</param>
		/// <param name="to">To.</param>
		static int CalculateHeuristic( in GoapWorldState fr, in GoapWorldState to ) {
			long care = to.DontCare ^ -1L;
			long diff = ( fr.Values & care ) ^ ( to.Values & care );
			int dist = 0;

			for ( var i = 0; i < GoapActionPlanner.MAX_CONDITIONS; ++i )
				if ( ( diff & ( 1L << i ) ) != 0 )
					dist++;
			return dist;
		}

	}


	public class AStarStorage {
		
		// The maximum number of nodes we can store
		private const int MAX_NODES = 128;

		private AStarNode[] _opened = new AStarNode[MAX_NODES];
		private AStarNode[] _closed = new AStarNode[MAX_NODES];
		private int _numOpened;
		private int _numClosed;

		private int _lastFoundOpened;
		private int _lastFoundClosed;


		internal AStarStorage() {}


		public void Clear() {
			for ( var i = 0; i < _numOpened; i++ ) {
				AStar.AStarNodePool.Recycle( _opened[ i ] );
				_opened[ i ] = null;
			}

			for ( var i = 0; i < _numClosed; i++ ) {
				AStar.AStarNodePool.Recycle( _closed[ i ] );
				_closed[ i ] = null;
			}

			_numOpened = _numClosed = 0;
			_lastFoundClosed = _lastFoundOpened = 0;
		}


		public AStarNode FindOpened( AStarNode node ) {
			for ( var i = 0; i < _numOpened; i++ ) {
				long care = node.WorldState.DontCare ^ -1L;
				if ( ( node.WorldState.Values & care ) == ( _opened[ i ].WorldState.Values & care ) ) {
					_lastFoundClosed = i;
					return _closed[ i ];
				}
			}

			return null;
		}


		public AStarNode FindClosed( AStarNode node ) {
			for ( var i = 0; i < _numClosed; i++ ) {
				long care = node.WorldState.DontCare ^ -1L;
				if ( ( node.WorldState.Values & care ) == ( _closed[ i ].WorldState.Values & care ) ) {
					_lastFoundClosed = i;
					return _closed[ i ];
				}
			}

			return null;
		}


		public bool HasOpened() {
			return _numOpened > 0;
		}


		public void RemoveOpened( AStarNode node ) {
			if ( _numOpened > 0 ) _opened[ _lastFoundOpened ] = _opened[ _numOpened - 1 ];
			_numOpened--;
		}


		public void RemoveClosed( AStarNode node ) {
			if ( _numClosed > 0 ) _closed[ _lastFoundClosed ] = _closed[ _numClosed - 1 ];
			_numClosed--;
		}


		public bool IsOpen( AStarNode node ) {
			return Array.IndexOf( _opened, node ) > -1;
		}


		public bool IsClosed( AStarNode node ) {
			return Array.IndexOf( _closed, node ) > -1;
		}


		public void AddToOpenList( AStarNode node ) {
			_opened[ _numOpened++ ] = node;
		}


		public void AddToClosedList( AStarNode node ) {
			_closed[ _numClosed++ ] = node;
		}


		public AStarNode RemoveCheapestOpenNode() {
			var lowestVal = int.MaxValue;
			_lastFoundOpened = -1;
			for ( var i = 0; i < _numOpened; i++ ) {
				if ( _opened[ i ].CostSoFarAndHeuristicCost < lowestVal ) {
					lowestVal = _opened[ i ].CostSoFarAndHeuristicCost;
					_lastFoundOpened = i;
				}
			}

			var val = _opened[ _lastFoundOpened ];
			RemoveOpened( val );

			return val;
		}

	}
	
	#endregion
	
}


namespace Pixpil.Assets {
	
	using Pixpil.AI;

	public class GoapScenarioAsset : GameAsset {
		
		public override string EditorFolder => "#\uf2dbGoapScenarios";

		public override char Icon => '\uf2db';
        
        public override Vector4 EditorColor => new ( 0f, 1f, 0f, 1f );

		[Bang.Serialize]
		public ImmutableArray< string > ConditionDefines = ImmutableArray< string >.Empty;
		
		[Bang.Serialize]
		public ImmutableArray< GoapScenarioAction > Actions = ImmutableArray< GoapScenarioAction >.Empty;
		
		[Bang.Serialize]
		public ImmutableArray< GoapScenarioGoal > Goals = ImmutableArray< GoapScenarioGoal >.Empty;
		
		[Bang.Serialize]
		public ImmutableDictionary< string, GoapCondition > Conditions = ImmutableDictionary< string , GoapCondition >.Empty;


		public GoapScenarioAsset() {}

		protected override void OnModified() {
			
			// sync ConditionDefines
			foreach ( var key in Conditions.Keys ) {
				if ( !ConditionDefines.Contains( key ) ) {
					Conditions = Conditions.Remove( key );
				}
			}

			foreach ( var key in ConditionDefines ) {
				if ( !Conditions.ContainsKey( key ) ) {
					Conditions = Conditions.Add( key, null );
				}
			}
		}

		public int GetConditionID( string aConditionName ) {
			var found = ConditionDefines.FirstOrDefault( item => item == aConditionName );
			if ( found is not null ) {
				return ConditionDefines.IndexOf( found );
			}

			return -1;
		}

		
		public string GetConditionName( int id ) {
			return ConditionDefines[ id ];
		}


		public GoapScenarioAction GetAction( string actionName ) => Actions.FirstOrDefault( action => action.Name == actionName );


		public bool HasCondition( string condition ) => Conditions.ContainsKey( condition );


		public GoapCondition GetConditionImpl( string condition ) {
			Conditions.TryGetValue( condition, out var conditionImpl );
			return conditionImpl;
		}


		// internal bool CheckCondition( string condition ) {
		// 	if ( !Conditions.ContainsKey( condition ) ) {
		// 		GameLogger.Error( $"condition not found: {condition}" );
		// 		return false;
		// 	}
		// 	return Conditions[ condition ].OnCheck( world, entity );
		// }
		//
		//
		// internal bool CheckGoalConditions( string goalName ) {
		// 	var goal = FindGoal( goalName );
		// 	if ( goal is null ) {
		// 		GameLogger.Error( $"goal not found: {goalName}" );
		// 		return false;
		// 	}
		// 	
		// 	foreach ( var goalCondition in goal.Conditions ) {
		// 		if ( CheckCondition( goalCondition.Key ) != goalCondition.Value ) {
		// 			return false;
		// 		}
		// 	}
		//
		// 	return true;
		// }
		
		
		public GoapScenarioGoal FindGoal( string goal ) {
			return Goals.FirstOrDefault( scenarioGoal => scenarioGoal.Name == goal );
		}
		
		
		public GoapScenarioGoal GetDefaultGoal() {
			return Goals.FirstOrDefault( scenarioGoal => scenarioGoal.IsDefault );
		}
	}
	
}
