using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using FoundMagic.Creatures;
using System.Drawing;
using FoundMagic.Magic;

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
					Tiles[x, y] = new Tile(GetCell(x, y), 0.1);
					SetCellProperties(x, y, Tiles[x, y].IsTransparent, Tiles[x, y].IsWalkable); // sync up our two sets of tiles
				}
			}

			// create stairs
			var places = Tiles.Cast<Tile>().Where(q => q.IsWalkable && q.Creature is null).ToList();
			var downStairsPlace = World.Instance.Rng.Pick(places);
			downStairsPlace.Terrain = Terrain.StairsDown;
			var upStairsPlace = World.Instance.Rng.Pick(places.Except(new[] { downStairsPlace }));
			upStairsPlace.Terrain = Terrain.StairsUp;

			// create monsters
			const int monsterRarity = 25;
			var numMonsters = places.Count() / monsterRarity;
			var maxMonsterDifficulty = MonsterType.All.Max(q => q.Difficulty);
			for (int i = 0; i < numMonsters; i++)
			{
				if (!places.Any())
					break; // nowhere to place a monster

				var place = World.Instance.Rng.Pick(places);
				var monsterType = World.Instance.Rng.PickWeighted(MonsterType.Spawnable, q => maxMonsterDifficulty / q.Difficulty / CompareDifficulty(q.Difficulty, Difficulty));
				place.Creature = new Monster(monsterType);
				if (monsterType.HasFlag(MonsterFlags.FinalBoss))
				{
					// we just spawned the final boss! warn the player, and delete the down stairs!
					Logger.Log("WARNING! Something very dangerous lurks here! You feel you can go no deeper in this dungeon...", Color.Red);
					downStairsPlace.Terrain = Terrain.Floor;
				}
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
		/// <param name="timeAllotted">The amount of time to process.</param>
		/// <param name="waitForHero">Should we stop once it's the hero's turn?</param>
		/// <returns>The amount of time actually spent.</returns>
		public double ProcessTime(double timeAllotted, bool waitForHero)
		{
			// degenerate case
			if (timeAllotted == 0)
				return 0;

			// declare some variables
			double timeSpent = 0;
			IEnumerable<ICreature> readyCreatures;
			bool heroHasActed = false;

			// spend our allotted time
			while (timeSpent < timeAllotted)
			{
				// pass time until at least one creature is ready to act, or the allotted time is up
				var minTime = Math.Min(Creatures.Min(q => q.Timer), timeAllotted);
				foreach (ICreature creature in Creatures)
					creature.ProcessTime(minTime);
				timeSpent += minTime;

				// find any creatures who are ready to act
				readyCreatures = Creatures.Where(q => q.Timer <= 0);

				// see if we need to stop early
				if (heroHasActed && !readyCreatures.OfType<Monster>().Any())
					break;
				if (timeSpent >= timeAllotted)
					break;
				if (waitForHero && Hero.Instance.Timer <= 0)
					break;

				// let them act "simultaneously"
				foreach (var creature in readyCreatures)
				{
					// moving monsters when the hero has left the floor is counterproductive...
					if (Hero.Instance.IsClimbing)
					{
						Hero.Instance.IsClimbing = false;
						goto done; // gotos are evil, yeah, but I need to break out of two loops at once! so there! 😛
					}
					creature.Timer += creature.Act();
					if (creature is Hero)
						heroHasActed = true;
				}
			}

			done:

			// deal with endgame stuff
			if (World.Instance.IsEndgame)
			{
				World.Instance.EndgameTimer -= timeSpent;
				if (World.Instance.EndgameTimer < 0)
				{
					// find out how many HP to drain (for now, 1 HP per 10 turns)
					// TODO: logarithmic rate of damage?
					var drain = -World.Instance.EndgameTimer * 0.1;
					World.Instance.EndgameDamage += drain;

					// did we save enough for at least one HP to be drained?
					var dmg = (int)World.Instance.EndgameDamage;

					if (dmg > 0)
					{
						// if so, inflict the damage
						Hero.Instance.InflictDamage(Hero.Instance, dmg);
						Logger.LogHPDrain(Hero.Instance, Hero.Instance, Hero.Instance.GetElement<Fire>(), dmg);
						World.Instance.EndgameDamage -= dmg;
					}

					// don't let the timer be negative
					World.Instance.EndgameTimer = 0;
				}
			}

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
