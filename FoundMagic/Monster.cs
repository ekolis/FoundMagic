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
			VisibleTiles = Enumerable.Empty<Tile>();
		}

		public MonsterType Type { get; }

		public char Glyph => Type.Glyph;
		public Color Color => Type.Color;
		public FieldOfView FieldOfView { get; set; }
		public int Vision => Type.Vision;

		public double Speed => Type.Speed;

		public double Timer { get; set; }

		public void UpdateFov()
		{
			var floor = Floor.Current;
			if (floor is null)
				return;

			if (FieldOfView is null)
				FieldOfView = new FieldOfView(floor);

			var newtile = floor.Find(this);
			var fovData = FieldOfView.ComputeFov(newtile.X, newtile.Y, Vision, true);
			var visibleTiles = new List<Tile>();
			foreach (var cell in fovData)
				visibleTiles.Add(floor.Tiles[cell.X, cell.Y]);
			VisibleTiles = visibleTiles;
		}

		private IEnumerable<Tile> VisibleTiles { get; set; }

		/// <summary>
		/// The monster will attempt to move toward the hero and attack him.
		/// </summary>
		/// <returns></returns>
		public double Act()
		{
			Floor floor = Floor.Current;
			var tile = floor.Find(this);
			UpdateFov();
			var heroTile = VisibleTiles.SingleOrDefault(q => q.Creature is Hero);
			if (heroTile is not null)
			{
				// monster can see hero, pursue him
				Path path = new PathFinder(floor).ShortestPath(tile, heroTile);
				Tile nextTile = floor.Tiles[path.CurrentStep.X, path.CurrentStep.Y];
				if (floor.Move(tile, nextTile))
					return 1.0 / Speed;
			}
			else
			{
				// monster can't see hero
				// for now let's just sit still
				// TODO: pursue last known location of hero? wander in a direction until we hit a wall, then turn?
			}
			
			// didn't do anything? just pass a turn
			return 1.0 / Speed;
		}
	}
}
