using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;

namespace FoundMagic
{
	/// <summary>
	/// Our intrepid hero, exploring the world... 🧙🏼‍
	/// </summary>
	public class Hero 
		: ICreature
	{
		/// <summary>
		/// Hero is a singleton, so the constructor is private.
		/// </summary>
		private Hero()
		{
		}

		/// <summary>
		/// The singleton instance of the hero.
		/// </summary>
		public static Hero Instance { get; } = new Hero();

		public char Glyph { get; } = '@';

		public Color Color { get; } = Color.Blue;

		public FieldOfView FieldOfView { get; set; }

		public void UpdateFov()
		{
			var floor = Floor.Current;
			if (floor is null)
				return;

			if (FieldOfView is null)
				FieldOfView = new FieldOfView(floor);

			var newtile = floor.Find(this);
			var fovData = FieldOfView.ComputeFov(newtile.X, newtile.Y, Vision, true);
			foreach (var tile in floor.Tiles)
				floor.Tiles[tile.X, tile.Y] = floor.Tiles[tile.X, tile.Y].WithInvisible();
			foreach (var fovCell in fovData)
				floor.Tiles[fovCell.X, fovCell.Y] = floor.Tiles[fovCell.X, fovCell.Y].WithVisible();
		}

		public int Vision { get; } = 3;

		public double Speed { get; } = 1;
	}
}
