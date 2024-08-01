using System;
using System.Numerics;
using System.Text;
using ImGuiNET;
using Murder;
using Murder.Core.Graphics;
using Murder.Editor.Attributes;
using Murder.Editor.CustomComponents;
using Murder.Editor.CustomEditors;
using Murder.Editor.ImGuiExtended;
using Murder.Editor.Reflection;
using OpenAbility.ImGui.Nodes;
using Pixpil.AI;
using Pixpil.AI.HFSM;


namespace DigitalRune.ImGuiTools;

[CustomEditorOf( typeof( HFSMScenarioAsset ) )]
public class HFSMScenarioAssetEditor : CustomEditor {
	
	private HFSMScenarioAsset _hfsmScenarioAsset = null!;
	private EditorMember _actionDefinesMember;
	private readonly NodeEditor _nodeEditor;
	private HFSMStateMachineScenario _currentStateMachineScenario;
	private string _inputTriggerName = string.Empty;
	private bool _focusDetailNextFrame;
	private bool _detailTabItemActived = true;
	private bool _triggersTabItemActived = true;

	private float _inspectorWidth = 380f;
	
	public override object Target => _hfsmScenarioAsset;

	public HFSMScenarioAssetEditor() {
		_nodeEditor = new NodeEditor( nameof( HFSMScenarioAssetEditor ) );
		_nodeEditor.DrawContext += DrawNodeEditorContext;
		_nodeEditor.DrawNodeContext += DrawNodeEditorNodeContext;
		_nodeEditor.BeforeCanvasDraw += OnBeforeNodeEditorCanvasDraw;
		_nodeEditor.OnNodeMoved += OnNodeMoved;
		_nodeEditor.OnNodeRemoved += OnNodeRemoved;
		_nodeEditor.OnSelectNodeChanged += OnSelectNodeChanged;
	}
	
	public override void OpenEditor( ImGuiRenderer imGuiRenderer, RenderContext renderContext, object target, bool overwrite ) {
		_hfsmScenarioAsset = ( HFSMScenarioAsset )target;
		if ( _hfsmScenarioAsset.RootFsmScenario is null ) {
			_hfsmScenarioAsset.RootFsmScenario = new HFSMStateMachineScenario();
			_hfsmScenarioAsset.RootFsmScenario.GetType().GetField( nameof( HFSMStateMachineScenario.Name ) ).SetValue( _hfsmScenarioAsset.RootFsmScenario, "New StateMachine" );
			SetCurrentStateMachineScenario( _hfsmScenarioAsset.RootFsmScenario );
		}
		PrepareNodeEditorForNewAsset();
	}

