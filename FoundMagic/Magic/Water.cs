using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundMagic.Mapping;

namespace FoundMagic.Magic
{
	/// <summary>
	/// Casting a water spell costs 1 mana and drains 3 mana from the target.
	/// </summary>
	public class Water
		: Element
	{
		protected override IEnumerable<string> Words { get; } = new string[]
		{
			"water",
			"aqua",
			"hydro",
			"swim",
		};

		public override void ApplyEffect(ICreature caster, Direction direction, double power, double efficiency, ICreature target)
		{
			// drain some mana
			var drain = (int)Math.Round(3 * power);
			drain = caster.DrainManaFrom(target, drain);
			Logger.LogManaDrain(caster, target, this, drain);
		}

		public override Color Color => Color.Blue;

		public override double BaseManaCost { get; } = 1;

		public override string EffectDescription => "Drains 3 MP from the target.";
	}
}
