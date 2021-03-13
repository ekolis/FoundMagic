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
	/// A magical element which can be used to cast spells.
	/// </summary>
	public abstract class Element
	{
		public static Element Create(string name, int essences)
		{
			return name.ToLower() switch
			{
				"fire" => new Fire(essences),
				"earth" => new Earth(essences),
				"air" => new Air(essences),
				"water" => new Water(essences),
				"light" => new Light(essences),
				"darkness" => new Darkness(essences),
				_ => throw new ArgumentException($"Invalid element name: {name}.", nameof(name))
			};
		}

		protected Element(int essences)
		{
			var t = GetType();
			if (!AllWords.ContainsKey(t))
				AllWords[t] = World.Instance.Rng.Pick(Words);
			Word = AllWords[t];
			Essences = essences;
		}

		/// <summary>
		/// Casts a spell using this element.
		/// </summary>
		/// <param name="caster">The creature casting the spell.</param>
		/// <param name="direction">The direction in which the spell is being cast.</param>
		/// <param name="power">How powerful was the spell? Where 0 = not powerful at all and 1 = full power.</param>
		/// <param name="efficiency">How efficient was the spell? Where 0 = not efficient at all and 1 = full efficiency</param>
		/// <param name="modifierElement">Optional element used to modify the spell.</param>
		/// <returns>The tiles affected by the spell (i.e. it travels over and/or hits those tiles).</returns>
		public IEnumerable<Tile> Cast(ICreature caster, Direction direction, double power, double efficiency, Element? modifierElement = null)
		{
			if (Attunement <= 0)
				return Enumerable.Empty<Tile>();

			bool crit = false;

			if (caster is Hero h)
			{
				// check for crits from darkness attunement
				var critChance = h.EssenceBoostCriticalHits;
				if (World.Instance.Rng.Chance(critChance))
					crit = true;
			}

			if (caster is Monster m && World.Instance.IsEndgame)
			{
				// check for crits from endgame
				var critChance = World.Instance.EndgameMonsterCritChance;
				if (World.Instance.Rng.Chance(critChance))
					crit = true;
			}

			if (crit)
			{
				power *= 3; // crits are triply powerful
				Logger.LogCriticalSpell(caster, this);
			}

			if (modifierElement is null)
			{
				return CastSingleTargetProjectile(caster, direction, power, efficiency, creature =>
					ApplyEffect(caster, direction, power, efficiency, creature));
			}
			else
			{
				// TODO: two-word spells
				/*return modifierElement.ApplyModifierEffect(caster, this, direction, power, efficiency, creature =>
					ApplyEffect(caster, direction, power, efficiency, creature));*/
				return Enumerable.Empty<Tile>();
			}
		}

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
		/// <param name="efficiency">How efficient was the spell? Where 0 = not efficient at all and 1 = full efficiency</param>
		/// <returns></returns>
		public int GetManaCost(double efficiency)
			=> (int)Math.Round(BaseManaCost / efficiency);

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
		/// The basic mana cost of the spell, with 100% efficiency.
		/// </summary>
		public abstract double BaseManaCost { get; }

		protected IEnumerable<Tile> CastSingleTargetProjectile(ICreature caster, Direction direction, double power, double efficiency, Action<ICreature> effect)
		{
			var manaCost = GetManaCost(efficiency);

			if (manaCost > caster.Mana)
			{
				// not enough mana, spell fizzles
				Logger.LogSpellFizzle(caster);
				return Enumerable.Empty<Tile>();
			}
			else if (manaCost < 0)
			{
				// REALLY inaccurate spell caused integer overflow? just let it fizzle.
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
				Logger.LogSpellCast(caster, power, efficiency, this);

				// do spell effects
				if (creatures.Any())
				{
					foreach (var creature in creatures)
					{
						effect(creature);
					}
				}

				return tiles;
			}
		}

		/// <summary>
		/// A brief description of the effect of an element's effect.
		/// </summary>
		public abstract string GetEffectDescription(ICreature caster);

		/// <summary>
		/// A brief description of the effect of an element's modifier effect.
		/// </summary>
		//public abstract string ModifierEffectDescription { get; }

		/// <summary>
		/// Applies the effect of the spell to a single creature.
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="direction"></param>
		/// <param name="power"></param>
		/// <param name="efficiency"></param>
		/// <param name="target"></param>
		public abstract void ApplyEffect(ICreature caster, Direction direction, double power, double efficiency, ICreature target);

		/// <summary>
		/// Applies the modifier effect of the spell.
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="baseElement"></param>
		/// <param name="direction"></param>
		/// <param name="power"></param>
		/// <param name="efficiency"></param>
		/// <param name="effect"></param>
		/// <returns>Tiles affected.</returns>
		//public abstract IEnumerable<Tile> ApplyModifierEffect(ICreature caster, Element baseElement, Direction direction, double power, double efficiency, Action<ICreature> effect);

		/// <summary>
		/// The number of essences required to cast a spell.
		/// </summary>
		public static int MinEssencesToCast { get; } = 10;

		/// <summary>
		/// The number of essences required to get standard spell attunement.
		/// </summary>
		public static int EssencesForStandardAttunement { get; } = 40;

		/// <summary>
		/// The number of elemental essences possessed.
		/// </summary>
		public int Essences { get; set; }

		/// <summary>
		/// The attunement of a creature to an element.
		/// 0 is no attunement at all.
		/// 1 is standard attunement.
		/// The scale is logarithmic, so the higher your attunement gets, the more essences you'll need to raise it.
		/// </summary>
		public double Attunement
		{
			get
			{
				if (Essences < MinEssencesToCast)
					return 0;
				return Math.Log2((double)Essences / (double)EssencesForStandardAttunement) + 1;
			}
		}

		/// <summary>
		/// The base effect amount of the spell, at standard attunement.
		/// </summary>
		public abstract int BaseEffectAmount { get; }

		/// <summary>
		/// The effect amount of the spell, accounting for attunement and essence boost (but not spell power from typing accuracy).
		/// </summary>
		public int GetEffectAmount(ICreature caster)
		{
			var amt = BaseEffectAmount * Attunement;
			if (caster is Hero h)
				amt *= 1.0 + h.EssenceBoostSpellPower;
			return (int)Math.Round(amt);
		}

		/// <summary>
		/// Can this spell be cast yet?
		/// </summary>
		public bool CanBeCast => Attunement > 0;
	}
}
