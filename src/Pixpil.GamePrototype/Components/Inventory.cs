using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Bang.Components;
using Bang.Entities;
using Pixpil.Data;
using Pixpil.Services;


namespace Pixpil.Data {

	public interface IInventoryEntry {
		public ItemType ItemType { get; set; }
		public int Count { get; set; }
		public void AddCount( int count );
		public void SetCount( int count );
	}

	[DebuggerDisplay("{ItemType?.Id}: {Count}")]
	public struct InventoryEntry : IInventoryEntry {

		public static readonly InventoryEntry Empty = new () { ItemType = null, Count = 0 };
    	
    	private ItemType _item;
		// [JsonConverter(typeof(ItemTypeConverter))]
		public ItemType ItemType {
			get => _item;
			set {
				_item = value;
				_count = 0;
			}
		}
    
    	private int _count;
		public int Count {
			get => _count;
			set => _count = value;
		}
		
    	public void AddCount( int count ) {
    		_count += count;
    	}
    	
    	public void SetCount( int count ) {
    		_count = count;
    	}
    
    	public void Clear() {
    		_count = 0;
    	}
    
    	public void Reset() {
    		_item = null;
    		_count = 0;
    	}
    	
    	public static InventoryEntry operator +( InventoryEntry a, InventoryEntry b ) {
    		if ( a._item == b._item ) {
    			a._count += b._count;
    		}
    
    		return a;
    	}
    }


	[DebuggerDisplay("{ItemType?.Id}: {Count}")]
	public struct ReadOnlyInventoryEntry : IInventoryEntry {
		private ItemType _item;
		// [JsonConverter(typeof(ItemTypeConverter))]
		public ItemType ItemType {
			get => _item;
			set {
				_item = value;
				_count = 0;
			}
		}
    
		private int _count;
		public int Count {
			get => _count;
			set { _count = value; }
		}
		
		public void AddCount( int count ) {}
    	
		public void SetCount( int count ) {
			_count = count;
		}
	}
	
}


namespace Pixpil.Components {
	
	using Pixpil.Messages;

