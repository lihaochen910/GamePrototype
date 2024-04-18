using System.Collections.Generic;
using System.Collections.Immutable;
using Bang;
using Bang.Components;
using Bang.Entities;
using Murder.Attributes;
using Murder.Utilities.Attributes;


namespace Pixpil.Components;

[RuntimeOnly, DoNotPersistOnSave]
public readonly struct AgentInBuildingRangeComponent : IComponent {
	
	/// <summary>
	/// Id of the entity that caused this collision.
	/// </summary>
	[ShowInEditor]
	private readonly ImmutableHashSet< int > _inRangeBuildings = ImmutableHashSet< int >.Empty;

	public AgentInBuildingRangeComponent( int id ) => _inRangeBuildings = ImmutableHashSet< int >.Empty.Add( id );
	
	public AgentInBuildingRangeComponent( ImmutableHashSet< int > idList ) => _inRangeBuildings = idList;


	public IEnumerable< Entity > GetInRangeEntities( World world ) {
		foreach ( var id in _inRangeBuildings ) {
			var entity = world.TryGetEntity( id );
			if ( entity != null && !entity.IsDestroyed )
				yield return entity;
		}
	}

	public bool HasId( int id ) => _inRangeBuildings.Contains( id );

	public AgentInBuildingRangeComponent Add( int id ) => new( _inRangeBuildings.Add( id ) );
	
	public AgentInBuildingRangeComponent Remove( int id ) => new( _inRangeBuildings.Remove( id ) );
	
}
