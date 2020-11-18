using System;

namespace GridOverlay
{
	public class ColorBlockResizedEventArgs : EventArgs
	{
		public ColorBlockResizedEventArgs(ColorBlock resizedColorBlock, int rowPos)
		{
			ResizedColorBlock = resizedColorBlock;
			RowPos = rowPos;
		}

		public bool CancelResize
		{ get; set; }

		public ColorBlock ResizedColorBlock
		{ get; private set; }

		public int RowPos
		{ get; private set; }
	}
}
