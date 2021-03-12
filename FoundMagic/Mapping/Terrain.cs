using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic.Mapping
{
	public record Terrain(string Name, char Glyph, Color Color, bool IsTransparent, bool IsWalkable)
	{
		public static readonly Terrain Floor = new("floor", '.', Color.Brown, true, true);
		public static readonly Terrain Water = new("water", '~', Color.Blue, true, false);
		public static readonly Terrain Tree = new("tree", '+', Color.Green, false, true);
		public static readonly Terrain Wall = new("wall", '#', Color.White, false, false);

		public static readonly Terrain StairsDown = new("stairs down", '>', Color.White, true, true);
		public static readonly Terrain StairsUp = new("stairs up", '<', Color.White, true, true);

		/// <summary>
		/// Basic terrains used in floor generation.
		/// </summary>
		public static IEnumerable<Terrain> Basic { get; } = new Terrain[] { Floor, Water, Tree, Wall };

		/// <summary>
		/// Special terrains with distinct purposes.
		/// </summary>
		public static IEnumerable<Terrain> Special { get; } = new Terrain[] { StairsDown, StairsUp };

		/// <summary>
		/// All terrains.
		/// </summary>
		public static IEnumerable<Terrain> All { get; } = Basic.Concat(Special);
	}
}
