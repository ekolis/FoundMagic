using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using FoundMagic.Creatures;
using System.Runtime.CompilerServices;

namespace FoundMagic.Mapping
{
	/// <summary>
	/// A single tile which makes up a <see cref="Floor"/>.
	/// </summary>
	public class Tile
		: Cell
	{
		/// <summary>
		/// Creates a tile.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="terrain"></param>
		/// <param name="isInFov"></param>
		/// <param name="isExplored"></param>
		public Tile(int x, int y, Terrain terrain, bool isInFov = false, bool isExplored = false)
			: base(x, y, terrain.IsTransparent, terrain.IsWalkable, isInFov, isExplored)
		{
			Terrain = terrain;
			Creature = null;
		}
		
		/// <summary>
		/// Copies an <see cref="ICell"/> into a tile and picks an appropriate terrain.
		/// </summary>
		/// <param name="cell"></param>
		public Tile(ICell cell, double flipOpacityChance, bool isInFov = false, bool isExplored = false)
			: base(cell.X, cell.Y, World.Instance.Rng.Chance(flipOpacityChance) ? !cell.IsTransparent : cell.IsTransparent, cell.IsWalkable, isInFov, isExplored)
		{
			Terrain = World.Instance.Rng.Pick(Terrain.Basic.Where(q => q.IsTransparent == IsTransparent && q.IsWalkable == IsWalkable));
			Creature = null;
		}

		/// <summary>
		/// Copies from another tile.
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="isInFov"></param>
		/// <param name="isExplored"></param>
		public Tile(Tile tile, bool isInFov = false, bool isExplored = false)
			: base(tile.X, tile.Y, tile.IsTransparent, tile.IsWalkable, isInFov, isExplored)
		{
			Terrain = tile.Terrain;
			Creature = tile.Creature;
		}

		/// <summary>
		/// The terrain of this tile.
		/// </summary>
		public Terrain Terrain { get; set; }

		/// <summary>
		/// The creature which is currently located at this tile (null if there is none).
		/// </summary>
		public ICreature? Creature { get; set; }

		/// <summary>
		/// If there is a creature here and the player can see it, its glyph will be used; otherwise the terrain's glyph will be used.
		/// </summary>
		public char Glyph
			=> IsInFov ? Creature?.Glyph ?? Terrain.Glyph : Terrain.Glyph;

		/// <summary>
		/// If there is a creature here and the player can see it, its color will be used; otherwise the terrain's color will be used.
		/// </summary>
		public Color Color
			=> IsInFov ? Creature?.Color ?? Terrain.Color : Terrain.Color;

		/// <summary>
		/// Creates a tile like this one but it is in FOV and explored.
		/// </summary>
		/// <returns></returns>
		public Tile WithVisible()
			=> new Tile(this, true, true);

		/// <summary>
		/// Creates a tile like this one but it is out of FOV.
		/// </summary>
		/// <returns></returns>
		public Tile WithInvisible()
			=> new Tile(this, false, IsExplored);

		/// <summary>
		/// Creates a tile like this one but it is explored.
		/// </summary>
		/// <returns></returns>
		public Tile WithExplored()
			=> new Tile(this, IsInFov, true);
		
		/// <summary>
		/// Determines if this tile is orthogonally aligned to another tile (in the same row or column).
		/// </summary>
		/// <param name="target">The other tile.</param>
		/// <returns>The direction to the target, or null if not orthogonally aligned.</returns>
		public Direction? CheckOrthogonality(Tile target)
		{
			if (X == target.X)
			{
				if (Y == target.Y)
					return Direction.Stationary;
				else if (Y < target.Y)
					return Direction.South;
				else if (Y > target.Y)
					return Direction.North;
			}
			else if (Y == target.Y)
			{
				if (X == target.X)
					return Direction.Stationary;
				else if (X < target.X)
					return Direction.East;
				else if (X > target.X)
					return Direction.West;
			}

			return null;
		}
	}
}