	public override void DrawEditor() {

		bool showNodeInspector = _nodeEditor.SelectedNode != null;
		if ( ImGui.BeginTable( "HFSMScenarioAsset Table", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingFixedFit ) ) {

			ImGui.TableSetupColumn( "canvas", ImGuiTableColumnFlags.WidthStretch, -1, 0 );
			// if ( showNodeInspector ) {
				ImGui.TableSetupColumn( "inspector", ImGuiTableColumnFlags.WidthFixed, _inspectorWidth, 1 );
			// }

			ImGui.TableNextRow();
			ImGui.TableNextColumn();
			
			_nodeEditor.Draw();

			ImGui.TableNextColumn();

			if ( ImGui.BeginTabBar( "HFSMScenarioAsset_TabBar" ) ) {
				var detailTabItemFlags = ImGuiTabItemFlags.NoCloseWithMiddleMouseButton;
				if ( _focusDetailNextFrame ) {
					detailTabItemFlags |= ImGuiTabItemFlags.SetSelected;
					_focusDetailNextFrame = false;
				}
				if ( ImGui.BeginTabItem( "Detail", ref _detailTabItemActived, detailTabItemFlags ) ) {
					if ( showNodeInspector ) {
						var inspectorRegion = ImGui.GetContentRegionAvail();
						// _inspectorWidth = Math.Clamp( inspectorRegion.X, 0f, ImGui.GetContentRegionMax().X - 8f );
						_inspectorWidth = inspectorRegion.X;
					
						if ( _nodeEditor.SelectedNode is HFSMStateNode stateNode ) {
							var stateScenario = stateNode.Data;
							_hfsmScenarioAsset.FileChanged |= CustomComponent.ShowEditorOf( ref stateScenario, CustomComponentsFlags.SkipSameLineForFilterField );
						}
					}
					else {
						ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
						ImGui.Text( "no state selected." );
						ImGui.PopStyleColor();
					}
					ImGui.EndTabItem();
				}
					
				if ( ImGui.BeginTabItem( "Triggers", ref _triggersTabItemActived, ImGuiTabItemFlags.NoCloseWithMiddleMouseButton ) ) {

					const float addTriggerRegion = 50f;
					var contentRegionAvail = ImGui.GetContentRegionAvail();
					
					if ( ImGui.BeginTable( "HFSMScenarioAsset Triggers Table", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders ) ) {
						ImGui.TableSetupColumn( "name", ImGuiTableColumnFlags.WidthStretch, -1, 0 );
						ImGui.TableSetupColumn( "ref", ImGuiTableColumnFlags.WidthFixed, 50, 1 );
						ImGui.TableHeadersRow();
						
						foreach ( var trigger in _hfsmScenarioAsset.Triggers ) {
							var triggerRefCount = CountTriggerRefs( trigger );
							
							ImGui.TableNextRow();
							ImGui.TableNextColumn();

							if ( triggerRefCount == 0 ) {
								ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
							}
							ImGui.Text( trigger );
							if ( triggerRefCount == 0 ) {
								ImGui.PopStyleColor();
							}
							
							ImGui.OpenPopupOnItemClick( $"HFSMScenarioAsset_Triggers_{trigger}_context", ImGuiPopupFlags.MouseButtonRight );
							
							ImGui.TableNextColumn();
							
							ImGui.Text( $"{triggerRefCount}" );
							if ( ImGui.IsItemHovered() ) {
								ImGuiHelpers.HelpTooltip( BuildTriggerRefTooltips( trigger ) );
							}
							
							if ( ImGui.BeginPopup( $"HFSMScenarioAsset_Triggers_{trigger}_context" ) ) {

								if ( ImGui.MenuItem( "Remove" ) ) {
									RemoveTrigger( trigger );
									ImGui.CloseCurrentPopup();
								}
								
								ImGui.EndPopup();
							}
						}
						
						ImGui.EndTable();
					}

					var triggersTableSize = ImGui.GetItemRectSize();
					var remainsY = contentRegionAvail.Y - triggersTableSize.Y - addTriggerRegion;
					if ( remainsY > 0f ) {
						ImGui.Dummy( new Vector2( contentRegionAvail.X, remainsY ) );
					}
					
					ImGui.Separator();
					ImGui.PushID( "HFSMScenarioAsset_AddTrigger" );
					ImGui.PushItemWidth( contentRegionAvail.X - 60f );
					// ImGui.InputTextWithHint( string.Empty, "trigger name here (not empty)", ref _inputTriggerName, 0xFF );
					ImGui.InputText( string.Empty, ref _inputTriggerName, 0xFF, ImGuiInputTextFlags.AutoSelectAll );
					ImGui.PopItemWidth();
					ImGui.PopID();
					ImGui.SameLine();
					if ( ImGui.Button( "Add" ) ) {
						bool notEmpty = !string.IsNullOrEmpty( _inputTriggerName );
						bool notContains = !_hfsmScenarioAsset.Triggers.Contains( _inputTriggerName );
						if ( notEmpty && notContains ) {
							_hfsmScenarioAsset.Triggers = _hfsmScenarioAsset.Triggers.Add( _inputTriggerName );
							_hfsmScenarioAsset.FileChanged = true;
							_inputTriggerName = string.Empty;
						}
					}
					
					ImGui.EndTabItem();
				}
					
				ImGui.EndTabBar();
			}
			
			ImGui.EndTable();
		}
		
	}

	public override void PrepareForSaveAsset() {
		foreach ( var node in _nodeEditor.Nodes ) {
			if ( node is HFSMStateNode { Data: not null } hfsmStateNode ) {
				hfsmStateNode.Data.PositionInEditor = node.Position;
			}
		}
	}

	private void PrepareNodeEditorForNewAsset() {
		_nodeEditor.Clean();

		void PrepareStateMachineScenario( HFSMStateMachineScenario stateMachineScenario, HFSMStateMachineScenario parentStateMachineScenario = null ) {
			stateMachineScenario.SetParentFsm( parentStateMachineScenario );
			
			var thisHFSMStateMachineNode = new HFSMStateNode {
            	Owner = parentStateMachineScenario,
            	Data = stateMachineScenario,
            	Position = stateMachineScenario.PositionInEditor
            };
			thisHFSMStateMachineNode.OnNodeDoubleClicked += OnStateMachineNodeDoubleClicked;
            _nodeEditor.AddNode( thisHFSMStateMachineNode );
            
            if ( !stateMachineScenario.States.IsDefaultOrEmpty ) {
				foreach ( var hfsmStateScenario in stateMachineScenario.States ) {
					_nodeEditor.AddNode( new HFSMStateNode { Owner = stateMachineScenario, Data = hfsmStateScenario, Position = hfsmStateScenario.PositionInEditor } );
				}
			}
			if ( !stateMachineScenario.ChildrenStateMachine.IsDefaultOrEmpty ) {
				foreach ( var chilStateMachineScenario in stateMachineScenario.ChildrenStateMachine ) {
					PrepareStateMachineScenario( chilStateMachineScenario, stateMachineScenario );
					// var hfsmStateMachineNode = new HFSMStateMachineNode {
					// 	Owner = stateMachineScenario,
					// 	Data = chilStateMachineScenario,
					// 	Position = chilStateMachineScenario.PositionInEditor
					// };
					// hfsmStateMachineNode.OnNodeDoubleClicked += OnStateMachineNodeDoubleClicked;
					//
					// _nodeEditor.AddNode( hfsmStateMachineNode );
				}
			}
		}
		
		if ( _hfsmScenarioAsset.RootFsmScenario is not null ) {
			PrepareStateMachineScenario( _hfsmScenarioAsset.RootFsmScenario );
			SetCurrentStateMachineScenario( _hfsmScenarioAsset.RootFsmScenario );
		}
	}

