using System;
using System.Numerics;


namespace Pixpil.AI;

[Serializable]
///Variables are stored in Blackboards and can optionaly be bound to Properties of a Unity Component
public record struct Variable {

    public static readonly Variable Null = new () { Value = null, VarType = typeof( object ) };
    
    // public event Action< string > OnNameChanged;
    // public event Action< string, object > OnValueChanged;


    ///The name of the variable
    // public string Name {
    //     get => _name;
    //     set {
    //         if ( _name != value ) {
    //             _name = value;
    //             OnNameChanged?.Invoke( value );
    //         }
    //     }
    // }
    // private string _name;


    // public string Id {
    //     get {
    //         if ( string.IsNullOrEmpty( _id ) ) {
    //             _id = Guid.NewGuid().ToString();
    //         }
    //
    //         return _id;
    //     }
    // }
    // private string _id;


    ///The value as object type when accessing from base class
    public object Value {
        get => _value;
        set => _value = value;
    }
    private object _value;


    ///Is the variable protected?
    // public bool IsProtected {
    //     get => _protected;
    //     set => _protected = value;
    // }
    // private bool _protected;

    
    //required
    public Variable() {}


    public Variable( object value ) {
        _value = value;
        VarType = value.GetType();
    }


    public Variable( object value, Type valueType ) {
        _value = value;
        VarType = valueType;
    }

    
    //we need this since onValueChanged is an event and we can't check != null outside of this class
    // protected bool HasValueChangeEvent() {
    //     return OnValueChanged != null;
    // }

    //invoke value changed event
    // protected void BroadcastOnValueChanged( string name, object value ) {
    //     OnValueChanged?.Invoke( name, value );
    // }

    
    // ///The System.Object value of the contained variable
    // public object ObjectValue { get; set; }

    ///The Type this Variable holds
    public Type VarType { get; set; }
    
    public bool CanConvertTo( Type toType ) => VarType.IsAssignableTo( toType );

    ///Returns whether or not the variable is property binded
    // public abstract bool HasBinding { get; }

    ///The path to the property this data is binded to. Null if none
    // public abstract string PropertyPath { get; set; }

    // ///Used to bind variable to a property
    // abstract public void BindProperty(MemberInfo prop, GameObject target = null);

    // ///Used to un-bind variable from a property
    // abstract public void UnBindProperty();

    // ///Called from Blackboard in Awake to Initialize the binding on specified Entity
    // public abstract void InitializePropertyBinding( Entity entity, bool callSetter = false );

    // ///Checks whether a convertion to type is possible
    // public bool CanConvertTo( Type toType ) => GetGetConverter( toType ) != null;
    //
    // ///Gets a Func<object> that converts the value ToType if possible. Null if not.
    // public Func<object> GetGetConverter(Type toType) {
    //
    //     if ( toType.IsAssignableFrom(VarType) ) {
    //         return () => Value;
    //     }
    //
    //     var converter = TypeConverter.Get(VarType, toType);
    //     if ( converter != null ) {
    //         return () => converter(Value);
    //     }
    //
    //     return null;
    // }
    //
    // ///Checks whether a convertion from type is possible
    // public bool CanConvertFrom( Type fromType ) => GetSetConverter( fromType ) != null;
    //
    // ///Gets an Action<object> that converts the value fromType if possible. Null if not.
    // public Action< object > GetSetConverter( Type fromType ) {
    //
    //     if ( VarType.IsAssignableFrom( fromType ) ) {
    //         return x => Value = x;
    //     }
    //
    //     var converter = TypeConverter.Get( fromType, VarType );
    //     if ( converter != null ) {
    //         return x => Value = converter( x );
    //     }
    //
    //     return null;
    // }

    // public override string ToString() {
    //     return Name;
    // }
}


///----------------------------------------------------------------------------------------------

