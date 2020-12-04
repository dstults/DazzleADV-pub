using System;
using DStults.Utils;

namespace DazzleADV
{
	public static class LocationActions
	{
		static Random rng = new Random();

		static public ActionOption<Player, MenuOptionType> WinTheGame = new ActionOption<Player, MenuOptionType>("Win the Game", "You've made it!",
				null,
				new Action<Player>(player => DoWinTheGame(player, Locale.Downtown)), MenuOptionType.Location, "win");
		static public ActionOption<Player, MenuOptionType> DrinkWater = new ActionOption<Player, MenuOptionType>("Drink Water", "The water seems clean enough...",
				null,
				new Action<Player>(player => DoDrinkWater(player, Locale.River)), MenuOptionType.Location, "dw", "drink");
		static public ActionOption<Player, MenuOptionType> ProdWater = new ActionOption<Player, MenuOptionType>("Prod Water", "Search water with stick",
				new Predicate<Player>(player => PlayerHasItem(player, ItemType.Stick)),
				new Action<Player>(player => DoProdWater(player, Locale.River)), MenuOptionType.Location, "pw", "prod");
		static public ActionOption<Player, MenuOptionType> PunchWood = new ActionOption<Player, MenuOptionType>("Punch Wood", "Harvest some wood by hitting it",
				new Predicate<Player>(player => !player.HasFlag(QuestFlags.PunchedWood)),
				new Action<Player>(player => DoPunchWood(player, Locale.Woods)), MenuOptionType.Location, "pw", "punch");
		static public ActionOption<Player, MenuOptionType> SearchForest = new ActionOption<Player, MenuOptionType>("Search Forest", "Maybe there are pretty pine cones",
				new Predicate<Player>(player => player.HasFlag(QuestFlags.PunchedWood)),
				new Action<Player>(player => DoSearchForest(player, Locale.Woods)), MenuOptionType.Location, "sf", "search");
		static public ActionOption<Player, MenuOptionType> CrossBridge = new ActionOption<Player, MenuOptionType>("Cross Bridge", "It looks completely safe...",
				null,
				new Action<Player>(player => DoCrossBridge(player, Locale.GoblinBridge)), MenuOptionType.Location, "cb", "cross");

		private static bool PlayerHasItem(Player player, ItemType itemType)
		{
			if (player is Player && (player).HasItem(itemType))
				return true;
			return false;
		}

		private static void DoWinTheGame(Player player, Location location)
		{
			if (player == null)
				throw new ArgumentNullException($"Error: LocationAction DoWinTheGame null player.");
			if (location == null)
				throw new ArgumentNullException($"Error: LocationAction DoWinTheGame null location.");

			if (player.IsAlive && player.Location == location)
			{
				GameEngine.SayToAll($"Congratulations to {player.Name} for deftly making it to THE TOWN and conquering the game!");
				if (player.HasClient)
				{
					player.Client.SendUpdate($"\n\nCongratulations to you for deftly making it to THE TOWN and conquering the game!\nBUT IS THIS REALLY THE END?");
					player.Client.KickClient("You won!");
				}
				GameEngine.Players.Remove(player);
			}
			else
			{
				GameEngine.SayToLocation(player.Location, $"{player.Name} was about to win the game but was interrupted!");
			}
		}

		private static void DoDrinkWater(Player player, Location location)
		{
			if (player == null)
				throw new ArgumentNullException($"Error: LocationAction DoDrinkWater null player.");
			if (location == null)
				throw new ArgumentNullException($"Error: LocationAction DoDrinkWater null location.");

			if (player.IsAlive && player.Location == location)
			{
				GameEngine.SayToLocation(location, $"{player.Name} drinks the water.");
				player.HealHP(4);
				player.AddEffect(new StatusEffect(EffectClass.Dysentery, effectValue: 5,
					turnTickEvent: StatusEvents.DysenteryTick, effectExpiresEvent: StatusEvents.DysenteryCured));
				if (!player.HasFlag(QuestFlags.LearnedAboutDysentary))
				{
					player.AddFlag(QuestFlags.LearnedAboutDysentary);
					player.Notify("  *  To prevent heal scumming, certain healing locations inflict mild status ailments. Avoid drinking too fast and you'll be fine.");
				}
			}
			else
			{
				GameEngine.SayToLocation(player.Location, $"{player.Name} was about to drink some water but didn't succeed...");
			}
		}

		private static void DoProdWater(Player player, Location location)
		{
			if (player == null)
				throw new ArgumentNullException($"Error: LocationAction DoProdWater null player.");
			if (location == null)
				throw new ArgumentNullException($"Error: LocationAction DoProdWater null location.");

			if (player.IsAlive && player.Location == location && player.HasItem(ItemType.Stick))
			{
				GameEngine.SayToLocation(location, $"{player.Name} prods the water with a crook...");
				if (!player.HasFlag(QuestFlags.ProddedCloths))
				{
					player.AddFlag(QuestFlags.ProddedCloths);
					player.Notify("  *  GET! You found yourself some new clothes!");
					GameEngine.SayToLocation(location, $"{player.Name} picks up bits of clothing from the putrid water.");
					Armor newArmor = (Armor)Prefabs.NewItem(ItemType.Cloth);
					newArmor.AddSocket();
					player.AddItem(newArmor);
				}
				else if (!player.HasFlag(QuestFlags.ProddedRustSword))
				{
					player.AddFlag(QuestFlags.ProddedRustSword);
					player.Notify("  *  GET! You found yourself a rusty sword!");
					GameEngine.SayToLocation(location, $"{player.Name} picks up a broken, rusty sword from the putrid water.");
					Weapon newWeapon = (Weapon)Prefabs.NewItem(ItemType.RustySword);
					newWeapon.AddSocket();
					player.AddItem(newWeapon);
				}
				else
				{
					GameEngine.SayToLocation(location, $"...but gets distracted by {player.Genderize("his handsome", "her beautiful", "its mesmerizing")} reflection.");
				}
			}
			else
			{
				GameEngine.SayToLocation(player.Location, $"{player.Name} was about to prod the water with a stick but didn't succeed...");
			}
		}