	private void SetCurrentStateMachineScenario( HFSMStateMachineScenario current ) {

		foreach ( var node in _nodeEditor.Nodes ) {
			if ( node is HFSMStateNode hfsmStateNode ) {
				if ( hfsmStateNode.Data is HFSMStateMachineScenario ) {
					node.IsVisible = hfsmStateNode.Owner == current;
				}
				else {
					node.IsVisible = current.States.Contains( hfsmStateNode.Data );
				}
			}
		}
		
		_currentStateMachineScenario = current;
	}

	private void DrawNodeEditorContext() {
		if ( _currentStateMachineScenario != null ) {
			if ( ImGui.MenuItem( "Add State" ) ) {
				var newState = new HFSMStateScenario();
				newState.SetStateName( "NewState" );
				_currentStateMachineScenario.AddState( newState );
				if ( _currentStateMachineScenario.HasNoStartState() ) {
					newState.SetStartState( true );
				}
				var node = new HFSMStateNode { Owner = _currentStateMachineScenario, Data = newState };
				node.Position = ImGui.GetCursorScreenPos() + _nodeEditor.Scrolling;
				foreach ( var existNode in _nodeEditor.Nodes ) {
					if ( existNode is HFSMStateNode hfsmStateNode ) {
						node.DrawTransitionLine = hfsmStateNode.DrawTransitionLine;
						break;
					}
				}
				_nodeEditor.AddNode( node );
				
				ImGui.CloseCurrentPopup();
			}
			
			ImGui.Separator();

			if ( ImGui.MenuItem( "Add Sub Fsm" ) ) {
				var newStateMachine = new HFSMStateMachineScenario();
				newStateMachine.SetParentFsm( _currentStateMachineScenario );
				_currentStateMachineScenario.AddChildStateMachine( newStateMachine );
				newStateMachine.SetStateMachineName( $"Child{_currentStateMachineScenario.ChildrenStateMachine.IndexOf( newStateMachine )} StateMachine" );
			
				var node = new HFSMStateNode { Owner = _currentStateMachineScenario, Data = newStateMachine };
				node.Position = ImGui.GetCursorScreenPos() + _nodeEditor.Scrolling;
				foreach ( var existNode in _nodeEditor.Nodes ) {
					if ( existNode is HFSMStateNode hfsmStateNode ) {
						node.DrawTransitionLine = hfsmStateNode.DrawTransitionLine;
						break;
					}
				}
				_nodeEditor.AddNode( node );
				
				ImGui.CloseCurrentPopup();
			}

			if ( _currentStateMachineScenario.ParentFsm != null ) {
				ImGui.Separator();
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Warning );
				if ( ImGui.MenuItem( $"Back To {_currentStateMachineScenario.ParentFsm.Name}" ) ) {
					SetCurrentStateMachineScenario( _currentStateMachineScenario.ParentFsm );
				}
				ImGui.PopStyleColor();
			}
		}
	}

	private void DrawNodeEditorNodeContext( Node node ) {
		if ( node is HFSMStateNode hfsmStateNode ) {
			ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
			ImGui.Text( $"{hfsmStateNode.Data.Name}" );
			ImGui.PopStyleColor();
			ImGui.Separator();
			if ( ImGui.BeginMenu( "Add Transition" ) ) {

				foreach ( var trigger in _hfsmScenarioAsset.Triggers ) {
					ImGui.PushID( $"HFSMScenarioAssetEditor_{node.Id}_AddTransition_{trigger}" );
					if ( !hfsmStateNode.Data.HasTransition( trigger ) &&
						 ImGui.MenuItem( trigger ) ) {
						hfsmStateNode.Data.AddTransition( new HFSMStateTransitionData( trigger, null ) );
					}
					ImGui.PopID();
				}

				ImGui.EndMenu();
			}
			
			if ( ImGui.BeginMenu( "Add Global Transition" ) ) {

				foreach ( var trigger in _hfsmScenarioAsset.Triggers ) {
					if ( !hfsmStateNode.Data.HasGlobalTransition( trigger ) &&
						 ImGui.MenuItem( trigger ) ) {
						hfsmStateNode.Data.AddGlobalTransition( new HFSMStateGlobalTransitionData( trigger ) );
					}
				}

				ImGui.EndMenu();
			}

			if ( !hfsmStateNode.Data.IsStartState ) {
				ImGui.Separator();
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.HighAccent );
				if ( ImGui.MenuItem( "Set as Start State" ) ) {
					hfsmStateNode.Owner.SetStartState( hfsmStateNode.Data );
					_hfsmScenarioAsset.FileChanged = true;
				}
				ImGui.PopStyleColor();
			}
			
			ImGui.Separator();
			ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Warning );
			if ( ImGui.MenuItem( "Remove" ) ) {
				_nodeEditor.RemoveNode( hfsmStateNode );
				_hfsmScenarioAsset.FileChanged = true;
			}
			ImGui.PopStyleColor();
		}
		// else if ( node is HFSMStateMachineNode hfsmStateMachineNode ) {
		// 	ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
		// 	ImGui.Text( $"{hfsmStateMachineNode.Data.StateMachineName}" );
		// 	ImGui.PopStyleColor();
		// 	ImGui.Separator();
		// 	
		// 	ImGui.Separator();
		// 	ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Warning );
		// 	if ( ImGui.MenuItem( "Remove" ) ) {
		// 		_nodeEditor.RemoveNode( hfsmStateMachineNode );
		// 		_hfsmScenarioAsset.FileChanged = true;
		// 	}
		// 	ImGui.PopStyleColor();
		// }
	}

	private void OnNodeMoved( Node node, Vector2 pos ) {
		if ( node is HFSMStateNode { Data: not null } hfsmStateNode ) {
			hfsmStateNode.Data.PositionInEditor = pos;
			_hfsmScenarioAsset.FileChanged = true;
		}
	}

	private void OnNodeRemoved( Node node ) {
		if ( node is HFSMStateNode { Data: not null } hfsmStateNode ) {
			if ( hfsmStateNode.Data is HFSMStateMachineScenario hfsmStateMachineScenario ) {
				foreach ( var hfsmStateScenario in hfsmStateMachineScenario.States ) {
					var foundNode = _nodeEditor.FindNode( node =>
						node is HFSMStateNode hfsmStateNode && hfsmStateNode.Data == hfsmStateScenario );
					_nodeEditor.RemoveNode( foundNode );
				}
				var stateMachineScenario = _currentStateMachineScenario.FindStateMachineScenarioBelongWith( hfsmStateMachineScenario );
				if ( stateMachineScenario != null ) {
					stateMachineScenario.RemoveChildStateMachine( hfsmStateMachineScenario );
					_hfsmScenarioAsset.FileChanged = true;
				}
			}
			else {
				var stateMachineScenario = _currentStateMachineScenario.FindStateScenarioBelongWith( hfsmStateNode.Data );
				if ( stateMachineScenario != null ) {
					stateMachineScenario.RemoveState( hfsmStateNode.Data );
					_hfsmScenarioAsset.FileChanged = true;
				}
			}
		}
	}
	
	private void OnSelectNodeChanged( Node node ) {
		if ( node is HFSMStateNode { Data: not null } ) {
			_focusDetailNextFrame = true;
		}
	}

	private void OnStateMachineNodeDoubleClicked( HFSMStateNode hfsmStateMachineNode ) {
		_nodeEditor.SelectNode( null );
		SetCurrentStateMachineScenario( hfsmStateMachineNode.Data as HFSMStateMachineScenario );
	}

	private bool _floatWindowOpen = true;
	private bool _floatWindowHovered = false;

	private void OnBeforeNodeEditorCanvasDraw( Vector2 canvasSize, ImDrawListPtr drawList ) {
		var windowFlags = ImGuiWindowFlags.NoDecoration |
						  ImGuiWindowFlags.NoDocking |
						  ImGuiWindowFlags.AlwaysAutoResize |
						  ImGuiWindowFlags.NoSavedSettings |
						  ImGuiWindowFlags.NoFocusOnAppearing |
						  ImGuiWindowFlags.NoNav;

		ImGui.SetNextWindowPos( ImGui.GetCursorScreenPos() + new Vector2( 10, 10 ), ImGuiCond.Always );
		ImGui.SetNextWindowBgAlpha( _floatWindowHovered ? 0.9f : 0.5f );
		if ( ImGui.Begin( "Hierachy", ref _floatWindowOpen, windowFlags ) ) {

			void DrawStateMachineScenario( HFSMStateMachineScenario stateMachineScenario, bool isRoot ) {
				var flags = ImGuiTreeNodeFlags.AllowOverlap |
							ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;
				if ( !isRoot ) {
					flags |= ImGuiTreeNodeFlags.DefaultOpen;
				}
				if ( _currentStateMachineScenario == stateMachineScenario ) {
					flags |= ImGuiTreeNodeFlags.Selected;
				}
				
				if ( ImGui.TreeNodeEx( $"{stateMachineScenario.Name ?? "Unnamed"}", flags ) ) {

					if ( ImGui.IsItemClicked( ImGuiMouseButton.Left ) ) {
						SetCurrentStateMachineScenario( stateMachineScenario );
						
						var foundNode = _nodeEditor.FindNode( node =>
							node is HFSMStateNode hfsmStateMachineNode && hfsmStateMachineNode.Data == stateMachineScenario ) as HFSMStateNode;
						if ( foundNode != null ) {
							_nodeEditor.SelectNode( foundNode );
						}
					}
					
					foreach ( var childStateMachineScenario in stateMachineScenario.ChildrenStateMachine ) {
						DrawStateMachineScenario( childStateMachineScenario, false );
					}
					
					foreach ( var stateScenario in stateMachineScenario.States ) {
						if ( ImGui.TreeNodeEx( $"{stateScenario.Name}", ImGuiTreeNodeFlags.Leaf ) ) {
							
							if ( ImGui.IsItemClicked( ImGuiMouseButton.Left ) ) {
								var foundNode = _nodeEditor.FindNode( node =>
									node is HFSMStateNode hfsmStateNode && hfsmStateNode.Data == stateScenario );
								_nodeEditor.SelectNode( foundNode );
								_nodeEditor.FocusNode( foundNode );

								if ( !foundNode.IsVisible ) {
									var stateMachineScenarioToBeCurrent = _hfsmScenarioAsset.RootFsmScenario.FindStateScenarioBelongWith( stateScenario );
									if ( stateMachineScenarioToBeCurrent != null ) {
										SetCurrentStateMachineScenario( stateMachineScenarioToBeCurrent );
									}
								}
							}

							ImGui.TreePop();
						}
					}
					
					ImGui.TreePop();
				}
			}

			if ( _hfsmScenarioAsset.RootFsmScenario is not null ) {
				DrawStateMachineScenario( _hfsmScenarioAsset.RootFsmScenario, true );
			}
			else {
				ImGui.Text( "RootFsmScenario is null." );
			}
			
			ImGui.Dummy( new Vector2( 8, 8 ) );
			
            _floatWindowHovered = ImGui.IsWindowHovered();
		}
		ImGui.End();
		
		ImGui.SetNextWindowPos( ImGui.GetCursorScreenPos() + new Vector2( canvasSize.X - 70, 0 ), ImGuiCond.Always );
		if ( ImGui.Begin( "Hierachy Toolbar", ref _floatWindowOpen, windowFlags ) ) {
			if ( ImGui.SmallButton( "R" ) ) {
				_nodeEditor.Scrolling = Vector2.Zero;
			}
			ImGuiHelpers.HelpTooltip( "Reset Scrolling to Zero." );
			ImGui.SameLine();
			if ( ImGui.SmallButton( "L" ) ) {
				foreach ( var node in _nodeEditor.Nodes ) {
					if ( node is HFSMStateNode hfsmStateNode ) {
						hfsmStateNode.DrawTransitionLine = !hfsmStateNode.DrawTransitionLine;
					}
				}
			}
			ImGuiHelpers.HelpTooltip( "Toggle State transition line." );
			// ImGui.Text( $"Scroll: {_nodeEditor.Scrolling}" );
			// ImGui.Text( $"CursorScreenPos: {_nodeEditor.LastDrawCursorScreenPos}" );
		}
		ImGui.End();
	}

	private void RemoveTrigger( string trigger ) {
		_hfsmScenarioAsset.TravelFsmScenarioTree( fsmScenario => {
			for ( var i = fsmScenario.Transitions.Length - 1; i >= 0; i-- ) {
				var hfsmStateTransitionData = fsmScenario.Transitions[ i ];
				if ( hfsmStateTransitionData.Event == trigger ) {
					fsmScenario.Transitions = fsmScenario.Transitions.Remove( hfsmStateTransitionData );
				}
			}
			for ( var i = fsmScenario.GlobalTransitions.Length - 1; i >= 0; i-- ) {
				var hfsmStateTransitionData = fsmScenario.GlobalTransitions[ i ];
				if ( hfsmStateTransitionData.Event == trigger ) {
					fsmScenario.GlobalTransitions = fsmScenario.GlobalTransitions.Remove( hfsmStateTransitionData );
				}
			}

			foreach ( var hfsmStateScenario in fsmScenario.States ) {
				for ( var i = hfsmStateScenario.Transitions.Length - 1; i >= 0; i-- ) {
					var hfsmStateTransitionData = hfsmStateScenario.Transitions[ i ];
					if ( hfsmStateTransitionData.Event == trigger ) {
						hfsmStateScenario.Transitions = hfsmStateScenario.Transitions.Remove( hfsmStateTransitionData );
					}
				}
				for ( var i = hfsmStateScenario.GlobalTransitions.Length - 1; i >= 0; i-- ) {
					var hfsmStateTransitionData = hfsmStateScenario.GlobalTransitions[ i ];
					if ( hfsmStateTransitionData.Event == trigger ) {
						hfsmStateScenario.GlobalTransitions = hfsmStateScenario.GlobalTransitions.Remove( hfsmStateTransitionData );
					}
				}
			}
		} );
		_hfsmScenarioAsset.Triggers = _hfsmScenarioAsset.Triggers.Remove( trigger );
		_hfsmScenarioAsset.FileChanged = true;
	}

	private int CountTriggerRefs( string trigger ) {
		var count = 0;
		_hfsmScenarioAsset.TravelFsmScenarioTree( fsmScenario => {
			foreach ( var hfsmStateTransitionData in fsmScenario.Transitions ) {
				if ( hfsmStateTransitionData.Event == trigger ) {
					count++;
				}
			}
			foreach ( var hfsmStateTransitionData in fsmScenario.GlobalTransitions ) {
				if ( hfsmStateTransitionData.Event == trigger ) {
					count++;
				}
			}

			foreach ( var hfsmStateScenario in fsmScenario.States ) {
				foreach ( var hfsmStateTransitionData in hfsmStateScenario.Transitions ) {
					if ( hfsmStateTransitionData.Event == trigger ) {
						count++;
					}
				}
				foreach ( var hfsmStateTransitionData in hfsmStateScenario.GlobalTransitions ) {
					if ( hfsmStateTransitionData.Event == trigger ) {
						count++;
					}
				}
			}
		} );
		return count;
	}

	private readonly StringBuilder _triggerRefTooltipsBuilder = new ( 0xFF );
	private string BuildTriggerRefTooltips( string trigger ) {
		_triggerRefTooltipsBuilder.Clear();

		var i = 0;
		_hfsmScenarioAsset.TravelFsmScenarioTree( fsmScenario => {
			foreach ( var hfsmStateTransitionData in fsmScenario.Transitions ) {
				if ( hfsmStateTransitionData.Event == trigger ) {
					_triggerRefTooltipsBuilder.AppendLine( $"#{i} {fsmScenario.Name}" );
					i++;
				}
			}
			foreach ( var hfsmStateTransitionData in fsmScenario.GlobalTransitions ) {
				if ( hfsmStateTransitionData.Event == trigger ) {
					_triggerRefTooltipsBuilder.AppendLine( $"#{i} {fsmScenario.Name} global" );
					i++;
				}
			}

			foreach ( var hfsmStateScenario in fsmScenario.States ) {
				foreach ( var hfsmStateTransitionData in hfsmStateScenario.Transitions ) {
					if ( hfsmStateTransitionData.Event == trigger ) {
						_triggerRefTooltipsBuilder.AppendLine( $"#{i} {hfsmStateScenario.Name}" );
						i++;
					}
				}
				foreach ( var hfsmStateTransitionData in hfsmStateScenario.GlobalTransitions ) {
					if ( hfsmStateTransitionData.Event == trigger ) {
						_triggerRefTooltipsBuilder.AppendLine( $"#{i} {hfsmStateScenario.Name} global" );
						i++;
					}
				}
			}
		} );
		
		return _triggerRefTooltipsBuilder.ToString();
	}
}


