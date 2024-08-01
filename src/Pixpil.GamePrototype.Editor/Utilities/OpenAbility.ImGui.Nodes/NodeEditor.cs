using System;
using System.Collections.Generic;
using ImGuiNET;
using System.Numerics;


namespace OpenAbility.ImGui.Nodes;

using ImGui = ImGuiNET.ImGui;


public class NodeEditor {
	
	public string Name;
	public Vector2 Scrolling = Vector2.Zero;

	public bool ShowGrid { get; set; } = true;
	public float GridSize { get; set; } = 64f;

	public uint GridColor { get; set; } = ImUtil.ImCol32( 200, 200, 200, 40 );

	public Node? SelectedNode => _nodes.ContainsKey( _selectedNode ) ? _nodes[ _selectedNode ] : null;

	private readonly Dictionary< uint, Node > _nodes = new ();
	public IEnumerable< Node > Nodes => _nodes.Values;

	private uint _hoveredNode;
	private uint _selectedNode;

	private uint _selectedConnectionPin;
	private uint _selectedConnectionNode;

	private Vector2 _lastDrawCursorScreenPos;
	public Vector2 LastDrawCursorScreenPos => _lastDrawCursorScreenPos;

	private Vector2 _lastDrawContentRegionAvail;
	
	public event Action< Vector2, ImDrawListPtr > BeforeCanvasDraw;
	public event Action< Vector2 > AfterCanvasDraw;
	public event Action DrawContext; // = () => { ImGui.Text( "Set NodeEditor.DrawContext!" ); };
	public event Action< Node > DrawNodeContext;
	public event Action< Node > OnSelectNodeChanged;
	public event Action< Node, Vector2 > OnNodeMoved;
	public event Action< Node > OnNodeRemoved;
	
	public NodeEditor( string name ) {
		Name = name;
	}

