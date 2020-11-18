using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace GridOverlay
{
	/// <summary>
	/// Represents colored rounded rectangle with text inside.
	/// </summary>
	public class ColorBlock
	{
		#region Enum

		[Flags]
		public enum ResizeBorder
		{
			None = 0x0,
			Left = 0x1,
			Right = 0x2,
		}

		#endregion

		#region Constants

		private static int sCornerRoundRadius = 5;
		private static float sSelectedBorderWidth = 1.5f;
		private static int sResizeGripZoneWidth = 4;
		private static int sBorderTrimWidth = 8;
		private static int sBorderTrimHeight = 6;

		private static StringFormat sNameStringFormat = new StringFormat()
		{
			Alignment = StringAlignment.Near,
			LineAlignment = StringAlignment.Center
		};

		#endregion

		#region Fields

		private Rectangle mLastRenderedBounds;
		private Font mLastRenderedTextFont;
		private Color mDragColor; // set in 'Color' property setter

		private Color mColor;
		public List<int> mColspan = new List<int>();

		#endregion

		#region Constructor

		public ColorBlock(
			Guid id, 
			Guid rowId, 
			int colspanStart, 
			int colspanCount, 
			Color color)
		{
			this.Id = id;
			this.RowId = rowId;
			this.ColspanStart = colspanStart;
			this.ColspanCount = colspanCount;
			this.TotalColumnCount = colspanCount;
			this.Color = color;
			this.AllowedRezizeBorders = ResizeBorder.None;
			this.IsImageRendered = false;
			this.Image = null;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Unique color block id.
		/// </summary>
		public Guid Id
		{ get; set; }

		/// <summary>
		/// Text rendered in block.
		/// </summary>
		public string Text
		{ get; set; }

		/// <summary>
		/// Indicates whether text will be rendered.
		/// </summary>
		public bool IsTextRendered
		{ get; set; }

		/// <summary>
		/// Unique id of row that this color block belongs to.
		/// </summary>
		public Guid RowId
		{ get; set; }

		/// <summary>
		/// First index of colspan.
		/// </summary>
		public int ColspanStart
		{ get; set; }

		/// <summary>
		/// Length of color block.
		/// </summary>
		public int ColspanCount
		{ get; set; }

		/// <summary>
		/// Color of color block.
		/// </summary>
		public Color Color
		{
			get
			{
				return mColor;
			}
			set
			{
				if (value != null)
				{
					// make color slightly transparent
					mColor = Color.FromArgb(175, value.R, value.G, value.B);
					mDragColor = Color.FromArgb(255, value.R, value.G, value.B);
				}
			}
		}

		/// <summary>
		/// Total number of columns required to display color block, irrespective of currently used.
		/// </summary>
		public int TotalColumnCount
		{ get; set; }

		public bool IsSelected
		{ get; set; }

		/// <summary>
		/// If specified it is not possible to select/move/extend color block.
		/// </summary>
		public bool ReadOnly
		{ get; set; }

		/// <summary>
		/// Used determine where to draw resize markers.
		/// </summary>
		public ResizeBorder AllowedRezizeBorders
		{ get; set; }

		public object Tag
		{ get; set; }

		/// <summary>
		/// Image that is rendered in square area at the beginning of color block.
		/// </summary>
		public Image Image
		{ get; set; }

		public bool IsImageRendered
		{ get; set; }

		#endregion

		#region Private Methods

		/// <summary>
		/// http://stackoverflow.com/questions/1855884/determine-font-color-based-on-background-color
		/// </summary>
		private Color GetContrastColor(Color color)
		{
			int d = 0;

			// Counting the perceptive luminance - human eye favors green color... 
			double a = 1 - (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

			if (a < 0.5)
				d = 0; // bright colors - black font
			else
				d = 255; // dark colors - white font

			return Color.FromArgb(d, d, d);
		}

		/// <summary>
		/// Creates rounded rectangle for rendering + trims original bounds according to visible part of color block.
		/// </summary>
		private GraphicsPath CalculateColorBlockBounds(ref Rectangle gridBounds)
		{
			gridBounds.Height -= sBorderTrimHeight;
			gridBounds.Y += sBorderTrimHeight / 2;

			GraphicsPath result;
			if (AllowedRezizeBorders.HasFlag(ResizeBorder.Left) && AllowedRezizeBorders.HasFlag(ResizeBorder.Right))
			{
				// whole block in 
				gridBounds.Width -= sBorderTrimWidth;
				gridBounds.X += sBorderTrimWidth / 2;
				result = RoundedRectangle.Create(gridBounds, sCornerRoundRadius, RoundedRectangle.RectangleCorners.All);
			}
			else if (AllowedRezizeBorders.HasFlag(ResizeBorder.Left))
			{
				// right side trimmed
				gridBounds.Width -= sBorderTrimWidth / 2;
				gridBounds.X += sBorderTrimWidth / 4;
				result = RoundedRectangle.Create(gridBounds, sCornerRoundRadius, RoundedRectangle.RectangleCorners.BottomLeft | RoundedRectangle.RectangleCorners.TopLeft);
			}
			else if (AllowedRezizeBorders.HasFlag(ResizeBorder.Right))
			{
				// left side trimmed
				gridBounds.Width -= sBorderTrimWidth / 2;
				result = RoundedRectangle.Create(gridBounds, sCornerRoundRadius, RoundedRectangle.RectangleCorners.BottomRight | RoundedRectangle.RectangleCorners.TopRight);
			}
			else
			{
				// both sides trimmed
				result = RoundedRectangle.Create(gridBounds, sCornerRoundRadius, RoundedRectangle.RectangleCorners.None);
			}

			return result;
		}

		private void RenderColorBlock(Graphics g, GraphicsPath boundaryPath, Rectangle boundaryRect, Font blockTextFont)
		{
			using (var brush = new SolidBrush(Color))
			{
				g.FillPath(brush, boundaryPath);
			}

			int beginRenderX = boundaryRect.X + 3;
			if (IsImageRendered && Image != null)
			{ 
				var imageSize = boundaryRect.Height - 4;
				var imageRectF = new RectangleF(beginRenderX, boundaryRect.Y + 2, imageSize, imageSize);
				g.DrawImage(Image, imageRectF);

				beginRenderX = beginRenderX + imageSize + 3;
			}
			if (IsTextRendered)
			{
				var stringRectF = new RectangleF(beginRenderX, boundaryRect.Y, boundaryRect.Width - 3, boundaryRect.Height - 1);
				using (var brush = new SolidBrush(GetContrastColor(Color)))
				{
					g.DrawString(
						Text,
						blockTextFont,
						brush,
						stringRectF,
						ColorBlock.sNameStringFormat);
				}
			}

			if (IsSelected)
			{
				using (var pen = new Pen(Color.Black, sSelectedBorderWidth))
				{
					pen.Alignment = PenAlignment.Inset;
					g.DrawPath(pen, boundaryPath);
				}

				// mini resize grip markers
				float middleY = boundaryRect.Location.Y + (float)boundaryRect.Height / 2;
				var miniRect = new RectangleF(0, middleY - 2.5f, 5f, 5f);
				using (var brush = new SolidBrush((Color.White)))
				using (var pen = new Pen(Color.Black, 1))
				{
					pen.Alignment = PenAlignment.Inset;

					// left
					if (AllowedRezizeBorders.HasFlag(ResizeBorder.Left))
					{
						miniRect.X = boundaryRect.Left;
						g.FillRectangle(brush, miniRect);
						g.DrawRectangle(pen, miniRect.Location.X, miniRect.Location.Y, miniRect.Width, miniRect.Height);
					}

					// right
					if (AllowedRezizeBorders.HasFlag(ResizeBorder.Right))
					{
						miniRect.X = boundaryRect.Right - miniRect.Width;
						g.FillRectangle(brush, miniRect);
						g.DrawRectangle(pen, miniRect.Location.X, miniRect.Location.Y, miniRect.Width, miniRect.Height);
					}
				}
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Determines whether cursor is in resize zone, and if, which it is.
		/// </summary>
		public ResizeBorder IsInRezizeZone(Point position)
		{
			var result = ResizeBorder.None;

			if (!ReadOnly)
			{
				if (position.X < mLastRenderedBounds.X + sBorderTrimWidth/2 + sResizeGripZoneWidth)
				{
					result = ResizeBorder.Left;
				}
				else if (position.X > mLastRenderedBounds.Right - sBorderTrimWidth / 2 - sResizeGripZoneWidth)
				{
					result = ResizeBorder.Right;
				}

				if (!AllowedRezizeBorders.HasFlag(result))
				{
					result = ResizeBorder.None;
				}
			}

			return result;
		}

		/// <summary>
		/// Detects whether there would be change if adjusting colspan by given parameters.
		/// </summary>
		public bool DetectColspanChange(ResizeBorder direction, int newBorderColPos)
		{
			bool result = false;

			if (direction == ResizeBorder.Left)
			{
				// can't shrink below 1 column
				var resizeToColumnPos = newBorderColPos > ColspanStart + ColspanCount - 1 ? ColspanStart + ColspanCount - 1 : newBorderColPos;

				// is there any change?
				if (resizeToColumnPos != ColspanStart)
				{
					result = true;
				}
			}
			else if (direction == ResizeBorder.Right)
			{
				// can't shrink below 1 column
				var resizeToColumnPos = newBorderColPos < ColspanStart ? ColspanStart : newBorderColPos;

				// is there any change?
				if (resizeToColumnPos != ColspanStart + ColspanCount - 1)
				{
					result = true;
				}
			}

			return result;
		}

		/// <summary>
		/// Resizes specified border of color block by adjusting its colspan.
		/// </summary>
		/// <returns>
		/// Flag whether colspan structure changed. 
		/// </returns>
		public bool AdjustColspan(ResizeBorder direction, int newBorderColPos)
		{
			bool colspanChanged = false;

			var initialColspanCount = ColspanCount;
			if (direction == ResizeBorder.Left)
			{
				// can't shrink below 1 column
				var resizeToColumnPos = newBorderColPos > ColspanStart + ColspanCount - 1 ? ColspanStart + ColspanCount - 1 : newBorderColPos;

				// is there any change?
				if (resizeToColumnPos != ColspanStart)
				{
					// resize
					ColspanCount += ColspanStart - resizeToColumnPos;
					ColspanStart = resizeToColumnPos;
					colspanChanged = true;
				}
			}
			else if (direction == ResizeBorder.Right)
			{
				// can't shrink below 1 column
				var resizeToColumnPos = newBorderColPos < ColspanStart ? ColspanStart : newBorderColPos;

				// is there any change?
				if (resizeToColumnPos != ColspanStart + ColspanCount - 1)
				{
					// resize
					var lastColumn = ColspanStart + ColspanCount - 1;
					ColspanCount += resizeToColumnPos - lastColumn;
					colspanChanged = true;
				}
			}

			// add difference (if any) to event length
			if (colspanChanged)
			{
				TotalColumnCount += ColspanCount - initialColspanCount;
			}

			return colspanChanged;
		}

		/// <summary>
		/// Renders color block using specified parameters. 
		/// When 'gridPaintRender' is true it also stores rendering properties for later use in other functions.
		/// </summary>
		public void RenderColorBlock(
			Graphics g,
			Rectangle bounds,
			Font blockTextFont)
		{
			mLastRenderedBounds = new Rectangle(bounds.Location, bounds.Size);
			mLastRenderedTextFont = blockTextFont;

			GraphicsPath roundedRect = CalculateColorBlockBounds(ref bounds);
			RenderColorBlock(g, roundedRect, bounds, blockTextFont);
		}

		/// <summary>
		/// Creates drag image by rendering color block using latest render parameters.
		/// </summary>
		public Bitmap GetDragImage(int maxColSpan)
		{
			// temporarily alter rendering properties to render into image
			var original_gridColor = Color;
			Color = mDragColor;

			var original_IsSelected = IsSelected;
			IsSelected = false;

			var leftBorderAdded = false;
			if (!AllowedRezizeBorders.HasFlag(ResizeBorder.Left))
			{
				AllowedRezizeBorders |=  ResizeBorder.Left;
				leftBorderAdded = true;
			}

			// draw right border if size of block is less/equal then max
			var rightBorderAdded = false;
			if (TotalColumnCount <= maxColSpan)
			{
				if (!AllowedRezizeBorders.HasFlag(ResizeBorder.Right))
				{
					AllowedRezizeBorders |= ResizeBorder.Right;
					rightBorderAdded = true;
				}
			}

			// estimate grid bounds (render full size color block even when on border)
			float averageCellWidth = ((float)mLastRenderedBounds.Width) / (float)ColspanCount;
			var colSpan = TotalColumnCount > maxColSpan ? maxColSpan : TotalColumnCount;
			int estimatedWidth = (int)(colSpan * averageCellWidth);
			var bounds = new Rectangle(new Point(0, 0), new Size(estimatedWidth, mLastRenderedBounds.Height));

			// calculate color block bounds 
			var boundaryPath = CalculateColorBlockBounds(ref bounds);

			// create image
			Bitmap result = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

			using (Graphics g = Graphics.FromImage(result))
			{
				g.Clear(Color.Magenta);

				// set offset as we need to draw from 0,0 onto bitmap
				g.TranslateTransform(-bounds.X, -bounds.Y);
				
				using (var brush = new SolidBrush(Color.White))
				{
					g.FillPath(brush, boundaryPath);
				}
				RenderColorBlock(g, boundaryPath, bounds, mLastRenderedTextFont);
			}

			// restore original rendering properties
			Color = original_gridColor;
			IsSelected = original_IsSelected;
			if (leftBorderAdded)
			{
				AllowedRezizeBorders &= ~ResizeBorder.Left;
			}
			if (rightBorderAdded)
			{
				AllowedRezizeBorders &= ~ResizeBorder.Right;
			}

			return result;
		}

		#endregion
	}
}
