﻿using System;
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

			// draw on it
			for (int x = 0; x < floor.Width; x++)
			{
				for (int y = 0; y < floor.Height; y++)
				{
					Point p = new(x * glyphSize, y * glyphSize);
					Tile tile = floor.Tiles[x, y];
					g.DrawString(tile.Glyph.ToString(), font, new SolidBrush(tile.Color), p);
				}
			}
		}

		private void GameForm_SizeChanged(object sender, EventArgs e)
		{
			// repaint the form
			Invalidate();
		}

		private void GameForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (Floor.Current is null)
				return; // nothing to do

			// movement keys
			var dir = e.KeyCode switch
			{
				Keys.Up or Keys.D8 or Keys.W => Direction.North,
				Keys.Down or Keys.D2 or Keys.S => Direction.South,
				Keys.Left or Keys.D4 or Keys.A => Direction.West,
				Keys.Right or Keys.D6 or Keys.D => Direction.East,
				_ => Direction.Stationary
			};

			// move hero
			Floor.Current.Move(Hero.Instance, dir);

			// redraw map
			Invalidate();
		}
	}
}
