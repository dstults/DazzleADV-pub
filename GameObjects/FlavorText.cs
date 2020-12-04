using System;

namespace DazzleADV
{

	public static class FlavorText
	{

		private static Random rng = new Random();

		public static string AmountOfAComparedtoB(double val1, double val2)
		{
			// Note: "player has {comparevals(current,max)} his/her HP left
			double ratio = val1 / val2;
			if (ratio == 0)
				return "none of";
			else if (ratio <= .2)
				return "a tiny sliver of";
			else if (ratio <= .4)
				return "about a quarter of";
			else if (ratio <= .65)
				return "about half of";
			else if (ratio <= .98)
				return "most of";
			else
				return "all of";
		}

		public static string ToSomewhere()
		{
			switch (rng.Next(15))
			{
				case 0:
					return "off in the distance";
				case 1:
					return "nearby";
				case 2:
					return "close by";
				case 3:
					return "not too far from here";
				case 4:
					return "in the distance";
				case 5:
					return "to the north";
				case 6:
					return "to the northeast";
				case 7:
					return "to the east";
				case 8:
					return "to the southeast";
				case 9:
					return "to the south";
				case 10:
					return "to the southwest";
				case 11:
					return "to the west";
				case 12:
					return "to the northwest";
				case 13:
					return "not far away";
				case 14:
					return "in the distance";
			}
			return "over the rainbow";
		}

		public static string FromSomewhere()
		{
			return $"from somewhere {ToSomewhere()}";
		}

		public static string RandomColor()
		{
			switch (rng.Next(13))
			{
				case 0:
					return "red";
				case 1:
					return "orange";
				case 2:
					return "yellow";
				case 3:
					return "green";
				case 4:
					return "blue";
				case 5:
					return "purple";
				case 6:
					return "brown";
				case 7:
					return "pink";
				case 8:
					return "gray";
				case 9:
					return "teal";
				case 10:
					return "white";
				case 11:
					return "black";
				case 12:
					return "magenta";
			}
			return "rainbow";
		}

		public static string ARandomBird()
		{
			string text = "";
			switch (rng.Next(9))
			{
				case 0:
					text =  "A swift-flying ";
					break;
				case 1:
					text =  $"A {RandomColor()} ";
					break;
				case 2:
					text =  $"A {RandomColor()}-tailed ";
					break;
				case 3:
					text =  $"A {RandomColor()}-feathered ";
					break;
				case 4:
					text =  $"A {RandomColor()}-headed ";
					break;
				case 5:
					text =  "A magnificent ";
					break;
				case 6:
					text =  "A feathery ";
					break;
				case 7:
					text =  "A long ";
					break;
				case 8:
					text =  "A large ";
					break;
			}
			switch (rng.Next(16))
			{
				case 0:
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
					text += "bird";
					break;
				case 9:
				case 10:
					text += "sparrow";
					break;
				case 11:
				case 12:
					text += "crow";
					break;
				case 13:
					text += "puffin";
					break;
				case 14:
					text += "pigeon";
					break;
				case 15:
					text += "eagle";
					break;
			}
			return text;
		}

	}
}