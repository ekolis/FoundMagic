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

		public override IEnumerable<Tile> Cast(ICreature caster, Direction direction, double power, double accuracy)
		{
			var manaCost = GetManaCost(1, accuracy);

			if (manaCost > caster.Mana)
			{
				// not enough mana, spell fizzles
				Logger.LogSpellFizzle(caster);
				return Enumerable.Empty<Tile>();
			}
			else
			{
				// consume mana
				caster.Mana -= manaCost;

				// determine targets of spell
				var tiles = GetTargets(caster, direction, false).ToImmutableList();
				var creatures = tiles.Select(q => q.Creature).OfType<ICreature>();

				// log cast
				Logger.LogSpellCast(caster, this);

				// do spell effects
				if (creatures.Any())
				{
					foreach (var creature in creatures)
					{
						var dmg = (int)Math.Round(3 * power);
						Logger.LogSpellDamage(creature, this, dmg);
						creature.TakeDamage(dmg);
					}
				}

				return tiles;
			}
		}

		public override Color Color => Color.Orange;
	}
}
