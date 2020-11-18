using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GridOverlay;

namespace ColorBlockGrid
{
	class DataGridViewEx : DataGridView, IOverlaidGrid
	{
		private GridOverlayRenderer mGridOverlayRenderer;

		public DataGridViewEx()
			: base()
		{
			mGridOverlayRenderer = new GridOverlayRenderer(this, this);
		}

		public GridOverlayRenderer GridOverlayRenderer
		{
			get
			{ 
				return mGridOverlayRenderer; 
			}
		}

		public void CleanUp()
		{
			mGridOverlayRenderer.CleanUp();
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint(e);

			// get visible rows
			if (FirstDisplayedCell != null)
			{
				var visibleRowsCount = DisplayedRowCount(true);
				var firstDisplayedRowIndex = FirstDisplayedCell.RowIndex;
				var lastDisplayedRowIndex = (firstDisplayedRowIndex + visibleRowsCount) - 1;

				// draw overlay
				mGridOverlayRenderer.Render(e, firstDisplayedRowIndex, lastDisplayedRowIndex);
			}
		}

		#region IOverlaidGrid

		public Guid GetRowId(int rowPos)
		{
			Guid result = Guid.Empty;

			var ds = this.DataSource as List<DataSourceObject>;
			if (ds != null)
			{
				if (rowPos >= 0 && rowPos < ds.Count)
				{
					result = ds[rowPos].Id;
				}
			}

			return result;
		}

		public Rectangle GetCellBoundaryRect(int rowPos, int colPos)
		{
			// we need to return cell rect even if it is hidden by
			// frozen columns (scrolled out) to be able to clip color block according to scroll position
			// GetCellDisplayRectangle will return empty rect for not visible cells (scrolled out)

			Rectangle cellRect = Rectangle.Empty;
			if (FirstDisplayedScrollingColumnIndex >= 0)
			{
				// as a base use rect of cell on border of scroll
				cellRect = GetCellDisplayRectangle(FirstDisplayedScrollingColumnIndex, rowPos, false);
				cellRect.X -= FirstDisplayedScrollingColumnHiddenWidth;

				if (colPos < FirstDisplayedScrollingColumnIndex)
				{
					// calculate x coordinate of cell (possibly invisible)
					for (int i = colPos; i < FirstDisplayedScrollingColumnIndex; i++)
					{
						cellRect.X -= Columns[i].Width;
					}

					// use column width
					cellRect.Width = Columns[colPos].Width;
				}
				else if (colPos == FirstDisplayedScrollingColumnIndex)
				{
					cellRect.Width += FirstDisplayedScrollingColumnHiddenWidth;
				}
				else if (colPos > FirstDisplayedScrollingColumnIndex)
				{
					// calculate x coordinate of cell (possibly invisible)
					for (int i = FirstDisplayedScrollingColumnIndex; i < colPos; i++)
					{
						cellRect.X += Columns[i].Width;
					}

					// use column width
					cellRect.Width = Columns[colPos].Width;
				}
			}

			return cellRect;
		}

		public int ColPosFromPoint(System.Drawing.Point location)
		{
			int result = int.MinValue;

			var hitTestInfo = HitTest(location.X, location.Y);
			if (hitTestInfo.ColumnIndex >= 0)
			{
				result = hitTestInfo.ColumnIndex;
			}

			return result;
		}

		public int RowPosFromPoint(System.Drawing.Point location)
		{
			int result = int.MinValue;

			var hitTestInfo = HitTest(location.X, location.Y);
			if (hitTestInfo.RowIndex >= 0)
			{
				result = hitTestInfo.RowIndex;
			}

			return result;
		}

		public int GetLeftScrollBoundary()
		{
			int result = 0;

			// get right edge of right-most visible frozen column
			for (int i = 0; i < ColumnCount; i++)
			{
				var col = Columns[i];
				if (col.Frozen && col.Visible)
				{
					var bounds = GetColumnDisplayRectangle(i, true);

					if (result < bounds.Right)
					{
						result = bounds.Right;
					}
				}
			}

			return result;
		}

		public bool CanDropColorBlock(ColorBlock colorBlock, int rowPos, int colPos)
		{
			if (colPos != 5)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool CanResizeColorBlock(ColorBlock colorBlock, ColorBlock.ResizeBorder border, int newBorderColPos, ref int? correctedBorderCol)
		{
			bool result = true;

			if (colorBlock.Tag is string && (string)colorBlock.Tag == "mark")
			{ 
				// make sure size is at least 3
				if (border == ColorBlock.ResizeBorder.Left)
				{
					if (newBorderColPos > colorBlock.ColspanStart + colorBlock.TotalColumnCount - 3)
					{
						correctedBorderCol = colorBlock.ColspanStart + colorBlock.TotalColumnCount - 3;
					}
				}
				else if (border == ColorBlock.ResizeBorder.Right)
				{
					if (newBorderColPos < colorBlock.ColspanStart + 2)
					{
						correctedBorderCol = colorBlock.ColspanStart + 2;
					}
				}
			}

			return result;
		}

		public void InvalidateRows(int[] rowPositions)
		{
			foreach (var rowPos in rowPositions)
			{
				InvalidateRow(rowPos);
			}
		}

		#endregion
	}
}
