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
	/// Casting an air spell costs 1 mana and pushes a single target back 2 spaces, unless there's something behind it.
	/// </summary>
	public class Air
		: Element
	{
		protected override IEnumerable<string> Words { get; } = new string[]
		{
			"aero",
			"wind",
			"breeze",
			"blow",
		};

		public override void ApplyEffect(ICreature caster, Direction direction, double power, double efficiency, ICreature target)
		{
			// push target back
			var dist = (int)Math.Round(2 * power);
			dist = Floor.Current.Move(target, direction, false, dist);
			Logger.LogKnockback(target, this, direction, dist);
		}

		public override Color Color => Color.Cyan;

		public override double BaseManaCost { get; } = 1;

		public override string EffectDescription => "Pushes the target back 2 spaces, unless there's something behind it.";
	}
}
