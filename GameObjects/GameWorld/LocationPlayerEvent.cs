using System;

namespace DazzleADV
{

	public class LocationPlayerEvent
	{
		public Predicate<Player> DisplayCondition { get; protected set; }
		public Action<Player> PerformAction { get; protected set; }

		public LocationPlayerEvent(Predicate<Player> displayCondition, Action<Player> action)
		{
			if (displayCondition == null)
				displayCondition = PlayerHasClient;
			if (action == null)
				throw new ArgumentNullException("No action specified to be triggered on event");

			this.DisplayCondition = displayCondition;
			this.PerformAction = action;
		}

		private static bool PlayerHasClient(Player player)
		{
			if (player == null)
				throw new ArgumentNullException("Tried checking LocationPlayerEvent against a null player");
			return player.HasClient;
		}

	}

}