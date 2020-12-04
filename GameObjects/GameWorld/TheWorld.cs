using System;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{
	public enum MenuOptionType { Travel, Location, Self, Targeted }
	public enum Faction { Players, Townsfolk, Monsters, Passive }

	public enum Direction { North, East, South, West, Up, Down }
	public enum QuestFlags
	{
		// Informational (resetable)
		PunchedWood, LearnedAboutDysentary,
		// Freebies (non-resetable 1-timers)
		ProddedCloths, ForestEmerald, ProddedRustSword,
		// Quest 1
		TownTrustsPlayer,
		// Special
		StoryCritical
	}

	public record Race(string Name, bool IsHumanoid, bool IsUndead = false)
	{
		public static Race Human = new Race("Human", IsHumanoid: true);
		public static Race UndeadHuman = new Race("Human", IsHumanoid: true, IsUndead: true);
		public static Race Goblin = new Race("Goblin", IsHumanoid: true);
		public static Race Arachnid = new Race("Arachnid", IsHumanoid: false);

		public override string ToString()
		{
			return this.Name;
		}
	}

	public static class Locale
	{
		public static Location Crossroads { get; } = new Location("The Crossroads",
			"You are at a crossing. The road leads north, south, east and west. A sign facing THE BRIDGE reads \"WARNING: GOBLINS\".");
		public static Location River { get; } = new Location("The River",
			"There is a large river blocking your path. It is too treacherous to cross.");
		public static Location Woods { get; } = new Location("The Woods",
			"There is a cliff to the west with a dense forest atop it. There are many trees nearby.");
		public static Location GoblinBridge { get; } = new Location("Goblin Bridge",
			"A large wooden bridge extends over a river.");
		public static Location NorthGate { get; } = new Location("North Gate",
			"A wall surrounds a town to the south. It has a gate manned with several guards. A road runs north and south, into the town.");
		public static Location Downtown { get; } = new Location("Downtown",
			"The streets are full of people going about their business.");
	}

	public static class WorldMap
	{

		public static List<Location> Locations { get; private set; } = new List<Location>(); // Populated during phases 1 (core) and 2 (plugins)
		public static Dictionary<string, Location> LocationLookupDictionary { get; private set; } = new Dictionary<string, Location>(); // Populated during phase 3
		public static Location PlayerSpawnPoint { get; private set; }

		public static void PopulateLookupDictionary()
		{
			GameEngine.SayToServer(" - Populating location lookup dictionary...");
			int i = 0;
			foreach (Location location in Locations)
			{
				LocationLookupDictionary.Add($"{++i}) {location.Name.Trim().Replace(" ", "").ToLower()}", location);
			}
			GameEngine.SayToServer("done.\n");
		}

		public static void Generate()
		{
			GameEngine.SayToServer(" - Associating map locations...");
			Locations.Add(Locale.NorthGate);
			Locations.Add(Locale.Crossroads);
			Locations.Add(Locale.Woods);
			Locations.Add(Locale.River);
			Locations.Add(Locale.GoblinBridge);
			GameEngine.SayToServer("done.\n");

			GameEngine.SayToServer(" - Setting map constants...");
			PlayerSpawnPoint = Locale.NorthGate;
			GameEngine.SayToServer("done.\n");

			GameEngine.SayToServer(" - Creating static NPCs...");
			Player guard1 = Prefabs.SpawnUnit(UnitType.NorthGateGuard, Locale.NorthGate);
			guard1.AddFlag(QuestFlags.StoryCritical);
			GameEngine.SayToServer("done.\n");

			GameEngine.SayToServer(" - Inter-connecting locations and events...");
			Locale.Crossroads.AddConnector(Direction.North, Locale.River);
			Locale.Crossroads.AddConnector(Direction.West, Locale.Woods);
			Locale.Crossroads.AddConnector(Direction.East, Locale.GoblinBridge);
			Locale.GoblinBridge.SetEventOnEntryFrom(Direction.West, LocationEvents.BadFeelingAboutBridge);
			Locale.Crossroads.AddConnector(Direction.South, Locale.NorthGate);
			Locale.NorthGate.AddConnector(Direction.South, Locale.Downtown, twoWay: false, isUnlocked: false);
			Locale.Downtown.AddConnector(Direction.North, Locale.NorthGate, twoWay: false, isUnlocked: true);
			Locale.NorthGate.SetEventOnEntryFrom(Direction.North, new LocationPlayerEvent(null, p1 => LocationEvents.TalkToMeBeforeEnteringTown(p1, guard1)));
			GameEngine.SayToServer("done.\n");

			GameEngine.SayToServer(" - Creating location self-fired events...");
			Locale.Crossroads.AddSelfEvent(LocationEvents.BirdsFlyBy);
			Locale.River.AddSelfEvent(LocationEvents.FrogsRibbit);
			Locale.River.AddSelfEvent(LocationEvents.BirdsFlyBy);
			Locale.Woods.AddSelfEvent(LocationEvents.BirdsFlyBy);
			GameEngine.SayToServer("done.\n");

			GameEngine.SayToServer(" - Adding locations actions...");
			Locale.River.AddAction(LocationActions.ProdWater);
			Locale.River.AddAction(LocationActions.DrinkWater);
			Locale.Woods.AddAction(LocationActions.PunchWood);
			Locale.Woods.AddAction(LocationActions.SearchForest);
			Locale.GoblinBridge.AddAction(LocationActions.CrossBridge);
			Locale.Downtown.AddAction(LocationActions.WinTheGame);
			GameEngine.SayToServer("done.\n");
		}

		public static Direction GetOppositeDirection(Direction direction)
		{
			switch (direction)
			{
				case Direction.North:
					return Direction.South;
				case Direction.East:
					return Direction.West;
				case Direction.South:
					return Direction.North;
				case Direction.West:
					return Direction.East;
				case Direction.Up:
					return Direction.Down;
				case Direction.Down:
					return Direction.Up;
			}

			throw new ArgumentOutOfRangeException($"Map/OppositeDirection: Unhandled direction: {direction}");
		}

		public static string GetLocationList()
		{
			List<string> locationKeys = new List<string>();
			foreach (string key in LocationLookupDictionary.Keys)
			{
				locationKeys.Add(key);
			}
			return TextUtils.Borderize(TextUtils.Columnize(locationKeys));
		}

		public static Location GetLocation(string locName)
		{
			if (locName == null)
				throw new ArgumentNullException("Error: Map.GetLocation null looseMatch");

			locName = locName.Trim().ToLower();
			if (locName.Length == 0)
				return null;

			foreach (string key in LocationLookupDictionary.Keys)
			{
				if (key.Contains(locName))
				{
					return WorldMap.LocationLookupDictionary[key];
				}
			}
			return null;
		}

	}

}
