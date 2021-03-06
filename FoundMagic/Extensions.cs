using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp.Random;

namespace FoundMagic
{
	public static class Extensions
	{
		/// <summary>
		/// Picks a random element from a sequence.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="random"></param>
		/// <param name="choices"></param>
		/// <returns></returns>
		public static T Pick<T>(this IRandom random, IEnumerable<T> choices)
		{
			int cnt = choices.Count();
			int idx = random.Next(0, cnt - 1);
			T item = choices.ElementAt(idx);
			return item;
		}
	}
}
