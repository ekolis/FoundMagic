using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoundMagic.Magic;
using FoundMagic.Mapping;
using FoundMagic.Creatures;

namespace FoundMagic.UI
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
		{ 
			var result = PressedKeys.Add(key);
			var h = Hero.Instance;
			if (h.IsCasting)
			{
				if (key >= Keys.A && key <= Keys.Z)
				{
					// start or continue the spell
					if (h.SpellTimestamp is null)
						h.SpellTimestamp = DateTime.Now;
					h.SpellWord += key.ToString();
				}
				else if (key == Keys.Enter)
				{
					// finish the spell
					h.Spell = Spell.FromWord(h, LastActionDirection, h.SpellWord, h.SpellDuration.Value);
				}
			}
			else
			{
				if (ActionDirection is not null)
					LastActionDirection = ActionDirection;
			}
			return result;
		}

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

		public static Direction? LastActionDirection { get; private set; }
	}
}
