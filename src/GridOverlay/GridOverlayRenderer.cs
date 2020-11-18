using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace GridOverlay
{
	/// <summary>
	/// Renders defined color blocks over underlying grid.
	/// </summary>
	public class GridOverlayRenderer
	{
		#region Events

		public event EventHandler<ColorBlockMovedEventArgs> ColorBlockMoved;
		public event EventHandler<ColorBlockResizedEventArgs> ColorBlockResized;

		#endregion

		#region Fields

		private IOverlaidGrid mOverlaidGrid;
		private Control mOverlaidControl;

		// Color blocks (rectangles) that will be rendered in 'Render' method. /rowId;ColorBlocks/
		private Dictionary<Guid, List<ColorBlock>> mRowColorBlocks;

		private Font mBlockTextFont;

		private ColorBlock mSelectedColorBlock = null;

		// total area across which color blocks are rendered
		private int mOverlayArea_FirstColPos;
		private int mOverlayArea_LastColPos;
		private int mOverlayArea_ColCount;

		// set on each call to 'Render' method
		private int mFirstVisibleRowPos;
		private int mLastVisibleRowPos;

		#endregion

		#region Constructor

		public GridOverlayRenderer(IOverlaidGrid overlaidGrid, Control overlaidControl)
		{
			mOverlaidGrid = overlaidGrid;
			mOverlaidControl = overlaidControl;

			// hook to required events
			mOverlaidControl.MouseDown += new MouseEventHandler(mOverlaidControl_MouseDown);
			mOverlaidControl.MouseUp += new MouseEventHandler(mOverlaidControl_MouseUp);
			mOverlaidControl.MouseMove += new MouseEventHandler(mOverlaidControl_MouseMove);

			mOverlaidControl.DragDrop += new DragEventHandler(mOverlaidControl_DragDrop);
			mOverlaidControl.DragEnter += new DragEventHandler(mOverlaidControl_DragEnter);
			mOverlaidControl.DragOver += new DragEventHandler(mOverlaidControl_DragOver);
			mOverlaidControl.DragLeave += new EventHandler(mOverlaidControl_DragLeave);

			mBlockTextFont = overlaidControl.Font;
		}
		
		#endregion

		#region Private Methods

		// checks whether given color blocks fits into configured overlay area
		// if not it corrects ColspanStart and/or ColspanCount properties.
		private void CheckAndFixSize(ColorBlock colorBlock)
		{
			if (colorBlock.ColspanStart < mOverlayArea_FirstColPos)
			{
				colorBlock.ColspanStart = mOverlayArea_FirstColPos;
			}
			if (colorBlock.ColspanStart + colorBlock.ColspanCount - 1 > mOverlayArea_LastColPos)
			{
				colorBlock.ColspanCount = mOverlayArea_LastColPos - colorBlock.ColspanStart + 1;
			}
		}

		private void DrawColorBlock(ColorBlock colorBlock, int rowIndex, Graphics g, int leftScrollBoundary)
		{
			CheckAndFixSize(colorBlock);

			// calculate boundary rectangle
			Rectangle bounds = Rectangle.Empty;
			for (int i = colorBlock.ColspanStart; i < colorBlock.ColspanStart + colorBlock.ColspanCount; i++)
			{
				var cellBounds = mOverlaidGrid.GetCellBoundaryRect(rowIndex, i);
				if (bounds == Rectangle.Empty)
				{
					bounds = new Rectangle(cellBounds.Location, cellBounds.Size);
				}
				else
				{
					// add up
					bounds.Width += cellBounds.Width;
				}
			}

			if (bounds != Rectangle.Empty)
			{
				// set clip region to prevent drawing onto frozen column portion of the grid
				// when using horizontal scrollbar
				if (bounds.X < leftScrollBoundary)
				{
					var clipRegion = new Rectangle(bounds.Location, bounds.Size);
					clipRegion.Width -= leftScrollBoundary - bounds.X;
					clipRegion.X = leftScrollBoundary;
					g.SetClip(clipRegion);
				}

				// draw
				colorBlock.RenderColorBlock(g, bounds, mBlockTextFont);

				g.ResetClip();
			}
		}

		// based on color block total length and its position initializes which resize borders will be visible
		private void InitResizeGrips(ColorBlock colorBlock)
		{
			if (colorBlock.ColspanCount < colorBlock.TotalColumnCount)
			{
				// not all block could be displayed in current area
				if (colorBlock.ColspanStart == mOverlayArea_FirstColPos &&
					colorBlock.ColspanStart + colorBlock.ColspanCount - 1 == mOverlayArea_LastColPos)
				{
					// restrained on both sides
					colorBlock.AllowedRezizeBorders = ColorBlock.ResizeBorder.None;
				}
				else if (colorBlock.ColspanStart == mOverlayArea_FirstColPos)
				{
					// restrained on left
					colorBlock.AllowedRezizeBorders = ColorBlock.ResizeBorder.Right;
				}
				else if (colorBlock.ColspanStart + colorBlock.ColspanCount - 1 == mOverlayArea_LastColPos)
				{
					// restrained on right
					colorBlock.AllowedRezizeBorders = ColorBlock.ResizeBorder.Left;
				}
			}
			else
			{
				// block is completely displayed allow resize on both sides
				colorBlock.AllowedRezizeBorders = ColorBlock.ResizeBorder.Left | ColorBlock.ResizeBorder.Right;
			}
		}

		// Increase or decrease size of color block based on cursor position.
		private void ResizingColorBlock(ColorBlock colorBlock, ColorBlock.ResizeBorder resizeBorder, int rowPos, Point cursorPosition)
		{
			// get current column
			var colPos = mOverlaidGrid.ColPosFromPoint(cursorPosition);

			if (colPos != int.MinValue)
			{
				// trim colpos to active column area			
				var resizeToColPos = colPos > mOverlayArea_FirstColPos ? colPos : mOverlayArea_FirstColPos;
				resizeToColPos = resizeToColPos < mOverlayArea_LastColPos ? resizeToColPos : mOverlayArea_LastColPos;

				// any change?
				if (colorBlock.DetectColspanChange(resizeBorder, resizeToColPos))
				{
					int? correctedPos = null;
					if (mOverlaidGrid.CanResizeColorBlock(colorBlock, resizeBorder, resizeToColPos, ref correctedPos))
					{
						// resize
						if (correctedPos.HasValue)
						{
							colorBlock.AdjustColspan(resizeBorder, correctedPos.Value);
						}
						else
						{
							colorBlock.AdjustColspan(resizeBorder, resizeToColPos);
						}

						// redraw
						mOverlaidGrid.InvalidateRows(new int[] { rowPos });
					}
				}
			}
		}

		private void MoveColorBlock(ColorBlock colorBlock, int newRowPos, int newColPos, int colspanCount = -1)
		{	
			// remove from original position
			if (mRowColorBlocks.ContainsKey(colorBlock.RowId))
			{
				mRowColorBlocks[colorBlock.RowId].Remove(colorBlock);
			}

			// alter rowId according to new row
			colorBlock.RowId = mOverlaidGrid.GetRowId(newRowPos);

			// move colspan so it starts at new position
			colorBlock.ColspanStart = newColPos;

			if (colspanCount > 0)
			{
				colorBlock.ColspanCount = colspanCount;
			}
			else
			{
				// calculate colspan based on TotalColumnCount
				if (colorBlock.ColspanStart + colorBlock.TotalColumnCount - 1 > mOverlayArea_LastColPos)
				{
					// trim to displayed area
					colorBlock.ColspanCount = mOverlayArea_LastColPos - colorBlock.ColspanStart + 1;
				}
				else
				{
					colorBlock.ColspanCount = colorBlock.TotalColumnCount;
				}
			}

			// add to new position
			if (!mRowColorBlocks.ContainsKey(colorBlock.RowId))
			{
				mRowColorBlocks.Add(colorBlock.RowId, new List<ColorBlock>());
			}
			var colorBlockList = mRowColorBlocks[colorBlock.RowId];
			colorBlockList.Add(colorBlock);

			// sort blocks by start date
			mRowColorBlocks[colorBlock.RowId] = colorBlockList.OrderBy(cb => cb.ColspanStart).ToList();
		}

		private void SelectBlockUnderCursor()
		{
			// is it different than previous?
			if (mSelectedColorBlock != mCurrentCursorPos_ColorBlock)
			{
				var rowsToInvalidate = new List<int>(2);

				// remove any previous selection
				if (mSelectedColorBlock != null)
				{
					mSelectedColorBlock.IsSelected = false;

					// find-out current rowPos of deselected block
					int deselectedRowPos = int.MinValue;
					for(int i = mFirstVisibleRowPos; i <= mLastVisibleRowPos; i++)
					{
						if(mSelectedColorBlock.RowId == mOverlaidGrid.GetRowId(i))
						{
							deselectedRowPos = i;
							break;
						}
					}
					if (deselectedRowPos >= 0)
					{
						rowsToInvalidate.Add(deselectedRowPos);
					}

					mSelectedColorBlock = null;
				}

				// select new one
				if (mCurrentCursorPos_ColorBlock != null && !mCurrentCursorPos_ColorBlock.ReadOnly)
				{
					mSelectedColorBlock = mCurrentCursorPos_ColorBlock;
					mSelectedColorBlock.IsSelected = true;
					rowsToInvalidate.Add(mCurrentCursorPos_RowPos);
				}

				mOverlaidGrid.InvalidateRows(rowsToInvalidate.Distinct().ToArray());
			}
		}

		#endregion

		#region Properties

		public Font BlockTextFont
		{
			get
			{
				return mBlockTextFont;
			}
			set
			{
				mBlockTextFont = value;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Call this method to render configured content.
		/// </summary>
		public void Render(PaintEventArgs e, int firstVisibleRowPos, int lastVisibleRowPos)
		{
			if (mRowColorBlocks != null && mRowColorBlocks.Count > 0)
			{
				mFirstVisibleRowPos = firstVisibleRowPos;
				mLastVisibleRowPos = lastVisibleRowPos;

				int leftScrollBoundary = mOverlaidGrid.GetLeftScrollBoundary();

				for (int i = firstVisibleRowPos; i <= lastVisibleRowPos; i++)
				{
					var rowId = mOverlaidGrid.GetRowId(i);
					if (rowId != Guid.Empty && mRowColorBlocks.ContainsKey(rowId))
					{
						var rowColorBlocks = mRowColorBlocks[rowId];
						foreach (var colorBlock in rowColorBlocks)
						{
							DrawColorBlock(colorBlock, i, e.Graphics, leftScrollBoundary);
						}
					}
				}
			}
		}

		/// <summary>
		/// Initializes renderer for drawing color blocks.
		/// </summary>
		/// <param name="colorBlocks">Color blocks which will be rendered.</param>
		/// <param name="overlayArea_firstColPos">Index of first column determining overlay area.</param>
		/// <param name="overlayArea_lastColPos">Index of last column determining overlay area.</param>
		public void InitializeOverlay(
			List<ColorBlock> colorBlocks,
			int overlayArea_firstColPos,
			int overlayArea_lastColPos)
		{			
			mOverlayArea_FirstColPos = overlayArea_firstColPos;
			mOverlayArea_LastColPos = overlayArea_lastColPos;
			mOverlayArea_ColCount = overlayArea_lastColPos - overlayArea_firstColPos + 1;

			// finish initialization of color blocks
			mRowColorBlocks = new Dictionary<Guid, List<ColorBlock>>();
			var groupedByRowId = from cb in colorBlocks
								 group cb by cb.RowId into r
								 select new { RowId = r.Key, ColorBlocks = r };

			foreach (var rowColorBlocks in groupedByRowId)
			{
				mRowColorBlocks.Add(rowColorBlocks.RowId, new List<ColorBlock>());
				mRowColorBlocks[rowColorBlocks.RowId].AddRange(rowColorBlocks.ColorBlocks);
				mRowColorBlocks[rowColorBlocks.RowId].ForEach(cb => 
				{
					CheckAndFixSize(cb);
					InitResizeGrips(cb);
				});

				// sort blocks by start column, block which starts later will be drawn on top of previous
				mRowColorBlocks[rowColorBlocks.RowId].Sort((x, y) => x.ColspanStart.CompareTo(y.ColspanStart));
			}
		}

		/// <summary>
		/// Returns top-most color block on specified coordinates.
		/// </summary>
		public ColorBlock GetTopMostColorBlock(int rowPos, int colPos)
		{
			ColorBlock lastDrawnColorBlock = null;

			var rowId = mOverlaidGrid.GetRowId(rowPos);
			if (rowId != Guid.Empty && mRowColorBlocks.ContainsKey(rowId))
			{
				var colorBlocks = mRowColorBlocks[rowId];

				// get last block which spans across selected cell (drawn above other ones)
				for (int i = 0; i < colorBlocks.Count; i++)
				{
					var colorBlock = colorBlocks[i];

					if (colPos >= colorBlock.ColspanStart &&
						colPos < colorBlock.ColspanStart + colorBlock.ColspanCount)
					{
						lastDrawnColorBlock = colorBlock;
					}
				}
			}

			return lastDrawnColorBlock;
		}

		/// <summary>
		/// Releases associated resources.
		/// </summary>
		public void CleanUp()
		{
			mOverlaidControl.MouseDown -= new MouseEventHandler(mOverlaidControl_MouseDown);
			mOverlaidControl.MouseUp -= new MouseEventHandler(mOverlaidControl_MouseUp);
			mOverlaidControl.MouseMove -= new MouseEventHandler(mOverlaidControl_MouseMove);

			mOverlaidControl.DragDrop -= new DragEventHandler(mOverlaidControl_DragDrop);
			mOverlaidControl.DragEnter -= new DragEventHandler(mOverlaidControl_DragEnter);
			mOverlaidControl.DragOver -= new DragEventHandler(mOverlaidControl_DragOver);
			mOverlaidControl.DragLeave -= new EventHandler(mOverlaidControl_DragLeave);
		}

		#endregion

		#region Drag & Drop & Resize & Select
		
		// Using this support library:
		// http://blogs.msdn.com/b/adamroot/archive/2008/02/19/shell-style-drag-and-drop-in-net-part-2.aspx

		private ColorBlock mCurrentCursorPos_ColorBlock;
		private int mCurrentCursorPos_ColumnPos;
		private int mCurrentCursorPos_RowPos;
		private ColorBlock.ResizeBorder mCurrentCursorPos_ResizeZone = ColorBlock.ResizeBorder.None;
		
		private bool mDragStart = false;
		private Point mDragging_StartPoint = new Point();
		private ColorBlock mDragging_ColorBlock;
		private int mDragging_OriginalRow;
		private int mDragging_OriginalCol;
		private int mDragging_OriginalColSpan;
		
		private bool mResizeStart = false;
		private int mResizing_OriginalCol;

		private void StartDragging()
		{
			var colPos = mOverlaidGrid.ColPosFromPoint(mDragging_StartPoint);
			var rowPos = mOverlaidGrid.RowPosFromPoint(mDragging_StartPoint);

			if (colPos != int.MinValue && rowPos != int.MinValue)
			{
				var colorBlock = GetTopMostColorBlock(rowPos, colPos);
				if (colorBlock != null)
				{
					mDragging_ColorBlock = colorBlock;
					mDragging_OriginalRow = rowPos;
					mDragging_OriginalCol = colPos;
					mDragging_OriginalColSpan = colorBlock.ColspanCount;
					DataObject data = new DataObject(new DragDropLib.DataObject());
					data.SetDragImage(colorBlock.GetDragImage(mOverlayArea_ColCount), new Point(10, 10));
					mOverlaidControl.DoDragDrop(data, DragDropEffects.Move | DragDropEffects.Scroll);
				}
			}
		}

		void mOverlaidControl_MouseMove(object sender, MouseEventArgs e)
		{
			if (mDragStart)
			{
				// determine if cursor moved configured minimum distance with pressed left button to start drag operation
				if (System.Math.Abs(e.X - mDragging_StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
					System.Math.Abs(e.Y - mDragging_StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					mDragStart = false;
					StartDragging();
				}
			}
			else if (mResizeStart)
			{
				// currently resizing
				ResizingColorBlock(mCurrentCursorPos_ColorBlock, mCurrentCursorPos_ResizeZone, mCurrentCursorPos_RowPos, e.Location);
			}
			else
			{
				// track color block under cursor
				mCurrentCursorPos_ColumnPos = mOverlaidGrid.ColPosFromPoint(e.Location);
				mCurrentCursorPos_RowPos = mOverlaidGrid.RowPosFromPoint(e.Location);

				if (mCurrentCursorPos_ColumnPos != int.MinValue && mCurrentCursorPos_RowPos != int.MinValue)
				{
					mCurrentCursorPos_ColorBlock = GetTopMostColorBlock(mCurrentCursorPos_RowPos, mCurrentCursorPos_ColumnPos);
				}
				else
				{
					mCurrentCursorPos_ColorBlock = null;
				}

				mCurrentCursorPos_ResizeZone =
					mCurrentCursorPos_ColorBlock == null ? ColorBlock.ResizeBorder.None : mCurrentCursorPos_ColorBlock.IsInRezizeZone(e.Location);

				// is cursor over color block resize zone?
				Cursor should = Cursors.Default;
				if (mCurrentCursorPos_ColorBlock != null && mCurrentCursorPos_ResizeZone != ColorBlock.ResizeBorder.None)
				{
					should = Cursors.SizeWE;
				}

				if (!mOverlaidControl.Cursor.Equals(should))
				{
					mOverlaidControl.Cursor = should;
				}
			}
		}

		void mOverlaidControl_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				SelectBlockUnderCursor();

				// is cursor over color block?				
				if (mCurrentCursorPos_ColorBlock != null && !mCurrentCursorPos_ColorBlock.ReadOnly)
				{
					if (mOverlaidControl.Cursor == Cursors.Default)
					{
						// mark possible start of drag & drop operation
						mDragStart = true;
						mDragging_StartPoint = new System.Drawing.Point(e.X, e.Y);
					}
					else if (mOverlaidControl.Cursor == Cursors.SizeWE)
					{
						// mark start of resizing
						mResizeStart = true;

						// store original border column for future revert (only this can by changed through resizing)
						mResizing_OriginalCol = mCurrentCursorPos_ColumnPos;
					}
				}
			}
		}

		void mOverlaidControl_MouseUp(object sender, MouseEventArgs e)
		{
			if (mDragStart)
			{
				mDragStart = false;
			}

			if (mResizeStart)
			{
				mResizeStart = false;

				// finish resize operation
				if (ColorBlockResized != null)
				{
					var eventArgs = new ColorBlockResizedEventArgs(mCurrentCursorPos_ColorBlock, mCurrentCursorPos_RowPos)
					{
						CancelResize = false
					};
					ColorBlockResized(this, eventArgs);

					if (eventArgs.CancelResize)
					{
						// cancel resize operation (resize to original size)
						if (mCurrentCursorPos_ColorBlock.AdjustColspan(mCurrentCursorPos_ResizeZone, mResizing_OriginalCol))
						{
							// redraw
							mOverlaidGrid.InvalidateRows(new int[] { mCurrentCursorPos_RowPos });
						}
					}
				}
			}
		}

		void mOverlaidControl_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent("DragContext", true))
			{
				e.Effect = DragDropEffects.Move | DragDropEffects.Scroll;

				DropTargetHelper.DragEnter(mOverlaidControl, e.Data, Cursor.Position, e.Effect);
			}
		}

		void mOverlaidControl_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent("DragContext", true))
			{
				DragDropEffects dropEffect = DragDropEffects.None;

				// check if cursor is above droppable cell
				var clientCords = mOverlaidControl.PointToClient(new Point(e.X, e.Y));
				var colPos = mOverlaidGrid.ColPosFromPoint(clientCords);
				var rowPos = mOverlaidGrid.RowPosFromPoint(clientCords);
				if (colPos != int.MinValue && rowPos != int.MinValue)
				{
					if (mOverlayArea_FirstColPos <= colPos &&
						mOverlayArea_LastColPos >= colPos &&
						mOverlaidGrid.CanDropColorBlock(mDragging_ColorBlock, rowPos, colPos))
					{
						dropEffect = DragDropEffects.Move | DragDropEffects.Scroll;
					}
				}
				e.Effect = dropEffect;

				DropTargetHelper.DragOver(Cursor.Position, e.Effect);
			}
		}

		void mOverlaidControl_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent("DragContext", true))
			{
				DropTargetHelper.Drop(e.Data, Cursor.Position, e.Effect);

				// color block moved
				var clientCords = mOverlaidControl.PointToClient(new Point(e.X, e.Y));

				var newColPos = mOverlaidGrid.ColPosFromPoint(clientCords);
				var newRowPos = mOverlaidGrid.RowPosFromPoint(clientCords);

				// was block moved to new position?
				if (newColPos != int.MinValue &&
					newRowPos != int.MinValue &&
					mDragging_ColorBlock != null &&
					newColPos >= mOverlayArea_FirstColPos &&
					(newRowPos != mDragging_OriginalRow || newColPos != mDragging_ColorBlock.ColspanStart)
					)
				{
					MoveColorBlock(mDragging_ColorBlock, newRowPos, newColPos);
					InitResizeGrips(mDragging_ColorBlock);

					// add left border flag, after drop it always begins inside displayed area
					if (!mDragging_ColorBlock.AllowedRezizeBorders.HasFlag(ColorBlock.ResizeBorder.Left))
					{
						mDragging_ColorBlock.AllowedRezizeBorders |= ColorBlock.ResizeBorder.Left;
					}

					// redraw
					mOverlaidGrid.InvalidateRows(new int[] { newRowPos, mDragging_OriginalRow });

					// raise block moved event
					if (ColorBlockMoved != null)
					{
						var eventArgs = new ColorBlockMovedEventArgs(
							mDragging_ColorBlock,
							mDragging_OriginalRow,
							mDragging_OriginalCol,
							newRowPos)
						{
							CancelMove = false
						};

						ColorBlockMoved(this, eventArgs);

						if (eventArgs.CancelMove)
						{ 
							// move back
							MoveColorBlock(mDragging_ColorBlock, mDragging_OriginalRow, mDragging_OriginalCol, mDragging_OriginalColSpan);
							InitResizeGrips(mDragging_ColorBlock);

							// redraw
							mOverlaidGrid.InvalidateRows(new int[] { newRowPos, mDragging_OriginalRow });
						}
					}
				}
			}
		}

		void mOverlaidControl_DragLeave(object sender, EventArgs e)
		{
			DropTargetHelper.DragLeave();
		}

		#endregion
	}
}
