using System;
using System.Collections.Generic;

namespace DazzleADV
{

	public interface IDefensible
	{
		int AverageDefense { get; }
		int Defense { get; }
		List<StatusEffect> GetStatusEffects();
	}

	public class Armor : Item, IDefensible, ISocketed
	{
		
		private int minDefense;
		private int maxDefense;
		public int AverageDefense => (int)((minDefense + maxDefense)/ 2);
		public int Defense => (int)(rng.Next(maxDefense - minDefense + 1)) + minDefense;

		public List<ISocketable> Gemstones { get; private set; }
		private StatusEffect statusEffect;
		public int Sockets { get; private set; }

		public Armor(Enum template, bool isNamed, string titleName, string setName, string generalName, string description,
			int minDefense, int maxDefense, StatusEffect statusEffect = null, int sockets = 0) :
				base(template, isNamed, titleName, setName, generalName, description)
		{
			if (minDefense < 0)
				throw new ArgumentOutOfRangeException("Error: Armor constructor minDefense must be >= 0");
			if (maxDefense < 0)
				throw new ArgumentOutOfRangeException("Error: Armor constructor maxDefense must be >= 0");
			if (sockets < 0 || sockets > 5)
				throw new ArgumentOutOfRangeException("Error: Item constructor sockets must be between 0 and 5 (inclusive).");

			this.minDefense = minDefense;
			this.maxDefense = maxDefense;

			if (statusEffect != null)
				this.statusEffect = statusEffect;

			this.Sockets = sockets;
			this.Gemstones = new List<ISocketable>();
		}

		public bool HasOpenSockets()
		{
			return this.Sockets > Gemstones.Count;
		}

		public void AddSocket(int sockets = 1)
		{
			if (sockets < 1)
				throw new ArgumentOutOfRangeException("Error: Item.AddSockets sockets must be >= 1");

			this.Sockets += sockets;
		}

		public bool Augment(ISocketable gemstone)
		{
			if (gemstone == null)
				throw new ArgumentNullException("Error: Armor.Augment null gemstone");
			if (!gemstone.CanSocketArmor)
				return false;

			if (HasOpenSockets())
			{
				Gemstones.Add(gemstone);
				this.minDefense += gemstone.Power;
				this.maxDefense += gemstone.Power;
				return true;
			}
			return false;
		}

		public List<StatusEffect> GetStatusEffects()
		{
			List<StatusEffect> myEffects = new List<StatusEffect>();
			if (this.statusEffect != null)
				myEffects.Add(this.statusEffect);
			foreach(ISocketable gem in Gemstones)
			{
				if (gem.StatusEffect != null)
					myEffects.Add(gem.StatusEffect);
			}
			return myEffects;
		}

		public void RemoveExpiredStatusEffects(Player equippedPlayer)
		{
			if (equippedPlayer == null)
				throw new ArgumentNullException("Error: Weapon.RemoveExpiredStatusEffects equippedPlayer null");

			if (this.statusEffect != null)
				if (this.statusEffect.IsExpired(equippedPlayer, this.statusEffect))
					this.statusEffect = null;
			foreach(ISocketable gem in Gemstones)
			{
				if (gem.StatusEffect != null)
					if (gem.StatusEffect.IsExpired(equippedPlayer, gem.StatusEffect))
						gem.LoseEffect();
			}
		}

		public override string GuiString()
		{
			return $"[{Title}({maxDefense})]";
		}

	}

}