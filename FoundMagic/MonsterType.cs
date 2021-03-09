using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic
{
	public record MonsterType(string Name, char Glyph, Color Color, int Vision, double Speed, int Strength, int MaxHitpoints, int MaxMana)
	{
		// TODO: put these in a JSON file or something
		public static readonly MonsterType Slime = new("slime", 's', Color.Blue, 2, 1, 1, 3, 3);

		public static readonly IEnumerable<MonsterType> All = new MonsterType[] { Slime };
	}
}
