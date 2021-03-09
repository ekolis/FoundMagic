using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FoundMagic
{
	/// <summary>
	/// Handles keyboard input.
	/// </summary>
	public static class Keyboard
	{
		public static bool IsKeyPressed(Keys key)
			=> PressedKeys.Contains(key);

		public static bool IsAnyKeyPressed(params Keys[] keys)
			=> keys.Any(key => IsKeyPressed(key));

		public static bool Press(Keys key)
			=> PressedKeys.Add(key);

		public static bool Release(Keys key)
			=> PressedKeys.Remove(key);

		public static void Reset()
			=> PressedKeys.Clear();

		private static ISet<Keys> PressedKeys { get; } = new HashSet<Keys>();

		/// <summary>
		/// The direction in which the player is trying to move or cast a spell.
		/// </summary>
		public static Direction? ActionDirection
		{
			get
			{
				if (IsAnyKeyPressed(Keys.Up, Keys.D8, Keys.W))
					return Direction.North;
				else if (IsAnyKeyPressed(Keys.Down, Keys.D2, Keys.S))
					return Direction.South;
				else if (IsAnyKeyPressed(Keys.Left, Keys.D4, Keys.A))
					return Direction.West;
				else if (IsAnyKeyPressed(Keys.Right, Keys.D6, Keys.D))
					return Direction.East;
				else if (IsAnyKeyPressed(Keys.OemPeriod, Keys.D5, Keys.LShiftKey))
					return Direction .Stationary;
				return null;
			}
		}
	}
}
