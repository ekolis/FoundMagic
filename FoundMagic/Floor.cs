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

		public void Setup()
		{
			Tiles = new Tile[Width, Height];
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					Tiles[x, y] = new Tile(GetCell(x, y));
				}
			}
		}

		/// <summary>
		/// The tiles on this floor.
		/// </summary>
		/// <remarks>
		/// Since there seems to be no way in RogueSharp to use custom cell classes in a <see cref="Map"/>, we need a duplicate array...
		/// </remarks>
		public Tile[,] Tiles { get; private set; }
	}
}
