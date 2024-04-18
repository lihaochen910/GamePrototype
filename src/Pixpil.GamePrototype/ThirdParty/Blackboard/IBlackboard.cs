/*
using System;
using System.Collections.Generic;


namespace Pixpil.AI;

/// An interface for Blackboards, or otherwise for a Variables container.
public interface IBlackboard {
	
	event Action< Variable > OnVariableAdded;
	event Action< Variable > OnVariableRemoved;

	string Name { get; set; }
	Dictionary< string, Variable > Variables { get; set; }

	// GameObject PropertiesBindTarget { get; }

	Variable AddVariable( string varName, Type type );
	Variable AddVariable( string varName, object value );
	Variable RemoveVariable( string varName );
	Variable GetVariable( string varName, Type ofType = null );
	Variable GetVariableByID( string id );
	Variable< T > GetVariable< T >( string varName );
	T GetValue< T >( string varName );
	Variable SetValue( string varName, object value );
	// string[] GetVariableNames();
	// string[] GetVariableNames( Type ofType );
}
*/