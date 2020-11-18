using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GridOverlay;

namespace ColorBlockGrid
{
	public partial class Form1 : Form
	{
		private List<DataSourceObject> mDataSource;

		public Form1()
		{
			InitializeComponent();

			dataGridView1.AutoGenerateColumns = false;

			mDataSource = new List<DataSourceObject>
			{
				new DataSourceObject() {Id = Guid.NewGuid(), Label1 = "test1", Label2 = "test1"},
				new DataSourceObject() {Id = Guid.NewGuid(), Label1 = "test2", Label2 = "test2"},
				new DataSourceObject() {Id = Guid.NewGuid(), Label1 = "test3", Label2 = "test3", data1 = "Any", data2 = "cell", data3 = "data", data4 = "in", data5 = "underlying", data6 = "grid."},
				new DataSourceObject() {Id = Guid.NewGuid(), Label1 = "test4", Label2 = "test4"},
				new DataSourceObject() {Id = Guid.NewGuid(), Label1 = "test5", Label2 = "test5"},
				new DataSourceObject() {Id = Guid.NewGuid(), Label1 = "test6", Label2 = "test6"},
				new DataSourceObject() {Id = Guid.NewGuid(), Label1 = "test7", Label2 = "test7"},
				new DataSourceObject() {Id = Guid.NewGuid(), Label1 = "test8", Label2 = "test8"},
				new DataSourceObject() {Id = Guid.NewGuid(), Label1 = "test9", Label2 = "test9"},
			};

			dataGridView1.DataSource = mDataSource;
		}

		private bool mAscending = true;
		private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			// sort
			if (e.ColumnIndex == 0)
			{
				if (mAscending)
				{
					mDataSource = mDataSource.OrderBy(o => o.Label1).ToList();
					dataGridView1.DataSource = mDataSource;
					mAscending = false;
				}
				else
				{
					mDataSource = mDataSource.OrderByDescending(o => o.Label1).ToList();
					dataGridView1.DataSource = mDataSource;
					mAscending = true;
				}
			}
		}

		private void dataGridView1_SelectionChanged(object sender, EventArgs e)
		{
			dataGridView1.ClearSelection();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// create some color blocks
			List<ColorBlock> colorBlocks = new List<ColorBlock>();

			var colorBlock = new ColorBlock(Guid.NewGuid(),mDataSource[0].Id,3,2,Color.Red)
			{
				Text = mDataSource[0].Label1,
				IsTextRendered = true,
				Image = imageList.Images[0],
				IsImageRendered = true
			};
			colorBlocks.Add(colorBlock);

			colorBlock = new ColorBlock(Guid.NewGuid(), mDataSource[0].Id, 4, 2, Color.DarkBlue)
			{
				Text = "Overlap!",
				IsTextRendered = true,
				Image = imageList.Images[1],
				IsImageRendered = true
			};
			colorBlocks.Add(colorBlock);

			colorBlock = new ColorBlock(Guid.NewGuid(), mDataSource[0].Id, 7, 3, Color.Yellow)
			{
				Text = "This one can't have colspan smaller than 3!",
				IsTextRendered = true,
				Tag = "mark"
			};
			colorBlocks.Add(colorBlock);

			colorBlock = new ColorBlock(Guid.NewGuid(), mDataSource[2].Id, 4, 2, Color.LightBlue)
			{
				IsTextRendered = false
			};
			colorBlocks.Add(colorBlock);

			colorBlock = new ColorBlock(Guid.NewGuid(), mDataSource[2].Id, 8, 2, Color.Black)
			{
				IsTextRendered = false,
				Image = imageList.Images[1],
				IsImageRendered = true
			};
			colorBlocks.Add(colorBlock);

			colorBlock = new ColorBlock(Guid.NewGuid(), mDataSource[3].Id, 4, 3, Color.Aqua)
			{
				Text = "Text color is picked-up based on background block color.",
				IsTextRendered = true
			};
			colorBlocks.Add(colorBlock);

			colorBlock = new ColorBlock(Guid.NewGuid(), mDataSource[5].Id, 2, 2, Color.Green)
			{
				Text = "Partially in",
				IsTextRendered = true,
				TotalColumnCount = 5
			};
			colorBlocks.Add(colorBlock);

			colorBlock = new ColorBlock(Guid.NewGuid(), mDataSource[5].Id, 11, 3, Color.DarkRed)
			{
				Text = "Partially out",
				IsTextRendered = true,
			};
			colorBlocks.Add(colorBlock);

			colorBlock = new ColorBlock(Guid.NewGuid(), mDataSource[7].Id, 2, 10, Color.Blue)
			{
				Text = "2 columns bigger than available colspan!",
				IsTextRendered = true,
				TotalColumnCount = 12
			};
			colorBlocks.Add(colorBlock);

			// init color block overlay
			dataGridView1.GridOverlayRenderer.InitializeOverlay(colorBlocks, 2, dataGridView1.ColumnCount - 1);
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			dataGridView1.CleanUp();
		}
	}
}
