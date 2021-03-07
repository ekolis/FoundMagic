using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic
{
	public record MonsterType(char Glyph, Color Color, int Vision, double Speed)
	{
		// TODO: put these in a JSON file or something
		public static readonly MonsterType Slime = new('s', Color.Blue, 2, 1);

		public static readonly IEnumerable<MonsterType> All = new MonsterType[] { Slime };
	}
}
