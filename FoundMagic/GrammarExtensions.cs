using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic
{
	/// <summary>
	/// Extension methods relating to grammar.
	/// </summary>
	public static class GrammarExtensions
	{
		/// <summary>
		/// Capitalizes the first letter of an object's string representation.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string? Capitalize(this object? o)
		{
			if (o is null)
				return null;

			string? s = o.ToString();
			if (string.IsNullOrWhiteSpace(s))
				return s;

			return s[0].ToString().ToUpper() + s.Substring(1);
		}

		/// <summary>
		/// Lowercases an object's string representation.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static string? Lowercase(this object? o)
		{
			if (o is null)
				return null;

			string? s = o.ToString();
			if (string.IsNullOrWhiteSpace(s))
				return s;

			return s.ToLower();
		}
	}
}
