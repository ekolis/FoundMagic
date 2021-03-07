using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;

namespace FoundMagic
{
	/// <summary>
	/// A single floor.
	/// </summary>
	/// <remarks>
	/// Everybody get down, get on the floor, everybody do the dinosaur! 🐱‍🐉
	/// </remarks>
	public class Floor
		: Map
	{
		public Floor()
			: base()
		{
			Tiles = new Tile[Width, Height];
		}

		public Floor(int width, int height)
			: base(width, height)
		{
			Tiles = new Tile[Width, Height];
		}

		/// <summary>
		/// Prepares a floor for playing the game.
		/// </summary>
		public void Setup()
		{
			// create tiles
			Tiles = new Tile[Width, Height];
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					Tiles[x, y] = new Tile(GetCell(x, y));
				}
			}

			// create monsters
			var places = Tiles.Cast<Tile>().Where(q => q.IsWalkable && q.Creature is null).ToList();
			const int monsterRarity = 10;
			var numMonsters = places.Count() / monsterRarity;
			for (int i = 0; i < numMonsters; i++)
			{
				if (!places.Any())
					break; // nowhere to place a monster

				var place = World.Instance.Rng.Pick(places);
				var monsterType = World.Instance.Rng.Pick(MonsterType.All);
				place.Creature = new Monster(monsterType);
			}
		}

		/// <summary>
		/// The floor that the hero is currently located on.
		/// </summary>
		public static Floor? Current
			=> World.Instance.CurrentFloor;

		/// <summary>
		/// The tiles on this floor.
		/// </summary>
		/// <remarks>
		/// Since there seems to be no way in RogueSharp to use custom cell classes in a <see cref="Map"/>, we need a duplicate array...
		/// </remarks>
		public Tile[,] Tiles { get; private set; }

		/// <summary>
		/// Gets the neighbor of a tile in a particular direction.
		/// </summary>
		/// <param name="origin">The original tile.</param>
		/// <param name="direction">The direction of travel.</param>
		/// <returns>The neighboring tile.</returns>
		public Tile? GetNeighbor(Tile origin, Direction direction)
		{
			var x = origin.X + direction.DX;
			var y = origin.Y + direction.DY;
			if (x < 0 || x >= Width || y < 0 || y >= Height)
				return null; // out of bounds
			return Tiles[x, y];
		}

		/// <summary>
		/// Finds the tile containing a particular creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">when this floor does not contain the creature.</exception>
		public Tile Find(ICreature creature)
			=> Tiles.Cast<Tile>().Single(q => q.Creature == creature);

		/// <summary>
		/// Tries to move a creature.
		/// </summary>
		/// <param name="creature">The creature to move.</param>
		/// <param name="direction">The direction in which to move the creature.</param>
		/// <returns>true if successful, otherwise false</returns>
		public bool Move(ICreature creature, Direction direction)
		{
			var oldtile = Find(creature);
			if (oldtile is null)
				return false; // creature is not on this floor

			var newtile = GetNeighbor(oldtile, direction);
			if (newtile is null)
				return false; // moving out of bounds

			if (oldtile == newtile)
				return true; // already at destination

			if (!newtile.IsWalkable)
				return false; // can't walk through walls

			// do the move
			oldtile.Creature = null;
			newtile.Creature = creature;

			// update FOV
			if (creature.FieldOfView is null)
				creature.FieldOfView = new FieldOfView(this);

			// success!
			return true;
		}
	}
}
