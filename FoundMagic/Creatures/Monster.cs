using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundMagic.Magic;
using FoundMagic.Mapping;
using FoundMagic.UI;
using RogueSharp;

namespace FoundMagic.Creatures
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
			Hitpoints = MaxHitpoints;
			Mana = MaxMana;
			type.NumberSpawned++;
		}

		public MonsterType Type { get; }

		public string Name => Type.HasFlag(MonsterFlags.Unique) ? Type.Name : $"the {Type.Name}";
		public char Glyph => Type.Glyph;
		public Color Color => Elements.Select(q => q.Color).Average();
		public FieldOfView FieldOfView { get; set; }
		public int Vision => Type.Vision;

		public double Speed
			=> Type.Speed / (this.HasStatusEffect(StatusEffect.Slow) ? 2 : 1);

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
		/// The monster will attempt to cast spells or pursue the hero and attack him.
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
				// monster can see hero, do stuff!

				// see if we can cast any spells that might be useful
				var castableElements = Elements.Where(q => q.BaseManaCost <= Mana);
				if (castableElements.Any())
				{
					// monster can cast spells, should we cast them?
					var attack = castableElements.OfType<Fire>().Concat<Element>(castableElements.OfType<Darkness>());
					var effect = castableElements.OfType<Earth>().Concat<Element>(castableElements.OfType<Air>());
					var healing = castableElements.OfType<Light>();
					var mana = castableElements.OfType<Water>();

					// is the hero in an orthogonal direction? then attack him with a spell!
					var dir = tile.CheckOrthogonality(heroTile);
					Element? spellElement = null;
					if (dir is not null)
					{
						// try to attack first, if we can't do that then apply an effect
						if (attack.Any())
							spellElement = World.Instance.Rng.Pick(attack);
						else if (effect.Any())
							spellElement = World.Instance.Rng.Pick(effect);

						// override: if we're low on mana, try to restore our mana!
						if (Mana <= MaxMana / 3 && mana.Any())
							spellElement = World.Instance.Rng.Pick(mana);

						if (spellElement is not null)
						{
							// ok, now cast the spell!
							spellElement.Cast(this, dir, 1, 1);
							return 1.0 / Speed;
						}
					}

					// low on HP? heal ourselves
					if (Hitpoints <= MaxHitpoints / 3 && healing.Any())
					{
						World.Instance.Rng.Pick(healing).Cast(this, Direction.Stationary, 1, 1);
						return 1.0 / Speed;
					}
				}

				// we couldn't cast a spell? then pursue the hero, or attack him if he's next to us!
				Path path = new PathFinder(floor).ShortestPath(tile, heroTile);
				var step = path.StepForward();
				Tile nextTile = floor.Tiles[step.X, step.Y];
				if (nextTile.Creature is not Monster)
					floor.Move(tile, nextTile);
				return 1.0 / Speed;
			}
			else
			{
				// monster can't see hero
				// for now let's just sit still
				// TODO: pursue last known location of hero? wander in a direction until we hit a wall, then turn?
				return 1.0 / Speed;
			}
		}

		public int Strength => Type.Strength;

		public int MaxHitpoints => Type.MaxHitpoints;

		public int Hitpoints { get; set; }

		public override string ToString()
			=> Name;

		public int MaxMana => Type.MaxMana;

		public int Mana { get; set; }

		public IEnumerable<Element> Elements => Type.Elements;

		public IDictionary<StatusEffect, double> StatusEffects { get; } = new Dictionary<StatusEffect, double>();

		public MonsterFlags Flags => Type.Flags;

		public bool HasFlag(MonsterFlags f)
			=> Type.HasFlag(f);

		public int Difficulty => Type.Difficulty;
	}
}
