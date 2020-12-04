using System;

namespace DazzleADV
{

	public class ConnectorSelfEvent
	{
		public Action<LocationConnector> PerformedAction { get; protected set; }

		public ConnectorSelfEvent(Action<LocationConnector> action, int countdown = -1)
		{
			if (action == null)
				throw new ArgumentNullException("No action specified to be triggered on event");
			if (countdown < 0)
				countdown = -1;

			this.PerformedAction = action;
			this.countdownTimer = countdown;
		}

		public void PerformAction(LocationConnector connector)
		{
			this.PerformedAction(connector);
		}

		private int countdownTimer;
		public bool TimesUp => countdownTimer == 0;
		public int CountdownTick()
		{
			if (countdownTimer > 0)
				countdownTimer--;
			return countdownTimer;
		}

	}

	public static class ConnectorEventTemplates
	{

		public static void DoNothing(LocationConnector connector) { }

		public static void ClosePortcullisOnCountdown(LocationConnector connector)
		{
			int timeLeft = connector.OnTickEvent.CountdownTick();
			if (connector.OnTickEvent.TimesUp)
			{
				connector.Lock();
				GameEngine.SayToLocation(connector.Parent, "The portcullis into the town comes crashing down!");
				GameEngine.SayToLocation(connector.Destination, "The portcullis into the town comes crashing down!");
				connector.ClearTickEvent();
			}
		}

	}


}