using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;

namespace FoundMagic
{
	/// <summary>
	/// A single tile which makes up a <see cref="Floor"/>.
	/// </summary>
	public class Tile
		: Cell
	{
		public Tile(int x, int y, Terrain terrain)
			: base(x, y, terrain.IsTransparent, terrain.IsWalkable, false, false)
		{
			Terrain = terrain;
		}
		
		/// <summary>
		/// Copies an <see cref="ICell"/> into a tile and picks an appropriate terrain.
		/// </summary>
		/// <param name="cell"></param>
		public Tile(ICell cell)
			: base(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, false, false)
		{
			Terrain = World.Instance.Rng.Pick(Terrain.All.Where(q => q.IsTransparent == IsTransparent && q.IsWalkable == IsWalkable));
		}

		public Terrain Terrain { get; }
	}
}
