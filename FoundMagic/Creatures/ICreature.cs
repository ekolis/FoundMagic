using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FoundMagic.Magic;
using RogueSharp;
using FoundMagic.Mapping;
using Microsoft.VisualBasic.Logging;

namespace FoundMagic.Creatures
{
	/// <summary>
	/// A creature which inhabits the game world. 🐍
	/// </summary>
	public interface ICreature
	{
		/// <summary>
		/// The name of this creature.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// A textual glyph used to represent the creature.
		/// </summary>
		char Glyph { get; }

		/// <summary>
		/// A color to render the creature's glyph in.
		/// </summary>
		Color Color { get; }

		/// <summary>
		/// The creature's field of view.
		/// </summary>
		FieldOfView FieldOfView { get; set; }

		/// <summary>
		/// Updates the creature's field of view.
		/// </summary>
		void UpdateFov();

		/// <summary>
		/// The vision radius of this creature.
		/// </summary>
		int Vision { get; }

		/// <summary>
		/// The speed of this creature.
		/// </summary>
		/// <remarks>
		/// A creature takes 1.0 / Speed time to act.
		/// For instance, if a creature's speed is 2, then it acts every 0.5 time.
		/// </remarks>
		double Speed { get; }

		/// <summary>
		/// The creature's action timer.
		/// Creatures must wait this long until acting.
		/// </summary>
		/// <remarks>
		/// When a creature acts, its timer is increased by the duration of the action, so it can wait its turn.
		/// Then all creatures' timers are decreased at once until one of them reaches zero, and it's then that creature's turn to act.
		/// Ties are resolved randomly.
		/// </remarks>
		double Timer { get; set; }

		/// <summary>
		/// Allows the creature to act.
		/// </summary>
		/// <returns>The amount of time spent.</returns>
		double Act();

		/// <summary>
		/// Number of HP of damage inflicted by this creature's attack.
		/// </summary>
		int Strength { get; }

		/// <summary>
		/// Maximum number of HP that this creature can have.
		/// </summary>
		int MaxHitpoints { get; }

		/// <summary>
		/// Number of HP of health that this creature has left.
		/// </summary>
		int Hitpoints { get; set; }

		/// <summary>
		/// Maximum amount of mana that this creature can have.
		/// </summary>
		int MaxMana { get; }

		/// <summary>
		/// Amount of mana that this creature has left.
		/// </summary>
		int Mana { get; set; }

		/// <summary>
		/// Elements known for spellcasting.
		/// </summary>
		IEnumerable<Element> Elements { get; }

		/// <summary>
		/// Any status effects applied to this creature, and their remaining durations.
		/// </summary>
		IDictionary<StatusEffect, double> StatusEffects { get; }
	}

	public static class CreatureExtensions
	{
		/// <summary>
		/// The amount of time it takes this creature to perform a standard action.
		/// </summary>
		public static double GetActionTime(this ICreature q)
			=> 1.0 / q.Speed;

		/// <summary>
		/// Allows one creature to attack another.
		/// </summary>
		/// <param name="attacker">The creature performing the attack.</param>
		/// <param name="target">The creature being attacked.</param>
		/// <returns>The amount of damage inflicted.</returns>
		public static int Attack(this ICreature attacker, ICreature target)
		{
			var dmg = attacker.Strength;

			bool crit = false;

			if (attacker is Hero h)
			{
				// check for crits from darkness attunement
				var critChance = h.EssenceBoostCriticalHits;
				if (World.Instance.Rng.Chance(critChance))
					crit = true;
			}

			if (crit)
			{
				dmg *= 3; // crits are triply powerful
			}

			// inflict damage
			dmg = attacker.InflictDamage(target, dmg);
			Logger.LogAttack(attacker, target, dmg, crit);

			// melee attacks will restore a little mana, so creatures can go casting lots of spells!
			var mana = attacker.RestoreMana(1);
			Logger.LogManaRestoration(attacker, World.Instance.Rng.Pick(attacker.Elements.Where(q => q.Essences == attacker.Elements.Max(w => w.Essences))), mana);

			return dmg;
		}

