using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DigitalRune.Collections;
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
	using Pixpil.Messages;

	/// AiAction Execution Status enumeration
	public enum AiActionExecuteStatus : byte {
		
		/// The action has succeeded.
		Success = 0,
		
		/// The action has failed.
		Failure = 1,
		
		/// The action is still running.
		Running = 2
	}


	[Flags]
	public enum AiActionExecutePolicy : int {
		None = 0,
		CantBeInterrupted = 1 << 1,
	}
	
	
	[Flags]
	public enum AiActionPhase : byte {
		None = 0,
		OnPre = 1,
		OnExecute = 1 << 2,
		OnPost = 1 << 3
	}


	public class AiActionScenario {
		
		[Bang.Serialize]
		public readonly string Name;
		
		[Bang.Serialize]
		public bool IsActived = true;
		
		[Bang.Serialize]
		public readonly AiActionExecutePolicy ExecutePolicy;
		
		[Bang.Serialize]
		public readonly ImmutableArray< AiAction > Impl = ImmutableArray< AiAction >.Empty;
		
		public AiActionScenario() {}

		public AiActionScenario( string name ) {
			Name = name;
		}
		
		public AiActionScenario( string name, AiActionExecutePolicy executePolicy ) {
			Name = name;
			ExecutePolicy = executePolicy;
		}

		public ImmutableArray< AiAction > MakeClonedImpl() {
			var builder = ImmutableArray.CreateBuilder< AiAction >( Impl.Length );
			foreach ( var action in Impl ) {
				builder.Add( action.MakeACopy() );
			}
			return builder.ToImmutable();
		}
	}


	[DebuggerDisplay("Action: {GetType().Name}, Hash: {GetHashCode()}")]
	[Serializable]
	public abstract class AiAction /*: ICloneable*/ {

		// public readonly AiActionExecutePolicy ExecutePolicy = AiActionExecutePolicy.None;
		
		[System.Text.Json.Serialization.JsonIgnore, ShowInEditor]
		public float ElapsedTime;
		
		[System.Text.Json.Serialization.JsonIgnore]
		public AiActionExecuteStatus PreExecuteResult = AiActionExecuteStatus.Running;
		
		[System.Text.Json.Serialization.JsonIgnore]
		public AiActionExecuteStatus ExecuteResult = AiActionExecuteStatus.Running;

		public AiAction() {
			GameLogger.Log( "[AiAction] created." );
			// PreExecuteResult = AiActionExecuteStatus.Running;
		}
		
		public virtual AiActionExecuteStatus OnPreExecute( World world, Entity entity ) => AiActionExecuteStatus.Running;
		// public virtual IEnumerator< Wait > OnCoroutine() { yield return Wait.Stop; }
		public virtual AiActionExecuteStatus OnExecute( World world, Entity entity ) => AiActionExecuteStatus.Success;
		public virtual void OnPostExecute( World world, Entity entity ) {}
		
		public AiAction MakeACopy() {
			// return SerializationHelper.DeepCopy( this );
			var clonedAiAction = ( AiAction )MemberwiseClone();
			clonedAiAction.ElapsedTime = 0f;
			clonedAiAction.PreExecuteResult = AiActionExecuteStatus.Running;
			clonedAiAction.ExecuteResult = AiActionExecuteStatus.Running;
			return clonedAiAction;
		}

		// public override int GetHashCode() {
		// 	return GetType().FullName.GetHashCode();
		// }
	}


	public readonly struct AiActionExecutorComponent : IModifiableComponent {
        
        [GameAssetId< AiActionScenarioAsset >]
        public readonly Guid AiActionScenarioAsset;

		public readonly AiActionScenario Action;
		
		public AiActionExecutorComponent( Guid aiActionScenarioAsset, AiActionScenario action ) {
			AiActionScenarioAsset = aiActionScenarioAsset;
			Action = action;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public AiActionScenarioAsset TryGetScenario() => Game.Data.TryGetAsset< AiActionScenarioAsset >( AiActionScenarioAsset );
		
		public AiActionScenario FindAction( string name ) {
			return TryGetScenario().GetAction( name );
		}

		public void Subscribe( Action notification ) {}

		public void Unsubscribe( Action notification ) {}
	}
	
	
	[RuntimeOnly, DoNotPersistOnSave]
	public readonly struct AiActionInExecutingComponent : IModifiableComponent {

		public readonly AiActionScenario Action;
		public readonly ImmutableArray< AiAction > ActionCloned = ImmutableArray< AiAction >.Empty;
		public readonly AiActionExecuteStatus ActionExecuteStatus;
		public readonly bool IsInterrupted;

		public AiActionInExecutingComponent( AiActionScenario action, in ImmutableArray< AiAction > actionCloned, AiActionExecuteStatus status, bool isInterrupted = false ) {
			Action = action;
			ActionCloned = actionCloned;
			ActionExecuteStatus = status;
			IsInterrupted = isInterrupted;
		}

		public float GetActionElapsedTime() {
			float time = default;
			foreach ( var action in ActionCloned ) {
				if ( action.ElapsedTime > time ) {
					time = action.ElapsedTime;
				}
			}
			return time;
		}

		public void Subscribe( Action notification ) {}

		public void Unsubscribe( Action notification ) {}
	}


	public record struct AiActionExecutingItem {
		
		public AiActionScenario Action;
		public AiActionExecuteStatus ActionExecuteStatus;
		public bool IsInterrupted;
		public float ElapsedTime;
		
		public AiActionExecutingItem( AiActionScenario action, AiActionExecuteStatus actionExecuteStatus, bool isInterrupted, float elapsedTime ) {
			Action = action;
			ActionExecuteStatus = actionExecuteStatus;
			IsInterrupted = isInterrupted;
			ElapsedTime = elapsedTime;
		}
	}


	/// <summary>
	/// for debug.
	/// </summary>
	[Requires( typeof( AiActionExecutorComponent ) )]
	public readonly struct AiActionExecutingHistoryComponent : IModifiableComponent {

		public readonly int Capacity;
		public readonly Deque< AiActionExecutingItem > Deque;
		
		public AiActionExecutingHistoryComponent( int capacity, Deque< AiActionExecutingItem > deque ) {
			Capacity = capacity;
			Deque = deque;
		}

		public void Subscribe( Action notification ) {}

		public void Unsubscribe( Action notification ) {}
	}


	[Watch( typeof( AiActionExecutorComponent ) )]
	[Filter( ContextAccessorFilter.AllOf, ContextAccessorKind.Read, typeof( AiActionExecutorComponent ) )]
    [Messager(typeof( StopExecutingAiActionMessage ))]
	public class AiActionExecutingSystem : IFixedUpdateSystem, IUpdateSystem, IReactiveSystem, IMessagerSystem, IMurderRenderSystem {

		public void FixedUpdate( Context context ) {
			// foreach ( var entity in context.Entities ) {
			// 	
			// 	if ( entity.HasAiActionInExecuting() ) {
			// 		continue;
			// 	}
			//
			// 	var aiActionExecutorComponent = entity.GetAiActionExecutor();
			// 	if ( aiActionExecutorComponent.Action != null ) {
			// 		SetupEntityForExecute( context.World, entity ) ;
			// 	}
			// }
		}

		public void Update( Context context ) {
			
			foreach ( var entity in context.Entities ) {
				if ( !entity.HasAiActionInExecuting() ) {
					continue;
				}
				
				var aiActionInExecuting = entity.GetAiActionInExecuting();
				switch ( aiActionInExecuting.ActionExecuteStatus ) {
					case AiActionExecuteStatus.Failure: {
						if ( !aiActionInExecuting.ActionCloned.IsDefaultOrEmpty ) {
							foreach ( var subTask in aiActionInExecuting.ActionCloned ) {
								subTask.OnPostExecute( context.World, entity );
							}
						}
					
						OnAiActionExecutingFinished( entity, in aiActionInExecuting, AiActionExecuteStatus.Failure );
					
						entity.RemoveAiActionInExecuting();
						entity.SendMessage( new AiActionExecutingFinishedMessage( AiActionExecuteStatus.Failure ) );

						// after previous, start next
						var aiActionExecutorComponent = entity.GetAiActionExecutor();
						var topAction = aiActionExecutorComponent.Action;
						if ( topAction != null ) {
							OnEntityAiActionModified( context.World, entity );
						}
						break;
					}
					case AiActionExecuteStatus.Success: {
                        entity.RemoveAiActionInExecuting();
                        
						var aiActionExecutorComponent = entity.GetAiActionExecutor();
						var topAction = aiActionExecutorComponent.Action;
						if ( topAction != null ) {
							OnEntityAiActionModified( context.World, entity );
						}
						break;
					}
					case AiActionExecuteStatus.Running:
						var allSuccessful = true;
						var anyRunning = false;
						foreach ( var subTask in aiActionInExecuting.ActionCloned ) {
							var previousStatue = subTask.ExecuteResult;
							if ( previousStatue is not AiActionExecuteStatus.Running ) {
								continue;
							}

							subTask.ElapsedTime += Game.DeltaTime;
							var result = subTask.OnExecute( context.World, entity );
							switch ( result ) {
								case AiActionExecuteStatus.Success:
									// do nothing
									break;
								case AiActionExecuteStatus.Running:
									anyRunning = true;
									break;
								case AiActionExecuteStatus.Failure:
									allSuccessful = false;
									break;
							}

							subTask.ExecuteResult = result;

							if ( !allSuccessful ) {
								break;
							}
						}

						// Successful and go next.
						if ( allSuccessful ) {

							if ( !anyRunning ) {
								foreach ( var subTask in aiActionInExecuting.ActionCloned ) {
									subTask.OnPostExecute( context.World, entity );
								}

								// if ( aiActionInExecuting.Action is not null ) {
								// 	GameLogger.Log( $"{aiActionInExecuting.Action?.Name}: Success." );
								// }
								
								entity.SetAiActionInExecuting( aiActionInExecuting.Action, aiActionInExecuting.ActionCloned, AiActionExecuteStatus.Success, false );
							
								OnAiActionExecutingFinished( entity, in aiActionInExecuting, AiActionExecuteStatus.Success );
							
								// entity.RemoveAiActionInExecuting();
								entity.SendMessage( new AiActionExecutingFinishedMessage( AiActionExecuteStatus.Success ) );
							}
							else {
								entity.SetAiActionInExecuting( aiActionInExecuting.Action, aiActionInExecuting.ActionCloned, AiActionExecuteStatus.Running, false );
							}

						}
						else { // Failure and Msg.
							foreach ( var subTask in aiActionInExecuting.ActionCloned ) {
								subTask.OnPostExecute( context.World, entity );
							}
						
							// GameLogger.Log( $"{aiActionInExecuting.Action.Name}: !allSuccessful, go next." );
							
							entity.SetAiActionInExecuting( aiActionInExecuting.Action, aiActionInExecuting.ActionCloned, AiActionExecuteStatus.Failure, false );
						
							OnAiActionExecutingFinished( entity, in aiActionInExecuting, AiActionExecuteStatus.Failure );
						
							entity.SendMessage( new AiActionExecutingFinishedMessage( AiActionExecuteStatus.Failure ) );
							// entity.RemoveAiActionInExecuting();
						}
						
						break;
				}

			}
		}

		public void OnAdded( World world, ImmutableArray< Entity > entities ) {
			foreach ( var entity in entities ) {
				SetupEntityForExecute( world, entity );
			}
		}

		public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
			// foreach ( var entity in entities ) {
			// 	if ( entity.TryGetGoapPlanInExecuting() is {} goapPlanInExecuting ) {
			// 		if ( goapPlanInExecuting.Action != null ) {
			// 			foreach ( var subTask in goapPlanInExecuting.ActionImplCloned ) {
			// 				subTask.OnPostExecute();
			// 			}
			// 			
			// 			entity.RemoveGoapPlanInExecuting();
			// 		}
			// 	}
			// }
		}

		public void OnModified( World world, ImmutableArray< Entity > entities ) {
			foreach ( var entity in entities ) {
				OnEntityAiActionModified( world, entity );
			}
		}

		public void OnMessage( World world, Entity entity, IMessage message ) {
			if ( message is StopExecutingAiActionMessage ) {
				if ( entity.HasAiActionInExecuting() ) {
					var aiActionInExecuting = entity.GetAiActionInExecuting();
					entity.SetAiActionInExecuting( aiActionInExecuting.Action, aiActionInExecuting.ActionCloned, aiActionInExecuting.ActionExecuteStatus, true );
				}
			}
		}
		
		public void Draw( RenderContext render, Context context ) {

			// if ( context.World.TryGetUnique< EditorComponent >() is {} editorComponent && editorComponent.EditorHook != null && editorComponent.EditorHook.ShowStates ) {
			// 	var drawInfo = new DrawInfo( Color.White, 0.2f ) { Outline = Color.Black, Scale = Vector2.One, Offset = new Vector2( 0, 0 ) };
			// 	var statusDrawInfo = new DrawInfo( Color.Green, 0.2f ) { Scale = Vector2.One * 0.8f, Offset = new Vector2( 0, 0 ) };
			// 	
			// 	foreach ( var entity in context.Entities ) {
			// 		if ( entity.HasInCamera() && entity.TryGetAiActionInExecuting() is {} aiActionInExecutingComponent ) {
			// 			var entityInScreen = entity.GetPosition().ToVector2() + new Vector2( 10f, 38f );
			// 			render.DebugBatch.DrawText( MurderFonts.PixelFont, $"{aiActionInExecutingComponent.Action?.Name}", entityInScreen + new Vector2( 0, 0 ), drawInfo );
			// 			render.DebugBatch.DrawText( MurderFonts.PixelFont, $"{aiActionInExecutingComponent.ActionExecuteStatus}", entityInScreen + new Vector2( 0, 8 ), statusDrawInfo );
			// 			// render.DebugBatch.DrawText( MurderFonts.PixelFont, $"{goapPlanInExecutingComponent.Action?.}", entityInScreen + new Vector2( 0, 16 ), statusDrawInfo );
			// 		}
			// 	}
			// }
		}
		
		private void SetupEntityForExecute( World world, Entity entity ) {
			var aiActionExecutorComponent = entity.GetAiActionExecutor();
			
			var topAction = aiActionExecutorComponent.Action;
			if ( topAction is { Impl.IsDefaultOrEmpty: false } ) {
				var allPreSuccessed = true;
				var anyPreFailed = false;
				var impl = topAction.MakeClonedImpl();
				
				foreach ( var action in impl ) {
					action.ElapsedTime = 0f;
					action.PreExecuteResult = action.OnPreExecute( world, entity );
					if ( action.PreExecuteResult is AiActionExecuteStatus.Failure ) {
						anyPreFailed = true;
						// GameLogger.Warning( $"{topAction.Name}: {action.GetType().Name}::OnPreExecute report Failure." );
					}
					if ( action.PreExecuteResult is not AiActionExecuteStatus.Success ) {
						allPreSuccessed = false;
					}
					action.ExecuteResult = AiActionExecuteStatus.Running;
				}

				if ( anyPreFailed ) {
					entity.SetAiActionInExecuting( topAction, impl, AiActionExecuteStatus.Failure, false );
				}
				else if ( allPreSuccessed ) {
					entity.SetAiActionInExecuting( topAction, impl, AiActionExecuteStatus.Success, false );
				}
				else {
					entity.SetAiActionInExecuting( topAction, impl, AiActionExecuteStatus.Running, false );
				}
			}
			else {
				entity.SetAiActionInExecuting( topAction, ImmutableArray< AiAction >.Empty, AiActionExecuteStatus.Success, false );
			}
		}

		private void OnEntityAiActionModified( World world, Entity entity ) {
			if ( !entity.HasAiActionInExecuting() ) {
				SetupEntityForExecute( world, entity );
			}
			else {
				var aiActionInExecuting = entity.GetAiActionInExecuting();
				var aiActionExecutor = entity.GetAiActionExecutor();
				var topAction = aiActionExecutor.Action;
				if ( topAction != aiActionInExecuting.Action ) {
					if ( topAction is null ) {
						entity.SetAiActionInExecuting( null, ImmutableArray< AiAction >.Empty, AiActionExecuteStatus.Success, true );
					}
					else {

						bool actionCanBeInterrupted = !aiActionInExecuting.Action.ExecutePolicy.HasFlag( AiActionExecutePolicy.CantBeInterrupted );
						bool actionExecutingFinished = aiActionInExecuting.ActionExecuteStatus is not AiActionExecuteStatus.Running;
						if ( actionCanBeInterrupted || actionExecutingFinished ) {
							
							foreach ( var subTask in aiActionInExecuting.ActionCloned ) {
								subTask.OnPostExecute( world, entity );
							}
							
							// entity.SetAiActionInExecuting( aiActionInExecuting.Action, aiActionInExecuting.ActionCloned, AiActionExecuteStatus.Failure, false );
						
							OnAiActionExecutingFinished( entity, in aiActionInExecuting, AiActionExecuteStatus.Running );
							
							entity.RemoveAiActionInExecuting();
							SetupEntityForExecute( world, entity );
						}
						else {
							// do nothing, execute current plan until no Running.
							
						}
					}
				}
				else {
					// bool actionCanBeInterrupted = !aiActionInExecuting.Action.ExecutePolicy.HasFlag( AiActionExecutePolicy.CantBeInterrupted );
					bool actionExecutingFinished = aiActionInExecuting.ActionExecuteStatus is not AiActionExecuteStatus.Running;
					if ( actionExecutingFinished ) {
						entity.RemoveAiActionInExecuting();
						SetupEntityForExecute( world, entity );
					}
				}
			}
		}

		private void OnAiActionExecutingFinished( Entity entity, in AiActionInExecutingComponent aiActionInExecutingComponent, AiActionExecuteStatus status ) {
			if ( aiActionInExecutingComponent.Action is null ) {
				return;
			}
			
			if ( entity.HasAiActionExecutingHistory() ) {
				var aiActionExecutingHistoryComponent = entity.GetAiActionExecutingHistory();
				var deque = aiActionExecutingHistoryComponent.Deque;
				if ( deque is null ) {
					deque = new Deque< AiActionExecutingItem >( aiActionExecutingHistoryComponent.Capacity + 1 );
				}
				deque.EnqueueHead( new AiActionExecutingItem( aiActionInExecutingComponent.Action, status, aiActionInExecutingComponent.IsInterrupted, aiActionInExecutingComponent.GetActionElapsedTime() ) );
				while ( aiActionExecutingHistoryComponent.Capacity > 0 && deque.Count > aiActionExecutingHistoryComponent.Capacity ) {
					deque.DequeueTail();
				}
				entity.SetAiActionExecutingHistory( aiActionExecutingHistoryComponent.Capacity, deque );
			}
		}
	}
	
}