internal class HFSMStateNode : Node {
	
	public bool DrawTransitionLine;
	public HFSMStateMachineScenario Owner;
	public HFSMStateScenario Data;

	private bool _isStateNameBeingEdited;
	
	public event Action< HFSMStateNode > OnNodeDoubleClicked;

	internal HFSMStateNode() {
		DrawTransitionLine = true;

		// AddInput( new PinType( "TypeA", 255, 255, 255 ) );
		// AddInput( new PinType( "TypeB", 255, 255, 0 ) );
		//
		// AddOutput( new PinType( "TypeC", 0, 255, 255 ) );
		// AddOutput( new PinType( "TypeD", 0, 0, 255 ) );
	}

	public override void Draw() {
		if ( !_isStateNameBeingEdited ) {
			if ( Data.IsStartState ) {
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.HighAccent );
				if ( Data is HFSMStateMachineScenario hfsmStateMachineScenario ) {
					ImGui.Text( $"{hfsmStateMachineScenario.Name}" );
				}
				else {
					ImGui.Text( $"{Data.Name}" );
				}
				ImGui.PopStyleColor();
			}
			else {
				if ( Data is HFSMStateMachineScenario hfsmStateMachineScenario ) {
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Warning );
					ImGui.Text( $"{hfsmStateMachineScenario.Name}" );
					ImGui.PopStyleColor();
				}
				else {
					ImGui.Text( $"{Data.Name}" );
				}
			}

