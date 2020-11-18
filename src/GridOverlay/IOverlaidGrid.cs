using System;
using System.Drawing;

namespace GridOverlay
{
	/// <summary>
	/// Interface between overlay renderer and underlying grid.
	/// </summary>
	public interface IOverlaidGrid
	{
		/// <summary>
		/// Gets unique identification of row for given position. Position can change during grid lifetime (e.g. sorting).
		/// Guid.Empty if there is no row at given position.
		/// </summary>
		Guid GetRowId(int rowPos);

		/// <summary>
		/// Boundary rectangle for given cell. 
		/// Full valid cell rectangle is required also when cell is scrolled out of view and thus invisible (behind margin defined by 'GetLeftScrollBoundary').
		/// This is required to properly clip invisible part of overlay.
		/// </summary>
		Rectangle GetCellBoundaryRect(int rowPos, int colPos);

		/// <summary>
		/// Drawn shapes will be clipped if they do not fall into active area, used when scrolling.
		/// </summary>
		int GetLeftScrollBoundary();

		/// <summary>
		/// Column position from given location (in client cords). int.MinValue if no column on given position.
		/// </summary>
		int ColPosFromPoint(Point location);

		/// <summary>
		/// Row position from given location (in client cords). int.MinValue if no row on given position.
		/// </summary>
		int RowPosFromPoint(Point location);

		/// <summary>
		/// Called when dragging color block.
		/// Indicates whether given cell accepts color block drop. 
		/// </summary>
		bool CanDropColorBlock(ColorBlock colorBlock, int rowPos, int colPos);

		/// <summary>
		/// Called when performing resize operation.
		/// Indicates whether color block can be resized, if not it will retain previous length. Allows client to specify corrected value.
		/// </summary>
		bool CanResizeColorBlock(ColorBlock colorBlock, ColorBlock.ResizeBorder border, int newBorderColPos, ref int? correctedBorderCol);

		/// <summary>
		/// Redraw is requested for given row.
		/// </summary>
		void InvalidateRows(int[] rowPositions);
	}
}
