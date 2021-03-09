﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public override IEnumerable<Tile> Cast(ICreature caster, Direction direction, double power, double efficiency)
		{
			return CastSingleTargetProjectile(caster, direction, power, efficiency, creature =>
			{
				// push target back
				var dist = (int)Math.Round(2 * power);
				dist = Floor.Current.Move(creature, direction, false, dist);
				Logger.LogKnockback(creature, this, direction, dist);
			});
		}

		public override Color Color => Color.Cyan;

		public override double BaseManaCost { get; } = 1;
	}
}