namespace Pixpil.Messages {
	
	using Pixpil.AI;

	public readonly struct AiActionExecutingFinishedMessage : IMessage {
		
		public readonly AiActionExecuteStatus Status;

		public AiActionExecutingFinishedMessage( AiActionExecuteStatus status ) {
			Status = status;
		}
	}
	
	
	public readonly struct StopExecutingAiActionMessage : IMessage;

}


namespace Pixpil.AI.Actions {

	public class AiActionAddComponent : AiAction {
		
		[NoLabel]
		public readonly IComponent Component;

		public override AiActionExecuteStatus OnPreExecute( World world, Entity entity ) {
			if ( Component is null ) {
				return AiActionExecuteStatus.Failure;
			}
			
			entity.AddOrReplaceComponent( Component );
			return AiActionExecuteStatus.Success;
		}
		
	}
	
}


namespace Pixpil.Assets {
	
	using Pixpil.AI;

	public class AiActionScenarioAsset : GameAsset {
		
		public override string EditorFolder => "#\uf135AiActionScenarios";

		public override char Icon => '\uf135';
        
        public override Vector4 EditorColor => "#A6E22E".ToVector4Color();

		[Bang.Serialize]
		public ImmutableArray< AiActionScenario > Actions = ImmutableArray< AiActionScenario >.Empty;

		public AiActionScenarioAsset() {}

		
		protected override void OnModified() {
			
		}
		
		
		public AiActionScenario GetAction( string actionName ) => Actions.FirstOrDefault( action => action.Name == actionName );
	}
	
}
