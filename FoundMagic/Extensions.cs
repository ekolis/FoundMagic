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

		/// <summary>
		/// Picks a random element from a sequence using weights.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="random"></param>
		/// <param name="choices"></param>
		/// <param name="weightSelector"></param>
		/// <returns></returns>
		public static T PickWeighted<T>(this IRandom random, IEnumerable<T> choices, Func<T, int> weightSelector)
		{
			var list = new List<(T Item, int Weight, int CumulativeWeight)>();
			int cumulativeWeight = 0;
			foreach (var item in choices)
			{
				var weight = weightSelector(item);
				cumulativeWeight += weight;
				list.Add((item, weight, cumulativeWeight));
			}
			var num = random.Next(0, list.Last().CumulativeWeight);
			var match = list.Last(q => q.CumulativeWeight > num);
			return match.Item;

		}
	}
}
