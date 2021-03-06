﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp.MapCreation;
using RogueSharp.Random;

namespace FoundMagic
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
			var mapper = new RandomRoomsMapCreationStrategy<Floor>(80, 45, 32, 12, 4, Rng);
			CurrentFloor = mapper.CreateMap();
		}

		/// <summary>
		/// A random number generator used by this world.
		/// </summary>
		public IRandom Rng { get; }

		/// <summary>
		/// The floor that the player character is currently located on.
		/// </summary>
		public Floor CurrentFloor { get; }
	}
}