	public void Draw() {
		
		_hoveredNode = 0;
		
		ImDrawListPtr drawList = ImGui.GetWindowDrawList();

		// ImGui.BeginChild(Name, ImGui.GetWindowSize(),
		// 	ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
		// ImGui.BeginChild( Name, ImGui.GetWindowSize() );
		var contentRegionAvail = ImGui.GetContentRegionAvail() - new Vector2( 0, ImGui.GetStyle().FramePadding.Y );
		ImGui.BeginChild( Name, new Vector2( -1, -1 ), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar );

		var canvasP0 = ImGui.GetCursorScreenPos();
		var canvasSz = ImGui.GetContentRegionAvail();
		var canvasP1 = canvasP0 + canvasSz;
		_lastDrawContentRegionAvail = canvasSz;
		
		BeforeCanvasDraw?.Invoke( contentRegionAvail, drawList );
		
		if ( ImGui.IsWindowHovered( ImGuiHoveredFlags.ChildWindows ) &&
			 ImGui.IsMouseDown( ImGuiMouseButton.Left ) &&
			 ImGui.IsAnyItemActive() ) {
			_selectedNode = 0;
			OnSelectNodeChanged( null );
		}

		// Time to draw all of our stuff
		ImGui.BeginGroup();
		
		drawList.AddText( canvasP0 + canvasSz / 2f, 1, $"{canvasP0}" );

		// drawList.PushClipRect( ImGui.GetCursorScreenPos(), ImGui.GetWindowSize() );
		drawList.PushClipRect( canvasP0, canvasP1, true );

		Vector2 offset = ImGui.GetCursorScreenPos() + Scrolling;
		_lastDrawCursorScreenPos = ImGui.GetCursorScreenPos();

		if ( ShowGrid ) {
			Vector2 winPos = ImGui.GetCursorScreenPos();
			// Vector2 winSize = ImGui.GetWindowSize();
			Vector2 winSize = contentRegionAvail;

			for ( float x = Scrolling.X % GridSize; x < winSize.X; x += GridSize ) {
				drawList.AddLine( new Vector2( x, 0 ) + winPos, new Vector2( x, winSize.Y ) + winPos, GridColor );
			}
			for ( float y = Scrolling.Y % GridSize; y < winSize.Y; y += GridSize ) {
				drawList.AddLine( new Vector2( 0, y ) + winPos, new Vector2( winSize.X, y ) + winPos, GridColor );
			}
			
			drawList.AddLine( new Vector2( winSize.X / 2f, 0 ) + Scrolling + winPos, new Vector2( winSize.X / 2f, winSize.Y ) + Scrolling + winPos, ImUtil.ImCol32( 0, 255, 0, 70 ) );
			drawList.AddLine( new Vector2( 0, winSize.Y / 2f ) + Scrolling + winPos, new Vector2( winSize.X, winSize.Y / 2f ) + Scrolling + winPos, ImUtil.ImCol32( 255, 0, 0, 70 ) );
		}

		drawList.ChannelsSplit( 2 );
		drawList.ChannelsSetCurrent( 0 ); // Background

		foreach ( var node in _nodes.Values ) {
			if ( !node.IsVisible ) {
				continue;
			}
			
			foreach ( var pin in node.GetPins() ) {
				int pinIndex = node.GetOutputPinIndex( pin.Id );
				Vector2 o0 = offset + node.GetOutputPinPosition( pinIndex );
				Vector2 o1 = o0 + new Vector2( 50, 0 );

				foreach ( var connection in pin.GetConnections() ) {
					Node target = _nodes[ connection.TargetNode ];
					NodePin targetPin = target.GetPin( connection.TargetPin );

					int targetPinIndex = target.GetInputPinIndex( targetPin.Id );

					Vector2 i0 = offset + target.GetInputPinPosition( targetPinIndex );
					Vector2 i1 = i0 + new Vector2( -50, 0 );

					drawList.AddBezierCubic( o0, o1, i1, i0, pin.PinType.Colour, 3.0f );
				}
			}
		}

		foreach ( var node in _nodes.Values ) {
			if ( !node.IsVisible ) {
				continue;
			}
			
			ImGui.PushID( new IntPtr( node.Id ) );

			Vector2 nodeRectMin = offset + node.Position;
			node.LastDrawNodeRectMin = nodeRectMin;

			drawList.ChannelsSetCurrent( 1 );
			bool oldAnyActive = ImGui.IsAnyItemActive();
			ImGui.SetCursorScreenPos( nodeRectMin + Node.NodeWindowPadding );
			ImGui.BeginGroup();
			node.Draw();
			ImGui.EndGroup();

			// Is the node being used
			bool isNodeAnyActive = !oldAnyActive && ImGui.IsAnyItemActive();
			node.Size = ImGui.GetItemRectSize() + Node.NodeWindowPadding * 2;
			Vector2 nodeRectMax = nodeRectMin + node.Size;
			node.LastDrawNodeRectMax = nodeRectMax;

			drawList.ChannelsSetCurrent( 0 );
			ImGui.SetCursorScreenPos( nodeRectMin );
			ImGui.InvisibleButton( "node", node.Size );
			if ( ImGui.IsItemHovered() ) {
				_hoveredNode = node.Id;

				if ( ImGui.IsMouseDown( ImGuiMouseButton.Left ) ) {
					_selectedNode = node.Id;
					OnSelectNodeChanged( _nodes[ _selectedNode ] );
				}
				else if ( ImGui.IsMouseDown( ImGuiMouseButton.Right ) && _selectedNode != node.Id ) {
					_selectedNode = node.Id;
					OnSelectNodeChanged( _nodes[ _selectedNode ] );
				}
			}
			
			if ( _selectedNode != 0 ) {
				ImGui.OpenPopupOnItemClick( $"{Name}_{_selectedNode}_context", ImGuiPopupFlags.MouseButtonRight );
			}
			
			if ( _selectedNode != 0 ) {
				if ( ImGui.BeginPopupContextWindow( $"{Name}_{_selectedNode}_context" ) ) {
					DrawNodeContext?.Invoke( _nodes[ _selectedNode ] );
					ImGui.EndPopup();
				}
			}
			
			bool moving = ImGui.IsItemActive();
			if ( moving && ImGui.IsMouseDragging( ImGuiMouseButton.Left ) ) {
				node.Position += ImGui.GetIO().MouseDelta;
				OnNodeMoved?.Invoke( node, node.Position );
			}

			uint colour = ImUtil.ImCol32( 50, 50, 50, 255 );
			if ( _selectedNode == node.Id || isNodeAnyActive ) {
				colour = ImUtil.ImCol32( 75, 75, 75, 255 );
			}
			else if ( _hoveredNode == node.Id ) {
				colour = ImUtil.ImCol32( 60, 60, 60, 255 );
			}

			drawList.AddRectFilled( nodeRectMin, nodeRectMax, colour, 4.0f );
			drawList.AddRect( nodeRectMin, nodeRectMax, ImUtil.ImCol32( 100, 100, 100, 255 ), 4.0f );

			foreach ( var pin in node.GetPins() ) {
				Vector2 position;
				if ( pin.PinMode == PinMode.Input )
					position = offset + node.GetInputPinPosition( node.GetInputPinIndex( pin.Id ) );
				else
					position = offset + node.GetOutputPinPosition( node.GetOutputPinIndex( pin.Id ) );

				drawList.AddCircleFilled( position, NodePin.Radius, pin.PinType.Colour );

				if ( ( ImGui.GetMousePos() - position ).Length() < NodePin.SelectRadius ) {
					if ( ImGui.IsMouseClicked( ImGuiMouseButton.Left ) ) {
						_selectedConnectionNode = node.Id;
						_selectedConnectionPin = pin.Id;
					}
					else if ( ImGui.IsMouseReleased( ImGuiMouseButton.Left ) &&
							  _selectedConnectionNode != 0 &&
							  _selectedConnectionPin != 0 ) {
						NodePin target = _nodes[ _selectedConnectionNode ].GetPin( _selectedConnectionPin );
						if ( pin.Connected( target ) )
							pin.Disconnect( target );
						else
							pin.Connect( target );
					}
				}
			}
			
			node.AfterNodeDraw( in drawList, nodeRectMin, nodeRectMax );

			ImGui.PopID();
		}

		// Draw the current connection
		if ( _selectedConnectionNode != 0 && _selectedConnectionPin != 0 ) {
			NodePin currentSelectedPin = _nodes[ _selectedConnectionNode ].GetPin( _selectedConnectionPin );
			drawList.ChannelsSetCurrent( 1 );

			Vector2 o0 = offset + _nodes[ _selectedConnectionNode ].GetPinPosition( _selectedConnectionPin );
			Vector2 o1 = o0 + new Vector2( currentSelectedPin.PinMode == PinMode.Input ? -50 : 50, 0 );

			Vector2 i0;

			NodePin? closest = null;
			float closestDistance = Single.MaxValue;

			foreach ( var node in _nodes.Values ) {
				foreach ( var pin in node.GetPins() ) {
					if ( pin.PinMode == currentSelectedPin.PinMode ) continue;
					if ( pin.Node == _selectedConnectionNode ) continue;
					if ( pin.PinTypeId != currentSelectedPin.PinTypeId ) continue;

					Vector2 delta = ImGui.GetMousePos() - ( offset + node.GetPinPosition( pin.Id ) );
					float distance = delta.Length();

					if ( distance > NodePin.SelectRadius || distance >= closestDistance ) {
						continue;
					}
					
					closestDistance = distance;
					closest = pin;
				}
			}
			
			i0 = closest != null ? offset + _nodes[ closest.Node ].GetPinPosition( closest.Id ) : ImGui.GetMousePos();

			Vector2 i1 = i0 + new Vector2( currentSelectedPin.PinMode == PinMode.Input ? 50 : -50, 0 );
			
			drawList.AddBezierCubic( o0, o1, i1, i0,
				_nodes[ _selectedConnectionNode ].GetPin( _selectedConnectionPin ).PinType.Colour, 3.0f );

		}

		drawList.ChannelsMerge();
		drawList.PopClipRect();

		if ( ImGui.IsMouseClicked( ImGuiMouseButton.Right ) )
			if ( ImGui.IsWindowHovered( ImGuiHoveredFlags.AllowWhenBlockedByPopup ) && !ImGui.IsAnyItemHovered() ) {
				ImGui.OpenPopup( $"{Name}_context" );
			}

		if ( ImGui.BeginPopup( $"{Name}_context" ) ) {
			ContextMenu();
			ImGui.EndPopup();
		}
		
		if ( ImGui.IsWindowHovered() && !ImGui.IsAnyItemActive() &&
			 ImGui.IsMouseDragging( ImGuiMouseButton.Middle, 0.0f ) )
			Scrolling += ImGui.GetIO().MouseDelta;

		ImGui.EndGroup();
		
		AfterCanvasDraw?.Invoke( contentRegionAvail );

		if ( _selectedNode != 0 ) {
			if ( ImGui.IsKeyPressed( ImGuiKey.Delete ) ) {
				var nodeToRemove = _nodes[ _selectedNode ];
				RemoveNode( nodeToRemove );
				_selectedNode = 0;
				OnSelectNodeChanged( null );
			}
		}

		if ( ImGui.IsMouseReleased( ImGuiMouseButton.Left ) ) {
			_selectedConnectionNode = 0;
			_selectedConnectionPin = 0;
		}
		
		ImGui.EndChild();
	}

	public virtual void ContextMenu() {
		DrawContext?.Invoke();
	}
	

	private static uint currentID = 1;

	internal static uint GetID() {
		return currentID++;
	}

	public void SelectNode( Node node ) {
		if ( node is not null ) {
			_selectedNode = node.Id;
			OnSelectNodeChanged( _nodes[ _selectedNode ] );
		}
		else {
			_selectedNode = 0;
			OnSelectNodeChanged( null );
		}
	}

	public void FocusNode( Node node ) {
		Scrolling = ( node.Position * -1f ) + _lastDrawContentRegionAvail * 0.5f;
	}

	public void RemoveNode( Node node ) {
		OnNodeRemoved?.Invoke( node );
		_nodes.Remove( node.Id );
	}

	public Node FindNode( Func< Node, bool > func ) {
		foreach ( var node in _nodes.Values ) {
			if ( func( node ) ) {
				return node;
			}
		}

		return null;
	}

	public void AddNode( Node node ) {
		_nodes.Add( node.Id, node );
		node.NodeEditor = this;
	}

	public void Clean() {
		_nodes.Clear();
		_hoveredNode = 0;
		_selectedNode = 0;
		_selectedConnectionPin = 0;
		_selectedConnectionNode = 0;
	}

}
