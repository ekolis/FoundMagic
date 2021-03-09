using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoundMagic.Magic;
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

			if (hero.IsCasting)
			{
				g.DrawSpellcastingInterface(floor, hero, font, glyphSize);
			}
			else
			{
				g.DrawMap(floor, font, glyphSize, fogBrush);
				g.DrawLog(font, glyphSize, Height);
				g.DrawBars(hero, font, glyphSize);
				g.DrawDeath(Width, Height);
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

			if (Keyboard.ActionDirection is not null)
			{
				// let the hero act once, and monsters during that duration
				Floor.Current.ProcessTime(Hero.Instance.Timer + Hero.Instance.GetActionTime());

				Invalidate();
			}
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
