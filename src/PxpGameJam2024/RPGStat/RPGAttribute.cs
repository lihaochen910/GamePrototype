﻿using System.Collections.Generic;


namespace Pixpil.RPGStatSystem {

    /// <summary>
    /// RPGStat that inherits from RPGStatModifiable and implements IStatScalable and IStatLinkable.
    /// </summary>
    public class RPGAttribute : RPGStatModifiable, IStatLinkable {
        
        /// <summary>
        /// Used By StatLinker Value Property
        /// </summary>
        private float _statLinkerValue;

        /// <summary>
        /// List of all stat linkers applied to the stat
        /// </summary>
        private readonly List<RPGStatLinker> _statLinkers;

        /// <summary>
        /// the value gained from all applied stat linkers
        /// </summary>
        public float StatLinkerValue => _statLinkerValue;

        /// <summary>
        /// Gets the stat base value with the StatLevelValue and StatLinkerValue added
        /// </summary>
        public override float StatBaseValue => base.StatBaseValue + StatLinkerValue;

        public List< RPGStatLinker > StatLinkers => _statLinkers;

        /// <summary>
        /// Constructor that takes a stat asset
        /// </summary>
        public RPGAttribute() {
            _statLinkers = new List< RPGStatLinker >();
        }

        /// <summary>
        /// Add a linker to the stat and listen to it's valueChange event
        /// </summary>
        public void AddLinker( RPGStatLinker linker ) {
            if ( linker != null ) {
                _statLinkers.Add( linker );
                linker.LinkedStat?.AddValueListener( OnLinkedStatValueChange );
            }
        }

        /// <summary>
        /// Removes a linker from the stat and stops listening to the value change event
        /// </summary>
        /// <param name="linker"></param>
        public void RemoveLinker( RPGStatLinker linker ) {
            if ( linker != null ) {
                _statLinkers.Remove( linker );
                linker.LinkedStat?.RemoveValueListener( OnLinkedStatValueChange );
            }
        }
        
        /// <summary>
        /// Removes all linkers from the stat
        /// </summary>
        public void ClearLinkers() {
            foreach ( var linker in _statLinkers ) {
                linker.LinkedStat?.RemoveValueListener( OnLinkedStatValueChange );
            }

            _statLinkers.Clear();
        }
        
        /// <summary>
        /// Get the number of modifiers active on the stat
        /// </summary>
        public int GetLinkerCount() {
            return _statLinkers != null ? _statLinkers.Count : 0;
        }

        /// <summary>
        /// Listens to the attached StatLinker's Linked Stat and Updates 
        /// the StatLinkerValue if a stat linker value changes
        /// </summary>
        private void OnLinkedStatValueChange( IStatValue iStatValue ) {
            UpdateLinkerValue();
        }

        /// <summary>
        /// Update the StatLinkerValue based of the currently applied stat linkers
        /// </summary>
        public void UpdateLinkerValue() {
    #if DEBUG_STAT_INFO
            UnityEngine.Debug.LogFormat("[Stat Category {0}]: Update Linker value", StatCategoryId);
    #endif

            // Get the new linker value from connected linkers
            float newLinkerValue = 0;
            foreach ( RPGStatLinker link in _statLinkers ) {
                newLinkerValue += link.GetValue();
            }

            // Trigger value change if new value

            if ( !Numeric.AreEqual( _statLinkerValue, newLinkerValue ) ) {
                _statLinkerValue = newLinkerValue;
                TriggerValueChange();
            }
        }
    }
    
}
