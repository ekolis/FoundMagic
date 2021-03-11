using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using FoundMagic.Creatures;

namespace FoundMagic.Mapping
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
			// set difficulty
			Difficulty = PreviousDifficulty + 1;
			PreviousDifficulty = Difficulty;

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
			var maxRarity = MonsterType.All.Max(q => q.Rarity);
			for (int i = 0; i < numMonsters; i++)
			{
				if (!places.Any())
					break; // nowhere to place a monster

				var place = World.Instance.Rng.Pick(places);
				var monsterType = World.Instance.Rng.PickWeighted(MonsterType.All, q => maxRarity / q.Rarity / CompareDifficulty(q.Rarity, Difficulty));
				place.Creature = new Monster(monsterType);
			}
		}

		private static int CompareDifficulty(int d1, int d2)
		{
			if (d1 > d2)
				return d1 / d2;
			else if (d2 > d1)
				return d2 / d2;
			else // equal
				return 1;
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
		/// <param name="allowAttack">Can the move be an attack?</param>
		/// <param name="distance">The distance to move the creature.</param>
		/// <returns>The number of spaces moved.</returns>
		public int Move(ICreature creature, Direction direction, bool allowAttack, int distance = 1)
		{
			int actualDistance = 0;
			var oldtile = Find(creature);

			for (var i = 0; i < distance; i++)
			{
				// find next step
				var newtile = GetNeighbor(oldtile, direction);

				// can't move off the map
				if (newtile is null)
					break;

				// if bumping into a creature when attacking is not allowed, stop
				if (!allowAttack && newtile.Creature is not null)
					break;

				// try to move
				if (Move(oldtile, newtile))
				{
					oldtile = newtile;
					actualDistance++;
				}
				else
					break; // failed to move, stop
			}
			return actualDistance;
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
		/// <param name="waitForHero">Should we stop once it's the hero's turn?</param>
		/// <returns>The amount of time actually spent.</returns>
		public double ProcessTime(double time, bool waitForHero)
		{
			double timeSpent = 0;

			// pass time until at least one creature is ready to act
			var minTime = Creatures.Min(q => q.Timer);
			foreach (ICreature creature in Creatures)
				creature.ProcessTime(minTime);
			timeSpent += minTime;

			// find any creatures who are ready to act
			var readyCreatures = Creatures.Where(q => q.Timer <= 0);

			// see if we need to stop early
			if (waitForHero && Hero.Instance.Timer <= 0)
				return timeSpent;

			// let them act "simultaneously"
			foreach (var creature in readyCreatures)
				creature.Timer += creature.Act();

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

		public int Difficulty { get; private set; }

		public static int PreviousDifficulty { get; private set; } = 0;
	}
}
