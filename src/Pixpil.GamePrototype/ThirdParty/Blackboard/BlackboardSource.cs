/*
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Murder.Diagnostics;


namespace Pixpil.AI;

/// Blackboard holds Variable and is able to save and load itself. It's usefull for interop
/// communication within the program, saving and loading systems etc. This is the main implementation class of IBlackboard and the one
/// being serialized.
[Serializable]
public sealed class BlackboardSource : IBlackboard {

    public event Action< Variable > OnVariableAdded;
    public event Action< Variable > OnVariableRemoved;

    // [JsonProperty]
    private string _name;

    // [JsonProperty]
    private Dictionary< string, Variable > _variables = new ( StringComparer.Ordinal );
    
    public string Name {
        get => _name;
        set => _name = value;
    }


    public Dictionary< string, Variable > Variables {
        get => _variables;
        set => _variables = value;
    }


    // public GameObject PropertiesBindTarget {
    //     get { return null; }
    // }


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
        set => SetValue( varName, value );
    }


    //required
    public BlackboardSource() {}


    public BlackboardSource( BlackboardSource other ) {
        foreach ( var varKV in other._variables ) {
            AddVariable( varKV.Key, varKV.Value.VarType );
            SetValue( varKV.Key, varKV.Value.Value );
        }
    }

    ///Initialize variables data binding for the target game object
    // public void InitializePropertiesBinding(GameObject targetGO, bool callSetter) {
    //     foreach ( var data in variables.Values ) {
    //         data.InitializePropertyBinding(targetGO, callSetter);
    //     }
    // }

    ///Adds a new Variable in the blackboard
    public Variable AddVariable( string varName, object value ) {

        if ( value == null ) {
            GameLogger.Error( "<b>Blackboard:</b> You can't use AddVariable with a null value. Use AddVariable(string, Type) to add the new data first" );
            return null;
        }

        var newData = AddVariable( varName, value.GetType() );
        if ( newData != null ) {
            newData.Value = value;
        }

        return newData;
    }

    ///Adds a new Variable in the blackboard defining name and type instead of value
    public Variable AddVariable( string varName, Type type ) {

        if ( Variables.ContainsKey( varName ) ) {
            var existing = GetVariable( varName, type );
            if ( existing == null ) {
                GameLogger.Error( string.Format(
                    "<b>Blackboard:</b> Variable with name '{0}' already exists in blackboard, but is of different type! Returning null instead of new.",
                    varName ) );
            }
            else {
                GameLogger.Warning( string.Format(
                    "<b>Blackboard:</b> Variable with name '{0}' already exists in blackboard. Returning existing instead of new.",
                    varName ) );
            }

            return existing;
        }

        var dataType = typeof( Variable<> ).MakeGenericType( type );
        var newData = ( Variable )Activator.CreateInstance( dataType );
        // newData.Name = varName;
        Variables[ varName ] = newData;
        OnVariableAdded?.Invoke( newData );

        return newData;
    }

    ///Deletes the Variable of name provided regardless of type and returns the deleted Variable object.
    public Variable RemoveVariable( string varName ) {
        if ( Variables.Remove( varName, out var data ) ) {
            OnVariableRemoved?.Invoke( data );
        }

        return data;
    }

    public bool HasVariable( string varName ) => _variables.ContainsKey( varName );

    public bool HasVariableWithType( string varName, Type type ) {
        if ( Variables != null && varName != null ) {
            if ( Variables.TryGetValue( varName, out var data ) ) {
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
            GameLogger.Error( string.Format(
                "<b>Blackboard:</b> No Variable of name '{0}' and type '{1}' exists on Blackboard. Returning default T...",
                varName, typeof( T ).Name ) );
            return default;
        }

        if ( variable is Variable< T > variableT ) {
            return variableT.Value;
        }

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
    public Variable SetValue( string varName, object value ) {

        if ( !Variables.TryGetValue( varName, out var variable ) ) {
            GameLogger.Log( string.Format(
                "<b>Blackboard:</b> No Variable of name '{0}' and type '{1}' exists on Blackboard '{2}'. Adding new instead...",
                varName, value != null ? value.GetType().Name : "null", Name ) );
            variable = AddVariable( varName, value );
            // variable.IsProtected = true;
            return variable;
        }

        try {
            variable.Value = value;
        }
        catch {
            GameLogger.Error( string.Format(
                "<b>Blackboard:</b> Can't cast value '{0}' to blackboard variable of name '{1}' and type '{2}'",
                value != null ? value.ToString() : "null", varName, variable.VarType.Name ) );
            return null;
        }

        return variable;
    }

    ///Generic version of GetVariable
    public Variable< T > GetVariable< T >( string varName ) {
        return ( Variable< T > )GetVariable( varName, typeof( T ) );
    }

    ///Gets the Variable object of a certain name and optional specified type
    public Variable GetVariable( string varName, Type ofType = null ) {
        if ( Variables != null && varName != null ) {
            if ( Variables.TryGetValue( varName, out var data ) ) {
                if ( ofType == null || data.CanConvertTo( ofType ) ) {
                    return data;
                }
            }
        }

        return null;
    }

    ///Gets the Variable object of a certain ID and optional specified type.
    public Variable GetVariableByID( string id ) {
        if ( Variables != null && id != null ) {
            foreach ( var pair in Variables ) {
                if ( pair.Value.Id == id ) {
                    return pair.Value;
                }
            }
        }

        return null;
    }

    ///Get all data names of the blackboard
    // public string[] GetVariableNames() {
    //     return Variables.Keys.ToArray();
    // }

    ///Get all data names of the blackboard and of specified type
    // public string[] GetVariableNames( Type ofType ) {
    //     return Variables.Values.Where( v => v.CanConvertTo( ofType ) ).Select( v => v.Name ).ToArray();
    // }


    ///Adds a new Variable<T> with provided value and returns it.
    public Variable< T > AddVariable< T >( string varName, T value ) {
        var data = AddVariable< T >( varName );
        data.Value = value;
        return data;
    }

    
    ///Adds a new Variable<T> with default T value and returns it
    public Variable< T > AddVariable< T >( string varName ) {
        return ( Variable< T > )AddVariable( varName, typeof( T ) );
    }
    
}
*/