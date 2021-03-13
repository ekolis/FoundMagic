using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundMagic.Mapping;
using FoundMagic.Creatures;
using FoundMagic.Magic;
using System.Collections.Immutable;

namespace FoundMagic.UI
{
	/// <summary>
	/// Extension methods for drawing graphics.
	/// </summary>
	public static class GraphicsExtensions
	{
		public static double DrawLogarithmicBar(this Graphics g, double value, double max, Brush brush, int x, int y, int scale)
		{
			// draw skinny bar to show max
			var widthMax = Math.Log2(max);
			g.FillRectangle(brush, x, y + scale * 2 / 5, (int)(widthMax * scale) + 1, scale / 5);

			// draw fat bar to show value
			// this bar is not logarithmic, that would look weird, rather it's proportional to the full size bar
			var widthValue = widthMax * value / max;
			g.FillRectangle(brush, x, y + scale / 4, (int)(widthValue * scale) + 1, scale / 2);

			return widthMax;
		}

		public static void DrawSpellcastingInterface(this Graphics g, Floor floor, Hero hero, Font font, int glyphSize)
		{
			int line = 0;
			g.DrawString("You know the following magic words:", font, Brushes.White, 0, line++ * glyphSize);
			foreach (var element in hero.Elements)
			{
				var brush = new SolidBrush(element.Color);
				g.DrawString($"{element.GetType().Name} at {Math.Round(element.Attunement * 100)}% attunement", font, brush, 0, line++ * glyphSize);
				if (element.CanBeCast)
					g.DrawString($"  {element.Word}: {element.GetEffectDescription(hero)} ({element.BaseManaCost} MP)", font, brush, 0, line++ * glyphSize);
				else
					g.DrawString($"  {Element.MinEssencesToCast - element.Essences} more essences required to cast", font, brush, 0, line++ * glyphSize);
				switch (element)
				{
					case Fire:
						g.DrawString($"  Elemental boost: +{Math.Round(hero.EssenceBoostSpellPower * 100)}% spell power", font, brush, 0, line++ * glyphSize);
						break;
					case Earth:
						g.DrawString($"  Elemental boost: +{hero.EssenceBoostMaxHitpoints} max HP", font, brush, 0, line++ * glyphSize);
						break;
					case Air:
						g.DrawString($"  Elemental boost: +{Math.Round(hero.EssenceBoostSpeed * 100)}% speed", font, brush, 0, line++ * glyphSize);
						break;
					case Water:
						g.DrawString($"  Elemental boost: +{hero.EssenceBoostMaxMana} max MP", font, brush, 0, line++ * glyphSize);
						break;
					case Light:
						g.DrawString($"  Elemental boost: +{hero.EssenceBoostRegeneration} HP/MP regen on stair climb", font, brush, 0, line++ * glyphSize);
						break;
					case Darkness:
						g.DrawString($"  Elemental boost: {Math.Round(hero.EssenceBoostCriticalHits * 100)}% melee/spell crit chance (3x strength)", font, brush, 0, line++ * glyphSize);
						break;
				}
				line++;
			}
			line++;
			g.DrawString("Start typing a magic word to cast a spell!", font, Brushes.White, 0, line++ * glyphSize);
			g.DrawString("Accurate typing makes the spell more powerful, but slow typing increases the mana cost!", font, Brushes.White, 0, line++ * glyphSize);
			g.DrawString("Or, kill a bunch of monsters of the appropriate element to power up your elemental attunement!", font, Brushes.White, 0, line++ * glyphSize);
		}

