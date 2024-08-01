using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;


namespace OpenAbility.ImGui.Nodes;

public abstract class Node {

	public static Vector2 NodeWindowPadding = new ( 8, 8 );

	public readonly uint Id = NodeEditor.GetID();
	
	private readonly Dictionary< uint, NodePin > _pins = new ();
	private readonly Dictionary< uint, NodePin > _inputs = new ();
	private readonly Dictionary< uint, NodePin > _outputs = new ();

	public bool IsVisible { get; set; } = true;

	public NodeEditor NodeEditor { get; internal set; }

	public Vector2 Position;
	internal Vector2 Size;
	
	public Vector2 LastDrawNodeRectMin { get; internal set; }
	public Vector2 LastDrawNodeRectMax { get; internal set; }


	public NodePin AddInput( PinType pinType ) {
		return AddInput( pinType.Id );
	}

	public NodePin AddOutput( PinType pinType ) {
		return AddOutput( pinType.Id );
	}

	public NodePin AddInput( uint pinType ) {
		NodePin pin = new NodePin( this, pinType, PinMode.Input );
		_pins[ pin.Id ] = pin;
		_inputs[ pin.Id ] = pin;
		return pin;
	}

	public NodePin AddOutput( uint pinType ) {
		var pin = new NodePin( this, pinType, PinMode.Output );
		_pins[ pin.Id ] = pin;
		_outputs[ pin.Id ] = pin;
		return pin;
	}

	public IEnumerable< NodePin > GetPins() {
		return _pins.Values;
	}

	public Vector2 GetPinPosition( uint pin ) {
		return _pins[ pin ].PinMode == PinMode.Input
			? GetInputPinPosition( GetInputPinIndex( pin ) )
			: GetOutputPinPosition( GetOutputPinIndex( pin ) );
	}

	public int GetPinIndex( uint pin ) {
		if ( _pins[ pin ].PinMode == PinMode.Input ) {
			return GetInputPinIndex( pin );
		}
		return GetOutputPinIndex( pin );
	}

	public int GetInputPinIndex( uint pin ) {
		var index = 0;
		foreach ( var kv in _inputs ) {
			if ( kv.Key != pin ) {
				index++;
			}
		}

		return index;
		// return _inputs.Keys.TakeWhile( x => x != pin ).Count();
	}

	public int GetOutputPinIndex( uint pin ) {
		return _outputs.Keys.TakeWhile( x => x != pin ).Count();
	}

	public Vector2 GetInputPinPosition( int index ) {
		return Position with { Y = Position.Y + Size.Y * ( ( float )index + 1 ) / ( ( float )_inputs.Count + 1 ) };
	}


	public Vector2 GetOutputPinPosition( int index ) {
		return new Vector2( Position.X + Size.X,
			Position.Y + Size.Y * ( ( float )index + 1 ) / ( ( float )_outputs.Count + 1 ) );
	}

	public NodePin GetPin( uint id ) {
		return _pins[ id ];
	}

	public abstract void Draw();
	
	public virtual void AfterNodeDraw( in ImDrawListPtr drawList, Vector2 nodeRectMin, Vector2 nodeRectMax ) {}


	public virtual void OnRemoved() {}
	
}
