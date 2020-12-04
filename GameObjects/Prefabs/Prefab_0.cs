using System;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{

	public static partial class Prefabs
	{

		static Random rng = new Random();

		private static Dictionary<string, ItemType> reverseItemDictionary = TextUtils.GetInvertedStringDictionaryFromEnum<ItemType>(numbered: true, lowered: true);
		private static Dictionary<string, UnitType> reverseUnitDictionary = TextUtils.GetInvertedStringDictionaryFromEnum<UnitType>(numbered: true, lowered: true);

		public static readonly Weapon Unarmed = new Weapon(ItemType.Unarmed, false, "Fists", "a pair of", "fists", "When all else fails, you have your hands.", 1, 2);
		public static readonly Armor Unarmored = new Armor(ItemType.Unarmored, false, "Tattered Cloths", "a set of", "tattered cloths", "At least you're not naked!", 0, 1);

		public static void TestGenerationDatabase()
		{
			GameEngine.SayToServer(" - Testing item prefab generation database...");
			foreach (ItemType it in Enum.GetValues(typeof(ItemType)))
			{
				GameEngine.SayToServer($"{it}...");
				if (it != ItemType.Unarmed && it != ItemType.Unarmored)
					NewItem(it);
			}
			GameEngine.SayToServer("done.\n");

			GameEngine.SayToServer(" - Testing unit prefab generation database...");
			foreach (UnitType ut in Enum.GetValues(typeof(UnitType)))
			{
				GameEngine.SayToServer($"{ut}...");
				if (ut != UnitType.PlayerCharacter)
				{
					foreach (Gender g in Enum.GetValues(typeof(Gender)))
					{
						if (g != Gender.Unset && g != Gender.Genderless)
							SpawnUnit(ut, null, g);
					}
				}
			}
			GameEngine.SayToServer("done.\n");
		}

		public static Item NewItem(Enum template)
		{
			if (template.Equals(ItemType.Unarmed))
				throw new ArgumentException("Cannot make weapon item out of 'Unarmed'");
			if (template.Equals(ItemType.Unarmored))
				throw new ArgumentException("Cannot make armor item out of 'Unarmored'");

			Item newItem;
			foreach (IFabricable templateMod in Plugins.TemplateMods)
			{
				newItem = templateMod.NewItem(template);
				if (newItem != null)
					return newItem;
			}

			if (template is ItemType)
			{
				template = (ItemType)template;
				switch (template)
				{
					// ---------------------- INGREDIENTS ----------------------
					case ItemType.RabbitsFoot:
						return new Item(template, false, "Rabbit's Foot", "a", "rabbit's foot", "A good luck charm. Maybe.");
					case ItemType.GoblinRing:
						return new Item(template, false, "Goblin's Ring", "a", "goblin's ring", "An adornment crudely crafted, with love.");
					case ItemType.BlackPowder:
						return new Item(template, false, "Black Powder", "a bag of", "black powder", "A bagfull of a strange, sooty substance.");
					case ItemType.SpiderLeg:
						return new Item(template, false, "Spider Leg", "a", "spider leg", "A hard, thorny curled-up spider leg.");
					case ItemType.VenomSac:
						return new Item(template, false, "Venom Sac", "a", "spider venom sac", "An organ full of poisonous bile.");
					case ItemType.SpiderFang:
						return new Item(template, false, "Spider Fang", "a", "spider fang", "A useful ingredient.");

					// ---------------------- GEMSTONES ----------------------
					case ItemType.BroodMotherEye:
						return new Gemstone(template, false, "Brood Mother Eye", "a", "brood mother eye", "[Weapon Socket] Poison everything...",
							power: 2, statusEffect: new StatusEffect(EffectClass.Poison, effectValue: 3,
							turnTickEvent: StatusEvents.PoisonPlayer, effectExpiresEvent: StatusEvents.PoisonCuredByTime),
							socketsWeapons: true, socketsArmor: false);
					case ItemType.GoblinTrinket:
						return new Gemstone(template, false, "Goblin Trinket", "a", "goblin trinket", "[Weapon Socket] Adds 1 point of damage.",
							power: 1,
							socketsWeapons: true, socketsArmor: false);
					case ItemType.Emerald:
						return new Gemstone(template, false, "Emerald", "an", "emerald", "[Armor Socket] Grants 1 point of regneration.",
							statusEffect: new StatusEffect(EffectClass.Regen, turnTickEvent: StatusEvents.RegenPlayer),
							socketsWeapons: false, socketsArmor: true);
					case ItemType.HolyAnkh:
						return new Item(template, false, "Holy Ankh", "a", "holy ankh", "Resurrects the holder on death.");

					// ---------------------- WEAPONS ----------------------
					case ItemType.Stick:
						return new Weapon(template, false, "Stick", "a", "stick", "A crooked wooden appendage.", 3, 4);
					case ItemType.Knife:
						return new Weapon(template, false, "Knife", "a", "knife", "A small pocket knife.", 3, 4);
					case ItemType.BoneClub:
						return new Weapon(template, false, "Bone Club", "a", "bone club", "Actually just a really big bone.", 4, 6);
					case ItemType.CultistKris:
						return new Weapon(template, false, "Kris", "a", "kris", "An swerving dagger used in sacrifices.", 4, 6);
					case ItemType.SpikedClub:
						return new Weapon(template, false, "Spiked Club", "a", "spiked club", "The spikes are more like bumps.", 6, 9);
					case ItemType.RustySword:
						return new Weapon(template, false, "Rusty Sword", "a", "rusty sword", "An old, weathered sword.", 6, 9);
					case ItemType.Longsword:
						return new Weapon(template, false, "Longsword", "a", "longsword", "A standard longsword.", 9, 12);
					case ItemType.MythrilSword:
						return new Weapon(template, false, "Mythril Sword", "a", "mythril sword", "An ornate sword made of refined mythril.", 9, 12);
					case ItemType.HolySpear:
						return new Weapon(template, false, "Holy Spear", "a", "holy spear", "A spear blessed by the temple priests.", 10, 15);
					case ItemType.Excalibur:
						return new Weapon(template, true, "Caliburnus", "a", "legendary sword", "The Avalonian sword of the Once King.", 25, 30);

					// ---------------------- ARMORS ----------------------
					case ItemType.Cloth:
						return new Armor(template, false, "Clothing", "an outfit of", "clothing", "Ordinary clothes.", 1, 2);
					case ItemType.Robes:
						return new Armor(template, false, "Robe", "a", "robe", "A thick gray robe with a rope to tighten around the waist.", 1, 3);
					case ItemType.Leather:
						return new Armor(template, false, "Leather Armor", "an outfit of", "leather armor", "Leather garments popular with adventurers.", 2, 3);
					case ItemType.ChainMail:
						return new Armor(template, false, "Chain Mail", "a set of", "chain mail", "Mail made up of chain links.", 3, 4);
					case ItemType.PlateArmor:
						return new Armor(template, false, "Plate Mail", "a set of", "plate mail", "Interconnected hardened iron plates.", 4, 5);
					case ItemType.MythrilChainMail:
						return new Armor(template, false, "Mythril Mail", "a set of", "mythril mail", "Gleaming mail made up of refined mythril chain links.", 6, 7);

					// ---------------------- Player BODY PARTS ----------------------
					case ItemType.WeakPoisonFangs:
						return new Weapon(template, false, "Poison Fangs (Weak)", "", "small poison fangs", "Samll fangs that inject a miniscule amount of poison into the target.",
							minDamage: 1, maxDamage: 3,
							statusEffect: new StatusEffect(EffectClass.Poison, effectValue: 1, attackHitEvent: StatusEvents.PoisonHit));
					case ItemType.PoisonFangs:
						return new Weapon(template, false, "Poison Fangs (Moderate)", "", "medium poison fangs", "Medium fangs that inject a moderate amount of poison into the target.",
							minDamage: 2, maxDamage: 5,
							statusEffect: new StatusEffect(EffectClass.Poison, effectValue: 2, attackHitEvent: StatusEvents.PoisonHit));
					case ItemType.StrongPoisonFangs:
						return new Weapon(template, false, "Poison Fangs (Strong)", "", "large poison fangs", "Large fangs that inject a significant amount of poison into the target.",
							minDamage: 5, maxDamage: 8,
							statusEffect: new StatusEffect(EffectClass.Poison, effectValue: 3, attackHitEvent: StatusEvents.PoisonHit));

					case ItemType.HardBone:
						return new Armor(template, false, "Hard Bone", "", "hard bone", "The natural defense of an animated skeleton.", 0, 3);
					case ItemType.Chitin:
						return new Armor(template, false, "Chitin", "a layer of", "chitin", "Bendable arthropod skin.", 1, 2);
					case ItemType.ThickChitin:
						return new Armor(template, false, "Thick Chitin", "a layer of", "thick chitin", "Thick, bendable arthropod skin.", 3, 4);
				}
			}
			throw new ArgumentOutOfRangeException($"[NewItem] Unhandled item template: {template}");
		}

		public static Player NewPlayer(string playerName, Gender gender, Location location = null, GameClient client = null)
		{
			if (playerName == null)
				throw new ArgumentNullException("Error: Player name string null at Prefabs.NewPlayer player creation");
			if (playerName.Length == 0)
				throw new ArgumentNullException("Error: Player name set to blank at Prefabs.NewPlayer player creation");
			if (playerName.Length < GameEngine.MinimumNameLength || playerName.Length > GameEngine.MaximumNameLength)
				throw new ArgumentOutOfRangeException($"Error: Player.NewPlayer name length must be between {GameEngine.MinimumNameLength} and {GameEngine.MaximumNameLength} characters.");
			if (location == null)
				location = WorldMap.PlayerSpawnPoint;
			// client null OK

			Player player = new Player(UnitType.PlayerCharacter, TextUtils.FormatName(playerName), 20, gender, Faction.Players, Race.Human, location: location);

			if (client != null)
			{
				player.AssignClient(client);
				client.AssignAvatar(player);
			}

			player.AddItem(ItemType.RabbitsFoot);
			GameEngine.AddPlayer(player);

			foreach (IPlayerModifiable playerMod in Plugins.PlayerMods)
			{
				player = playerMod.ModifyNewPlayer(player);
			}
			GameEngine.SayToLocation(location, $"{player.Name} appears!");

			return player;
		}

		public static Player SpawnUnit(Enum template, Location location, Gender gender = Gender.Unset)
		{
			// Location == null : valid

			Player newUnit = null;
			foreach (IFabricable templateMod in Plugins.TemplateMods)
			{
				newUnit = templateMod.NewUnit(template);
				if (newUnit != null)
					break;
			}

			if (newUnit == null && template is UnitType)
			{
				template = (UnitType)template;
				string unitName = "";
				if (gender == Gender.Unset)
					gender = GetRandomGender();
				switch (template)
				{
					case UnitType.NorthGateGuard:
						unitName = $"{GetRandomName(Race.Human, gender, template)} the Guard";
						newUnit = new Player(template, unitName, 30, gender, Faction.Townsfolk, Race.Human, location,
							ItemType.Longsword, ItemType.ChainMail);
						break;
					case UnitType.SouthGateGuard:
						unitName = $"{GetRandomName(Race.Human, gender, template)} the Guard";
						newUnit = new Player(template, unitName, 30, gender, Faction.Townsfolk, Race.Human, location,
							ItemType.Longsword, ItemType.ChainMail);
						break;
					case UnitType.KeepGuard:
						unitName = $"{GetRandomName(Race.Human, gender, template)} the Guard";
						newUnit = new Player(template, unitName, 40, gender, Faction.Townsfolk, Race.Human, location,
							ItemType.MythrilSword, ItemType.PlateArmor);
						break;
					case UnitType.Chancellor:
						unitName = $"Chancellor {GetRandomName(Race.Human, gender, template)}";
						newUnit = new Player(template, unitName, 50, gender, Faction.Townsfolk, Race.Human, location,
							ItemType.MythrilSword, ItemType.PlateArmor);
						break;
					case UnitType.KingQueen:
						unitName = gender == Gender.Male ? $"King {GetRandomName(Race.Human, gender, template)}" : $"Queen {GetRandomName(Race.Human, gender, template)}";
						newUnit = new Player(template, unitName, 50, gender, Faction.Townsfolk, Race.Human, location,
							ItemType.MythrilSword, ItemType.PlateArmor);
						break;
					case UnitType.WeakGoblin:
						unitName = GetRandomName(Race.Goblin, gender, template) + " the Goblin";
						newUnit = new Player(template, unitName, 20, gender, Faction.Monsters, Race.Goblin, location,
							ItemType.BoneClub, ItemType.Leather);
						AddGoblinLoot(newUnit);
						break;
					case UnitType.Cultist:
						unitName = $"{GetRandomName(Race.Human, gender, template)} the Cultist";
						newUnit = new Player(template, unitName, 20, gender, Faction.Monsters, Race.Human, location,
							ItemType.CultistKris, ItemType.Robes);
						AddCultistLoot(newUnit);
						break;
					case UnitType.Skeleton:
						unitName = "Skeleton";
						newUnit = new Player(template, unitName, 15, Gender.Genderless, Faction.Monsters, Race.UndeadHuman, location,
							ItemType.Longsword, ItemType.HardBone);
						break;
					case UnitType.Spiderling:
						unitName = "Spiderling";
						newUnit = new Player(template, unitName, 8, Gender.Genderless, Faction.Monsters, Race.Arachnid, location,
							ItemType.WeakPoisonFangs, ItemType.Chitin);
						AddSpiderlingLoot(newUnit);
						break;
					case UnitType.Spider:
						unitName = "Spider";
						newUnit = new Player(template, unitName, 20, Gender.Genderless, Faction.Monsters, Race.Arachnid, location,
							ItemType.PoisonFangs, ItemType.Chitin);
						AddSpiderLoot(newUnit);
						break;
					case UnitType.BroodMother:
						unitName = "Brood Mother";
						newUnit = new Player(template, unitName, 50, Gender.Female, Faction.Monsters, Race.Arachnid, location,
							ItemType.StrongPoisonFangs, ItemType.ThickChitin);
						AddBroodMotherLoot(newUnit);
						break;
				}
			}
			if (newUnit == null)
				throw new ArgumentOutOfRangeException($"[NewNpc] Unhandled NPC template: {template}");

			if (location != null)
			{
				GameEngine.AddPlayer(newUnit);
				GameEngine.SayToLocation(location, $"{newUnit.Name} appears!");
			}
			return newUnit;
		}

		public static string GetPrefabItemList()
		{
			return TextUtils.Borderize(TextUtils.Columnize(TextUtils.GetStringListFromEnum<ItemType>(numbered: true)));
		}

		public static Item SpawnItemAtLocation(string itemName, Location location)
		{
			if (itemName == null)
				throw new ArgumentNullException("Error: Prefabs.SpawnItemAtLocation null itemName");
			if (location == null)
				throw new ArgumentNullException("Error: Prefabs.SpawnItemAtLocation null location");

			itemName = itemName.Trim().ToLower();
			if (itemName.Length == 0)
				return null;

			foreach (string key in reverseItemDictionary.Keys)
			{
				if (key.Contains(itemName))
				{
					Item spawnedItem = NewItem(reverseItemDictionary[key]);
					location.AddItem(spawnedItem);
					return spawnedItem;
				}
			}
			return null;
		}

		public static string GetPrefabUnitList()
		{
			return TextUtils.Borderize(TextUtils.Columnize(TextUtils.GetStringListFromEnum<UnitType>(numbered: true)));
		}

		public static Player SpawnUnitAtLocation(string unitName, Location location)
		{
			if (unitName == null)
				throw new ArgumentNullException("Error: Prefabs.SpawnUnitAtLocation null unitName");
			if (location == null)
				throw new ArgumentNullException("Error: Prefabs.SpawnUnitAtLocation null location");

			unitName = unitName.Trim().ToLower();
			if (unitName.Length == 0)
				return null;

			foreach (string key in reverseUnitDictionary.Keys)
			{
				if (key.Contains(unitName))
				{
					UnitType myType = reverseUnitDictionary[key];
					if (myType == UnitType.PlayerCharacter)
					{
						Console.Write(" Player gender? ( M , F ) ( X to abort ) > ");
						char genderChar = TextUtils.GetKeyInput(new List<char> { 'm', 'f', 'x' });
						if (genderChar == 'x')
						{
							Console.WriteLine("\nAborted!");
							return null;
						}
						Gender gender = genderChar == 'm' ? Gender.Male : Gender.Female;
						string nameInput = TextUtils.GetStringInput(prompt: "\n Player name? ", GameEngine.MinimumNameLength, GameEngine.MaximumNameLength, forceInput: false);
						if (nameInput.Length == 0)
						{
							Console.WriteLine("Invalid name, aborted!");
							return null;
						}
						Player spawnedPlayer = NewPlayer(nameInput, gender, location, null);
						return spawnedPlayer;
					}
					else
					{
						Player spawnedUnit = SpawnUnit(myType, location);
						return spawnedUnit;
					}
				}
			}
			return null;
		}

	}
}