///The actual Variable
// [Serializable]
// public class Variable< T > : Variable {
//
//     // [JsonProperty]
//     private T _value;
//
//     // [JsonProperty]
//     // private string _propertyPath;
//
//     //required
//     public Variable() {}
//
//     //delegates for property binding
//     private Func< T > _getter;
//     private Action< T > _setter;
//
//
//     // public override string PropertyPath {
//     //     get => _propertyPath;
//     //     set => _propertyPath = value;
//     // }
//
//
//     // public override bool HasBinding => !string.IsNullOrEmpty( _propertyPath );
//
//
//     protected override object ObjectValue {
//         get => Value;
//         set => Value = ( T )value;
//     }
//
//
//     public override Type VarType => typeof( T );
//
//
//     ///The value as correct type when accessing as this type
//     public new T Value {
//         get => _getter != null ? _getter() : _value;
//         set {
//             if ( HasValueChangeEvent() ) {
//                 //check this first to avoid possible unescessary value boxing
//                 if ( !TrueEquals( _value, value ) ) {
//                     _value = value;
//                     _setter?.Invoke( value );
//
//                     BroadcastOnValueChanged( null, value );
//                 }
//
//                 return;
//             }
//
//             _value = value;
//             _setter?.Invoke( value );
//         }
//     }
//
//
//     ///Used to bind this to BBParameters
//     public T GetValue() => Value;
//
//     ///Used to bind this to BBParameters
//     public void SetValue( T newValue ) => Value = newValue;
//
//
//     // ///Set the property binding. Providing target also initializes the property binding
//     // public override void BindProperty(MemberInfo prop, GameObject target = null) {
//     //     if ( prop is PropertyInfo || prop is FieldInfo ) {
//     //         _propertyPath = string.Format("{0}.{1}", prop.RTReflectedOrDeclaredType().FullName, prop.Name);
//     //         if ( target != null ) {
//     //             InitializePropertyBinding(target, false);
//     //         }
//     //     }
//     // }
//     //
//     // ///Removes the property binding
//     // public override void UnBindProperty() {
//     //     _propertyPath = null;
//     //     getter = null;
//     //     setter = null;
//     // }
//     //
//     // ///Initialize the property binding for target gameobject. The gameobject is only used in case the binding is not static.
//     // public override void InitializePropertyBinding( Entity entity, bool callSetter = false ) {
//     //
//     //     if ( !HasBinding/* || !Application.isPlaying*/ ) {
//     //         return;
//     //     }
//     //
//     //     _getter = null;
//     //     _setter = null;
//     //
//     //     var idx = _propertyPath.LastIndexOf('.');
//     //     var typeString = _propertyPath.Substring(0, idx);
//     //     var memberString = _propertyPath.Substring(idx + 1);
//     //     var type = ReflectionHelper.GetType(typeString, /*fallback?*/ true);
//     //     
//     //     if ( type == null ) {
//     //         GameLogger.Error(string.Format("Type '{0}' not found for Blackboard Variable '{1}' Binding.", typeString, Name), go);
//     //         return;
//     //     }
//     //     
//     //     var prop = type.RTGetProperty(memberString);
//     //     if ( prop != null ) {
//     //         var getMethod = prop.RTGetGetMethod();
//     //         var setMethod = prop.RTGetSetMethod();
//     //         var isStatic = ( getMethod != null && getMethod.IsStatic ) || ( setMethod != null && setMethod.IsStatic );
//     //         var instance = isStatic ? null : go.GetComponent(type);
//     //         if ( instance == null && !isStatic ) {
//     //             GameLogger.Error(string.Format("A Blackboard Variable '{0}' is due to bind to a Component type that is missing '{1}'. Binding ignored.", Name, typeString), go);
//     //             return;
//     //         }
//     //     
//     //         if ( prop.CanRead ) {
//     //             try { _getter = getMethod.RTCreateDelegate<Func<T>>(instance); } //JIT
//     //             catch { _getter = () => { return (T)getMethod.Invoke(instance, null); }; } //AOT
//     //         } else {
//     //             _getter = () => { GameLogger.Error(string.Format("You tried to Get a Property Bound Variable '{0}', but the Bound Property '{1}' is Write Only!", Name, _propertyPath), go); return default(T); };
//     //         }
//     //     
//     //         if ( prop.CanWrite ) {
//     //             try { _setter = setMethod.RTCreateDelegate<Action<T>>(instance); } //JIT
//     //             catch { _setter = (o) => { setMethod.Invoke(instance, ReflectionTools.SingleTempArgsArray(o)); }; } //AOT
//     //     
//     //             if ( callSetter ) {
//     //                 _setter(_value);
//     //             }
//     //     
//     //         } else {
//     //             _setter = (o) => { GameLogger.Error(string.Format("You tried to Set a Property Bound Variable '{0}', but the Bound Property '{1}' is Read Only!", Name, _propertyPath), go); };
//     //         }
//     //     
//     //         return;
//     //     }
//     //     
//     //     var field = type.RTGetField(memberString);
//     //     if ( field != null ) {
//     //         var instance = field.IsStatic ? null : go.GetComponent(type);
//     //         if ( instance == null && !field.IsStatic ) {
//     //             GameLogger.Error(string.Format("A Blackboard Variable '{0}' is due to bind to a Component type that is missing '{1}'. Binding ignored", Name, typeString), go);
//     //             return;
//     //         }
//     //         if ( field.IsConstant() ) {
//     //             T value = (T)field.GetValue(instance);
//     //             _getter = () => { return value; };
//     //         } else {
//     //             _getter = () => { return (T)field.GetValue(instance); };
//     //             _setter = o => { field.SetValue(instance, o); };
//     //         }
//     //     
//     //         return;
//     //     }
//     //     
//     //     GameLogger.Error(string.Format("A Blackboard Variable '{0}' is due to bind to a property/field named '{1}' that does not exist on type '{2}'. Binding ignored", Name, memberString, type.FullName), go);
//     // }
//
//     private static bool TrueEquals( object a, object b ) {
//
//         //regardless calling ReferenceEquals, unity is still doing magic and this is the only true solution (I've found)
//         // if ( a is Godot.GodotObject goA && b is Godot.GodotObject goB ) {
//         //     return goA == goB;
//         // }
//
//         return a == b || object.Equals( a, b ) || object.ReferenceEquals( a, b );
//     }
// }