			if ( Data is HFSMStateMachineScenario ) {
				if ( ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked( 0 ) ) {
					OnNodeDoubleClicked?.Invoke( this );
				}
			}
			else {
				if ( ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked( 0 ) ) {
					_isStateNameBeingEdited = true;
				}
			}
		}
		else {
			var stateName = Data.Name;
			if ( ImGui.InputText( string.Empty, ref stateName, 64, ImGuiInputTextFlags.AutoSelectAll ) ) {
				Data.SetStateName( stateName );
			}
			
			if ( !ImGui.IsItemHovered() && ImGui.IsMouseClicked( ImGuiMouseButton.Left ) ) {
				Data.SetStateName( stateName );
				_isStateNameBeingEdited = false;
			}
		}
	}
	
	public override void AfterNodeDraw( in ImDrawListPtr drawList, Vector2 nodeRectMin, Vector2 nodeRectMax ) {
		var nextRectMin = nodeRectMin + new Vector2( 0, Size.Y );
		var globalTransitionBGColor = ImUtil.ImCol32( 30, 30, 30, 255 );
		var globalTransitionTextColor = ImUtil.ImCol32( 0, 255, 0, 200 );
		var transitionBGColor = ImUtil.ImCol32( 125, 125, 125, 255 );
		var transitionTextColor = ImUtil.ImCol32( 255, 255, 255, 220 );
		var transitionConnectLineColor = ImUtil.ImCol32( 255, 255, 255, 150 );
		
		if ( !Data.GlobalTransitions.IsDefaultOrEmpty ) {
			// const float arrowLength = 30f;
			// var arrowStartPos = new Vector2( nodeRectMin.X + ( nodeRectMax.X - nodeRectMin.X ) / 2f, nodeRectMin.Y );
			// var arrowEndPos = new Vector2( arrowStartPos.X, nodeRectMin.Y - arrowLength );
			// drawList.AddLine( arrowStartPos, arrowEndPos, ImUtil.ImCol32( 255, 255, 255, 200 ), 3f );

			// nextRectMin.Y = arrowEndPos.Y - arrowLength / 2f;
			foreach ( var hfsmStateTransitionData in Data.GlobalTransitions ) {
				var transitionId = $"{Owner.Name}_{Data.Name}_{hfsmStateTransitionData.Event}_Global";
				
				ImGui.SetCursorScreenPos( nextRectMin + Node.NodeWindowPadding );
				
				ImGui.BeginGroup();
			
				ImGui.PushItemWidth( nodeRectMax.X - nodeRectMin.X );
				ImGui.PushStyleColor( ImGuiCol.Text, globalTransitionTextColor );
				ImGui.Text( hfsmStateTransitionData.Event );
				ImGui.PopStyleColor();
				ImGui.PopItemWidth();
			
				ImGui.EndGroup();
			
				var transitionItemsSize = ImGui.GetItemRectSize() + Node.NodeWindowPadding * 2;
				transitionItemsSize.X = Math.Max( transitionItemsSize.X, nodeRectMax.X - nodeRectMin.X );
				var nextRectMax = nextRectMin + new Vector2( transitionItemsSize.X, transitionItemsSize.Y );
				
				drawList.AddRectFilled( nextRectMin, nextRectMax, globalTransitionBGColor, 4.0f );
				drawList.AddRect( nextRectMin, nextRectMax, ImUtil.ImCol32( 100, 100, 100, 255 ), 4.0f );
				
				ImGui.SetCursorScreenPos( nextRectMin + Node.NodeWindowPadding );
				ImGui.PushID( $"{Id}_{hfsmStateTransitionData.Event}" );
				ImGui.InvisibleButton( $"##{transitionId}", transitionItemsSize, ImGuiButtonFlags.MouseButtonRight );
				ImGui.PopID();
				
				ImGuiHelpers.HelpTooltip( $"Global Trigger: {hfsmStateTransitionData.Event} -> this" );
				
				if ( ImGui.IsItemHovered() ) {
					ImGui.OpenPopupOnItemClick( $"{transitionId}_context", ImGuiPopupFlags.MouseButtonRight );
				}
				
				if ( ImGui.BeginPopup( $"{transitionId}_context" ) ) {
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
					ImGui.Text( $"{Data.Name}" );
					ImGui.PopStyleColor();
					ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.HighAccent );
					ImGui.Text( $"GlobalTransition: {hfsmStateTransitionData.Event}" );
					ImGui.PopStyleColor();
					ImGui.Separator();
					if ( ImGui.MenuItem( "Remove" ) ) {
						Data.RemoveGlobalTransition( hfsmStateTransitionData );
					}
					
					ImGui.EndPopup();
				}

				nextRectMin.Y += transitionItemsSize.Y;
			}
		}
		
		foreach ( var hfsmStateTransitionData in Data.Transitions ) {
			var transitionId = $"{Owner.Name}_{Data.Name}_{hfsmStateTransitionData.Event}";
			
			ImGui.SetCursorScreenPos( nextRectMin + Node.NodeWindowPadding );
			
			ImGui.BeginGroup();
		
			ImGui.PushItemWidth( nodeRectMax.X - nodeRectMin.X );
			ImGui.PushStyleColor( ImGuiCol.Text, transitionTextColor );
			ImGui.Text( hfsmStateTransitionData.Event );
			ImGui.PopStyleColor();
			ImGui.PopItemWidth();
		
			ImGui.EndGroup();
		
			var transitionItemsSize = ImGui.GetItemRectSize() + Node.NodeWindowPadding * 2;
			transitionItemsSize.X = Math.Max( transitionItemsSize.X, nodeRectMax.X - nodeRectMin.X );
			var nextRectMax = nextRectMin + new Vector2( transitionItemsSize.X, transitionItemsSize.Y );
			
			drawList.AddRectFilled( nextRectMin, nextRectMax, transitionBGColor, 4.0f );
			drawList.AddRect( nextRectMin, nextRectMax, ImUtil.ImCol32( 100, 100, 100, 255 ), 4.0f );
			
			// draw connection
			if ( !string.IsNullOrEmpty( hfsmStateTransitionData.TransitionTo ) && DrawTransitionLine ) {
				var foundNode = NodeEditor.FindNode( node =>
					node is HFSMStateNode hfsmStateNode && hfsmStateNode.Data.Name == hfsmStateTransitionData.TransitionTo ) as HFSMStateNode;
				if ( foundNode != null ) {
					var startPoint0 = new Vector2( foundNode.Position.X < Position.X ? nextRectMin.X : nextRectMax.X, nextRectMin.Y + transitionItemsSize.Y / 2f );
					var startPoint1 = startPoint0 + new Vector2( foundNode.Position.X < Position.X ? -50 : 50, 0 );
					var endPoint0 =
						foundNode.FindTransitionBetterConnectionPoint( startPoint0 );
					var endPoint1 = endPoint0 + new Vector2( 50, 0 );

					drawList.AddBezierCubic( startPoint0, startPoint1, endPoint1, endPoint0, transitionConnectLineColor, 1.0f );
				}
			}
			
			ImGui.SetCursorScreenPos( nextRectMin + Node.NodeWindowPadding );
			ImGui.PushID( $"{Id}_{hfsmStateTransitionData.Event}" );
			ImGui.InvisibleButton( $"##{transitionId}", transitionItemsSize, ImGuiButtonFlags.MouseButtonRight );
			ImGui.PopID();

			if ( !string.IsNullOrEmpty( hfsmStateTransitionData.TransitionTo ) ) {
				ImGuiHelpers.HelpTooltip( $"-> {hfsmStateTransitionData.TransitionTo}" );
			}
			else {
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Red );
				ImGuiHelpers.HelpTooltip( "transition not set!" );
				ImGui.PopStyleColor();
			}

			if ( ImGui.IsItemHovered() ) {
				ImGui.OpenPopupOnItemClick( $"{transitionId}_context", ImGuiPopupFlags.MouseButtonRight );
			}
			
			if ( ImGui.BeginPopup( $"{transitionId}_context" ) ) {
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
				ImGui.Text( $"State: {Data.Name}" );
				ImGui.PopStyleColor();
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.HighAccent );
				ImGui.Text( $"Transition: {hfsmStateTransitionData.Event}" );
				ImGui.PopStyleColor();
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Faded );
				ImGui.Text( $"TransitionTo: {hfsmStateTransitionData.TransitionTo ?? "null"}" );
				ImGui.PopStyleColor();
				ImGui.Separator();
				if ( ImGui.BeginMenu( "Set TransitionTo:" ) ) {
					foreach ( var hfsmStateScenario in Owner.States ) {
						if ( hfsmStateScenario != Data ) {
							if ( ImGui.MenuItem( hfsmStateScenario.Name ?? string.Empty ) ) {
								Data.SetTransitions(
									Data.Transitions.Replace( hfsmStateTransitionData,
										new HFSMStateTransitionData( hfsmStateTransitionData.Event,
											hfsmStateScenario.Name ) )
								);
							}
						}
					}
					foreach ( var hfsmStateMachineScenario in Owner.ChildrenStateMachine ) {
						if ( hfsmStateMachineScenario != Data ) {
							ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Warning );
							if ( ImGui.MenuItem( hfsmStateMachineScenario.Name ?? string.Empty ) ) {
								Data.SetTransitions(
									Data.Transitions.Replace( hfsmStateTransitionData,
										new HFSMStateTransitionData( hfsmStateTransitionData.Event,
											hfsmStateMachineScenario.Name ) )
								);
							}
							ImGui.PopStyleColor();
						}
					}
					ImGui.EndMenu();
				}
				ImGui.PushStyleColor( ImGuiCol.Text, Game.Profile.Theme.Red );
				if ( ImGui.MenuItem( "Remove" ) ) {
					Data.RemoveTransition( hfsmStateTransitionData );
				}
				ImGui.PopStyleColor();
				
				ImGui.EndPopup();
			}

			nextRectMin.Y += transitionItemsSize.Y;
		}
		
	}

	public Vector2 FindTransitionBetterConnectionPoint( Vector2 fromNodeRectMin ) {
		if ( fromNodeRectMin.X > LastDrawNodeRectMin.X ) {
			return new Vector2( LastDrawNodeRectMax.X, LastDrawNodeRectMin.Y );
		}
		else {
			return new Vector2( LastDrawNodeRectMin.X, LastDrawNodeRectMin.Y );
		}
	}

}
