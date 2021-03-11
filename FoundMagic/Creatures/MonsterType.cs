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

namespace FoundMagic.Creatures
{
	public record MonsterType(string Name, char Glyph, IEnumerable<string> ElementNames, int Vision, double Speed, int Strength, int MaxHitpoints, int MaxMana, int Difficulty, IEnumerable<string> FlagNames)
	{
		public static IEnumerable<MonsterType> All { get; }
			= JsonSerializer.Deserialize<IEnumerable<MonsterType>>(File.ReadAllText("Data/MonsterTypes.json")) ?? Enumerable.Empty<MonsterType>();

		public IEnumerable<Element> Elements { get; }
			= ElementNames.Select(q => Element.Create(q, Element.EssencesForStandardAttunement)).ToImmutableList();

		public MonsterFlags Flags { get; }
			= FlagNames?.Select(q => Enum.Parse<MonsterFlags>(q, true)).Aggregate((a, b) => a | b) ?? MonsterFlags.None;

		public bool HasFlag(MonsterFlags f)
			=> Flags.HasFlag(f);

		/// <summary>
		/// How many of this monster type have been spawned already?
		/// </summary>
		public int NumberSpawned { get; set; }

		/// <summary>
		/// Monster types which can be spawned.
		/// </summary>
		public static IEnumerable<MonsterType> Spawnable
			=> All.Where(q => !q.HasFlag(MonsterFlags.Unique) || q.NumberSpawned == 0);
	}
}