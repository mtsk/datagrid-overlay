using System;

namespace GridOverlay
{
	public class ColorBlockMovedEventArgs : EventArgs
	{
		public ColorBlockMovedEventArgs(
			ColorBlock movedColorBlock,
			int originalRowPosition,
			int originalColPos,
			int newRowPos)
		{
			MovedColorBlock = movedColorBlock;
			OriginalRowPos = originalRowPosition;
			OriginalColPos = originalColPos;
			NewRowPos = newRowPos;
		}

		public bool CancelMove
		{ get; set; }

		public ColorBlock MovedColorBlock
		{ get; private set; }

		public int NewRowPos
		{ get; private set; }

		public int OriginalRowPos
		{ get; private set; }

		public int OriginalColPos
		{ get; private set; }
	}
}
