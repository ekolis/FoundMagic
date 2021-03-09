using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic.Magic
{
	/// <summary>
	/// Casting an earth spell costs 1 mana and causes a single target to be reduced to half speed for 8 turns.
	/// </summary>
	public class Earth
		: Element
	{
		protected override IEnumerable<string> Words { get; } = new string[]
		{
			"earth",
			"terra",
			"rock",
			"ground",
		};

		public override IEnumerable<Tile> Cast(ICreature caster, Direction direction, double power, double efficiency)
		{
			return CastSingleTargetProjectile(caster, direction, power, efficiency, creature =>
			{
				// slow the target
				var duration = Math.Round(8 * power);
				duration = creature.ApplyStatusEffect(StatusEffect.Slow, duration);
				Logger.LogStatusEffectStart(creature, this, StatusEffect.Slow, duration);
			});
		}

		public override Color Color => Color.Brown;

		public override double BaseManaCost { get; } = 1;

		public override string EffectDescription => "Slows the target (to half speed) for 8 turns.";
	}
}
