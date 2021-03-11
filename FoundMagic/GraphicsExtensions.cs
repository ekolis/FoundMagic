using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundMagic.Mapping;
using FoundMagic.Creatures;

namespace FoundMagic
{
	/// <summary>
	/// Extension methods for drawing graphics.
	/// </summary>
	public static class GraphicsExtensions
	{
		public static void DrawLogarithmicBar(this Graphics g, double value, double max, Brush brush, int x, int y, int scale)
		{
			// draw skinny bar to show max
			var widthMax = Math.Log2(max);
			g.FillRectangle(brush, x, y + scale * 2 / 5, (int)(widthMax * scale) + 1, scale / 5);

			// draw fat bar to show value
			// this bar is not logarithmic, that would look weird, rather it's proportional to the full size bar
			var widthValue = widthMax * value / max;
			g.FillRectangle(brush, x, y + scale / 4, (int)(widthValue * scale) + 1, scale / 2);
		}

		public static void DrawSpellcastingInterface(this Graphics g, Floor floor, Hero hero, Font font, int glyphSize)
		{
			int line = 0;
			g.DrawString("You know the following magic words:", font, Brushes.White, 0, line++ * glyphSize);
			foreach (var element in hero.Elements)
			{
				var brush = new SolidBrush(element.Color);
				g.DrawString($"{element.GetType().Name} ", font, brush , 0, line++ * glyphSize);
				g.DrawString($"  {element.Word}: {element.EffectDescription} ({element.BaseManaCost} MP)", font, brush, 0, line++ * glyphSize);
			}
			g.DrawString("Start typing a magic word to cast a spell!", font, Brushes.White, 0, line++ * glyphSize);
			g.DrawString("Accurate typing makes the spell more powerful, but slow typing increases the mana cost!", font, Brushes.White, 0, line++ * glyphSize);
		}

		public static void DrawMap(this Graphics g, Floor floor, Font font, int glyphSize, Brush fogBrush)
		{
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
					// TODO: draw tiles affected by spells
				}
			}
		}

		public static void DrawLog(this Graphics g, Font font, int glyphSize, int gfxHeight)
		{
			var now = DateTime.Now;
			double lastHowHigh = double.MinValue;
			foreach (var entry in Logger.List())
			{
				// compute some stuff to see where to draw
				var age = now - entry.Timestamp;
				var ageRatio = (double)age.Ticks / (double)Logger.Duration.Ticks;
				var height = gfxHeight - glyphSize; // let the last message still be visible
				var howHigh = height - height * ageRatio;

				// don't let messages overlap
				if (howHigh - lastHowHigh < glyphSize)
					howHigh = lastHowHigh + glyphSize;
				lastHowHigh = howHigh;

				// also need an alpha value
				int alpha = 255 - (int)(255 * ageRatio);

				// draw it
				g.DrawString(entry.Message, font, new SolidBrush(Color.FromArgb(alpha, entry.Color)), 0f, (float)howHigh);
			}
		}

		public static void DrawBars(this Graphics g, Hero hero, Font font, int glyphSize)
		{
			// draw the HP/MP
			g.FillRectangle(Brushes.Black, 0, 0, 10 * glyphSize, 4 * glyphSize);
			g.DrawString($"H: {hero.Hitpoints} / {hero.MaxHitpoints}", font, Brushes.Red, 0, 0 * glyphSize);
			g.DrawLogarithmicBar(hero.Hitpoints, hero.MaxHitpoints, Brushes.Red, 0, 1 * glyphSize, glyphSize);
			g.DrawString($"M: {hero.Mana} / {hero.MaxMana}", font, Brushes.Blue, 0, 2 * glyphSize);
			g.DrawLogarithmicBar(hero.Mana, hero.MaxMana, Brushes.Blue, 0, 3 * glyphSize, glyphSize);
		}

		public static void DrawDeath(this Graphics g,  int gfxWidth, int gfxHeight)
		{
			if (Hero.Instance.DeathTimestamp != null)
			{
				var howLongAgo = DateTime.Now - Hero.Instance.DeathTimestamp.Value;
				Color deathColor;
				var ageRatio = (double)howLongAgo.Ticks / (double)Hero.Instance.DeathFadeTime.Ticks;
				int alphaRed = (int)(255 * ageRatio);
				int alphaBlack = (int)(255 * (ageRatio - 1));
				if (ageRatio < 1)
				{
					deathColor = Color.FromArgb(alphaRed, Color.Red);
				}
				else if (ageRatio < 2)
				{
					deathColor = Color.FromArgb(255, 255 - alphaBlack, 0, 0);
				}
				else
				{
					deathColor = Color.Black;
				}
				g.FillRectangle(new SolidBrush(deathColor), 0, 0, gfxWidth, gfxHeight);
			}
		}

		public static Color Average(this IEnumerable<Color> colors)
			=> Color.FromArgb((int)colors.Average(q => q.A), (int)colors.Average(q => q.R), (int)colors.Average(q => q.G), (int)colors.Average(q => q.B));
	}
}
