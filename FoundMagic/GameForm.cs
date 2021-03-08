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
			Instance = this;
			InitializeComponent();
		}

		private static readonly Brush fogBrush = new SolidBrush(Color.FromArgb(128, 16, 16, 16));

		private void GameForm_Paint(object sender, PaintEventArgs e)
		{
			// get some globals
			Floor floor = Floor.Current;
			Hero hero = Hero.Instance;

			// find out how big our glyphs are going to be
			int glyphSize = Math.Min(Width / (floor.Width + 1), Height / (floor.Height + 1));

			// get our graphics context etc
			Graphics g = e.Graphics;
			Font font = new Font("Consolas", glyphSize / 2);

			// draw the map
			for (int x = 0; x < floor.Width; x++)
			{
				for (int y = 0; y < floor.Height; y++)
				{
					Point p = new(x * glyphSize, y * glyphSize);
					Tile tile = floor.Tiles[x, y];
					if (tile.IsExplored)
					{
						// draw tile
						g.DrawString(tile.Glyph.ToString(), font, new SolidBrush(tile.Color), p);
					}
					if (!tile.IsInFov)
					{
						// draw some translucent fog
						g.FillRectangle(fogBrush, p.X, p.Y, glyphSize, glyphSize);
					}
				}
			}

			// draw the log
			var now = DateTime.Now;
			double lastHowHigh = double.MinValue;
			foreach (var entry in Logger.List())
			{
				// compute some stuff to see where to draw
				var age = now - entry.Timestamp;
				var ageRatio = (double)age.Ticks / (double)Logger.Duration.Ticks;
				var height = Height - glyphSize; // let the last message still be visible
				var howHigh = height - height * ageRatio;

				// don't let messages overlap
				if (howHigh - lastHowHigh < glyphSize)
					howHigh = lastHowHigh + glyphSize;
				lastHowHigh = howHigh;

				// draw it
				g.DrawString(entry.Message, font, new SolidBrush(entry.Color), 0f, (float)howHigh);
			}
		}
		private void GameForm_SizeChanged(object sender, EventArgs e)
		{
			// repaint the form
			Invalidate();
		}

		private void GameForm_KeyDown(object sender, KeyEventArgs e)
		{
			// note a key press
			Keyboard.Press(e.KeyCode);

			// let the hero move once, and monsters during that duration
			Floor.Current.ProcessTime(Hero.Instance.Timer + Hero.Instance.GetActionTime());

			Invalidate();
		}

		private void GameForm_KeyUp(object sender, KeyEventArgs e)
		{
			// note a key release
			Keyboard.Release(e.KeyCode);
		}

		public static GameForm? Instance { get; private set; }

		private void logTimer_Tick(object sender, EventArgs e)
		{
			Invalidate();
		}
	}
}
