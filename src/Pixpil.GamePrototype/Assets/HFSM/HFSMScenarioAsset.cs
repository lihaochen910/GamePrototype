using System;
using System.Collections.Immutable;
using System.Numerics;
using DigitalRune.Linq;
using Murder.Assets;
using Murder.Utilities;
using Pixpil.AI.HFSM;


namespace Pixpil.AI;

public class HFSMScenarioAsset : GameAsset {
	
	public override string EditorFolder => "#\uf085HFSMScenarios";

	public override char Icon => '\uf085';

	public override Vector4 EditorColor => "#F39C12".ToVector4Color();

	[Bang.Serialize]
	public HFSMStateMachineScenario RootFsmScenario;
	
	/// <summary>
	/// 参考之前使用的Playmaker, 现在只使用HFSM的Trigger来进行状态切换
	/// 由于TTrigger类型为string, 存在运行时比对成本(:字符串比较方法, 如何让其使用引用对比?), 所以还存在优化空间
	/// </summary>
	[Bang.Serialize]
	public ImmutableArray< string > Triggers = ImmutableArray< string >.Empty;

	
	public BangStateMachine CreateInstance() {
		var fsm = new BangStateMachine();

		void BuildFsm( HFSMStateMachineScenario fsmScenario, BangStateMachine contextFsm, BangStateMachine parentFsm ) {
			contextFsm.SetActions( fsmScenario.MakeClonedImpl() );
			
			foreach ( var stateScenario in fsmScenario.States ) {
				var stateInstance = new BangActionState< string >( stateScenario.MakeClonedImpl() );
				contextFsm.AddState( stateScenario.Name, stateInstance );
				
				foreach ( var stateTransitionData in stateScenario.Transitions ) {
					if ( !string.IsNullOrEmpty( stateTransitionData.TransitionTo ) ) {
						contextFsm.AddTriggerTransition( stateTransitionData.Event, new Transition( stateScenario.Name, stateTransitionData.TransitionTo ) );
					}
				}

				foreach ( var stateGlobalTransitionData in stateScenario.GlobalTransitions ) {
					contextFsm.AddTriggerTransitionFromAny( stateGlobalTransitionData.Event, new Transition( string.Empty, stateScenario.Name ) );
				}
				
				if ( stateScenario.IsStartState ) {
					contextFsm.SetStartState( stateScenario.Name );
				}
			}

			foreach ( var childStateMachineScenario in fsmScenario.ChildrenStateMachine ) {
				var childFsm = new BangStateMachine();
				BuildFsm( childStateMachineScenario, childFsm, contextFsm );
				contextFsm.AddState( childStateMachineScenario.Name, childFsm );
			}

			if ( parentFsm != null ) {
				foreach ( var stateTransitionData in fsmScenario.Transitions ) {
					if ( !string.IsNullOrEmpty( stateTransitionData.TransitionTo ) ) {
						parentFsm.AddTriggerTransition( stateTransitionData.Event, new Transition( fsmScenario.Name, stateTransitionData.TransitionTo ) );
					}
				}

				foreach ( var stateGlobalTransitionData in fsmScenario.GlobalTransitions ) {
					parentFsm.AddTriggerTransitionFromAny( stateGlobalTransitionData.Event, new Transition( string.Empty, fsmScenario.Name ) );
				}
			}
			
		}

		BuildFsm( RootFsmScenario, fsm, null );
		fsm.AssetGuid = Guid;
		
		return fsm;
	}


	public void TravelFsmScenarioTree( Action< HFSMStateMachineScenario > func ) {
		func?.Invoke( RootFsmScenario );
		foreach ( var fsmScenario in TreeHelper.GetDescendants( RootFsmScenario, fsm => fsm.ChildrenStateMachine ) ) {
			func?.Invoke( fsmScenario );
		}
	}

}
