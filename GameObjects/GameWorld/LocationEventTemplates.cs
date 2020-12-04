using System;

namespace DazzleADV
{

	public static class LocationEvents
	{

		private static Random rng = new Random();

		public static LocationSelfEvent BirdsFlyBy = new LocationSelfEvent(15, null, LocationEvents.DoBirdsFlyBy);
		public static LocationSelfEvent FrogsRibbit = new LocationSelfEvent(30, null, LocationEvents.DoFrogsRibbit);
		public static LocationPlayerEvent BadFeelingAboutBridge = new LocationPlayerEvent(null, LocationEvents.DoBadFeelingAboutBridge);

		public static void DoNothing(Player player) {}

		private static void DoBirdsFlyBy(Location location)
		{
			string text = "";
			switch (rng.Next(11))
			{
				case 0:
					text = $"{FlavorText.ARandomBird()} goes flying past overhead.";
					break;
				case 1:
					text = "  *  You see a large eagle overhead.";
					break;
				case 2:
					text = "  *  You hear the chirping of birds.";
					break;
				case 3:
					text = "  *  The chirping of birds fills the air.";
					break;
				case 4:
					text = $"  *  You hear a fluttering of wings {FlavorText.ToSomewhere()}.";
					break;
				case 5:
					text = "  *  Some leaves rustle as two birds fly through them.";
					break;
				case 6:
					text = $"{FlavorText.ARandomBird()} flies through the air.";
					break;
				case 7:
				case 8:
				case 9:
				case 10:
					text = $"{FlavorText.ARandomBird()} can be seen flying {FlavorText.ToSomewhere()}.";
					break;
			}
			GameEngine.SayToLocation(location, text);
		}

		private static void DoFrogsRibbit(Location location)
		{
			string text = "";
			switch (rng.Next(6))
			{
				case 0:
					text = $"  *  You hear a frog croaking {FlavorText.FromSomewhere()}.";
					break;
				case 1:
					text = "A loud 'ribbit' bursts out from the shallows.";
					break;
				case 2:
					text = $"  *  You see a splash of water {FlavorText.FromSomewhere()}.";
					break;
				case 3:
					text = "A frog ribbits.";
					break;
				case 4:
					text = "A pair of eyes emerge from the water staring at you, then re-submerge.";
					break;
				case 5:
					text = $"  *  You hear a 'ribbit-ribbit' come {FlavorText.FromSomewhere()}.";
					break;
			}
			GameEngine.SayToLocation(location, text);
		}

		public static void TalkToMeBeforeEnteringTown(Player player, Player guard)
		{
			GameEngine.SayToLocation(guard.Location, $"The guard addresses {player.Name}, \"Talk to me if you wish to enter the town.\"");
		}
 
		public static void DoBadFeelingAboutBridge(Player player)
		{
			string text = "";
			switch (rng.Next(3))
			{
				case 0:
					text = $"  *  You suddenly feel uneasy, as though something is not quite right...";
					break;
				case 1:
					text = $"  *  A sudden cold shiver passes over you as you approach the bridge...";
					break;
				case 2:
					text = $"  *  You think you see a shadow go running under the bridge...";
					break;
			}
			player.Notify(text);
		}

	}

}