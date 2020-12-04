using System;

namespace DazzleADV
{

	public class LocationSelfEvent
	{
		public int TriggerRatePercentage { get; protected set; }
		public Func<Location, int, bool> Condition { get; protected set; }
		public Action<Location> PerformAction { get; protected set; }

		public LocationSelfEvent(int triggerRatePercentage, Func<Location, int, bool> condition, Action<Location> action)
		{
			if (triggerRatePercentage <= 0 || triggerRatePercentage > 100)
				throw new ArgumentOutOfRangeException("Event trigger rate must be 1 - 100");
			if (condition == null)
				condition = LocationHasAnyPlayer;
			if (action == null)
				throw new ArgumentNullException("No action specified to be triggered on event");

			this.TriggerRatePercentage = triggerRatePercentage;
			this.Condition = condition;
			this.PerformAction = action;
		}

		private static bool LocationHasAnyPlayer(Location location, int playersLocatedHere)
		{
			return playersLocatedHere > 0;
		}

	}

}