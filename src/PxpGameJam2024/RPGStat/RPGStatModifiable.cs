using System.Collections.Generic;


namespace Pixpil.RPGStatSystem {
    
    /// <summary>
    /// A RPGStat Type that implements both IStatModifiable
    /// </summary>
    public class RPGStatModifiable : RPGStat, IStatModifiable {
        
        /// <summary>
        /// List of RPGStatModifiers applied to the stat
        /// </summary>
        private readonly List<RPGStatModifier> _statMods;
        
        /// <summary>
        /// Used by the StatModifierValue Property
        /// </summary>
        private float _statModValue;

        /// <summary>
        /// The stat's total value including StatModifiers
        /// </summary>
        public override float StatValue => base.StatValue + StatModifierValue;


        /// <summary>
        /// The total value of the stat modifiers applied to the stat
        /// </summary>
        public float StatModifierValue => _statModValue;
        
        
        public List< RPGStatModifier > StatMods => _statMods;


        /// <summary>
        /// Constructor that takes a stat asset
        /// </summary>
        public RPGStatModifiable() {
            _statModValue = 0;
            _statMods = new List<RPGStatModifier>();
        }

        /// <summary>
        /// Get the number of modifiers active on the stat
        /// </summary>
        public int GetModifierCount() {
            return _statMods.Count;
        }

        /// <summary>
        /// Get the stat modifier with the given index
        /// </summary>
        public RPGStatModifier GetModifierAt( int index ) {
            if ( index >= 0 && index < _statMods.Count - 1 ) {
                return _statMods[ index ];
            }

            return null;
        }

        /// <summary>
        /// Adds Modifier to stat and listens to the mod's value change event
        /// </summary>
        public void AddModifier( RPGStatModifier mod ) {
            _statMods.Add( mod );
            mod.AddValueListener( OnModValueChange );
            UpdateModifiers();
        }

        /// <summary>
        /// Removes modifier from stat and stops listening to value change event
        /// </summary>
        public void RemoveModifier( RPGStatModifier mod ) {
            mod.RemoveValueListener( OnModValueChange );
            _statMods.Remove( mod );
            UpdateModifiers();
        }

        /// <summary>
        /// Removes all modifiers from the stat and stops listening to the value change event
        /// </summary>
        public void ClearModifiers() {
            foreach ( var mod in _statMods ) {
                mod.RemoveValueListener( OnModValueChange );
                mod.OnModifierRemove();
            }

            _statMods.Clear();
            UpdateModifiers();
        }

        /// <summary>
        /// Updates the StatModifierValue based of the currently applied modifier values
        /// </summary>
        public void UpdateModifiers() {
            float newStatModValue = 0;

            // Group modifers by the order they are applied
    //            var orderGroups = _statMods.GroupBy(m => m.Order);
    //            foreach (var group in orderGroups) {
    //                // Find the total sum for all stackable modifiers
    //                // and the max value of all not stackable modifiers
    //                float sum = 0, max = 0;
    //                foreach (var mod in group) {
    //                    if (mod.Stacks == false) {
    //                        max = Mathf.Max(max, mod.Value);
    //                    } else {
    //                        sum += mod.Value;
    //                    }
    //                }
    //
    //                // Apply the stat modifier with either the total sum or
    //                // the max value, depending on which on is greater
    //                newStatModValue += group.First().ApplyModifier(
    //                    StatBaseValue + newStatModValue,
    //                    sum > max ? sum : max);
    //            }
            
            foreach (var mod in _statMods) {
                
                newStatModValue += mod.ApplyModifier(
                    StatBaseValue + newStatModValue,
                    mod.Value);
            }

            // Trigger value change if stat mod value changed
            if (_statModValue != newStatModValue) {
                _statModValue = newStatModValue;
                TriggerValueChange();
            }
        }

        /// <summary>
        /// Used to listen to the applied stat modifier OnValueChange events
        /// </summary>
        private void OnModValueChange( RPGStatModifier mod ) {
            UpdateModifiers();
        }
    }

}
