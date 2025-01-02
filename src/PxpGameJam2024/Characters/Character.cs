using System.Collections.Immutable;
using Pixpil.RPGStatSystem;


namespace PxpGameJam2024;

public abstract class Character {

	public string Name;
	public readonly RPGVital Hp = new ();
	public readonly RPGAttribute Attack = new ();
	public readonly RPGAttribute BattleEncounteringRate = new ();
	public readonly RPGAttribute DiscoverPropsRate = new ();
	public GameMapLocation Location;
	public readonly List< Item > Items = new ( 3 );
	public ImmutableArray< CharacterAction > Actions = ImmutableArray< CharacterAction >.Empty;

	public bool Alive => Hp.StatValueCurrent > 0;

	public Weapon EquipedWeapon {
		get => _equipedWeapon;
		set {
			if ( value is null ) {
				if ( _equipedWeapon != null ) {
					Attack.RemoveModifier( _equipedWeapon.AttrMod );
				}
				_equipedWeapon = null;
				return;
			}
			
			_equipedWeapon = value;
			Attack.AddModifier( _equipedWeapon.AttrMod );
		}
	}
	private Weapon _equipedWeapon;

	public void PushAction( CharacterAction characterAction ) => Actions = Actions.Add( characterAction );
	public void PushItem( Item item ) => Items.Add( item );
	public void PopItem( Item item ) => Items.Remove( item );

	public bool HasItem< T >() where T : Item {
		foreach ( var item in Items ) {
			if ( item is T ) {
				return true;
			}
		}

		return false;
	}
	
	public T? TryGetItem< T >() where T : Item {
		foreach ( var item in Items ) {
			if ( item is T tItem ) {
				return tItem;
			}
		}

		return null;
	}
	
	public ImmutableArray< CharacterAction > ListAvailableActions() {
		return Actions.Where( act => act.CheckAvailable( this ) ).ToImmutableArray();
	}


	public ImmutableArray< Item > ListItems() {
		return [..Items];
	}
	
}
