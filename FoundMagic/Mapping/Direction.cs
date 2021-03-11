using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundMagic.Mapping
{
	public record Direction(string Name, int DX, int DY)
	{
		public static readonly Direction Stationary = new("stationary", 0, 0);
		public static readonly Direction North = new("north", 0, -1);
		public static readonly Direction South = new("south", 0, 1);
		public static readonly Direction West = new("west", -1, 0);
		public static readonly Direction East = new("east", 1, 0);

		public static IEnumerable<Direction> All { get; } = new Direction[] { Stationary, North, South, West, East };

		public override string ToString()
			=> Name;
	}
}
