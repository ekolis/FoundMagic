using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoundMagic.Magic;
using RogueSharp;
using FoundMagic.Mapping;
using FoundMagic.UI;

namespace FoundMagic.Creatures
{
	/// <summary>
	/// Our intrepid hero, exploring the world... 🧙🏼‍
	/// </summary>
	public class Hero
		: ICreature
	{
		/// <summary>
		/// Hero is a singleton, so the constructor is private.
		/// </summary>
		private Hero()
		{
			Hitpoints = MaxHitpoints;
			Mana = MaxMana;
		}

		/// <summary>
		/// The singleton instance of the hero.
		/// </summary>
		public static Hero Instance { get; } = new Hero();

		public string Name
			=> "you";

		public char Glyph { get; } = '@';

		public Color Color => Elements.Select(q => q.Color).Average();

		public FieldOfView? FieldOfView { get; set; }

		public void UpdateFov()
		{
			var floor = Floor.Current;
			if (floor is null)
				return;

			if (FieldOfView is null)
				FieldOfView = new FieldOfView(floor);

			var newtile = floor.Find(this);
			var fovData = FieldOfView.ComputeFov(newtile.X, newtile.Y, Vision, true);
			foreach (var tile in floor.Tiles)
				floor.Tiles[tile.X, tile.Y] = floor.Tiles[tile.X, tile.Y].WithInvisible();
			foreach (var fovCell in fovData)
				floor.Tiles[fovCell.X, fovCell.Y] = floor.Tiles[fovCell.X, fovCell.Y].WithVisible();
		}

		public void ResetFov()
			=> FieldOfView = null;

		public double Act()
		{
			if (Floor.Current is null)
				return 0; // nothing to do

			// get globals
			Floor floor = Floor.Current;

			// which way are we moving/casting?
			var dir = Keyboard.ActionDirection;

			// shift is used for climbing stairs
			bool shift = Keyboard.IsKeyPressed(Keys.ShiftKey);

			// HACK: why is this necessary?
			Keyboard.Reset();

			bool success = false;

			if (Spell is not null)
			{
				if (Spell.CanCast)
				{
					// we are casting a spell
					Spell.Cast();
					success = true;
				}
				else
				{
					// not enough attunement from essences
					Logger.LogSpellFizzle(this);
					success = false;
				}
			}
			else if (dir is not null)
			{
				if (shift)
				{
					if (dir == Direction.Stationary)
					{
						// holding shift while standing still climbs stairs
						// because > is shift plus .
						success = ClimbStairs();
					}
					else
					{
						// can't use shift with other directions besides stationary
						success = false;
					}
				}
				else
				{
					// move hero
					success = floor.Move(this, dir, true) > 0;
				}
			}

			if (success)
			{
				// update the hero's field of view
				UpdateFov();

				// TODO: maybe log any spotted monsters so the player knows what they are? maybe even the direction in which they are spottted?

				// spend time
				return 1.0 / Speed;
			}

			return 0;
		}

		public int Vision { get; } = 3;

		public double Speed
			=> (1d + EssenceBoostSpeed) / (this.HasStatusEffect(StatusEffect.Slow) ? 2 : 1);

		public double Timer { get; set; }

		public int Strength { get; } = 1;

		public int MaxHitpoints => 10 + EssenceBoostMaxHitpoints;

		public int Hitpoints { get; set; }

		public int MaxMana => 10 + EssenceBoostMaxMana;

		public int Mana { get; set; }

		public override string ToString()
			=> Name;

		/// <summary>
		/// When did the hero die? Or null if he didn't.
		/// </summary>
		public DateTime? DeathTimestamp { get; set; }

		/// <summary>
		/// How long should the screen take to fade when the hero dies?
		/// </summary>
		public TimeSpan DeathFadeTime { get; } = new TimeSpan(0, 0, 5);

		/// <summary>
		/// Is the spellcasting interface open?
		/// </summary>
		public bool IsCasting { get; set; } = false;

		private Element[]? elements;

		public IEnumerable<Element> Elements
		{
			get
			{
				if (elements is null)
				{
					// you get one attack spell
					var fire = new Fire(40);
					var darkness = new Darkness(40);
					World.Instance.Rng.Pick(new Element[] { fire, darkness }).Essences = Element.EssencesForStandardAttunement;

					// and one utility spell
					var air = new Air(40);
					var earth = new Earth(40);
					var water = new Water(40);
					var light = new Light(40);
					World.Instance.Rng.Pick(new Element[] { air, earth, water, light }).Essences = Element.EssencesForStandardAttunement;

					// set up elements list
					elements = new Element[] { fire, earth, air, water, light, darkness };
				}
				return elements;
			}
		}

		/// <summary>
		/// The magic word currently being typed/cast.
		/// </summary>
		public string SpellWord { get; set; } = "";

		/// <summary>
		/// The spell that the hero is casting.
		/// </summary>
		public Spell? Spell { get; set; }

		/// <summary>
		/// How long did it take the player to type the magic words to cast a spell?
		/// </summary>
		public TimeSpan? SpellDuration
			=> DateTime.Now - SpellTimestamp;

		/// <summary>
		/// When did the player start typing the magic words to cast a spell?
		/// </summary>
		public DateTime? SpellTimestamp { get; set; }

		public IDictionary<StatusEffect, double> StatusEffects { get; } = new Dictionary<StatusEffect, double>();

		/// <summary>
		/// Attempts to climb stairs, if present.
		/// </summary>
		/// <returns>true if successful, otherwise false.</returns>
		public bool ClimbStairs()
		{
			if (Floor.Current.Find(this).Terrain == Terrain.Stairs)
			{
				// there are stairs here, so let's generate a new floor!
				World.Instance.GenerateNextFloor();
				if (EssenceBoostRegeneration > 0)
				{
					var healing = this.Heal(EssenceBoostRegeneration);
					Logger.LogHealing(this, GetElement<Light>(), healing);
					var manaRestoration = this.RestoreMana(EssenceBoostRegeneration);
					Logger.LogManaRestoration(this, GetElement<Light>(), manaRestoration);
				}
				IsClimbing = true;
				return true;
			}
			else
			{
				// no stairs here to climb.
				return false;
			}
		}

		/// <summary>
		/// Are we currently climbing stairs? If so, let's not move any monsters on the old floor, that won't work...
		/// </summary>
		public bool IsClimbing { get; set; }

		public T GetElement<T>()
			where T : Element
			=> Elements.OfType<T>().Single();

		/// <summary>
		/// Gets the attunement value of an element.
		/// </summary>
		/// <typeparam name="T">The element to check.</typeparam>
		/// <returns>The attunement.</returns>
		public double GetAttunement<T>()
			where T : Element
			=> Elements.OfType<T>().Max(q => q.Attunement);

		/// <summary>
		/// Fire essences boost your spell power.
		/// </summary>
		public double EssenceBoostSpellPower => GetAttunement<Fire>() * 0.20;

		/// <summary>
		/// Earth essences boost your max HP.
		/// </summary>
		public int EssenceBoostMaxHitpoints => (int)Math.Round(GetAttunement<Earth>() * 5);

		/// <summary>
		/// Air essences boost your speed.
		/// </summary>
		public double EssenceBoostSpeed => GetAttunement<Air>() * 0.10;

		/// <summary>
		/// Water essences boost your max mana.
		/// </summary>
		public int EssenceBoostMaxMana => (int)Math.Round(GetAttunement<Water>() * 5);

		/// <summary>
		/// Light essences let you regenerate some of your HP and mana when you climb stairs.
		/// </summary>
		public int EssenceBoostRegeneration => (int)Math.Round(GetAttunement<Light>() * 5);

		/// <summary>
		/// Darkness essences give you a chance for critical hits from melee attacks and spells.
		/// </summary>
		public double EssenceBoostCriticalHits => GetAttunement<Darkness>() * 0.10;
	}
}