		private static void DoPunchWood(Player player, Location location)
		{
			if (player == null)
				throw new ArgumentNullException($"Error: LocationAction DoPunchWood null player.");
			if (location == null)
				throw new ArgumentNullException($"Error: LocationAction DoPunchWood null location.");

			if (player.IsAlive && player.Location == location)
			{
				player.AddFlag(QuestFlags.PunchedWood);
				GameEngine.SayToLocation(location, $"{player.Name} tries to harvest wood by punching a tree really hard.");
				player.DamageHP(1, "hard wood", null);
				GameEngine.SayToLocation(location, $"{player.Genderize("He", "She", "It")} realizes that this isn't Minecraft and decides that searching around might be wiser.");
			}
			else
			{
				GameEngine.SayToLocation(player.Location, $"{player.Name} was about to punch wood but didn't succeed...");
			}
		}

		private static void DoSearchForest(Player player, Location location)
		{
			if (player == null)
				throw new ArgumentNullException($"Error: LocationAction DoSearchForest null player.");
			if (location == null)
				throw new ArgumentNullException($"Error: LocationAction DoSearchForest null location.");

			if (player.IsAlive && player.Location == location)
			{
				GameEngine.SayToLocation(location, $"{player.Name} searches the area.");
				if (!player.HasFlag(QuestFlags.ForestEmerald))
				{
					player.AddFlag(QuestFlags.ForestEmerald);
					if (player is Player)
					{
						GameEngine.SayToLocation(location, $"{player.Name} looks very closely at a green object...it's an emerald! {player.Genderize("He", "She", "It")} picks it up.");
						(player).AddItem(ItemType.Emerald);
						player.Notify("  *  Once you find some armor you can insert this to gain a passive ability!");
					}
					else
					{
						GameEngine.SayToLocation(location, $"{player.Name} looks very closely at a green object...it's an emerald!");
						location.AddItem(ItemType.Emerald);
					}							
				}
				else if (player is Player && !(player).HasItem(ItemType.Stick))
				{
					GameEngine.SayToLocation(location, $"{player.Name} discovers a nice stick!");
					player.Notify("  *  It has a crooked head. You might even be able to pull something from the river.");
					GameEngine.SayToLocation(location, $"{player.Name} picks up a stick.");
					(player).AddItem(ItemType.Stick);
				}
				else
				{
					switch (new Random().Next(5))
					{
						case 0:
							player.Notify("  *  You discovered a Long Sword and some Chain Mail hidden behind a tree!");
							GameEngine.SayToLocation(location, $"{player.Name} suddenly looks very excited! Then not.");
							player.Notify("  *  Oh wait...it was just a shadow.");
							break;
						case 1:
							player.Notify("  *  A huge spider is crawling up one of the trees! Prepare yourself!");
							GameEngine.SayToLocation(location, $"{player.Name} gets startled by the local wildlife!");
							Prefabs.SpawnUnit(UnitType.Spiderling, location);
							break;
						case 2:
							GameEngine.SayToLocation(location, $"{player.Name} looks very closely at a blue object.");
							player.Notify("  *  You discovered a beautiful blue butterfly. A shame it serves no purpose.");
							break;
						case 3:
							player.Notify("  *  You don't find anything of interest.");
							break;
						case 4:
							GameEngine.SayToLocation(location, $"{player.Name} discovers a nice stick!");
							if (player is Player)
								(player).AddItem(ItemType.Stick);
							else
								player.Location.AddItem(ItemType.Stick);
							break;
					}
				}
			}
			else
			{
				GameEngine.SayToLocation(player.Location, $"{player.Name} was about to search the foliage but didn't succeed...");
			}
		}

		private static void DoCrossBridge(Player player, Location location)
		{
			if (player == null)
				throw new ArgumentNullException($"Error: LocationAction DoCrossBridge null player.");
			if (location == null)
				throw new ArgumentNullException($"Error: LocationAction DoCrossBridge null location.");

			if (player.IsAlive && player.Location == location)
			{
				GameEngine.SayToLocation(location, $"{player.Name} tries to cross the bridge...");
				GameEngine.SayToLocation(location, $"A nasty goblin charges at {player.Genderize("him", "her", "it")} from under the bridge and attacks!");
				Player newGoblin = Prefabs.SpawnUnit(UnitType.WeakGoblin, location);
				PlayerActions.Attack(newGoblin, player);
				newGoblin.ResetTimeToAct();
			}
			else
			{
				GameEngine.SayToLocation(player.Location, $"{player.Name} was about to search the foliage but didn't succeed...");
			}
		}

	}
}