	public readonly struct InventoryComponent : IComponent {

		public readonly bool CanExpand;
		public readonly ImmutableArray< InventoryEntry > Cells = ImmutableArray< InventoryEntry >.Empty;
		
		public int Capacity => Cells.Length;
		
		public InventoryComponent( bool canExpand, int initSize ) {
			CanExpand = canExpand;
			Cells = ImmutableArray.CreateBuilder< InventoryEntry >( initSize ).ToImmutable();
		}

		public InventoryComponent( bool canExpand, ImmutableArray< InventoryEntry > cells ) {
			CanExpand = canExpand;
			Cells = cells;
		}
		
		public bool HasItem( string itemId ) {
    		return Cells.FirstOrDefault( entry => entry.ItemType != null && entry.ItemType.Id == itemId && entry.Count > 0 ).ItemType is not null;
    	}
    
    	public bool HasItem( ItemType item ) {
    		return Cells.FirstOrDefault( entry => entry.ItemType != null && entry.ItemType.Equals( item ) && entry.Count > 0 ).ItemType is not null;
    	}

		public int GetItemCount( ItemType item ) {
			if ( !HasItem( item ) ) {
				return 0;
			}
			return Cells.Where( entry => entry.ItemType != null && entry.ItemType.Equals( item ) ).Select( entry => entry.Count ).FirstOrDefault();
		}
		
		// public ref InventoryEntry FindEntry( ItemType itemType ) {
		// 	for ( var i = 0; i < Cells.Length; i++ ) {
		// 		ref var entry = ref Cells[ i ];
		// 		if ( entry.ItemType != null && entry.ItemType.Equals( itemType ) ) {
		// 			return ref entry;
		// 		}
		// 	}
		// 	
		// 	return ref EmptyCell;
		// }
		//
		// public InventoryEntry FindEntry( string id ) {
		// 	return Cells.FirstOrDefault( entry => entry.ItemType != null && entry.ItemType.Id == id );
		// }
    
    	// private bool ExistItem( Item item ) => _cells.FirstOrDefault( entry => entry.ItemType == item ) is not null;

		/// <summary>
		/// find InventoryEntry with null ItemType
		/// </summary>
		/// <returns></returns>
		public bool HasFreeSpace() {
			foreach ( var entry in Cells ) {
				if ( entry.ItemType is null ) {
					return true;
				}
			}

			return false;
		}


		public int FindFreeEntryIndex() {
			for ( var i = 0; i < Cells.Length; i++ ) {
				ref readonly var entry = ref Cells.ItemRef( i );
				if ( entry.ItemType is null ) {
					return i;
				}
			}

			return -1;
		}
    
    	public int FindEntryIndex( ItemType item ) {
			for ( var i = 0; i < Cells.Length; i++ ) {
				ref readonly var entry = ref Cells.ItemRef( i );
				if ( entry.ItemType != null && entry.ItemType.Equals( item ) ) {
					return i;
				}
			}

			return -1;
		}
    
    	public bool AddItem( ItemType item, int count, Entity owner ) {
			if ( count == 0 ) {
				return true;
			}
			
			var entryIndex = FindEntryIndex( item );
			if ( entryIndex != -1 ) {
				ref readonly var entry = ref Cells.ItemRef( entryIndex );
				if ( entry.ItemType != null ) {
					owner.SetInventory( CanExpand, Cells.SetItem( entryIndex, new InventoryEntry { ItemType = item, Count = entry.Count + count } ) );
				
					if ( owner.HasInventoryMessageListener() ) {
						if ( owner.GetInventoryMessageListener().ListenType is InventoryMessageListenType.Simple ) {
							owner.SendMessage< InventoryChangedMessage >();
						}
						else {
							var msg = new InventoryChangedDetailMessage( InventoryItemChangedType.Increased, item, count );
							owner.SendMessage( msg );
						}
					}
				
					return true;
				}
			}

			if ( CanExpand ) {
				owner.SetInventory( CanExpand, Cells.Add( new InventoryEntry { ItemType = item, Count = count } ) );
				return true;
			}
			else {
				if ( !HasFreeSpace() ) {
					return false;
				}
				
				var freeEntryIndex = FindFreeEntryIndex();
				owner.SetInventory( CanExpand, Cells.SetItem( freeEntryIndex, new InventoryEntry { ItemType = item, Count = count } ) );
				return true;
			}
		}

		public void RemoveItem( ItemType item, int count, Entity owner ) {
			if ( count == 0 ) {
				return;
			}
			var entryIndex = FindEntryIndex( item );
			ref readonly var entry = ref Cells.ItemRef( entryIndex );
			if ( entry.ItemType != null ) {
				owner.SetInventory( CanExpand, Cells.SetItem( entryIndex, new InventoryEntry { ItemType = item, Count = entry.Count - count } ) );
				
				if ( owner.HasInventoryMessageListener() ) {
					if ( owner.GetInventoryMessageListener().ListenType is InventoryMessageListenType.Simple ) {
						owner.SendMessage< InventoryChangedMessage >();
					}
					else {
						var msg = new InventoryChangedDetailMessage( InventoryItemChangedType.Decreased, item, count );
						owner.SendMessage( msg );
					}
				}
			}
		}
		
		
		/// <param name="item"></param>
		/// <param name="other"></param>
		/// <param name="count">negative is all</param>
		/// <returns></returns>
		public ( bool successed, int realCount ) MoveItemToOther( string item, Entity owner, Entity other, int count = -1 ) {
			var itemType = ItemTypeServices.GetItemType( item );
			return MoveItemToOther( itemType, other, owner, count );
		}


		public ( bool successed, int realCount ) MoveItemToOther( ItemType item, Entity owner, Entity other, int count = -1 ) {
			var max = GetItemCount( item );
			var checkedCount = Math.Clamp( count < 0 ? max : count, 0, max );

			if ( other.GetInventory().AddItem( item, checkedCount, other ) ) {
				goto Successed;
			}

			return ( false, 0 );
			
Successed:
			RemoveItem( item, checkedCount, owner );
			return ( true, checkedCount );
		}
    
		
    	public void MoveAllToOther( Entity other, Entity owner ) {
    		foreach ( var cell in Cells ) {
    			if ( cell.ItemType is null ) {
    				continue;
    			}

				var ( successed, _ ) = MoveItemToOther( cell.ItemType, owner, other );
				if ( successed ) {
					cell.Reset();
				}
    		}
    	}
		
		public void CopyAllToOther( Entity other ) {

			foreach ( var cell in Cells ) {
				if ( cell.ItemType is null ) {
					continue;
				}

				if ( !other.GetInventory().AddItem( cell.ItemType, cell.Count, other ) ) {
					break;
				}
			}
		}
		
		public ( bool successed, int realCount ) MoveItemToOther( ItemType itemType, int count, Entity owner, Entity other ) {
			if ( !other.HasInventory() ) {
				return ( false, 0 );
			}

			var ( result, realCount ) = MoveItemToOther( itemType, owner, other, count );
			if ( result ) {
				if ( owner.HasInventoryMessageListener() ) {
					if ( owner.GetInventoryMessageListener().ListenType is InventoryMessageListenType.Simple ) {
						owner.SendMessage< InventoryChangedMessage >();
					}
					else {
						var msg = new InventoryChangedDetailMessage( InventoryItemChangedType.Decreased, itemType, realCount );
						owner.SendMessage( msg );
					}
				}
			}

			return ( result, realCount );
		}
	}


	public enum InventoryMessageListenType : byte {
		Simple,
		Detail
	}


	public enum InventoryItemChangedType : byte {
		Increased,
		Decreased
	}


	public readonly struct InventoryMessageListenerComponent : IComponent {
		
		public readonly InventoryMessageListenType ListenType;

		public InventoryMessageListenerComponent( InventoryMessageListenType listenType ) {
			ListenType = listenType;
		}
	}

}


namespace Pixpil.Messages {
	
	using Pixpil.Components;

	public readonly record struct InventoryChangedMessage : IMessage {
		
		public InventoryChangedMessage() {}
	
	}
	
	
	public readonly record struct InventoryChangedDetailMessage : IMessage {

		public readonly InventoryItemChangedType ChangedType;
		public readonly ItemType ItemType;
		public readonly int Count;

		public InventoryChangedDetailMessage( InventoryItemChangedType changedType, ItemType itemType, int count ) {
			ChangedType = changedType;
			ItemType = itemType;
			Count = count;
		}
	
	}

}
