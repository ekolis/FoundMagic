using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp.MapCreation;
using RogueSharp.Random;
using FoundMagic.Creatures;

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
		public static World Instance { get; } = new World(42);

		/// <summary>
		/// Creates a whole new world. A wondrous *ducks*
		/// </summary>
		/// <param name="seed">The seed to pass to the RNG, so we can recreate the same world again if need be.</param>
		private World(int seed)
		{
			Rng = new DotNetRandom(seed);
		}

		/// <summary>
		/// Sets up the world.
		/// </summary>
		public void Setup()
		{
			var mapper = new RandomRoomsMapCreationStrategy<Floor>(80, 45, 32, 12, 4, Rng);
			CurrentFloor = mapper.CreateMap();
			CurrentFloor.Setup();
			var emptyTiles = CurrentFloor.Tiles.Cast<Tile>().Where(q => q.IsWalkable && q.Creature is null);
			var startTile = Rng.Pick(emptyTiles);
			startTile.Creature = Hero.Instance;
			Hero.Instance.UpdateFov();
		}

		/// <summary>
		/// A random number generator used by this world.
		/// </summary>
		public IRandom Rng { get; private set; }

		/// <summary>
		/// The floor that the hero is currently located on.
		/// </summary>
		public Floor? CurrentFloor { get; private set; }
	}
}