		/// <summary>
		/// Allows a creature to take damage.
		/// </summary>
		/// <param name="attacker">The creature inflicting damage.</param>
		/// <param name="target">The creature taking damage.</param>
		/// <param name="dmg">The amount of damage to inflict.</param>
		/// <returns>The amount of damage taken.</returns>
		public static int InflictDamage(this ICreature attacker, ICreature target, int dmg)
		{
			target.Hitpoints -= dmg;
			if (target.Hitpoints <= 0)
			{
				dmg += target.Hitpoints; // overkill, report smaller number of HP lost
				attacker.Kill(target);
			}
			return dmg;
		}

		/// <summary>
		/// Kills a creature, removing it from the game.
		/// </summary>
		/// <param name="killer">The creature which performed the kill.</param>
		/// <param name="victim">The creature to kill.</param>
		public static void Kill(this ICreature killer, ICreature victim)
		{
			victim.Hitpoints = 0;
			killer.DrainEssencesFrom(victim);
			Floor.Current.Find(victim).Creature = null;
			Logger.LogDeath(victim);
			if (victim is Hero h)
				h.DeathTimestamp = DateTime.Now;
			else if (victim is Monster m && m.HasFlag(MonsterFlags.FinalBoss))
			{
				// defeated the final boss? then let's start the endgame!
				Logger.Log($"DANGER! The defeat of {m} triggered the collapse of the dungeon!", Color.Red);
				Logger.Log($"You must escape in {World.Instance.EndgameTimer} turns, or bad things will happen!", Color.Red);
				World.Instance.IsEndgame = true;
			}
		}

		/// <summary>
		/// Perfomms bookkeeping related to the passing of time, such as updating the action timer and expiring status effects.
		/// </summary>
		/// <param name="creature">The creature to process.</param>
		/// <param name="time">The amount of time passing.</param>
		public static void ProcessTime(this ICreature creature, double time)
		{
			creature.Timer -= time;
			foreach (var kvp in creature.StatusEffects.ToArray())
			{
				creature.StatusEffects[kvp.Key] -= time;
				if (creature.StatusEffects[kvp.Key] <= 0)
					creature.StatusEffects.Remove(kvp.Key);
			}
		}

		/// <summary>
		/// Applies a status effect to a creature. Durations don't stack linearly, to make things fairer.
		/// </summary>
		/// <param name="creature">The creature to apply the status effect to.</param>
		/// <param name="fx">The status effect to apply.</param>
		/// <param name="duration">How long should the effect last? Reduced if the creature already has the effect.</param>
		public static double ApplyStatusEffect(this ICreature creature, StatusEffect fx, double duration)
		{
			// find existing duration
			double existingDuration = 0;
			if (creature.StatusEffects.ContainsKey(fx))
				existingDuration = creature.StatusEffects[fx];

			// compute additional duration
			var addedDuration = MathX.Pythagoras(existingDuration, duration) - existingDuration;
			if (duration < 0)
				addedDuration *= -1;

			// set status effect duration
			creature.StatusEffects[fx] = existingDuration + addedDuration;
			if (creature.StatusEffects[fx] <= 0)
				creature.StatusEffects.Remove(fx);

			// retunr additional duration
			return addedDuration;
		}

		/// <summary>
		/// Does this creature have a particular status effect?
		/// </summary>
		/// <param name="creature">The creature to check.</param>
		/// <param name="fx">The status effect to check for.</param>
		/// <returns>true if the creature has the status effect, otherwise false.</returns>
		public static bool HasStatusEffect(this ICreature creature, StatusEffect fx)
			=> creature.StatusEffects.ContainsKey(fx) && creature.StatusEffects[fx] > 0;

