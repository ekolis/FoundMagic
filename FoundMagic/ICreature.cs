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
	/// A creature which inhabits the game world. 🐍
	/// </summary>
	public interface ICreature
	{
		/// <summary>
		/// A textual glyph used to represent the creature.
		/// </summary>
		char Glyph { get; }

		/// <summary>
		/// A color to render the creature's glyph in.
		/// </summary>
		Color Color { get; }

		/// <summary>
		/// The creature's field of view.
		/// </summary>
		FieldOfView FieldOfView { get; set; }

		/// <summary>
		/// Updates the creature's field of view.
		/// </summary>
		void UpdateFov();

		/// <summary>
		/// The vision radius of this creature.
		/// </summary>
		int Vision { get; }

		/// <summary>
		/// The speed of this creature.
		/// </summary>
		/// <remarks>
		/// A creature takes 1.0 / Speed time to act.
		/// For instance, if a creature's speed is 2, then it acts every 0.5 time.
		/// </remarks>
		double Speed { get; }

		/// <summary>
		/// The creature's action timer.
		/// Creatures must wait this long until acting.
		/// </summary>
		/// <remarks>
		/// When a creature acts, its timer is increased by the duration of the action, so it can wait its turn.
		/// Then all creatures' timers are decreased at once until one of them reaches zero, and it's then that creature's turn to act.
		/// Ties are resolved randomly.
		/// </remarks>
		double Timer { get; set; }

		/// <summary>
		/// Allows the creature to act.
		/// </summary>
		/// <returns>The amount of time spent.</returns>
		double Act();
	}

	public static class CreatureExtensions
	{
		/// <summary>
		/// The amount of time it takes this creature to perform a standard action.
		/// </summary>
		public static double GetActionTime(this ICreature q)
			=> 1.0 / q.Speed;
	}
}
