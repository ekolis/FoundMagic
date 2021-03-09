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
		/// Tries to move a creature, or attack an enemy.
		/// </summary>
		/// <param name="creature">The creature to move.</param>
		/// <param name="direction">The direction in which to move the creature.</param>
		/// <returns>true if successful, otherwise false</returns>
		public bool Move(ICreature creature, Direction direction)
		{
			var oldtile = Find(creature);
			var newtile = GetNeighbor(oldtile, direction);
			return Move(oldtile, newtile);
		}

		/// <summary>
		/// Tries to move a creature, or attack an enemy.
		/// </summary>
		/// <param name="oldtile">The location of the creature to move.</param>
		/// <param name="newtile">The tile to which the creature will be moved.</param>
		/// <returns>true if successful, otherwise false</returns>
		public bool Move(Tile? oldtile, Tile? newtile)
		{
			if (oldtile is null)
				return false; // creature is not on this floor

			if (newtile is null)
				return false; // moving out of bounds

			if (oldtile == newtile)
				return true; // already at destination

			if (!newtile.IsWalkable)
				return false; // can't walk through walls or other creatures

			ICreature? creature = oldtile.Creature;
			if (creature is null)
				return false; // nothing to move

			// do the move/attack
			if (newtile.Creature is null)
			{
				// move
				oldtile.Creature = null;
				newtile.Creature = creature;
			}
			else
			{
				// attack
				oldtile.Creature.Attack(newtile.Creature);
			}

			// update FOV
			if (creature.FieldOfView is null)
				creature.FieldOfView = new FieldOfView(this);

			// success!
			return true;
		}

		/// <summary>
		/// Processes actions.
		/// </summary>
		/// <param name="time">The amount of time to process.</param>
		/// <returns>The amount of time actually spent.</returns>
		public double ProcessTime(double time)
		{
			double timeSpent = 0;

			// pass time until at least one creature is ready to act
			var minTime = Creatures.Min(q => q.Timer);
			foreach (ICreature creature in Creatures)
				creature.ProcessTime(minTime);
			timeSpent += minTime;

			// find any creatures who are ready to act
			var readyCreatures = Creatures.Where(q => q.Timer <= 0);

			// let them act "simultaneously"
			double creatureTime = 0;
			foreach (var creature in readyCreatures)
				creatureTime = Math.Max(creatureTime, creature.Act());

			// tally up any time spent
			timeSpent += creatureTime;

			// all done, return time spent
			return timeSpent;
		}

		/// <summary>
		/// Creature which has priority to move first because it took a zero time action.
		/// </summary>
		private ICreature? PriorityCreature { get; set; }

		/// <summary>
		/// Any creatures on this floor.
		/// </summary>
		public IEnumerable<ICreature> Creatures
			=> Tiles.Cast<Tile>().Select(q => q.Creature).OfType<ICreature>();

		/// <summary>
		/// Any monsters on this floor.
		/// </summary>
		public IEnumerable<Monster> Monsters
			=> Creatures.OfType<Monster>();
	}
}