		/// <summary>
		/// Drains mana from one creature and gives it to another.
		/// </summary>
		/// <param name="caster">The creature performing the drain.</param>
		/// <param name="target">The creature being drained.</param>
		/// <param name="drain">The amount of mana to drain.</param>
		/// <returns>The amount of mana drained.</returns>
		public static int DrainManaFrom(this ICreature caster, ICreature target, int drain)
		{
			drain = Math.Min(target.Mana, drain);
			target.Mana -= drain;
			caster.Mana += drain;
			if (caster.Mana > caster.MaxMana)
				caster.Mana = caster.MaxMana;
			return drain;
		}

		/// <summary>
		/// Heals a creature.
		/// </summary>
		/// <param name="creature">The creature to heal.</param>
		/// <param name="heal">How many HP to heal.</param>
		/// <returns>HP healed.</returns>
		public static int Heal(this ICreature creature, int heal)
		{
			var actualHeal = Math.Min(heal, creature.MaxHitpoints - creature.Hitpoints);
			creature.Hitpoints += actualHeal;
			return actualHeal;
		}

		/// <summary>
		/// Stuns a creature, preventing it from acting temporarily.
		/// Stunning is less effective if the creature is already stunned.
		/// </summary>
		/// <param name="creature">The creature to stun.</param>
		/// <param name="stun">How long to stun.</param>
		/// <returns>How long the creature was stunned./returns>
		public static double Stun(this ICreature creature, int stun)
		{
			var actualStun = MathX.Pythagoras(creature.Timer, stun) - creature.Timer;
			creature.Timer += actualStun;
			return actualStun;
		}

		/// <summary>
		/// Drains HP from one creature and gives it to another.
		/// </summary>
		/// <param name="caster">The creature performing the drain.</param>
		/// <param name="target">The creature being drained.</param>
		/// <param name="drain">The number of HP to drain.</param>
		/// <returns>The number of HP drained.</returns>
		public static int DrainHPFrom(this ICreature caster, ICreature target, int drain)
		{
			drain = Math.Min(target.Hitpoints, drain);
			target.Hitpoints -= drain;
			if (target.Hitpoints <= 0)
				caster.Kill(target);
			caster.Hitpoints += drain;
			if (caster.Hitpoints > caster.MaxHitpoints)
				caster.Hitpoints = caster.MaxHitpoints;
			return drain;
		}

		/// <summary>
		/// Drains essences from one creature and gives them to another.
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="target"></param>
		/// <param name="drain"></param>
		/// <returns>The total number of essences drained.</returns>
		public static int DrainEssencesFrom(this ICreature caster, ICreature target, int? drain = null)
		{
			if (drain is null)
			{
				if (target is Hero)
					drain = int.MaxValue; // who cares, he's dying anyway
				else if (target is Monster m)
				{
					drain = m.Difficulty;
					if (m.HasFlag(MonsterFlags.Unique))
						drain *= 3; // a nice bonus for killing uniques!
				}
				else
					drain = 0; // huh? what kind of creature is not a hero or a monster?
			}
			var totalDrain = 0;
			foreach (var element in target.Elements)
			{
				var matchingElement = caster.Elements.FirstOrDefault(q => q.GetType() == element.GetType());
				if (matchingElement is not null)
				{
					var thisDrain = Math.Min(drain.Value, element.Essences);
					totalDrain += thisDrain;
					element.Essences -= thisDrain;
					matchingElement.Essences += thisDrain;
					Logger.LogEssenceDrain(caster, target, matchingElement, element, thisDrain);
				}
			}
			return totalDrain;
		}

		/// <summary>
		/// Gets the number of essences possessed of a particular element.
		/// Pass in the <see cref="Element"/> base class to add up all elements.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static int GetEssences<T>(this ICreature creature)
			where T : Element
		{
			return creature.Elements.OfType<T>().Sum(q => q.Essences);
		}

		/// <summary>
		/// Restores a creature's mana.
		/// </summary>
		/// <param name="creature">The creature to restore.</param>
		/// <param name="heal">How many MP to restore.</param>
		/// <returns>MP restored.</returns>
		public static int RestoreMana(this ICreature creature, int mana)
		{
			var actualMana = Math.Min(mana, creature.MaxMana - creature.Mana);
			creature.Mana += actualMana;
			return actualMana;
		}
	}
}
