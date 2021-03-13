using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundMagic.Mapping;
using FoundMagic.Creatures;

namespace FoundMagic.Magic
{
	/// <summary>
	/// Casting a darkness spell costs 4 mana and drains 2 HP from the target.
	/// </summary>
	public class Darkness
		: Element
	{
		public Darkness(int essences)
			: base(essences)
		{
		}

		protected override IEnumerable<string> Words { get; } = new string[]
		{
			"dark",
			"shade",
			"evil",
			"death",
		};

		public override void ApplyEffect(ICreature caster, Direction direction, double power, double efficiency, ICreature target)
		{
			// drain some HP
			var drain = (int)Math.Round(GetEffectAmount(caster) * power);
			drain = caster.DrainHPFrom(target, drain);
			Logger.LogHPDrain(caster, target, this, drain);
		}

		public override Color Color => Color.Purple;

		public override double BaseManaCost { get; } = 4;

		public override string GetEffectDescription(ICreature caster) => $"Drains {GetEffectAmount(caster)} HP from the target.";

		public override int BaseEffectAmount { get; } = 2;
	}
}
