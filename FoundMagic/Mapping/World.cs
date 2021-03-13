using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp.MapCreation;
using RogueSharp.Random;
using FoundMagic.Creatures;
using System.Drawing;
using System.ComponentModel;

namespace FoundMagic.Mapping
{
	/// <summary>
	/// The game world. 🌎
	/// </summary>
	/// <remarks>
	/// A whole new world, a shining, shimmering, wondrous place...
	/// From way up here it's crystal clear that now I'm in a whole new world with you!
	/// </remarks>
	public class World
	{
		/// <summary>
		/// The singleton instance of the world.
		/// </summary>
		/// <remarks>
		/// He's got the whole world in his hands...
		/// </remarks>
		public static World Instance { get; private set; }

		public static void Setup(int seed)
		{
			Instance = new World(seed);
		}

		/// <summary>
		/// Creates a whole new world. A wondrous *ducks*
		/// </summary>
		/// <param name="seed">The seed to pass to the RNG, so we can recreate the same world again if need be.</param>
		private World(int seed)
		{
			Rng = new DotNetRandom(seed);
		}

		/// <summary>
		/// The floors that exist in the world.
		/// A new one will be created when you go down from the last one that exists.
		/// </summary>
		public IList<Floor> Floors { get; } = new List<Floor>();

		/// <summary>
		/// Generates the next floor in the world.
		/// </summary>
		public Floor GenerateNextFloor()
		{
			var mapper = new RandomRoomsMapCreationStrategy<Floor>(80, 45, 32, 12, 4, Rng);
			var floor = mapper.CreateMap();
			Floors.Add(floor);
			floor.Setup();
			return floor;
		}

		public void ClimbDown()
		{
			var floor = GenerateNextFloor();
			CurrentDepth++;
			var upStairs = floor.Tiles.Cast<Tile>().Where(q => q.Terrain == Terrain.StairsUp);
			var startTile = Rng.Pick(upStairs);
			startTile.Creature = Hero.Instance;
			Hero.Instance.ResetFov();
			Hero.Instance.UpdateFov();
			if (CurrentDepth == 0)
				Logger.Log("Welcome to Found Magic! Press H or ? for help.", Color.White);
			Logger.Log($"Welcome to floor {CurrentDepth + 1}!", Color.White);
		}

		public void ClimbUp()
		{
			CurrentDepth--;
			if (CurrentDepth < 0)
			{
				// you made it to the surface! you win!
				IsEndgame = false;
				Win();
			}
			else
			{
				var floor = CurrentFloor;
				var downStairs = floor.Tiles.Cast<Tile>().Where(q => q.Terrain == Terrain.StairsDown);
				var startTile = Rng.Pick(downStairs);
				startTile.Creature = Hero.Instance;
				Hero.Instance.ResetFov();
				Hero.Instance.UpdateFov();
				Logger.Log($"Welcome back to floor {CurrentDepth + 1}!", Color.White);
			}
		}

		/// <summary>
		/// A random number generator used by this world.
		/// </summary>
		public IRandom Rng { get; private set; }

		/// <summary>
		/// The index of the floor that the hero is currently located on.
		/// </summary>
		public int CurrentDepth { get; private set; } = -1;

		/// <summary>
		/// The floor that the hero is currently located on.
		/// </summary>
		public Floor? CurrentFloor => Floors[CurrentDepth];

		/// <summary>
		/// Are we in the endgame scenario?
		/// During the endgame, monsters will be buffed and up stairs rather than down stairs will be enabled.
		/// Also there will be a timer; when the timer runs out, the hero will start to take damage over time!
		/// </summary>
		public bool IsEndgame { get; set; }

		/// <summary>
		/// The endgame timer.
		/// You will have this many turns to escape the dungeon, starting from the time you defeat the final boss.
		/// If you don't, you'll take damage over time.
		/// </summary>
		public double EndgameTimer { get; set; } = 5000;

		/// <summary>
		/// Built-up damage to apply to the hero once the endgame timer runs out.
		/// </summary>
		public double EndgameDamage { get; set; }

		/// <summary>
		/// When did the player win the game? Or null if not yet.
		/// </summary>
		public DateTime? VictoryTimestamp { get; set; }

		/// <summary>
		/// Did the player win the game?
		/// </summary>
		public bool IsWinner => VictoryTimestamp is not null;

		/// <summary>
		/// Is the game over (either victory or defeat)?
		/// </summary>
		public bool IsGameOver => IsWinner || Hero.Instance.DeathTimestamp is not null;

		/// <summary>
		/// Declares victory for the player.
		/// </summary>
		public void Win() => VictoryTimestamp = DateTime.Now;
	}
}
