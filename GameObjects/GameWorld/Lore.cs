
namespace DazzleADV
{
	public static class Lore
	{

		public static bool FactionsHostile(Faction f1, Faction f2)
		{
			// Rules: - Townsfolk and Monsters fight each other on sight
			//        - Monsters additionally fight players on sight
			//        - Neutrals never attack others
			switch (f1)
			{
				case Faction.Players:
				case Faction.Townsfolk:
					if (f2 == Faction.Monsters)
						return true;
					return false;
				case Faction.Monsters:
					if (f2 == Faction.Players || f2 == Faction.Townsfolk)
						return true;
					return false;
			}
			return false;
		}

	}
}