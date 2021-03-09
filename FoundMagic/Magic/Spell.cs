using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic.Magic
{
	/// <summary>
	/// A magic spell.
	/// </summary>
	public record Spell(ICreature Caster, Direction Direction, Element Element, double Power, double Accuracy)
	{
		public void Cast()
		{
			Element.Cast(Caster, Direction, Power, Accuracy);
		}
	}
}
