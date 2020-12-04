using System;

namespace DazzleADV
{

	public static class PlayerDialogue
	{
		static Random rng = new Random();

		public static void HandlePlayerDialogue(Player p1, Player p2, string speech)
		{
			if (speech == "")
			{
				GameEngine.SayToLocation(p1.Location, $"{p1.Name} waves toward {p2.Name}.");
			}
			else
			{
				GameEngine.SayToLocation(p1.Location, $"{p1.Name} says to {p2.Name}, \"{speech}\"");
			}

			bool speechHandled = false;
			if (p2.IsAlive)
			{
				string speechLower = speech.ToLower();

				foreach (ISpeakable speechMod in Plugins.SpeechMods)
				{
					speechHandled = speechMod.HandleDialogue(p1, p2, speech, speechLower);
					if (speechHandled) break;
				}

				if (!speechHandled)
				{
					switch (p2.Race.Name)
					{
						case "Goblin":
							GameEngine.SayToLocation(p2.Location, $"{p2.Name} screeches back, \"HOK HOK HOK!\"");
							speechHandled = true;
							break;
						case "Human":
							switch (p2.UnitType)
							{
								case UnitType.PlayerCharacter:
									speechHandled = true;
									break;
								case UnitType.NorthGateGuard:
									if (GuardHostileResponse(p1, p2, speech))
									{
										speechHandled = true;
									}
									else if (GuardEmptySpeech(p1, p2, speech))
									{
										speechHandled = true;
									}
									else
									{
										if (speechLower.Contains("enter") || speechLower.Contains("let me in") || speechLower.Contains("open"))
										{
											if (p1.HasFlag(QuestFlags.TownTrustsPlayer))
											{
												LetPlayerEnterTownGate((Player)p1, (Player)p2, Locale.Downtown);
											}
											else if ((p1).HasItem(ItemType.GoblinRing) || (p1).HasItem(ItemType.BoneClub) || (p1).HasItem(ItemType.GoblinTrinket))
											{
												GameEngine.SayToLocation(p1.Location, $"{p2.Name} says, \"You carry the loot of a slain enemy. I see you are a friend. You may enter.\"");
												p1.AddFlag(QuestFlags.TownTrustsPlayer);
												LetPlayerEnterTownGate((Player)p1, (Player)p2, Locale.Downtown);
											}
											else
											{
												GameEngine.SayToLocation(p1.Location, $"{p2.Name} says, \"We don't trust strangers. Come back when you can prove yourself trustworthy.\"");
											}
											speechHandled = true;
										}
										else if (GuardHelpResponse(p1, p2, speech, speechLower))
										{
											speechHandled = true;
										}
										else
										{
											SayIdleGuardWords(p2);
											speechHandled = true;
										}
									}
									break;
							}
							break;

					}
				}
			}
			if (!speechHandled)
			{
				GameEngine.SayToLocation(p2.Location, $"{p2.Name} doesn't give any response.");
			}
		}

		public static bool GuardHostileResponse(Player p1, Player p2, string speech)
		{
			if (p2.IsHostileToward(p1))
			{
				if (speech == "")
				{
					GameEngine.SayToLocation(p2.Location, $"{p2.Name} ignores {p1.Name}.");
				}
				else
				{
					switch (rng.Next(3))
					{
						case 0:
							GameEngine.SayToLocation(p2.Location, $"{p2.Name} shouts, \"I will not parley with the enemy!\"");
							break;
						case 1:
							GameEngine.SayToLocation(p2.Location, $"{p2.Name} shouts, \"You will pay for your transgressions!\"");
							break;
						case 2:
							GameEngine.SayToLocation(p2.Location, $"{p2.Name} shouts, \"Prepare to meet your untimely end!\"");
							break;
					}
				}
				return true;
			}
			return false;
		}

		public static bool GuardEmptySpeech(Player p1, Player p2, string speech)
		{
			if (speech == "")
			{
				GameEngine.SayToLocation(p2.Location, $"{p2.Name} replies, \"Traveler, state your business.\"");
				p1.Notify("  *  Type 't1 let me in'");
				return true;
			}
			return false;
		}

		public static bool GuardHelpResponse(Player p1, Player p2, string speech, string speechLower)
		{
			if (speechLower.Contains("can you") || speechLower.Contains("help") || speechLower.Contains("come") || speechLower.Contains("follow"))
			{
				switch (rng.Next(3))
				{
					case 0:
						GameEngine.SayToLocation(p1.Location, $"{p2.Name} shakes {p2.Genderize("his", "her", "it")} head, \"I must stay my post.\"");
						break;
					case 1:
						GameEngine.SayToLocation(p1.Location, $"{p2.Name} shakes {p2.Genderize("his", "her", "it")} head, \"I must obey my orders.\"");
						break;
					case 2:
						GameEngine.SayToLocation(p1.Location, $"{p2.Name} shakes {p2.Genderize("his", "her", "it")} head, \"I cannot.\"");
						break;
				}
				return true;
			}
			return false;
		}

		public static void LetPlayerEnterTownGate(Player p1, Player p2, Location destination)
		{
			GameEngine.SayToLocation(p1.Location, $"{p2.Name} stoically looks you up and down then signals the gatekeeper.");
			LocationConnector portcullis = p1.Location.GetConnector(destination);
			portcullis.Unlock();
			GameEngine.SayToLocation(p1.Location, $"The massive portcullis rattles as it raises up.");
			portcullis.SetTickEvent(new ConnectorSelfEvent(ConnectorEventTemplates.ClosePortcullisOnCountdown, countdown: 2));
		}

		public static void SayIdleGuardWords(Player player)
		{
			switch (new Random().Next(4))
			{
				case 0:
					GameEngine.SayToLocation(player.Location, $"{player.Name} says, \"I used to be an adventurer like you. Then I took an arrow in the knee...\"");
					break;
				case 1:
					GameEngine.SayToLocation(player.Location, $"{player.Name} says, \"Let me guess... someone stole your sweetroll.\"");
					break;
				case 2:
					GameEngine.SayToLocation(player.Location, $"{player.Name} says, \"No lollygaggin'.\"");
					break;
				case 3:
					GameEngine.SayToLocation(player.Location, $"{player.Name} says, \"I'd be a lot warmer and a lot happier with a bellyful of mead...\"");
					break;
			}
		}

	}

}