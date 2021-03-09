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
	/// Casting a darkness spell costs 2 mana and drains 2 HP from the target.
	/// </summary>
	public class Darkness
		: Element
	{
		protected override IEnumerable<string> Words { get; } = new string[]
		{
			"dark",
			"shade",
			"evil",
			"death",
		};

		public override IEnumerable<Tile> Cast(ICreature caster, Direction direction, double power, double efficiency)
		{
			return CastSingleTargetProjectile(caster, direction, power, efficiency, creature =>
			{
				// drain some HP
				var drain = (int)Math.Round(2 * power);
				drain = caster.DrainHPFrom(creature, drain);
				Logger.LogHPDrain(caster, creature, this, drain);
			});
		}

		public override Color Color => Color.Purple;

		public override double BaseManaCost { get; } = 2;

		public override string EffectDescription => "Drains 2 HP from the target.";
	}
}
