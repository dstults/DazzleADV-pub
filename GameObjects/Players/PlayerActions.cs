using System;
using System.Reflection;
using DStults.Utils;

namespace DazzleADV
{

	public static class PlayerActions
	{
		static Random rng = new Random();

		public readonly static ActionOption<Player, MenuOptionType> Idle = new ActionOption<Player, MenuOptionType>(
			"Idle", "Pass time doing nothing", null, DoIdle, MenuOptionType.Self, "i", "idle");

		public static bool IsHumanoid(Player player) => player.IsHumanoid;

		public static void TravelFromPointToPoint(Player player, Location startingPoint, Location destination, string exitText = "", string enterText = "")
		{
			if (player == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, null player");
			if (destination == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, null starting location");
			if (destination == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, null ending location");
			if (exitText == "")
				exitText = $"{player.Name} leaves {startingPoint} and heads for {destination}.";
			if (enterText == "")
				enterText = $"{player.Name} enters {destination} from {startingPoint}.";

			if (player.IsAlive && player.Location == startingPoint)
			{
				LocationConnector exitConnector = startingPoint.GetConnector(destination);
				if (exitConnector == null || (exitConnector.IsUnlocked))
				{
					exitConnector.OnPlayerExit.PerformAction(player);
					GameEngine.SayToLocation(player.Location, exitText);
					player.Relocate(destination);
					GameEngine.SayToLocation(player.Location, enterText);
					LocationConnector enterConnector = destination.GetConnector(startingPoint);
					enterConnector.OnPlayerEnter.PerformAction(player);
				}
				else
				{
					GameEngine.SayToLocation(player.Location, $"{player.Name} tries to go to {destination.Name} but the way is locked.");
				}
			}
			else
			{
				GameEngine.SayToLocation(player.Location, $"{player.Name} was travelling but got interrupted.");
			}

		}

		private static void DoIdle(Player player)
		{
			if (player == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, null player");

			if (player.IsAlive)
			{
				switch (rng.Next(6))
				{
					case 0:
						GameEngine.SayToLocation(player.Location, $"{player.Name} wanders about.");
						break;
					case 1:
						GameEngine.SayToLocation(player.Location, $"{player.Name} stands around doing nothing.");
						break;
					case 2:
						GameEngine.SayToLocation(player.Location, $"{player.Name} waits patiently.");
						break;
					case 3:
						GameEngine.SayToLocation(player.Location, $"{player.Name} shifts {player.Genderize("his", "her", "its")} weight from side to side.");
						break;
					case 4:
						if (player.HasState(EffectClass.Dysentery))
						{
							GameEngine.SayToLocation(player.Location, $"{player.Name} prominently farts.");
							GameEngine.SayToAdjacent(player.Location, $"There is a sound of somebody farting coming from {player.Location}.");
						}
						else
						{
							GameEngine.SayToLocation(player.Location, $"{player.Name} sighs.");
						}
						break;
					case 5:
						if (player.HasState(EffectClass.Dysentery))
						{
							GameEngine.SayToLocation(player.Location, $"{player.Name} coughs a wet cough.");
							GameEngine.SayToAdjacent(player.Location, $"There is a wet coughing sound coming from from {player.Location}.");
						}
						else
						{
							GameEngine.SayToLocation(player.Location, $"{player.Name} has a hiccup.");
						}
						break;
					default:
						break;
				}
			}
		}

		public static void Chat(Player player, string text = "")
		{
			if (player == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, null player");
			if (text == null)
				text = "";
			if (text.Length == 0)
				text = "I like swords!";

			if (player.IsAlive)
			{
				text = TextUtils.CurateDialogue(text);
				GameEngine.SayToLocation(player.Location, $"{player.Name} says, \"{text}\"");
			}
		}

		public static void Emote(Player player, string text = "")
		{
			if (player == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, null player");
			if (text == null)
				text = "";
			if (text == "")
				text = "likes swords.";

			if (player.IsAlive)
			{
				text = text.Trim();
				text = TextUtils.CheckForPunctuation(text);

				GameEngine.SayToLocation(player.Location, $"{player.Name} {text}");
			}
		}

		public static void Yell(Player player, string text)
		{
			if (player == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, null player");
			if (text == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, null string");
			// text.Length == 0   OK

			if (player.IsAlive)
			{
				text = TextUtils.CurateDialogue(text);
				if (text == "")
				{
					GameEngine.SayToLocation(player.Location, $"{player.Name} yells loudly.");
					GameEngine.SayToAdjacent(player.Location, $"There is a sound of somebody yelling coming from {player.Location}.");
				}
				else
				{
					GameEngine.SayToLocation(player.Location, $"{player.Name} yells, \"{text}\"");
					GameEngine.SayToAdjacent(player.Location, $"Somebody is yelling from {player.Location}, \"{text}\"");
				}
			}
		}

		public static void DropItem(Player player, Location location, string arg2)
		{
			if (player == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, player null error");
			if (location == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, location null error");
			if (arg2 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, arg2 null error");
			// if (arg2.Length == 0) OK

			if (player.IsAlive)
			{
				if (arg2.Length == 0)
				{
					player.Notify("  *  Type: 'drop item-name'");
					return;
				}

				Item item = player.GetItem(arg2);
				if (item == null)
				{
					player.Notify($"  *  Could not find an item that goes by '{arg2}' on your person");
					return;
				}

				if (player.RemoveItem(item))
				{
					GameEngine.SayToLocation(player.Location, $"{player.Name} dropped {item.SetName()} on the ground.");
					player.Location.AddItem(item);
				}
			}
			else if (!player.IsAlive)
			{
				player.Notify($"  *  You died before you could drop the item.");
			}
			else if (player.Location != location)
			{
				player.Notify($"  *  You left the area before you could drop the item.");
			}
		}

		public static void EquipItem(Player player, string arg2)
		{
			if (player == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, player null error");
			if (arg2 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, arg2 null error");
			// if (arg2.Length == 0) OK

			if (player.IsAlive)
			{
				if (arg2.Length == 0)
				{
					player.Notify("  *  Type: 'equip item-name'");
					return;
				}

				Item item = player.GetItem(arg2);
				if (item == null)
				{
					player.Notify($"  *  Could not find an item that goes by '{arg2}' on your person");
					return;
				}

				if (item is Weapon)
					player.Equip((Weapon)item);
				else if (item is Armor)
					player.Equip((Armor)item);
				else
				{
					player.Notify($"  *  {item.Title} cannot be equipped!");
				}
			}
			else if (!player.IsAlive)
			{
				player.Notify($"  *  You died before you could equip the item.");
			}
		}

		public static void GetItemFromLocation(Player player, Location location, string arg2)
		{
			if (player == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, player null error");
			if (location == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, location null error");
			if (arg2 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, arg2 null error");
			// if (arg2.Length == 0) OK

			if (player.IsAlive && player.Location == location)
			{
				if (arg2.Length == 0)
				{
					player.Notify("  *  Type: 'get item-name'");
					return;
				}
				Item item = location.GetItem(arg2);
				if (item == null)
				{
					player.Notify($"  *  Cannot find item '{arg2}' at [{location.Name}]");
					return;
				}

				if (location.RemoveItem(item))
				{
					GameEngine.SayToLocation(player.Location, $"{player.Name} picks up the {item}.");
					player.AddItem(item);
				}
			}
			else if (!player.IsAlive)
			{
				player.Notify($"  *  You died before you could pick up the item.");
			}
			else if (player.Location != location)
			{
				player.Notify($"  *  You left the area before you could pick up the item.");
			}
		}

		public static void SocketItem(Player player, ISocketed item, Gemstone gem)
		{
			if (player == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, player null error");
			if (item == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, item null error");
			if (gem == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, gem null error");

			if (player.IsAlive)
			{
				if (item.Augment(gem))
				{
					GameEngine.SayToLocation(player.Location, $"{player} sockets {gem.SetName()} in {item.SetName()}.");
					player.RemoveItem(gem);
				}
				else
				{
					player.Notify($"  *  You were unable to insert the item in the socket.");
				}
			}
			else if (!player.IsAlive)
			{
				player.Notify($"  *  You died before you could socket the item.");
			}
		}

		public static void Talk(Player p1, Player p2, string speech = "")
		{
			if (p1 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, p1 null error");
			if (p2 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, p2 null error");
			if (speech == null)
				speech = "";

			if (p1.IsAlive && p1.Location == p2.Location)
			{
				speech = TextUtils.CurateDialogue(speech);
				PlayerDialogue.HandlePlayerDialogue(p1, p2, speech);
			}

		}

		public static void Look(Player p1, Player p2)
		{
			if (p1 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, p1 null error");
			if (p2 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, p2 null error");

			if (p1.IsAlive && p1.Location == p2.Location)
			{
				GameEngine.SayToLocation(p1.Location, $"{p1.Name} takes a close look at {p2.Name}.");
				p1.Notify(p2.LookString());
			}
			else if (!p1.IsAlive)
			{
				p1.Notify($" * Your vision fades to black before you could get a good look.");
			}
			else if (p1.Location != p2.Location)
			{
				p1.Notify($" * You were separated from {p2.Name} before you could get a good look.");
			}
		}

		public static void Attack(Player p1, Player p2)
		{
			if (p1 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, p1 null error");
			if (p2 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, p2 null error");

			if (p1.IsAlive && p2.IsAlive && p1.Location == p2.Location)
			{
				if (Plugins.AttackMod != null)
				{
					Plugins.AttackMod.AttackOverride(p1, p2);
				}
				else
				{
					if (p1.Weapon == Prefabs.Unarmed)
						GameEngine.SayToLocation(p1.Location, $"{p1.Name} punches {p2.Name}!");
					else
						GameEngine.SayToLocation(p1.Location, $"{p1.Name} attacks {p2.Name} with {p1.Weapon.SetName()}!");
					int damageDealt = p1.Weapon.Damage - p2.Armor.Defense;
					if (p2.HasFlag(QuestFlags.StoryCritical) && p2.HP - damageDealt <= 0)
					{
						GameEngine.SayToLocation(p1.Location, $"{p2.Name} deftly avoids it!");
					}
					else
					{
						if (damageDealt > 0)
						{
							p2.DamageHP(damageDealt, $"wounds from {p1.Weapon.SetName()}", p1);
							p1.GetStatusEffects().ForEach(se => se.OnAttackHit(p2, se));
							p1.RemoveExpiredStatusEffects();
						}
						else
						{
							p2.DamageHP(damageDealt, $"wounds from {p1.Weapon.SetName()}", p1);
						}
					}
				}
			}
			else if (!p1.IsAlive)
			{
				GameEngine.SayToLocation(p1.Location, $"{p1.Name} perished trying to strike down {p2.Name}.");
			}
			else if (!p2.IsAlive)
			{
				GameEngine.SayToLocation(p1.Location, $"{p1.Name} is pleased to see that {p2.Name} has perished.");
			}
			else if (p1.Location != p2.Location)
			{
				GameEngine.SayToLocation(p1.Location, $"{p1.Name} attacks the air but no one is there!");
			}

		}

		public static void SearchCorpse(Player p1, Player p2)
		{
			if (p1 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, p1 null error");
			if (p2 == null)
				throw new ArgumentNullException($"{MethodBase.GetCurrentMethod().ReflectedType.Name}, p2 null error");

			if (p1.IsAlive && !p2.IsAlive && p1.Location == p2.Location)
			{
				string firstHalf = $"{p1.Name} searches {p2.Name}'s body...";
				if (p2.HasLoot())
				{
					Item foundItem = p2.GetLoot();
					GameEngine.SayToLocation(p1.Location, $"{firstHalf}{p1.Genderize("he", "she", "it")} finds {foundItem.SetName()}!");
					p1.AddItem(foundItem);
				}
				else if (p2.HasClient)
				{
					GameEngine.SayToLocation(p1.Location, $"{firstHalf}but couldn't find anything of value.");
				}
				else
				{
					GameEngine.SayToLocation(p1.Location, $"{firstHalf}but couldn't find anything of value so {p1.Genderize("he", "she", "it")} buries {p2.Name}.");
					GameEngine.Players.Remove(p2);
				}
			}
			else if (!p1.IsAlive)
			{
				p1.Notify($" * You died before you could search {p2.Name}'s p2.");
			}
			else if (p2.IsAlive)
			{
				p1.Notify($" * {p2.Name} is not dead and cannot be searched for items.");
			}
			else if (p1.Location != p2.Location)
			{
				p1.Notify($" * {p2.Name} is not anywhere nearby.");
			}
		}

	}
}