using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FoundMagic.Magic;

namespace FoundMagic
{
	public record MonsterType(string Name, char Glyph, IEnumerable<string> ElementNames, int Vision, double Speed, int Strength, int MaxHitpoints, int MaxMana, int Rarity)
	{
		public static IEnumerable<MonsterType> All { get; }
			= JsonSerializer.Deserialize<IEnumerable<MonsterType>>(File.ReadAllText("MonsterTypes.json")) ?? Enumerable.Empty<MonsterType>();

		public IEnumerable<Element> Elements { get; }
			= ElementNames.Select(Element.Create).ToImmutableList();
	}
}