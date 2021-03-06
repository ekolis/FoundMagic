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
	}
}
