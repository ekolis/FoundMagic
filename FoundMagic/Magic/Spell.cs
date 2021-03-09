using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic.Magic
{
	/// <summary>
	/// A magic spell.
	/// </summary>
	public record Spell(ICreature Caster, Direction Direction, Element Element, double Power, double Efficiency)
	{
		public void Cast()
		{
			Element.Cast(Caster, Direction, Power, Efficiency);
		}

		public static Spell FromWord(ICreature caster, Direction direction, string s, TimeSpan duration)
		{
			// find most accurately typed magic word
			var dict = new Dictionary<Element, double>();
			foreach (var element in caster.Elements)
			{
				var distance = element.Word.ToLower().Distance(s.ToLower());
				var accy = 1.0 - (1.0 / element.Word.Length * distance); // so a four letter word would lose 25% power from each letter of distance
				dict[element] = accy;
			}
			var bestElement = dict.OrderByDescending(q => q.Value).First().Key;
			var bestPower = dict[bestElement];

			// find spell efficiency based on duration
			// logarithmic so instantaneous is 100%, one second is 50%, two seconds is 25%, etc...
			var efficiency = 1.0 / Math.Log2(duration.TotalSeconds + 2);

			// create the spell
			return new Spell(caster, direction, bestElement, bestPower, efficiency);
		}
	}
}
