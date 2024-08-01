using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using DigitalRune.Collections;
using Murder.Core.Graphics;
using Pixpil.AI;
using Pixpil.AI.HFSM;
using Pixpil.Components;
using Pixpil.Messages;
#if MURDER
using Game = Murder.Game;
#endif


namespace Pixpil.Systems;

[Watch( typeof( HFSMAgentComponent ) )]
[Filter( ContextAccessorFilter.AnyOf, ContextAccessorKind.Read, typeof( HFSMAgentComponent ) )]
[Filter( ContextAccessorFilter.NoneOf, typeof( HFSMPausedComponent ) )]
[Messager( typeof( HFSMTriggerMessage ) )]
public class HFSMTickSystem : IUpdateSystem, IFixedUpdateSystem, IReactiveSystem, IMessagerSystem
#if MURDER
	,IMurderRenderSystem
#endif
{

	public void Update( Context context ) {
		foreach ( var entity in context.Entities ) {
			var hfsmAgent = entity.GetHFSMAgent();
			if ( hfsmAgent.StateMachine != null ) {
				hfsmAgent.StateMachine.OnLogic();
			}
		}
	}

	public void FixedUpdate( Context context ) {
		foreach ( var entity in context.Entities ) {
			var hfsmAgent = entity.GetHFSMAgent();
			if ( hfsmAgent.StateMachine != null ) {
				hfsmAgent.StateMachine.OnFixedUpdate();
			}
		}
	}
	
#if MURDER
	public void Draw( RenderContext render, Context context ) {
		foreach ( var entity in context.Entities ) {
			var hfsmAgent = entity.GetHFSMAgent();
			var hasDrawAbility = entity.HasHFSMAgentDraw();
			if ( hasDrawAbility && hfsmAgent.StateMachine != null ) {
				hfsmAgent.StateMachine.OnMurderDraw( render );
			}
		}
	}
#endif

	public void OnAdded( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			OnHFSMAgentComponentChanged( world, entity );
		}
	}

	public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

	public void OnModified( World world, ImmutableArray< Entity > entities ) {
		foreach ( var entity in entities ) {
			OnHFSMAgentComponentChanged( world, entity );
		}
	}

	private void OnHFSMAgentComponentChanged( World world, Entity entity ) {
		var hfsmAgent = entity.GetHFSMAgent();
		if ( hfsmAgent.StateMachine is null || hfsmAgent.StateMachine.AssetGuid != hfsmAgent.FsmAsset ) {
			var hfsmScenarioAsset = Game.Data.TryGetAsset< HFSMScenarioAsset >( hfsmAgent.FsmAsset );
			if ( hfsmScenarioAsset is not null ) {
				if ( entity.GetHFSMAgent().StateMachine is not null && entity.HasHFSMAgentHistory() ) {
					entity.GetHFSMAgent().StateMachine.StateChangedYetAnother -= entity.GetHFSMAgentHistory().StateChangedCallback;
				}
				
				var stateMachineInstance = hfsmScenarioAsset.CreateInstance();
				stateMachineInstance.SetBangContext( world, entity );
                stateMachineInstance.Init();

				if ( entity.TryGetHFSMAgentHistory() is {} hfsmAgentHistory ) {
					var stateChangedCallback = ( StateBase< string > from, StateBase< string > to, TransitionBase< string > transition ) => {
						OnStateChanged( entity, from, to, transition );
					};
					stateMachineInstance.StateChangedYetAnother += stateChangedCallback;
					entity.SetHFSMAgentHistory( hfsmAgentHistory.Capacity, hfsmAgentHistory.Deque, stateChangedCallback );
				}
				
				entity.SetHFSMAgent( hfsmAgent.FsmAsset, stateMachineInstance );
			}
		}
	}

	public void OnMessage( World world, Entity entity, IMessage message ) {
		if ( message is HFSMTriggerMessage hfsmTriggerMessage ) {
			var hfsmAgent = entity.GetHFSMAgent();
			if ( hfsmAgent.StateMachine is not null ) {
				hfsmAgent.StateMachine.Trigger( hfsmTriggerMessage.Trigger );
			}
		}
	}

	private void OnStateChanged( Entity entity, StateBase< string > from, StateBase< string > to, TransitionBase< string > transition ) {
		
		string BuildStackTrace( int count ) {
			#if DEBUG
			StackTrace trace = new( 3, true );
			
			int traceCount = count;
			StackFrame[] frames = trace.GetFrames();

			StringBuilder stack = new();
			for ( var i = 0; i < frames.Length && i < count + 1; i++ ) {
				stack.AppendLine($" {frames[i].GetMethod()?.DeclaringType?.FullName ?? string.Empty}.{frames[i].GetMethod()?.Name ?? "Unknown"}:line {frames[i].GetFileLineNumber()}");

				if (traceCount-- <= 0) {
					break;
				}
			}

			return stack.ToString();
			#else
			return "Unavaliable in release.";
			#endif
		}
		
		var historyEntry = new HFSMStateHistroyEntry( from, to, transition as Transition, Game.Now, BuildStackTrace( 15 ) );

		var capacity = entity.GetHFSMAgentHistory().Capacity;
		var deque = entity.GetHFSMAgentHistory().Deque;
		if ( deque is null ) {
			deque = new Deque< HFSMStateHistroyEntry >( capacity + 1 );
		}
		deque.EnqueueHead( historyEntry );
		while ( capacity > 0 && deque.Count > capacity ) {
			deque.DequeueTail();
		}
		
		entity.SetHFSMAgentHistory( capacity, deque, entity.GetHFSMAgentHistory().StateChangedCallback );
	}
	
}
