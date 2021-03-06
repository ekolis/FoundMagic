using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RogueSharp;
using Point = System.Drawing.Point;

namespace FoundMagic
{
	public partial class GameForm : Form
	{
		public GameForm()
		{
			InitializeComponent();
		}

		private void GameForm_Paint(object sender, PaintEventArgs e)
		{
			// get our floor
			Floor floor = World.Instance.CurrentFloor;

			// find out how big our glyphs are going to be
			int glyphSize = Math.Min(Width / (floor.Width + 1), Height / (floor.Height + 1));

			// get our graphics context etc
			Graphics g = e.Graphics;
			Font font = new Font("Consolas", glyphSize / 2);
			Brush brush = Brushes.White;

			// draw on it
			for (int x = 0; x < floor.Width; x++)
			{
				for (int y = 0; y < floor.Height; y++)
				{
					Point p = new(x * glyphSize, y * glyphSize);
					ICell cell = floor.GetCell(x, y);
					char glyph;
					if (cell.IsWalkable && cell.IsTransparent)
						glyph = '.';
					else if (cell.IsWalkable)
						glyph = '+';
					else if (cell.IsTransparent)
						glyph = '~';
					else
						glyph = '#';
					g.DrawString(glyph.ToString(), font, brush, p);
				}
			}
		}

		private void GameForm_SizeChanged(object sender, EventArgs e)
		{
			// repaint the form
			Invalidate();
		}
	}
}
