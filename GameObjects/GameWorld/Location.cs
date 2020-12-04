using System;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{

	public class Location
	{
		public string Name { get; private set; }
		public string Description { get; private set; }
		public Dictionary<Direction, LocationConnector> Connections { get; private set; }
		private List<ActionOption<Player, MenuOptionType>> actions;
		private List<LocationSelfEvent> PassiveSelfEvents;
		private List<Item> items;
		public int GetItemCount => items.Count;

		public Location(string name, string description)
		{
			if (name == null)
				throw new ArgumentNullException("Error: Location constructor name null string error");
			if (name.Length == 0)
				throw new ArgumentException("Error: Location constructor namer empty string error");
			if (description == null)
				throw new ArgumentNullException("Error: Location constructor description null string error");
			if (description.Length == 0)
				throw new ArgumentException("Error: Location constructor description empty string error");

			this.Name = name;
			this.Description = description;

			Connections = new Dictionary<Direction, LocationConnector>();
			actions = new List<ActionOption<Player, MenuOptionType>>();
			PassiveSelfEvents = new List<LocationSelfEvent>();
			items = new List<Item>();
		}

		public string GetDescription(Player whosPerspective)
		{
			if (whosPerspective == null)
				throw new ArgumentNullException("Tried to get location description without specifying from whose point of view");

			string result = Description;
			if (items.Count > 0)
			{
				List<string> itemNames = new List<string>();
				items.ForEach(i => itemNames.Add(i.SetName()));
				result += $"\n\nYou can see {TextUtils.PrettyPrint(itemNames, true)} on the ground.";
			}
			string deadText;
			foreach (Player player in GameEngine.Players.FindAll(p => p.Location == this && p != whosPerspective))
			{
				deadText = player.IsAlive ? "" : " (dead)";
				result += $"\n{player.Name}{deadText} is here.";
			}
			return result;
		}

		public void AddConnector(Direction direction, Location destination, bool twoWay = true, bool isVisible = true, bool isUnlocked = true)
		{
			if (this.Connections.ContainsKey(direction))
				throw new ArgumentException($"Error: Tried placing two {direction} connections to {destination} atop {this.Name}, ?2way: {twoWay}");
			if (destination == null)
				throw new ArgumentNullException($"Error: No destination specified when trying to add connection to {this.Name}'s {direction} exit");

			this.Connections.Add(direction, new LocationConnector(this, destination, visible: isVisible, unlocked: isUnlocked));
			if (twoWay) destination.AddConnector(WorldMap.GetOppositeDirection(direction), this, twoWay: false, isVisible, isUnlocked);
		}

		public LocationConnector GetConnector(Location destination)
        {
			if (destination == null)
				throw new ArgumentNullException($"Error: {Name}.HasConnection null destination");

			foreach(LocationConnector connector in Connections.Values)
            {
				if (connector.Destination == destination) return connector;
			}
			return null;
        }

		public bool HasAction(ActionOption<Player, MenuOptionType> actionOption)
		{
			if (actionOption == null)
				throw new ArgumentNullException($"Error: {Name}.HasAction null actionOption");

			return actions.Contains(actionOption);
		}

		public void AddAction(ActionOption<Player, MenuOptionType> actionOption)
		{
			if (actionOption == null)
				throw new ArgumentNullException("Location AO null error");

			actions.Add(actionOption);
		}

		public void RemoveAction(ActionOption<Player, MenuOptionType> actionOption)
		{
			if (actionOption == null)
				throw new Exception($"Error: Location RemoveAction passed null actionOption");
			
			actions.Remove(actionOption);
		}

		public List<ActionOption<Player, MenuOptionType>> GetActions()
		{
			List<ActionOption<Player, MenuOptionType>> actionList = new List<ActionOption<Player, MenuOptionType>>();
			string locked;
			foreach (Direction direction in Enum.GetValues(typeof(Direction)))
			{
				if (this.Connections.ContainsKey(direction))
				{
					Location destination = this.Connections[direction].Destination;
					locked = this.Connections[direction].IsUnlocked ? "" : " (locked)";
					actionList.Add(new ActionOption<Player, MenuOptionType>($"Go {direction}{locked}", $"Travel to {destination.Name}",
						null,
						new Action<Player>(player => PlayerActions.TravelFromPointToPoint(player, player.Location, destination)),
							MenuOptionType.Travel, direction.ToString().Substring(0, 1).ToLower(), direction.ToString().ToLower()));
				}
			}
			foreach (ActionOption<Player, MenuOptionType> actionOption in actions)
			{
				actionList.Add(actionOption);
			}

			return actionList;
		}

		public void AddSelfEvent(LocationSelfEvent locationSelfEvent)
		{
			if (locationSelfEvent == null)
				throw new ArgumentNullException($"Tried to add a null LocationSelfEvent to {Name}.");

			PassiveSelfEvents.Add(locationSelfEvent);
		}

		public void SetEventOnEntryFrom(Direction direction, LocationPlayerEvent locationEvent)
		{
			if (locationEvent == null)
				throw new ArgumentNullException($"Error: {Name}.AddEntryEvent null locationPlayerEvent.");
			if (!Connections.ContainsKey(direction))
				throw new InvalidOperationException($"Error: {Name}.AddEntryEvent no valid {direction} connection.");
			
			Connections[direction].SetEntryEvent(locationEvent);
		}

		public void SetExitEvent(Direction direction, LocationPlayerEvent locationEvent)
		{
			if (locationEvent == null)
				throw new ArgumentNullException($"Error: {Name}.AddExitEvent null locationPlayerEvent.");
			if (!Connections.ContainsKey(direction))
				throw new InvalidOperationException($"Error: {Name}.AddExitEvent no valid {direction} connection.");

			Connections[direction].SetExitEvent(locationEvent);;
		}

		public void DoWorldTickActions()
		{
			Random rng = new Random();
			List<Player> playersAtLocation = GameEngine.Players.FindAll(p => p.Location == this);
			for (int i = PassiveSelfEvents.Count - 1; i >= 0; i--)
			{
				if (PassiveSelfEvents[i].Condition(this, playersAtLocation.Count))
				{
					if (rng.Next(101) <= PassiveSelfEvents[i].TriggerRatePercentage)
					{
						PassiveSelfEvents[i].PerformAction(this);
					}
				}
			}
			foreach (LocationConnector connector in this.Connections.Values)
			{
				connector.RunTickEvent();
			}
		}

		public Item GetItem(string looseMatch)
		{
			if (looseMatch == null)
				throw new ArgumentNullException("Pickup Item by string null string");
			if (looseMatch.Length == 0)
				throw new ArgumentException("Cannot search for item by string with empty string");

			return items.Find(i => i.Matches(looseMatch));
		}

		public bool RemoveItem(Item item)
		{
			if (item == null)
				throw new ArgumentNullException("Pickup Item null item");

			if (items.Contains(item))
			{
				items.Remove(item);
				return true;
			}
			else
			{
				return false;
			}
		}

		public void AddItem(Item item)
		{
			if (item == null)
				throw new ArgumentNullException($"No item was specified when trying to add item to {this.Name}");

			if (items.Count >= 5)
			{
				Item randomItem = items[new Random().Next(items.Count)];
				GameEngine.SayToLocation(this, $"A magical leprechaun appears, grabs a {item} and runs off with it!");
				items.Remove(randomItem);
			}
			items.Add(item);
		}

		public void AddItem(ItemType itemType)
		{
			AddItem(Prefabs.NewItem(itemType));
		}

		public override string ToString()
		{
			return Name;
		}

	}
}
