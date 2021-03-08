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
	}
}
