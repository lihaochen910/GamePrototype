using System.Collections.Generic;
using System.Linq;

namespace OpenAbility.ImGui.Nodes;

public class NodePin {
	
	private Dictionary< uint, ConnectionTarget > _connections = new ();

	public readonly uint Id = NodeEditor.GetID();
	public readonly uint Node;
	public readonly uint PinTypeId;

	public readonly PinMode PinMode;

	public static float Radius = 4.0f;
	public static float RadiusSelectRatio = 2f;

	public static float SelectRadius => Radius * RadiusSelectRatio;


	public PinType PinType => PinType.GetPinType( PinTypeId );

	internal NodePin( Node node, uint pinTypeID, PinMode mode ) {
		Node = node.Id;
		PinTypeId = pinTypeID;
		PinMode = mode;
	}

	public bool Connected( NodePin pin ) {
		if ( pin.Id == Id || pin.Node == Node || pin.PinMode == PinMode ) {
			return false;
		}

		if ( PinMode == PinMode.Input ) {
			return pin.Connected( this );
		}
		
		return _connections.Any( c => c.Value.TargetPin == pin.Id );
	}

	public void Disconnect( NodePin pin ) {
		if ( pin.Id == Id || pin.Node == Node || pin.PinMode == PinMode ) return;

		if ( PinMode == PinMode.Input )
			pin.Disconnect( this );
		else if ( Connected( pin ) ) {
			_connections.Remove( _connections.First( c => c.Value.TargetPin == pin.Id ).Key );
		}
	}

	/// <summary>
	/// Connect to another pin
	/// </summary>
	/// <param name="pin">The pin to connect to</param>
	/// <returns>The connection ID, or 0 if the connection is invalid(same node or same pin or same mode)</returns>
	public uint Connect( NodePin pin ) {
		if ( pin.Id == Id || pin.Node == Node || pin.PinMode == PinMode ) {
			return 0;
		}

		if ( PinMode == PinMode.Input ) {
			return pin.Connect( this );
		}

		uint id = NodeEditor.GetID();
		_connections.Add( id, new ConnectionTarget( pin.Node, pin.Id ) );
		return id;
	}

	public ConnectionTarget[] GetConnections() {
		return _connections.Values.ToArray();
	}
}


public enum PinMode : byte {
	Input,
	Output
}
