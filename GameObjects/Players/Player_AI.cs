using System;
using System.Text;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{

	public partial class Player
	{

		public void AssignActions()
		{
			actions = new List<ActionOption<Player, MenuOptionType>>();
			if (CanAct)
			{
				int i = 0;
				if (HasClient)
				{
					// -------------------------------------- SELF ACTIONS --------------------------------------
					actions.Add(PlayerActions.Idle);
					actions.Add(new ActionOption<Player, MenuOptionType>("Chat", "Chat with people nearby",
						null,
						new Action<Player>(player => PlayerActions.Chat(player, actionArg2)), MenuOptionType.Self, "c", "chat"));
					actions.Add(new ActionOption<Player, MenuOptionType>("Emote", "Emote with people nearby",
						null,
						new Action<Player>(player => PlayerActions.Emote(player, actionArg2)), MenuOptionType.Self, "me", "emote"));
					actions.Add(new ActionOption<Player, MenuOptionType>("Yell Loudly", "Be heard even in adjacent zones",
						null,
						new Action<Player>(player => PlayerActions.Yell(player, actionArg2)), MenuOptionType.Self, "y", "yell"));

					// -------------------------------------- SELF ACTIONS  --------------------------------------
					if (Inventory.Items.FindAll(i => i is Armor || i is Weapon).Count > 0)
					{
						actions.Add(new ActionOption<Player, MenuOptionType>("Equip (Item)", "Equips item from inventory",
							PlayerActions.IsHumanoid,
							new Action<Player>(player => PlayerActions.EquipItem(this, actionArg2)), MenuOptionType.Self, "eq", "equip"));
					}
					if (Inventory.Items.Count > 0 || weapon != null || armor != null)
					{
						actions.Add(new ActionOption<Player, MenuOptionType>("Drop (Item)", "Discards item from inventory",
							PlayerActions.IsHumanoid,
							new Action<Player>(player => PlayerActions.DropItem(this, player.Location, actionArg2)), MenuOptionType.Self, "d", "drop"));
					}
					List<Gemstone> gems = Inventory.GetGemstones();
					if (gems.Count > 0)
					{
						Weapon myWeapon = Weapon;
						if (myWeapon.HasOpenSockets())
						{
							i = 0;
							foreach (Gemstone gem in gems)
							{
								if (gem.CanSocketWeapons)
								{
									i++;
									actions.Add(new ActionOption<Player, MenuOptionType>("Socket Weapon", $"Place {gem.Title} inside {myWeapon.Title}",
										PlayerActions.IsHumanoid,
										new Action<Player>(player => PlayerActions.SocketItem(player, myWeapon, gem)), MenuOptionType.Self, $"sw{i}"));
								}
							}
						}
						Armor myArmor = Armor;
						if (myArmor.HasOpenSockets())
						{
							i = 0;
							foreach (Gemstone gem in gems)
							{
								if (gem.CanSocketArmor)
								{
									i++;
									actions.Add(new ActionOption<Player, MenuOptionType>("Socket Armor", $"Place {gem.Title} inside {myArmor.Title}",
										PlayerActions.IsHumanoid,
										new Action<Player>(player => PlayerActions.SocketItem(player, myArmor, gem)), MenuOptionType.Self, $"sa{i}"));
								}
							}
						}
					}

					// -------------------------------------- LOCATION-TARGETING ACTIONS --------------------------------------
					foreach (ActionOption<Player, MenuOptionType> ao in Location.GetActions())
					{
						if (ao.Condition(this)) actions.Add(ao);
					}

					// -------------------------------------- LOCATION-TARGETING ACTIONS (HUMANOID) --------------------------------------
					if (Location.GetItemCount > 0)
					{
						actions.Add(new ActionOption<Player, MenuOptionType>($"Get (Item)", $"Pick up item from location",
							PlayerActions.IsHumanoid,
							new Action<Player>(player => PlayerActions.GetItemFromLocation(player, player.Location, actionArg2)), MenuOptionType.Location, "g", "get"));
					}

					i = 0;
					foreach (Player p2 in GameEngine.Players.FindAll(p => p.Location == this.Location && p != this))
					{
						// -------------------------------------- PLAYER-TARGETING ACTIONS --------------------------------------
						i++;
						actions.Add(new ActionOption<Player, MenuOptionType>($"Look at {p2.Name}", $"Try to examine {p2.Name}", null,
							new Action<Player>(p1 => PlayerActions.Look(p1, p2)), MenuOptionType.Targeted, $"l{i}"));
						actions.Add(new ActionOption<Player, MenuOptionType>($"Talk to {p2.Name}", $"Attempt a heart-to-heart", null,
							new Action<Player>(p1 => PlayerActions.Talk(p1, p2, actionArg2)), MenuOptionType.Targeted, $"t{i}"));
						if (p2.IsAlive)
						{
							if (IsHostileToward(p2) || HasClient)
							{
								actions.Add(new ActionOption<Player, MenuOptionType>($"Attack {p2.Name}", $"Engage in combat with {p2.Name}", null,
									new Action<Player>(p1 => PlayerActions.Attack(p1, p2)), MenuOptionType.Targeted, $"a{i}"));
							}
						}
						// -------------------------------------- PLAYER-TARGETING ACTIONS (HUMANOID) --------------------------------------
						if (!p2.IsAlive)
						{
							actions.Add(new ActionOption<Player, MenuOptionType>($"Search {p2.Name}", $"Look for ph47 l3w7.",
								PlayerActions.IsHumanoid,
								new Action<Player>(p1 => PlayerActions.SearchCorpse(p1, p2)), MenuOptionType.Targeted, $"s{i}"));
						}
					}

				}
				else
				{
					List<Player> enemies = GameEngine.Players.FindAll(p2 => this.Location == p2.Location && p2.IsAlive && this.IsHostileToward(p2));
					if (enemies.Count > 0)
					{
						i = 0;
						foreach (Player p2 in enemies)
						{
							actions.Add(new ActionOption<Player, MenuOptionType>($"Attack {p2.Name}", $"Engage in combat with {p2.Name}", null,
								new Action<Player>(p1 => PlayerActions.Attack(p1, p2)), MenuOptionType.Targeted, $"a{i}"));
						}
					}
					else
					{
						if (rng.Next(10) == 0)
						{
							actions.Add(PlayerActions.Idle);
							if (rng.Next(3) == 0)
							{
								i = 0;
								foreach (Player p2 in GameEngine.Players.FindAll(p2 => this.Location == p2.Location && p2.IsAlive && this != p2))
								{
									i++;
									actions.Add(new ActionOption<Player, MenuOptionType>($"Look at {p2.Name}", $"Try to examine {p2.Name}", null,
										new Action<Player>(p1 => PlayerActions.Look(p1, p2)), MenuOptionType.Targeted, $"l{i}"));
									actions.Add(new ActionOption<Player, MenuOptionType>($"Talk to {p2.Name}", $"Attempt a heart-to-heart", null,
										new Action<Player>(p1 => PlayerActions.Talk(p1, p2)), MenuOptionType.Targeted, $"t{i}"));
								}
							}
						}
					}
				}
			}
		}


	}
}