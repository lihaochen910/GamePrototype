using System;
using DigitalRune.Mathematics;


namespace Pixpil.RPGStatSystem;
    
/// <summary>
/// RPGStat that inherits from RPGAttribute and implement IStatCurrentValueChange
/// </summary>
public class RPGVital : RPGAttribute, IStatValueCurrent {
    
    /// <summary>
    /// Used by the StatCurrentValue Property
    /// </summary>
    private float _statCurrentValue;

    /// <summary>
    /// Event called when the StatCurrentValue changes
    /// </summary>
    private StatValueCurrentChangeEvent _onValueCurrentChange;

    /// <summary>
    /// The current value of the stat. Restricted between the values 0 
    /// and StatValue. When set will trigger the OnCurrentValueChange event.
    /// </summary>
    public float StatValueCurrent {
        get => _statCurrentValue;
        set {
            // Clamp value between the stat value and 0;
            float valueClamped = Clamp( value, 0f, StatValue );

            if ( !Numeric.AreEqual( _statCurrentValue, valueClamped ) ) {
                _statCurrentValue = valueClamped;
                TriggerCurrentValueChange();
            }
        }
    }

    /// <summary>
    /// Constructor that takes a stat asset
    /// </summary>
    public RPGVital() {
        _statCurrentValue = 0;
    }

    /// <summary>
    /// Sets the StatCurrentValue to StatValue
    /// </summary>
    public void SetCurrentValueToMax() {
        StatValueCurrent = StatValue;
    }

    /// <summary>
    /// Triggers the OnCurrentValueChange Event
    /// </summary>
    private void TriggerCurrentValueChange() {
        _onValueCurrentChange?.Invoke( this );
    }

    /// <summary>
    /// Adds a function of type CurrentValueEvent to the OnValueChange delagate.
    /// </summary>
    public void AddCurrentValueListener( StatValueCurrentChangeEvent func ) {
        _onValueCurrentChange += func;
    }

    /// <summary>
    /// Removes a function of type CurrentValueEvent to the OnValueChange delagate.
    /// </summary>
    public void RemoveCurrentValueListener( StatValueCurrentChangeEvent func ) {
        _onValueCurrentChange -= func;
    }
    
    
    private static T Clamp< T >( T value, T min, T max ) where T : IComparable< T > {
        if ( min.CompareTo( max ) > 0 ) {
            // min and max are swapped.
            ( max, min ) = ( min, max );
        }

        if ( value.CompareTo( min ) < 0 )
            value = min;
        else if ( value.CompareTo( max ) > 0 )
            value = max;

        return value;
    }
}
