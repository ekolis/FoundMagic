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
	/// Casting a light spell costs 1 mana and heals 3 HP. If cast on an enemy, it also stuns the enemy for 3 turns.
	/// </summary>
	public class Light
		: Element
	{
		protected override IEnumerable<string> Words { get; } = new string[]
		{
			"light",
			"lumen",
			"flash",
			"holy",
		};

		public override void ApplyEffect(ICreature caster, Direction direction, double power, double efficiency, ICreature target)
		{
			// heal some HP
			var amt = (int)Math.Round(3 * power);
			var heal = target.Heal(amt);
			Logger.LogHealing(target, this, heal);

			// stun enemies
			if (caster is Hero && target is Monster || caster is Monster && target is Hero)
			{
				var stun = target.Stun(amt);
				Logger.LogStun(target, this, stun);
			}
		}

		public override Color Color => Color.Pink;

		public override double BaseManaCost { get; } = 1;

		public override string EffectDescription => "Heals the target 3 HP. If cast on an enemy, also stuns it for 3 turns.";
	}
}
