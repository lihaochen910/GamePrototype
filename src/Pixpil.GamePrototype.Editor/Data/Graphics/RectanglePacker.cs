using System.Collections.Generic;


namespace Pixpil.Editor.Data.Graphics;

/**
 * Class used to store rectangles values inside rectangle packer
 * ID parameter needed to connect rectangle with the originally inserted rectangle
 */
public class IntegerRectangle {

	public int x;
	public int y;
	public int width;
	public int height;
	public int right;
	public int bottom;
	public int id;

	public IntegerRectangle(int x = 0, int y = 0, int width = 0, int height = 0) {

		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
		this.right = x + width;
		this.bottom = y + height;
	}
}


/**
 * Class used for sorting the inserted rectangles based on the dimensions
 */
public record SortableSize {

	public int width;
	public int height;
	public int id;

	public SortableSize(int width, int height, int id) {
		this.width = width;
		this.height = height;
		this.id = id;
	}
}


/**
 * Class used to pack rectangles within container rectangle with close to optimal solution.
 */
public class RectanglePacker {

	public const string VERSION = "1.3.0";

	private int _width = 0;
	private int _height = 0;
	private int _padding = 8;

	private int _packedWidth = 0;
	private int _packedHeight = 0;

	private List< SortableSize > _insertList = new List< SortableSize >();

	private List< IntegerRectangle > _insertedRectangles = new List< IntegerRectangle >();
	private List< IntegerRectangle > _freeAreas = new List< IntegerRectangle >();
	private List< IntegerRectangle > _newFreeAreas = new List< IntegerRectangle >();

	private IntegerRectangle _outsideRectangle;

	private List< SortableSize > _sortableSizeStack = new List< SortableSize >();
	private List< IntegerRectangle > _rectangleStack = new List< IntegerRectangle >();


	public int RectangleCount => _insertedRectangles.Count;
	
	public int PackedWidth => _packedWidth;

	public int PackedHeight => _packedHeight;
	
	public int Padding => _padding;

	
	public RectanglePacker( int width, int height, int padding = 0 ) {
		_outsideRectangle = new IntegerRectangle( width + 1, height + 1, 0, 0 );
		Reset( width, height, padding );
	}

	public void Reset( int width, int height, int padding = 0 ) {

		while ( _insertedRectangles.Count > 0 )
			FreeRectangle( _insertedRectangles.Pop() );

		while ( _freeAreas.Count > 0 )
			FreeRectangle( _freeAreas.Pop() );

		_width = width;
		_height = height;

		_packedWidth = 0;
		_packedHeight = 0;

		_freeAreas.Add( AllocateRectangle( 0, 0, _width, _height ) );

		while ( _insertList.Count > 0 )
			FreeSize( _insertList.Pop() );

		_padding = padding;
	}

	public IntegerRectangle GetRectangle( int index, in IntegerRectangle rectangle ) {

		IntegerRectangle inserted = _insertedRectangles[ index ];

		rectangle.x = inserted.x;
		rectangle.y = inserted.y;
		rectangle.width = inserted.width;
		rectangle.height = inserted.height;

		return rectangle;
	}

	public int GetRectangleId( int index ) {

		IntegerRectangle inserted = _insertedRectangles[ index ];
		return inserted.id;
	}

	public void InsertRectangle( int width, int height, int id ) {

		SortableSize sortableSize = AllocateSize( width, height, id );
		_insertList.Add( sortableSize );
	}

	public int PackRectangles( bool sort = true ) {

		if ( sort )
			_insertList.Sort( ( emp1, emp2 ) => emp1.width.CompareTo( emp2.width ) );

		while ( _insertList.Count > 0 ) {

			SortableSize sortableSize = _insertList.Pop();
			int width = sortableSize.width;
			int height = sortableSize.height;

			int index = GetFreeAreaIndex( width, height );
			if ( index >= 0 ) {

				IntegerRectangle freeArea = _freeAreas[ index ];
				IntegerRectangle target = AllocateRectangle( freeArea.x, freeArea.y, width, height );
				target.id = sortableSize.id;

				// Generate the new free areas, these are parts of the old ones intersected or touched by the target
				GenerateNewFreeAreas( target, _freeAreas, _newFreeAreas );

				while ( _newFreeAreas.Count > 0 ) _freeAreas.Add( _newFreeAreas.Pop() );

				_insertedRectangles.Add( target );

				if ( target.right > _packedWidth ) _packedWidth = target.right;

				if ( target.bottom > _packedHeight ) _packedHeight = target.bottom;
			}

			FreeSize( sortableSize );
		}

		return RectangleCount;
	}

	private void FilterSelfSubAreas( List< IntegerRectangle > areas ) {

		for ( int i = areas.Count - 1; i >= 0; i-- ) {

			IntegerRectangle filtered = areas[ i ];
			for ( int j = areas.Count - 1; j >= 0; j-- ) {

				if ( i != j ) {

					IntegerRectangle area = areas[ j ];
					if ( filtered.x >= area.x && filtered.y >= area.y && filtered.right <= area.right &&
						 filtered.bottom <= area.bottom ) {

						FreeRectangle( filtered );
						IntegerRectangle topOfStack = areas.Pop();
						if ( i < areas.Count ) {

							// Move the one on the top to the freed position
							areas[ i ] = topOfStack;
						}

						break;
					}
				}
			}
		}
	}

