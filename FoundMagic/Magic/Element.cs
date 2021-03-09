using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic.Magic
{
	/// <summary>
	/// A magical element which can be used to cast spells.
	/// </summary>
	public abstract class Element
	{
		protected Element()
		{
			var t = GetType();
			if (!AllWords.ContainsKey(t))
				AllWords[t] = World.Instance.Rng.Pick(Words);
			Word = AllWords[t];
		}

		/// <summary>
		/// Casts a spell using this element.
		/// </summary>
		/// <param name="caster">The creature casting the spell.</param>
		/// <param name="direction">The direction in which the spell is being cast.</param>
		/// <param name="power">How powerful was the spell? Where 0 = not powerful at all and 1 = full power.</param>
		/// <param name="accuracy">How accurate was the spell? Where 0 = not accurate at all and 1 = full accuracy</param>
		/// <returns>The tiles affected by the spell (i.e. it travels over and/or hits those tiles).</returns>
		public abstract IEnumerable<Tile> Cast(ICreature caster, Direction direction, double power, double accuracy);

		/// <summary>
		/// Gets a spell target in line of sight of the specified creature.
		/// </summary>
		/// <param name="caster">The creature casting the spell.</param>
		/// <param name="direction">The direction in which the spell is being cast.</param>
		/// <param name="pierce">Should the spell pierce enemies?</param>
		/// <returns>The tiles affected by the spell.</returns>
		protected IEnumerable<Tile> GetTargets(ICreature caster, Direction direction, bool pierce)
		{
			// where are we?
			var tile = Floor.Current.Find(caster);

			// look outward using our vision radius
			for (var i = 0; i < caster.Vision; i++)
			{
				// find the next tile (if direction is stationary this will just be where the caster is of course)
				tile = Floor.Current.GetNeighbor(tile, direction);

				// did we hit the edge of the map?
				if (tile is null)
					yield break;
				
				// this tile was affected
				yield return tile;

				// can't cast through creatures unless spell is piercing
				if (tile.Creature is not null && !pierce)
					yield break;

				// can't cast through walls
				if (!tile.IsTransparent)
					yield break;
			}

			// no creature found to target
			yield break;
		}
		
		/// <summary>
		/// Pool of magic words which is randomly chosen from each game to represent this element.
		/// </summary>
		protected abstract IEnumerable<string> Words { get; }

		/// <summary>
		/// The magic word which represents this element in the current game.
		/// </summary>
		public string Word { get; }

		/// <summary>
		/// Gets the mana cost of a spell.
		/// </summary>
		/// <param name="accuracy">How accurate was the spell? Where 0 = not accurate at all and 1 = full accuracy</param>
		/// <returns></returns>
		public int GetManaCost(double accuracy)
			=> (int)Math.Round(BaseManaCost / accuracy);

		public override string ToString()
			=> GetType().Name;

		/// <summary>
		/// The color used to represent this element.
		/// </summary>
		public abstract Color Color { get; }

		/// <summary>
		/// The magic word which represents each element in the current game.
		/// </summary>
		private static IDictionary<Type, string> AllWords { get; } = new Dictionary<Type, string>();

		/// <summary>
		/// The basic mana cost of the spell, with 100% accuracy.
		/// </summary>
		public abstract double BaseManaCost { get; }
	}
}
