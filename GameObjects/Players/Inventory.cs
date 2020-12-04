using System;
using System.Collections.Generic;

namespace DazzleADV
{

	public class Inventory
	{
		private static Random rng = new Random();

		public List<Item> Items { get; private set; }

		public Inventory()
		{
			Items = new List<Item>();
		}

		public bool HasItem(Enum itemType)
		{
			return GetItemByType(itemType) != null;
		}

		public bool HasItem(Item item)
		{
			return Items.Contains(item);
		}

		public int HasItems(Enum itemType)
		{
			return Items.FindAll(i => i.Template == itemType).Count;
		}

		public int GetCount()
		{
			return Items.Count;
		}

		public Item GetItemByLooseMatch(string looseMatch)
		{
			if (looseMatch == null)
				throw new ArgumentNullException($"Player/GetItem, match string null error");
			if (looseMatch.Length == 0)
				throw new ArgumentException($"Player/GetItem, match string empty");

			return Items.Find(i => i.Matches(looseMatch));
		}

		public Item GetItemByType(Enum itemType)
		{
			return Items.Find(i => i.Template.Equals(itemType));
		}

		public List<Item> GetItemsByType(Enum itemType, int count)
		{
			if (count <= 0)
				throw new ArgumentException($"Inventory Get Items by Name [{itemType}]: count [{count}] must be greater than zero");

			List<Item> queryItems = Items.FindAll(i => i.Template.Equals(itemType));
			if (queryItems.Count < count)
				return null;
			if (queryItems.Count > count)
				queryItems.RemoveRange(count, count - queryItems.Count);
			return Items;
		}

		public List<Gemstone> GetGemstones()
		{
			List<Gemstone> results = new List<Gemstone>();
			foreach(Item item in Items.FindAll(i => i is Gemstone))
			{
				results.Add((Gemstone)item);
			}
			return results;
		}

		public bool RemoveItem(Item item)
		{
			if (item == null)
				throw new ArgumentNullException("Inventory Remove Item by Item: null item error");

			if (Items.Contains(item))
			{
				Items.Remove(item);
				return true;
			}
			return false;
		}

		public Item PopItem(Enum itemType)
		{
			Item myItem = GetItemByType(itemType);
			if (myItem != null) Items.Remove(myItem);
			return myItem;
		}

		public Item PopRandom() // getrandomitem get random item
		{
			if (GetCount() == 0)
				return null;
			
			Item myItem = Items[rng.Next(GetCount())];
			if (myItem != null) Items.Remove(myItem);
			return myItem;
		}

		public List<Item> RemoveItems(ItemType itemType, int count)
		{
			if (count <= 0)
				throw new ArgumentException($"Inventory Remove Items by Name [{itemType}]: count [{count}] must be greater than zero");

			List<Item> myItems = GetItemsByType(itemType, count);
			if (myItems != null)
			{
				foreach(Item item in myItems)
				{
					Items.Remove(item);
				}
			}
			return myItems;
		}

		public void AddItem(Enum itemType)
		{
			Items.Add(Prefabs.NewItem(itemType));
		}

		public void AddItem(Item item)
		{
			if (item == null)
				throw new ArgumentNullException("Inventory Add Item: null item error");

			Items.Add(item);
		}

		public string GuiStringHorizontal()
		{
			string result = "[", comma = "";
			foreach (Item item in Items)
			{
				result += $"{comma}{item.GuiString()}";
				comma = ", ";
			}
			result += "]";
			return result;
		}

		public string GuiStringVertical()
		{
			string result = "";
			foreach (Item item in Items)
			{
				result += $"{item.GuiString()}\n";
			}
			return result;
		}

		public override string ToString()
		{
			string result = "", comma = "";
			foreach (Item item in Items)
			{
				result += $"{comma}{item.SetName()}";
				comma = ", ";
			}
			return result;
		}

	}

}