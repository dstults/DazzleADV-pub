using System;

namespace DazzleADV
{

	public class TimeText
	{
		private ulong TimeToExpire;
		public string Text { get; private set; }

		public TimeText(ulong timeToExpire, string text)
		{
			if (timeToExpire <= 0)
				throw new ArgumentOutOfRangeException("Error: TimeText Constructor time must be > 0");
			if (text == null)
				throw new ArgumentNullException("Error: TimeText Constructor null text error");
			if (text.Length == 0)
				throw new ArgumentException("Error: TimeText Constructor empty text error");

			TimeToExpire = timeToExpire;
			Text = text;
		}

		public bool HasExpired()
		{
			return TimeToExpire < GameEngine.WorldTime;
		}

	}


}