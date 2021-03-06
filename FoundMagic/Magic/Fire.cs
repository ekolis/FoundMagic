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
	/// Casting a fire spell costs 2 mana and causes a single target to lose 3 hit points.
	/// </summary>
	public class Fire
		: Element
	{
		public Fire(int essences)
			: base(essences)
		{
		}

		protected override IEnumerable<string> Words { get; } = new string[]
		{
			"fire",
			"frizz",
			"burn",
			"singe",
		};

		public override void ApplyEffect(ICreature caster, Direction direction, double power, double efficiency, ICreature target)
		{
			// inflict some damage
			var dmg = (int)Math.Round(GetEffectAmount(caster) * power);
			caster.InflictDamage(target, dmg);
			Logger.LogSpellDamage(target, this, dmg);
		}

		public override Color Color => Color.Orange;

		public override double BaseManaCost { get; } = 2;

		public override string GetEffectDescription(ICreature caster) => $"Inflicts {GetEffectAmount(caster)} points of damage.";

		public override int BaseEffectAmount { get; } = 3;
	}
}