	private void GenerateNewFreeAreas( IntegerRectangle target, List< IntegerRectangle > areas, List< IntegerRectangle > results ) {

		// Increase dimensions by one to get the areas on right / bottom this rectangle touches
		// Also add the padding here
		int x = target.x;
		int y = target.y;
		int right = target.right + 1 + _padding;
		int bottom = target.bottom + 1 + _padding;

		IntegerRectangle targetWithPadding = null;
		if ( _padding == 0 ) targetWithPadding = target;

		for ( int i = areas.Count - 1; i >= 0; i-- ) {

			IntegerRectangle area = areas[ i ];
			if ( !( x >= area.right || right <= area.x || y >= area.bottom || bottom <= area.y ) ) {

				if ( targetWithPadding == null )
					targetWithPadding = AllocateRectangle( target.x, target.y, target.width + _padding,
						target.height + _padding );

				GenerateDividedAreas( targetWithPadding, area, results );
				IntegerRectangle topOfStack = areas.Pop();
				if ( i < areas.Count ) {

					// Move the one on the top to the freed position
					areas[ i ] = topOfStack;
				}
			}
		}

		if ( targetWithPadding != null && targetWithPadding != target ) FreeRectangle( targetWithPadding );

		FilterSelfSubAreas( results );
	}

	private void GenerateDividedAreas( IntegerRectangle divider, IntegerRectangle area, List< IntegerRectangle > results ) {

		int count = 0;

		int rightDelta = area.right - divider.right;
		if ( rightDelta > 0 ) {
			results.Add( AllocateRectangle( divider.right, area.y, rightDelta, area.height ) );
			count++;
		}

		int leftDelta = divider.x - area.x;
		if ( leftDelta > 0 ) {
			results.Add( AllocateRectangle( area.x, area.y, leftDelta, area.height ) );
			count++;
		}

		int bottomDelta = area.bottom - divider.bottom;
		if ( bottomDelta > 0 ) {
			results.Add( AllocateRectangle( area.x, divider.bottom, area.width, bottomDelta ) );
			count++;
		}

		int topDelta = divider.y - area.y;
		if ( topDelta > 0 ) {
			results.Add( AllocateRectangle( area.x, area.y, area.width, topDelta ) );
			count++;
		}

		if ( count == 0 && ( divider.width < area.width || divider.height < area.height ) ) {

			// Only touching the area, store the area itself
			results.Add( area );

		}
		else
			FreeRectangle( area );
	}

	private int GetFreeAreaIndex( int width, int height ) {

		IntegerRectangle best = _outsideRectangle;
		int index = -1;

		int paddedWidth = width + _padding;
		int paddedHeight = height + _padding;

		int count = _freeAreas.Count;
		for ( int i = count - 1; i >= 0; i-- ) {

			IntegerRectangle free = _freeAreas[ i ];
			if ( free.x < _packedWidth || free.y < _packedHeight ) {

				// Within the packed area, padding required
				if ( free.x < best.x && paddedWidth <= free.width && paddedHeight <= free.height ) {

					index = i;
					if ( ( paddedWidth == free.width && free.width <= free.height && free.right < _width ) ||
						 ( paddedHeight == free.height && free.height <= free.width ) )
						break;

					best = free;
				}

			}
			else {

				// Outside the current packed area, no padding required
				if ( free.x < best.x && width <= free.width && height <= free.height ) {

					index = i;
					if ( ( width == free.width && free.width <= free.height && free.right < _width ) ||
						 ( height == free.height && free.height <= free.width ) )
						break;

					best = free;
				}
			}
		}

		return index;
	}

	private IntegerRectangle AllocateRectangle( int x, int y, int width, int height ) {

		if ( _rectangleStack.Count > 0 ) {

			IntegerRectangle rectangle = _rectangleStack.Pop();
			rectangle.x = x;
			rectangle.y = y;
			rectangle.width = width;
			rectangle.height = height;
			rectangle.right = x + width;
			rectangle.bottom = y + height;

			return rectangle;
		}

		return new IntegerRectangle( x, y, width, height );
	}

	private void FreeRectangle( IntegerRectangle rectangle ) {

		_rectangleStack.Add( rectangle );
	}

	private SortableSize AllocateSize( int width, int height, int id ) {

		if ( _sortableSizeStack.Count > 0 ) {

			SortableSize size = _sortableSizeStack.Pop();
			size.width = width;
			size.height = height;
			size.id = id;

			return size;
		}

		return new SortableSize( width, height, id );
	}

	private void FreeSize( SortableSize size ) {

		_sortableSizeStack.Add( size );
	}
}


static class ListExtension {

	static public T Pop<T>(this List<T> list) {

		int index = list.Count - 1;

		T r = list[index];
		list.RemoveAt(index);
		return r;
	}

}
