using System;
using System.Text;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{

	public interface IEquippable
	{
		Inventory Inventory { get; }
		void Equip(Armor armor);
		void Equip(Weapon weapon);
		void AddItem(Enum itemType);
		void AddItem(Item item);
		bool HasItem(Enum itemType);
		int HasItems(Enum itemType);
		Item GetItem(string looseMatch);
		Item GetItem(Enum itemType);
		Item RemoveItem(Enum itemType);
		bool RemoveItem(Item item);
	}

	public interface ILootable
	{
		Inventory Inventory { get; }
		bool HasLoot();
		Item GetLoot();
		void AddLoot(Item item);
		void AddLoot(Enum itemType);
	}

	public partial class Player : IEquippable, ILootable
	{
		public Inventory Inventory { get; protected set; } = new Inventory();

		public void Equip(Weapon equippedWeapon) // equipitem
		{
			if (equippedWeapon == null)
				return;

			if (this.weapon != null)
			{
				Inventory.AddItem(this.weapon);
			}
			if (Inventory.HasItem(equippedWeapon))
			{
				Inventory.RemoveItem(equippedWeapon);
			}
			GameEngine.SayToLocation(Location, equippedWeapon.EquipString(this));
			this.weapon = equippedWeapon;
		}

		public void Equip(Armor equippedArmor)
		{
			if (equippedArmor == null)
				return;

			if (this.armor != null)
			{
				Inventory.AddItem(this.armor);
			}
			if (Inventory.HasItem(equippedArmor))
			{
				Inventory.RemoveItem(equippedArmor);
			}
			GameEngine.SayToLocation(Location, equippedArmor.EquipString(this));
			this.armor = equippedArmor;
		}

		public void AddItem(Enum itemType)
		{
			AddItem(Prefabs.NewItem(itemType));
		}

		public void AddItem(Item item)
		{
			if (HasItems(item.Template) >= 5)
			{
				GameEngine.SayToLocation(Location, $"{this.Name} realizes {Genderize("he", "she", "it")} has too many {item}s and drops one on the ground.");
				Location.AddItem(item);
			}
			else
			{
				if (item is Weapon)
				{
					Weapon newWeapon = (Weapon)item;
					if (newWeapon.AverageDamage > Weapon.AverageDamage)
					{
						Equip(newWeapon);
					}
					else
					{
						Inventory.AddItem(item);
					}
				}
				else if (item is Armor)
				{
					Armor newArmor = (Armor)item;
					if (newArmor.AverageDefense > Armor.AverageDefense)
					{
						Equip(newArmor);
					}
					else
					{
						Inventory.AddItem(item);
					}
				}
				else
				{
					Inventory.AddItem(item);
				}
			}
		}

		public bool HasItem(Enum itemType)
		{
			if (Inventory.HasItem(itemType)) return true;
			if (weapon != null && weapon.Template.Equals(itemType)) return true;
			if (armor != null && armor.Template.Equals(itemType)) return true;
			return false;
		}

		public int HasItems(Enum itemType)
		{
			int myTotal = 0;
			myTotal += Inventory.HasItems(itemType);
			if (weapon != null && weapon.Template.Equals(itemType)) myTotal++;
			if (armor != null && armor.Template.Equals(itemType)) myTotal++;
			return myTotal;
		}

		public bool HasLoot()
		{
			return Inventory.Items.Count > 0;
		}

		public Item GetLoot()
		{
			return Inventory.PopRandom();
		}

		public void AddLoot(Enum itemType)
		{
			Inventory.AddItem(itemType);
		}

		public void AddLoot(Item item)
		{
			Inventory.AddItem(item);
		}

		public Item GetItem(string looseMatch)
		{
			if (looseMatch == null)
				throw new ArgumentNullException($"Error: Player.GetItem match string null");
			if (looseMatch.Length == 0)
				return null;

			Item possibleItem = Inventory.GetItemByLooseMatch(looseMatch);
			if (possibleItem != null) return possibleItem;
			else if (weapon != null && weapon.Matches(looseMatch)) return weapon;
			else if (armor != null && armor.Matches(looseMatch)) return armor;
			else return null;
		}

		public Item GetItem(Enum itemType)
		{
			Item possibleItem = Inventory.GetItemByType(itemType);
			if (possibleItem != null) return possibleItem;
			else if (weapon != null && weapon.Template.Equals(itemType)) return weapon;
			else if (armor != null && armor.Template.Equals(itemType)) return armor;
			else return null;
		}

		public Item RemoveItem(Enum itemType)
		{
			Item possibleItem = Inventory.GetItemByType(itemType);
			if (possibleItem != null)
			{
				Inventory.RemoveItem(possibleItem);
				return possibleItem;
			}
			else if (weapon != null && weapon.Template.Equals(itemType))
			{
				possibleItem = weapon;
				weapon = null;
				return possibleItem;
			}
			else if (armor != null && armor.Template.Equals(itemType))
			{
				possibleItem = armor;
				armor = null;
				return possibleItem;
			}
			else return null;
		}

		public bool RemoveItem(Item item)
		{
			if (Inventory.RemoveItem(item))
			{
				return true;
			}
			else if (weapon != null && weapon == item)
			{
				weapon = null;
				return true;
			}
			else if (armor != null && armor == item)
			{
				armor = null;
				return true;
			}
			else
			{
				return false;
			}
		}

	}

}
