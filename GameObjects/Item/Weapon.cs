using System;
using System.Collections.Generic;

namespace DazzleADV
{

	public interface IAttackable
	{
		int AverageDamage { get; }
		int Damage { get; }
		List<StatusEffect> GetStatusEffects();
	}

	public class Weapon : Item, IAttackable, ISocketed
	{

		private int minDamage;
		private int maxDamage;
		public int AverageDamage => (int)Math.Round((double)((minDamage + maxDamage) / 2));
		public int Damage => (int)(rng.Next(maxDamage - minDamage + 1)) + minDamage;

		public List<ISocketable> Gemstones { get; private set; }
		private StatusEffect statusEffect;
		public int Sockets { get; private set; }

		public Weapon(Enum template, bool isNamed, string titleName, string setName, string generalName, string description,
			int minDamage, int maxDamage, StatusEffect statusEffect = null, int sockets = 0) :
				base(template, isNamed, titleName, setName, generalName, description)
		{
			if (minDamage < 0)
				throw new ArgumentOutOfRangeException("Error: Weapon constructor minDamage must be >= 0.");
			if (maxDamage < 0)
				throw new ArgumentOutOfRangeException("Error: Weapon constructor maxDamage must be >= 0.");
			if (sockets < 0 || sockets > 5)
				throw new ArgumentOutOfRangeException("Error: Item constructor sockets must be between 0 and 5 (inclusive).");

			this.minDamage = minDamage;
			this.maxDamage = maxDamage;

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
				throw new ArgumentNullException("Error: Weapon.Augment null gemstone");
			if (!gemstone.CanSocketWeapons)
				return false;

			if (HasOpenSockets())
			{
				Gemstones.Add(gemstone);
				this.minDamage += gemstone.Power;
				this.maxDamage += gemstone.Power;
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
			return $"[{Title}({AverageDamage}]";
		}

	}

}