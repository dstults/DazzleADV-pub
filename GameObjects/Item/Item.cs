using System;
using System.Collections.Generic;

namespace DazzleADV
{

	public interface IItemizable
	{
		bool IsNamed { get; }
		string Title { get; }
		string ASetOf { get; }
		string Generic { get; }
		string SetName();
	}

	public class Item : IItemizable
	{
		protected static Random rng = new Random();

		public readonly Enum Template;
		public bool IsNamed {get; protected set; } // Excalibur = true, Long Sword = false
		public string Title {get; protected set; } // "Exacalibur" ("named") / "Long Sword" ("unnamed")
		public string ASetOf {get; protected set; } // as in "a pair of" fists, "an outfit of" chainmail, or "a" longsword
		public string Generic {get; protected set; } // as in "sword"
		public string Description {get; protected set; }

		public Item(Enum template, bool isNamed, string titleName, string setName, string generalName, string description)
		{
			if (titleName == null)
				throw new ArgumentNullException("Error: Item constructor null string title name");
			if (titleName.Length == 0)
				throw new ArgumentException("Error: Item constructor empty string title name");
			if (setName == null)
				throw new ArgumentNullException("Error: Item constructor null string set name");
			// if (setName.Length == 0) OK
				
			if (generalName == null)
				throw new ArgumentNullException("Error: Item constructor null string general name");
			if (generalName.Length == 0)
				throw new ArgumentException("Error: Item constructor empty string general name");
			if (description == null)
				throw new ArgumentNullException("Error: Item constructor null string description");
			if (description.Length == 0)
				throw new ArgumentException("Error: Item constructor empty string description");

			this.Template = template;
			this.IsNamed = isNamed;
			this.Title = titleName;
			this.ASetOf = setName;
			this.Generic = generalName;
			this.Description = description;
		}

		public bool Matches(string looseMatch)
		{
			if (looseMatch == null)
				throw new ArgumentNullException("Error: Item.Matches null looseMatch");
			if (looseMatch.Length == 0)
				return false;

			looseMatch = looseMatch.ToLower().Replace(" ", "");
			string easyMatchName = $"{Title}{Generic}{Title}".ToLower().Replace(" ", "");
			if (easyMatchName.Contains(looseMatch))
				return true;
			return false;
		}

		public virtual string GuiString()
		{
			return $"[{Title}]";
		}

		public string SetName()
		{
			if (IsNamed)
				return $"the {Generic} {Title}";
			if (ASetOf == "")
				return Generic;
			return $"{ASetOf} {Generic}";
		}

		public string EquipString(Player player)
		{
			if (IsNamed)
				return $"{player.Name} equips {this.Title}.";
			return $"{player.Name} equips the {this.Generic}.";
		}

		public override string ToString()
		{
			return SetName();
		}

	}

}
