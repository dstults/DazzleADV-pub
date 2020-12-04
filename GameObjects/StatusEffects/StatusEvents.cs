using System;

namespace DazzleADV
{
	public static class StatusEvents
	{

		public static void DoNothing(Player player, StatusEffect se)
		{}

		public static void SleeperTakesDamage(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} is awoken by pain!");
			se.Value = 0;
		}

		public static void CorpseRot(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} is dissolving away...");
			se.Value -= 1;
			player.Notify($"  *  Your [Despawn Time]: -1 => {se.Value}");
		}

		public static void PoisonHit(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} was injected with poison!");
			player.AddEffect(new StatusEffect(EffectClass.Poison, turnTickEvent: PoisonPlayer, effectExpiresEvent: PoisonCuredByTime, effectValue: se.Value));
		}

		public static void PoisonPlayer(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} takes damage from poison!");
			player.DamageHP(se.Value, "poison", null);
			se.Value -= 1;
		}

		public static void SleepPlayer(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} is blissfully resting!");
			player.HealHP(2);
		}

		public static void DysenteryTick(Player player, StatusEffect se)
		{
			if (se.Value > 6)
			{
				player.Notify($"  *  Dysentery damages your internal organs!");
				player.DamageHP((int)Math.Ceiling((((double)se.Value - 6) / 5)), "dysentery", null);
			}
			se.Value -= 1;
		}

		public static void RegenPlayer(Player player, StatusEffect se)
		{
			player.HealHP(1);
		}

		public static void ParalysisCured(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} is no longer paralyzed!");
		}

		public static void SleepCuredByTime(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} has naturally woken up!");
		}

		public static void DysenteryCured(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} has recovered from {player.Genderize("his", "her", "its")} case of dysentery!");
		}

		public static void PoisonCuredByTime(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} has overcome {player.Genderize("his", "her", "its")} poison!");
		}

		public static void CorpseRotTimesUp(Player player, StatusEffect se)
		{
			GameEngine.SayToLocation(player.Location, $"{player.Name} has completely dissolved into dust!");
		}

	}

}