using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;

namespace FoundMagic
{
	/// <summary>
	/// Rawr! It's out to kill you! 💀
	/// </summary>
	public class Monster
		: ICreature
	{
		public Monster(MonsterType type)
		{
			Type = type;
		}

		public MonsterType Type { get; }

		public char Glyph => Type.Glyph;
		public Color Color => Type.Color;
		public FieldOfView FieldOfView { get; set; }
		public int Vision => Type.Vision;

		public double Speed => Type.Speed;

		public void UpdateFov()
		{
			throw new NotImplementedException();
		}
	}
}
