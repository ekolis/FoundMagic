﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
			Hitpoints = MaxHitpoints;
			Mana = MaxMana;
		}

		/// <summary>
		/// The singleton instance of the hero.
		/// </summary>
		public static Hero Instance { get; } = new Hero();

		public string Name
			=> "you";

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

		public double Act()
		{
			if (Floor.Current is null)
				return 0; // nothing to do

			// get globals
			Floor floor = Floor.Current;

			// movement keys
			Direction? dir = null;
			if (Keyboard.IsAnyKeyPressed(Keys.Up, Keys.D8, Keys.W))
				dir = Direction.North;
			else if (Keyboard.IsAnyKeyPressed(Keys.Down, Keys.D2, Keys.S))
				dir = Direction.South;
			else if (Keyboard.IsAnyKeyPressed(Keys.Left, Keys.D4, Keys.A))
				dir = Direction.West;
			else if (Keyboard.IsAnyKeyPressed(Keys.Right, Keys.D6, Keys.D))
				dir = Direction.East;
			else if (Keyboard.IsAnyKeyPressed(Keys.OemPeriod, Keys.D5, Keys.LShiftKey))
				dir = Direction.Stationary;

			// HACK: why is this necessary?
			Keyboard.Reset();

			if (dir is not null)
			{
				// move hero
				bool success = floor.Move(this, dir);

				if (success)
				{
					// update the hero's field of view
					UpdateFov();

					// TODO: maybe log any spotted monsters so the player knows what they are? maybe even the direction in which they are spottted?

					// spend time
					return 1.0 / Speed;
				}
			}

			return 0;
		}

		public int Vision { get; } = 3;

		public double Speed { get; } = 1;

		public double Timer { get; set; }

		public int Strength { get; } = 1;

		public int MaxHitpoints { get; } = 10;

		public int Hitpoints { get; set; }

		public int MaxMana { get; } = 10;

		public int Mana { get; set; }

		public override string ToString()
			=> Name;

		/// <summary>
		/// When did the hero die? Or null if he didn't.
		/// </summary>
		public DateTime? DeathTimestamp { get; set; }

		/// <summary>
		/// How long should the screen take to fade when the hero dies?
		/// </summary>
		public TimeSpan DeathFadeTime { get; } = new TimeSpan(0, 0, 5);
	}
}
