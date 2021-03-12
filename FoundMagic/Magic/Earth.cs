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
	/// Casting an earth spell costs 1 mana and causes a single target to be reduced to half speed for 8 turns.
	/// </summary>
	public class Earth
		: Element
	{
		public Earth(int essences)
			: base(essences)
		{
		}

		protected override IEnumerable<string> Words { get; } = new string[]
		{
			"earth",
			"terra",
			"rock",
			"ground",
		};

		public override void ApplyEffect(ICreature caster, Direction direction, double power, double efficiency, ICreature target)
		{
			// slow the target
			var duration = Math.Round(GetEffectAmount(caster) * power);
			duration = target.ApplyStatusEffect(StatusEffect.Slow, duration);
			Logger.LogStatusEffectStart(target, this, StatusEffect.Slow, duration);
		}

		public override Color Color => Color.Brown;

		public override double BaseManaCost { get; } = 1;

		public override string GetEffectDescription(ICreature caster) => $"Slows the target (to half speed) for {GetEffectAmount(caster)} turns.";

		public override int BaseEffectAmount { get; } = 8;
	}
}
