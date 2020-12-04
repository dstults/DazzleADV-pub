using System;
using System.Collections.Generic;

namespace DazzleADV
{

	public interface ISocketed : IItemizable
	{
		List<ISocketable> Gemstones { get; }
		int Sockets { get; }
		bool HasOpenSockets();
		void AddSocket(int sockets = 1);
		bool Augment(ISocketable gem);
		List<StatusEffect> GetStatusEffects();
	}

	public interface ISocketable : IItemizable
	{
		bool CanSocketWeapons { get; }
		bool CanSocketArmor { get; }
		int Power { get; }
		StatusEffect StatusEffect { get; }
		void LosePower();
		void LoseEffect();
	}

	public class Gemstone : Item, ISocketable
	{

		public bool CanSocketWeapons { get; private set; }
		public bool CanSocketArmor { get; private set; }

		public int Power { get; private set;  }
		public StatusEffect StatusEffect { get; private set;  }

		public Gemstone(Enum template, bool isNamed, string titleName, string setName, string generalName, string description,
			int power = 0, StatusEffect statusEffect = null, bool socketsWeapons = true, bool socketsArmor = true) :
				base(template, isNamed, titleName, setName, generalName, description)
		{
			if (!socketsWeapons && !socketsArmor)
				throw new ArgumentOutOfRangeException("Error: Gemstone constructor socketables must be able to at least either socket weapons or armor");
			// power can be negative
			if (power == 0 && statusEffect == null)
				throw new ArgumentOutOfRangeException("Error: Gemstone constructor ineffective gemstone error, power set to 0 and status modifier set to none");

			this.CanSocketWeapons = socketsWeapons;
			this.CanSocketArmor = socketsArmor;
			this.Power = power;
			this.StatusEffect = statusEffect;
		}

		public void LosePower()
		{
			Power = 0;	
		}

		public void LoseEffect()
		{
			StatusEffect = null;
		}

	}

}