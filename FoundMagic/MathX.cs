using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic
{
	/// <summary>
	/// Handy mathematical equations and stuff.
	/// </summary>
	public static class MathX
	{
		/// <summary>
		/// Computes the Pythagorean theorem: a^2 + b^2 = c^2
		/// </summary>
		/// <param name="dimensions">All dimensions to aggregate.</param>
		/// <returns>The square root of the sum of the squares.</returns>
		public static double Pythagoras(params double[] dimensions)
		{
			return Math.Sqrt(dimensions.Select(q => q * q).Sum());
		}
	}
}