		public static void DrawMap(this Graphics g, Floor floor, Font font, int glyphSize, Brush fogBrush)
		{
			for (int x = 0; x < floor.Width; x++)
			{
				for (int y = 0; y < floor.Height; y++)
				{
					Point p = new(x * glyphSize, (y + BarsRows) * glyphSize);
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
				int alpha = 255 - (int)(255 * Math.Pow(ageRatio, 0.25));

				// draw it
				g.DrawString(entry.Message, font, new SolidBrush(Color.FromArgb(alpha, entry.Color)), 0f, (float)howHigh);
			}
		}

		public const int BarsRows = 2;

		public static void DrawBars(this Graphics g, Hero hero, Font font, int glyphSize)
		{
			// draw the HP
			var hpStr = $"H: {hero.Hitpoints} / {hero.MaxHitpoints}";
			g.DrawString(hpStr, font, Brushes.Red, 0, 0 * glyphSize);
			var hpBarWidth = g.DrawLogarithmicBar(hero.Hitpoints, hero.MaxHitpoints, Brushes.Red, 0, 1 * glyphSize, glyphSize);
			hpBarWidth = Math.Max(hpBarWidth, hpStr.Length * glyphSize);

			// draw the MP
			var mpStr = $"M: {hero.Mana} / {hero.MaxMana}";
			g.DrawString(mpStr, font, Brushes.Blue, (int)Math.Ceiling(hpBarWidth) + glyphSize, 0 * glyphSize);
			var mpBarWidth = g.DrawLogarithmicBar(hero.Mana, hero.MaxMana, Brushes.Blue, (int)Math.Ceiling(hpBarWidth) + glyphSize, 1 * glyphSize, glyphSize);
			mpBarWidth = Math.Max(mpBarWidth, mpStr.Length * glyphSize);

			if (World.Instance.IsEndgame)
			{
				g.DrawString($"T: {Math.Round(World.Instance.EndgameTimer)}", font, Brushes.Magenta, (int)Math.Ceiling(hpBarWidth) + (int)Math.Ceiling(mpBarWidth) + glyphSize, 0 * glyphSize);
			}
		}

		public static void DrawDeath(this Graphics g, int gfxWidth, int gfxHeight, Font font)
		{
			if (Hero.Instance.DeathTimestamp != null)
			{
				// color fade
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

				// draw GAME OVER
				float x = gfxWidth / 2;
				float y = gfxHeight / 2;
				var stringFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
				var maxTicks = Hero.Instance.DeathFadeTime.Ticks;
				var ticks = Math.Min(howLongAgo.Ticks, maxTicks);
				var lerp = ((float)ticks / (float)maxTicks);
				font = new Font(font.FontFamily, font.Size * 10 * lerp);
				g.DrawString("*GAME OVER*", font, new SolidBrush(Color.Magenta), x, y, stringFormat);
			}
		}

		public static void DrawVictory(this Graphics g, int gfxWidth, int gfxHeight, Font font)
		{
			if (World.Instance.IsWinner)
			{
				// color fade
				var howLongAgo = DateTime.Now - World.Instance.VictoryTimestamp.Value;
				Color winColor;
				var ageRatio = (double)howLongAgo.Ticks / (double)Hero.Instance.DeathFadeTime.Ticks; // TODO: make a separate variable for this
				int alphaWhite = (int)(255 * ageRatio);
				int alphaBlack = (int)(255 * (ageRatio - 1));
				if (ageRatio < 1)
				{
					winColor = Color.FromArgb(alphaWhite, Color.White);
				}
				else if (ageRatio < 2)
				{
					winColor = Color.FromArgb(255, 255 - alphaBlack, 255 - alphaBlack, 255 - alphaBlack);
				}
				else
				{
					winColor = Color.Black;
				}
				g.FillRectangle(new SolidBrush(winColor), 0, 0, gfxWidth, gfxHeight);

				// draw some text in a pretty pattern
				double angle = 0;
				var stringFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
				var gfxSize = Math.Min(gfxWidth, gfxHeight);
				var maxTicks = Hero.Instance.DeathFadeTime.Ticks;
				var ticks = Math.Min(howLongAgo.Ticks, maxTicks);
				var lerp = ((float)ticks / (float)maxTicks);
				var derp = 1f - lerp;
				font = new Font(font.FontFamily, font.Size * (5 - derp * 4));
				foreach (var element in Hero.Instance.Elements.OrderBy(q => World.Instance.Rng.Next(10) + AntiSeizureMagic(q, Hero.Instance.Elements, lerp)))
				{
					float x = (float)Math.Cos(angle) * derp * gfxSize + gfxWidth / 2;
					float y = (float)Math.Sin(angle) * derp * gfxSize + gfxHeight / 2;
					g.DrawString("*YOU WIN*", font, new SolidBrush(element.Color), x, y, stringFormat);
					angle += Math.PI / 3;
				}
			}
		}
		
		/// <summary>
		/// Make the victory screen less seizure inducing by slowing down the color shuffling as the text gets larger over time.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="elements"></param>
		/// <param name="lerp"></param>
		/// <returns></returns>
		private static float AntiSeizureMagic(Element element, IEnumerable<Element> elements, float lerp)
		{
			var idx = elements.ToImmutableList().IndexOf(element);
			var factor = Math.Sqrt(lerp) * 10f;
			return (float)(idx * factor);
		}

		public static Color Average(this IEnumerable<Color> colors)
			=> Color.FromArgb((int)colors.Average(q => q.A), (int)colors.Average(q => q.R), (int)colors.Average(q => q.G), (int)colors.Average(q => q.B));

		/// <summary>
		/// Gets the scale factor of a <see cref="Graphics"/> by comparing its DPI to the default value of 96.
		/// </summary>
		/// <param name="g"></param>
		/// <returns></returns>
		public static float ScaleFactor(this Graphics g)
			=> Math.Max(g.DpiX, g.DpiY) / 96;
	}
}
