
namespace FoundMagic.UI
{
	partial class GameForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.logTimer = new System.Windows.Forms.Timer(this.components);
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// logTimer
			// 
			this.logTimer.Enabled = true;
			this.logTimer.Interval = 10;
			this.logTimer.Tick += new System.EventHandler(this.logTimer_Tick);
			// 
			// GameForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(1256, 656);
			this.DoubleBuffered = true;
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "GameForm";
			this.Text = "Found Magic 🧙🏼‍";
			this.SizeChanged += new System.EventHandler(this.GameForm_SizeChanged);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.GameForm_Paint);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameForm_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GameForm_KeyUp);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GameForm_MouseMove);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer logTimer;
		private System.Windows.Forms.ToolTip toolTip;
	}
}

