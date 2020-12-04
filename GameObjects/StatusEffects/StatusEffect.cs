using System;

namespace DazzleADV
{
	public enum EffectClass { Dysentery, Poison, Sleep, Paralysis, Decay, Regen }

	public class StatusEffect
	{
		public Enum EffectClass { get; private set; }
		public int Value { get; set; }
		public Action<Player, StatusEffect> OnTurnTick { get; private set; }
		public Action<Player, StatusEffect> OnExpire { get; private set; }
		public Action<Player, StatusEffect> OnAttackHit { get; private set; }

		public bool DisablesActions { get; private set; }

		public StatusEffect(Enum effectClass, Action<Player, StatusEffect> turnTickEvent = null, Action<Player, StatusEffect> attackedEvent = null,
			Action<Player, StatusEffect> effectExpiresEvent = null, Action<Player, StatusEffect> attackHitEvent = null, int effectValue = -1)
		{			
			if (turnTickEvent == null)
				turnTickEvent = StatusEvents.DoNothing;
			if (attackedEvent == null)
				attackedEvent = StatusEvents.DoNothing;
			if (effectExpiresEvent == null)
				effectExpiresEvent = StatusEvents.DoNothing;
			if (attackHitEvent == null)
				attackHitEvent = StatusEvents.DoNothing;
			if (effectValue < 0)
				effectValue = -1;

			this.EffectClass = effectClass;
			this.OnTurnTick = turnTickEvent;
			this.OnExpire = effectExpiresEvent;
			this.OnAttackHit = attackHitEvent;
			this.Value = effectValue;
		}

		public void Kill()
		{
			this.Value = 0;
		}

		public bool IsExpired(Player player, StatusEffect se)
		{
			if (this.Value == 0)
				OnExpire(player, this);
			return this.Value == 0;
		}

		public void Combine(StatusEffect other)
		{
			this.Value += other.Value;
		}

		public bool Matches(StatusEffect other)
		{
			if (!this.EffectClass.Equals(other.EffectClass)) return false;
			if (!this.OnTurnTick.Equals(other.OnTurnTick)) return false;
			if (!this.OnExpire.Equals(other.OnExpire)) return false;
			return true;
		}

		public override string ToString()
		{
			if (Value > 0)
				return $"[{EffectClass}({Value})]";
			return $"[{EffectClass}]";
		}

	}

}