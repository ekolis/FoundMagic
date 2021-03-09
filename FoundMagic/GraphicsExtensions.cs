using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
