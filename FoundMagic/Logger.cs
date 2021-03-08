﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
