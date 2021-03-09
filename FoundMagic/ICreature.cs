﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FoundMagic.Magic;
using RogueSharp;

namespace FoundMagic
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
			Logger.LogAttack(attacker, target, attacker.Strength);
			return target.TakeDamage(attacker.Strength);
		}

		/// <summary>
		/// Allows a creature to take damage.
		/// </summary>
		/// <param name="target">The creature taking damage.</param>
		/// <param name="dmg">The amount of damage to inflict.</param>
		/// <returns>The amount of damage taken.</returns>
		public static int TakeDamage(this ICreature target, int dmg)
		{
			target.Hitpoints -= dmg;
			if (target.Hitpoints <= 0)
			{
				dmg += target.Hitpoints; // overkill, report smaller number of HP lost
				target.Hitpoints = 0;
				target.Kill();
			}
			return dmg;
		}

		/// <summary>
		/// Kills a creature, removing it from the game.
		/// </summary>
		/// <param name="decedent">The creature to kill.</param>
		public static void Kill(this ICreature decedent)
		{
			decedent.Hitpoints = 0;
			Floor.Current.Find(decedent).Creature = null;
			Logger.LogDeath(decedent);
			if (decedent is Hero h)
				h.DeathTimestamp = DateTime.Now;
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
	}
}