///----------------------------------------------------------------------------------------------

///This is a very special dummy class for variable header separators
public class VariableSeperator {
    public bool IsEditingName { get; set; }
}


///Auto "Convenience Converters" from type to type (boxing).
///This includes unconventional data conversions like for example GameObject to Vector3 (by transform.position).
internal static class TypeConverter {
    
    ///Custom Converter delegate
    public delegate Func< object, object > TypeConverterCustomConverter( Type fromType, Type toType );


    ///Subscribe custom converter
    public static event TypeConverterCustomConverter CustomConverter;

    ///Returns a function that can convert provided first arg value from type to type
    public static Func< object, object > Get( Type fromType, Type toType ) {

        // Custom Converter
        if ( CustomConverter != null ) {
            var converter = CustomConverter( fromType, toType );
            if ( converter != null ) {
                return converter;
            }
        }

        // Normal assignment.
        if ( toType.IsAssignableFrom( fromType ) ) {
            return value => value;
        }

        // Anything to string
        if ( toType == typeof( string ) ) {
            return value => value != null ? value.ToString() : "NULL";
        }

        // Convertible to convertible.
        if ( typeof( IConvertible ).IsAssignableFrom( toType ) &&
             typeof( IConvertible ).IsAssignableFrom( fromType ) ) {
            return value => {
                try {
                    return Convert.ChangeType( value, toType );
                }
                catch {
                    return !toType.IsAbstract ? Activator.CreateInstance( toType ) : null;
                }
            };
        }

        // Unity Object to bool.
        // if ( typeof( Godot.GodotObject ).IsAssignableFrom( fromType ) && toType == typeof( bool ) ) {
        //     return value => value != null;
        // }

        // GameObject to Component.
        // if ( fromType == typeof( GameObject ) && typeof( Component ).IsAssignableFrom( toType ) ) {
        //     return ( value ) => value as GameObject != null ? ( value as GameObject ).GetComponent( toType ) : null;
        // }

        // Component to GameObject.
        // if ( typeof( Component ).IsAssignableFrom( fromType ) && toType == typeof( GameObject ) ) {
        //     return ( value ) => value as Component != null ? ( value as Component ).gameObject : null;
        // }

        // Component to Component.
        // if ( typeof( Godot.Node ).IsAssignableFrom( fromType ) && typeof( Godot.Node ).IsAssignableFrom( toType ) ) {
        //     return ( value ) =>
        //         value as Godot.Node != null ? ( value as Godot.Node ).gameObject.GetComponent( toType ) : null;
        // }

        // GameObject to Interface
        // if ( fromType == typeof( GameObject ) && toType.IsInterface() ) {
        //     return ( value ) => value as GameObject != null ? ( value as GameObject ).GetComponent( toType ) : null;
        // }

        // Component to Interface
        // if ( typeof( Component ).IsAssignableFrom( fromType ) && toType.IsInterface ) {
        //     return ( value ) =>
        //         value as Component != null ? ( value as Component ).gameObject.GetComponent( toType ) : null;
        // }
        
        // Node2D to Vector2 (position).
        // if ( fromType == typeof( Node2D ) && toType == typeof( Vector2 ) ) {
        //     return ( value ) => {
        //         return value as Node2D != null ? ( value as Node2D ).GlobalPosition : Vector2.Zero;
        //     };
        // }

        // Node3D to Vector3 (position).
        // if ( fromType == typeof( Node3D ) && toType == typeof( Vector3 ) ) {
        //     return ( value ) => {
        //         return value as Node3D != null ? ( value as Node3D ).GlobalPosition : Vector3.Zero;
        //     };
        // }

        // Component to Vector3 (position).
        // if ( typeof( Component ).IsAssignableFrom( fromType ) && toType == typeof( Vector3 ) ) {
        //     return ( value ) => {
        //         return value as Component != null ? ( value as Component ).transform.position : Vector3.zero;
        //     };
        // }

        // GameObject to Quaternion (rotation).
        // if ( fromType == typeof( GameObject ) && toType == typeof( Quaternion ) ) {
        //     return ( value ) => {
        //         return value as GameObject != null ? ( value as GameObject ).transform.rotation : Quaternion.identity;
        //     };
        // }

        // Component to Quaternion (rotation).
        // if ( typeof( Component ).IsAssignableFrom( fromType ) && toType == typeof( Quaternion ) ) {
        //     return ( value ) => {
        //         return value as Component != null ? ( value as Component ).transform.rotation : Quaternion.identity;
        //     };
        // }

        // Quaternion to Vector3 (Euler angles).
        // if ( fromType == typeof( Quaternion ) && toType == typeof( Vector3 ) ) {
        //     return ( value ) => ( ( Quaternion )value ).GetEuler();
        // }

        // Vector3 (Euler angles) to Quaternion.
        // if ( fromType == typeof( Vector3 ) && toType == typeof( Quaternion ) ) {
        //     return ( value ) => Quaternion.FromEuler( ( Vector3 )value );
        // }

        // Vector2 to Vector3.
        if ( fromType == typeof( Vector2 ) && toType == typeof( Vector3 ) ) {
            return value => {
                var vector2 = ( Vector2 )value;
                return new Vector3( vector2.X, vector2.Y, 0f );
            };
        }

        // Vector3 to Vector2.
        if ( fromType == typeof( Vector3 ) && toType == typeof( Vector2 ) ) {
            return value => {
                var vector3 = ( Vector3 )value;
                return new Vector2( vector3.X, vector3.Y );
            };
        }

        return null;
    }

    ///Returns whether a conversion exists
    public static bool CanConvert( Type fromType, Type toType ) {
        return Get( fromType, toType ) != null;
    }
}
