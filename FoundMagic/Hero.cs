using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
