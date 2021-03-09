using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundMagic.Magic;

namespace FoundMagic
{
	public record MonsterType(string Name, char Glyph, IEnumerable<Element> Elements, int Vision, double Speed, int Strength, int MaxHitpoints, int MaxMana)
	{
		// TODO: put these in a JSON file or something
		public static readonly MonsterType Slime = new("slime", 's', new[] { new Water() }, 2, 1, 1, 3, 3);

		public static readonly IEnumerable<MonsterType> All = new MonsterType[] { Slime };
	}
}
