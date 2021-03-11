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
using FoundMagic.Mapping;
using FoundMagic.Creatures;

namespace FoundMagic.UI
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
			Font font = new Font("Consolas", glyphSize / g.ScaleFactor());

			if (hero.IsCasting)
			{
				g.DrawSpellcastingInterface(floor, hero, font, glyphSize);
			}
			else
			{
				g.DrawMap(floor, font, glyphSize, fogBrush);
				g.DrawLog(font, glyphSize, Height);
				g.DrawBars(hero, font, glyphSize);
				// TODO: render current status effects and their durations?
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
			// find hero
			var h = Hero.Instance;

			// note a key press
			Keyboard.Press(e.KeyCode);

			if (Keyboard.ActionDirection is not null)
			{
				if (!h.IsCasting)
				{
					if (Keyboard.IsKeyPressed(Keys.ControlKey))
					{
						// begin spellcasting
						h.IsCasting = true;
					}
					else
					{
						// let the hero act once, and monsters during that duration
						Floor.Current.ProcessTime(h.Timer + h.GetActionTime(), false);

						// let the monsters act until the hero is ready again
						Floor.Current.ProcessTime(h.Timer, true);
					}
				}
			}

			if (h.Spell is not null)
			{
				// cast the spell and exit casting mode
				Floor.Current.ProcessTime(h.Timer + h.GetActionTime(), false);
				h.IsCasting = false;
				h.Spell = null;
				h.SpellWord = "";
				h.IsCasting = false;
				h.SpellTimestamp = null;

				// let the monsters act until the hero is ready again
				Floor.Current.ProcessTime(h.Timer, true);
			}

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
