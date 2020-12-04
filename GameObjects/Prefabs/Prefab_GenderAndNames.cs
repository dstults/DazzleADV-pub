using System;
using System.Collections.Generic;

namespace DazzleADV
{

	public static partial class Prefabs
	{

		private static Gender GetRandomGender(int choices = 2)
		{
			int choice = new Random().Next(choices);
			switch (choice)
			{
				case 0:
					return Gender.Male;
				case 1:
					return Gender.Female;
				default:
					return Gender.Genderless;
			}
		}

		private readonly static List<string> goblinMaleNames = new List<string> { "Buzztooth", "Gobbo", "Screech", "Clank", "Slap", "Dangle", "Shangle", "Yarrn" };
		private readonly static List<string> goblinFemaleNames = new List<string> { "Streak", "Blin", "Swez", "Flan", "Nill", "Zoink", "Dingle", "Beri" };

		private readonly static List<string> guardMaleNames = new List<string> { "Don", "Paul", "Ratsel", "Lark", "Johan", "Herb", "Rorik", "Tanev" };
		private readonly static List<string> guardFemaleNames = new List<string> { "Helga", "Sally", "Jada", "Fran", "Lizzy", "Joan", "Bess", "Rillo" };

		private readonly static List<string> kingNames = new List<string> { "Henry", "Olivier", "Stratsburg", "Copernicus", "Adamaris", "Romulus" };
		private readonly static List<string> queenNames = new List<string> { "Elizabeth", "Margot", "Delphine", "Allegra", "Eleanora", "Madelaine" };

		private static string GetRandomFromList(List<string> list)
		{
			if (list == null)
				throw new ArgumentNullException("Tried to get a random string from a null list.");
			if (list.Count == 0)
				throw new InvalidOperationException("Tried to get a random string from an empty list.");

			return list[rng.Next(list.Count)];
		}

		private static string GetRandomName(Race race, Gender gender, Enum template)
		{
			switch (race.IsUndead)
			{
				case true:
					switch (template)
					{
						case UnitType.Skeleton:
							return "Skeleton";
					}
					break;
				case false:
					switch (race.Name)
					{
						case "Human":
							switch (template)
							{
								case UnitType.NorthGateGuard:
								case UnitType.SouthGateGuard:
								case UnitType.KeepGuard:
									switch (gender)
									{
										case Gender.Male:
											return GetRandomFromList(guardMaleNames);
										case Gender.Female:
											return GetRandomFromList(guardFemaleNames);
									}
									break;
								case UnitType.Chancellor:
								case UnitType.KingQueen:
								case UnitType.Cultist:
									switch (gender)
									{
										case Gender.Male:
											return GetRandomFromList(kingNames);
										case Gender.Female:
											return GetRandomFromList(queenNames);
									}
									break;
							}
							break;
						case "Goblin":
							switch (gender)
							{
								case Gender.Male:
									return GetRandomFromList(goblinMaleNames);
								case Gender.Female:
									return GetRandomFromList(goblinFemaleNames);
							}
							break;
					}
					break;
			}

			throw new ArgumentOutOfRangeException($"Prefab name generator error: No namelist for race-gender-template combination of [ {race} - {gender} - {template} ]!");
		}

	}

}