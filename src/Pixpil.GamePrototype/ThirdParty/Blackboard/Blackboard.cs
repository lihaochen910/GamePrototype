using System;
using System.Collections.Immutable;
using Bang.Components;
using Murder.Diagnostics;


namespace Pixpil.AI;

public readonly struct BlackboardComponent : IComponent {
	
	// public readonly BlackboardSource Blackboard;
	public readonly ImmutableDictionary< string, Variable > Variables = ImmutableDictionary< string , Variable >.Empty;
	
	///An indexer to access variables on the blackboard. It's highly recomended to use GetValue<T> instead
	public object this[ string varName ] {
		get {
			try {
				return Variables[ varName ].Value;
			}
			catch {
				return null;
			}
		}
		// set => SetValue( varName, value );
	}
	
	// [JsonConstructor]
	// public BlackboardComponent() {}
	
	public BlackboardComponent( ImmutableDictionary< string, Variable > variables ) {
		Variables = variables;
	}
	
	
	///Adds a new Variable in the blackboard
    public BlackboardComponent AddVariable( string varName, object value ) {

        if ( value is null ) {
            GameLogger.Error( "<b>Blackboard:</b> You can't use AddVariable with a null value. Use AddVariable(string, Type) to add the new data first" );
            return this;
        }
        
        return AddVariable( varName, value.GetType(), value );
    }

    ///Adds a new Variable in the blackboard defining name and type instead of value
    public BlackboardComponent AddVariable( string varName, Type type, object value = default ) {

        if ( Variables.ContainsKey( varName ) ) {
            var existing = GetVariable( varName, type );
            if ( existing == Variable.Null ) {
                GameLogger.Warning( string.Format(
                    "<b>Blackboard:</b> Variable with name '{0}' already exists in blackboard, but is of different type! Returning null instead of new.",
                    varName ) );
            }
            else {
                GameLogger.Warning( string.Format(
                    "<b>Blackboard:</b> Variable with name '{0}' already exists in blackboard. Returning existing instead of new.",
                    varName ) );
            }

            return this;
        }

        // var dataType = typeof( Variable<> ).MakeGenericType( type );
        // var newData = ( Variable )Activator.CreateInstance( dataType );
        // Variables[ varName ] = newData;

        return new BlackboardComponent( Variables.SetItem( varName, new Variable { Value = value, VarType = type } ) );
    }

    ///Deletes the Variable of name provided regardless of type and returns the deleted Variable object.
    public BlackboardComponent RemoveVariable( string varName ) {
        // if ( Variables.Remove( varName, out var data ) ) {
        //     OnVariableRemoved?.Invoke( data );
        // }
        //
        // return data;
        return new BlackboardComponent( Variables.Remove( varName ) );
    }

    public bool HasVariable( string varName ) => Variables.ContainsKey( varName );

    public bool HasVariableWithType( string varName, Type type ) {
        if ( Variables != null && varName != null ) {
            if ( Variables.TryGetValue( varName, out var data ) ) {
                // if ( type == null || data.CanConvertTo( type ) ) {
                if ( type == null || data.CanConvertTo( type ) ) {
                    return true;
                }
            }
        }

        return false;
    }

    ///Gets the variable data value from the blackboard with provided name and type T.
    public T GetValue< T >( string varName ) {

        if ( !Variables.TryGetValue( varName, out var variable ) ) {
            GameLogger.Warning( string.Format(
                "<b>Blackboard:</b> No Variable of name '{0}' and type '{1}' exists on Blackboard. Returning default T...",
                varName, typeof( T ).Name ) );
            return default;
        }

        // if ( variable is Variable< T > variableT ) {
        //     return variableT.Value;
        // }

        try {
            return ( T )variable.Value;
        }
        catch {
            GameLogger.Error( string.Format(
                "<b>Blackboard:</b> Can't cast value of variable with name '{0}' to type '{1}'", varName,
                typeof( T ).Name ) );
        }

        return default;
    }

    ///Set the value of the Variable variable defined by its name. If a data by that name and type doesnt exist, a new data is added by that name
    public BlackboardComponent SetValue( string varName, object value ) {

        if ( !Variables.TryGetValue( varName, out var variable ) ) {
            GameLogger.Log( string.Format(
                "<b>Blackboard:</b> No Variable of name '{0}' and type '{1}' exists on Blackboard '{2}'. Adding new instead...",
                varName, value != null ? value.GetType().Name : "null", this ) );
            var changed = AddVariable( varName, value );
            // variable.IsProtected = true;
            return changed;
        }

        try {
            variable.Value = value;
            return new BlackboardComponent( Variables.SetItem( varName, variable ) );
        }
        catch {
            GameLogger.Error( string.Format(
                "<b>Blackboard:</b> Can't cast value '{0}' to blackboard variable of name '{1}' and type '{2}'",
                value != null ? value.ToString() : "null", varName, variable.VarType.Name ) );
            return this;
        }

        // return variable;
    }

    ///Generic version of GetVariable
    // public Variable< T > GetVariable< T >( string varName ) {
    //     return ( Variable< T > )GetVariable( varName, typeof( T ) );
    // }

    ///Gets the Variable object of a certain name and optional specified type
    public Variable GetVariable( string varName, Type ofType = null ) {
        if ( Variables != null && varName != null ) {
            if ( Variables.TryGetValue( varName, out var data ) ) {
                if ( ofType == null || data.CanConvertTo( ofType ) ) {
                    return data;
                }
            }
        }

        return Variable.Null;
    }
    
    
    ///Adds a new Variable<T> with provided value and returns it.
    // public BlackboardComponent AddVariable< T >( string varName ) {
    //     return AddVariable< T >( varName );
    // }

    
    ///Adds a new Variable<T> with default T value and returns it
    // public BlackboardComponent AddVariable< T >( string varName, T value = default ) {
    //     return AddVariable( varName, typeof( T ), value  );
    // }
}
