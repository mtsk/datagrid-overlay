namespace ColorBlockGrid
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.dataGridView1 = new ColorBlockGrid.DataGridViewEx();
			this.label1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.label2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.clr10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "1");
			this.imageList.Images.SetKeyName(1, "2");
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowDrop = true;
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.label1,
            this.label2,
            this.clr1,
            this.clr2,
            this.clr3,
            this.clr4,
            this.clr5,
            this.clr6,
            this.clr7,
            this.clr8,
            this.clr9,
            this.clr10});
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(0, 0);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.Size = new System.Drawing.Size(800, 258);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_ColumnHeaderMouseClick);
			this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
			// 
			// label1
			// 
			this.label1.DataPropertyName = "Label1";
			this.label1.Frozen = true;
			this.label1.HeaderText = "label1";
			this.label1.Name = "label1";
			this.label1.ReadOnly = true;
			// 
			// label2
			// 
			this.label2.DataPropertyName = "Label2";
			this.label2.Frozen = true;
			this.label2.HeaderText = "label2";
			this.label2.Name = "label2";
			this.label2.ReadOnly = true;
			// 
			// clr1
			// 
			this.clr1.DataPropertyName = "data1";
			this.clr1.HeaderText = "1";
			this.clr1.Name = "clr1";
			this.clr1.ReadOnly = true;
			// 
			// clr2
			// 
			this.clr2.DataPropertyName = "data2";
			this.clr2.HeaderText = "2";
			this.clr2.Name = "clr2";
			this.clr2.ReadOnly = true;
			// 
			// clr3
			// 
			this.clr3.DataPropertyName = "data3";
			this.clr3.HeaderText = "3";
			this.clr3.Name = "clr3";
			this.clr3.ReadOnly = true;
			// 
			// clr4
			// 
			this.clr4.DataPropertyName = "data4";
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.clr4.DefaultCellStyle = dataGridViewCellStyle1;
			this.clr4.HeaderText = "Cannot drop to this column!";
			this.clr4.Name = "clr4";
			this.clr4.ReadOnly = true;
			// 
			// clr5
			// 
			this.clr5.DataPropertyName = "data5";
			this.clr5.HeaderText = "5";
			this.clr5.Name = "clr5";
			this.clr5.ReadOnly = true;
			// 
			// clr6
			// 
			this.clr6.DataPropertyName = "data6";
			this.clr6.HeaderText = "6";
			this.clr6.Name = "clr6";
			this.clr6.ReadOnly = true;
			// 
			// clr7
			// 
			this.clr7.DataPropertyName = "data7";
			this.clr7.HeaderText = "7";
			this.clr7.Name = "clr7";
			this.clr7.ReadOnly = true;
			// 
			// clr8
			// 
			this.clr8.DataPropertyName = "data8";
			this.clr8.HeaderText = "8";
			this.clr8.Name = "clr8";
			this.clr8.ReadOnly = true;
			// 
			// clr9
			// 
			this.clr9.DataPropertyName = "data9";
			this.clr9.HeaderText = "9";
			this.clr9.Name = "clr9";
			this.clr9.ReadOnly = true;
			// 
			// clr10
			// 
			this.clr10.DataPropertyName = "data10";
			this.clr10.HeaderText = "10";
			this.clr10.Name = "clr10";
			this.clr10.ReadOnly = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 258);
			this.Controls.Add(this.dataGridView1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DataGridViewEx dataGridView1;
		private System.Windows.Forms.DataGridViewTextBoxColumn label1;
		private System.Windows.Forms.DataGridViewTextBoxColumn label2;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr1;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr2;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr3;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr4;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr5;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr6;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr7;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr8;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr9;
		private System.Windows.Forms.DataGridViewTextBoxColumn clr10;
		private System.Windows.Forms.ImageList imageList;
	}
}

