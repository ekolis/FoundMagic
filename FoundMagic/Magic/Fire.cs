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
	/// Casting a fire spell costs 1 mana and causes a single target to lose 3 hit points.
	/// </summary>
	public class Fire
		: Element
	{
		protected override IEnumerable<string> Words { get; } = new string[]
		{
			"fire",
			"frizz",
			"burn",
			"singe",
		};

		public override IEnumerable<Tile> Cast(ICreature caster, Direction direction, double power, double efficiency)
		{
			return CastSingleTargetProjectile(caster, direction, power, efficiency, creature =>
			{
				// inflict some damage
				var dmg = (int)Math.Round(3 * power);
				Logger.LogSpellDamage(creature, this, dmg);
				creature.TakeDamage(dmg);
			});
		}

		public override Color Color => Color.Orange;

		public override double BaseManaCost { get; } = 1;
	}
}
