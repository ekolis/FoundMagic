using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic
{
	public record Terrain(string Name, char Glyph, Color Color, bool IsTransparent, bool IsWalkable)
	{
		public static readonly Terrain Floor = new("floor", '.', Color.Brown, true, true);
		public static readonly Terrain Water = new("water", '~', Color.Blue, true, false);
		public static readonly Terrain Tree = new("tree", '+', Color.Green, false, true);
		public static readonly Terrain Wall = new("wall", '#', Color.White, false, false);

		public static IEnumerable<Terrain> All { get; } = new Terrain[] { Floor, Water, Tree, Wall };
	}
}
