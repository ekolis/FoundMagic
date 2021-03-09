using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundMagic.Magic;

namespace FoundMagic
{
	/// <summary>
	/// Logs things for the player to read.
	/// </summary>
	public static class Logger
	{
		/// <summary>
		/// Logs a message with a color.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="color"></param>
		public static void Log(string message, Color color)
		{
			Entries.Add(new(message, DateTime.Now, color));
		}
		
		/// <summary>
		/// Logs an attack.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		public static void LogAttack(ICreature attacker, ICreature target, int damage)
		{
			if (attacker is Hero)
				Log($"{attacker.Capitalize()} attack {target}. ({damage} dmg, {target.Hitpoints} HP left)", Color.White);
			else // monster
				Log($"{attacker.Capitalize()} attacks {target}. ({damage} dmg, {target.Hitpoints} HP left)", Color.Yellow);
		}

		/// <summary>
		/// Logs a death.
		/// </summary>
		/// <param name="decedent"></param>
		public static void LogDeath(ICreature decedent)
		{
			if (decedent is Hero)
				Log($"{decedent.Capitalize()} die...", Color.Red);
			else
				Log($"{decedent.Capitalize()} dies.", Color.Cyan);
		}

		public static void LogSpellCast(ICreature caster, double power, double efficiency, params Element[] elements)
		{
			string powerStr = "", efficencyStr = "";
			var thresholdLow = 1.0 / 3.0;
			var thresholdHigh = 2.0 / 3.0;
			if (power >= thresholdHigh)
				powerStr = "powerful";
			else if (power <= thresholdLow)
				powerStr = "weak";
			if (efficiency >= thresholdHigh)
				efficencyStr = "keen";
			if (efficiency <= thresholdLow)
				efficencyStr = "blunt";
			var desc = string.Join(" ", new string[] { powerStr, efficencyStr }).Trim();
			if (desc == "")
				desc = "standard";
			if (caster is Hero)
				Log($"{caster.Capitalize()} cast a {desc} spell: {string.Join<Element>("/", elements)} ({elements.Last().GetManaCost(efficiency)} MP).", elements.Last().Color);
			else
				Log($"{caster.Capitalize()} casts a {desc} spell: {string.Join<Element>("/", elements)} ({elements.Last().GetManaCost(efficiency)} MP).", elements.Last().Color);
		}

		public static void LogSpellDamage(ICreature target, Element element, int dmg)
		{
			if (element is Fire)
				Log($"{target.Capitalize()} {(target is Hero ? "are" : "is")} burned! ({dmg} dmg, {target.Hitpoints} HP left)", element.Color);
			else
				Log($"{target.Capitalize()} {(target is Hero ? "are" : "is")} hit with an unknown element! ({dmg} dmg, {target.Hitpoints} HP left)", element.Color);
		}

		public static void LogSpellFizzle(ICreature caster)
		{
			Log($"{caster.Capitalize()} {(caster is Hero ? "try" : "tries")} to cast a spell, but it fizzles.", Color.White);
		}

		public static void LogStatusEffectStart(ICreature creature, Element element, StatusEffect fx, double duration)
		{
			var desc = fx switch
			{
				StatusEffect.Slow => "slowed",
				_ => "afflicted with a bizarre, never before seen status effect (did someone forget to write a description?)"
			};
			Log($"{creature.Capitalize()} {(creature is Hero ? "are" : "is")} {desc} for {Math.Round(duration)} turn(s).", element.Color);
		}

		/// <summary>
		/// Gets a list of all unexpired entries.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Entry> List()
		{
			var now = DateTime.Now;
			Entries = Entries.Where(q => now - q.Timestamp < Duration).ToList();
			return Entries;
		}

		public static TimeSpan Duration { get; } = new(0, 0, 5);

		private static IList<Entry> Entries { get; set; } = new List<Entry>();

		/// <summary>
		/// A log entry.
		/// </summary>
		public record Entry(string Message, DateTime Timestamp, Color Color);
	}
}
