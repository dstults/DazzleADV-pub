using System;

namespace DStults.Utils
{

	public interface INameable
	{
		string Name { get;}
	}

	public class ActionOption<TClass, TCategory> where TCategory : System.Enum
	{
		public string Name { get; private set; }
		public string ShortCommand { get; private set; }
		public string CommandAlias { get; private set; }
		public string Description { get; private set; }
		public Predicate<TClass> Condition { get; private set; }
		public Action<TClass> PerformAction { get; private set; }
		public TCategory OptionType { get; private set; }

		public ActionOption(string name, string description, Predicate<TClass> condition, Action<TClass> performedAction, TCategory optionType, string shortCommand, string commandAlias = "")
		{
			if (name == null)
				throw new ArgumentNullException("Error: ActionOption name null string error");
			if (name.Length == 0)
				throw new ArgumentException("Error: ActionOption name empty string error");
			if (description == null)
				throw new ArgumentNullException("Error: ActionOption description null string error");
			if (description.Length == 0)
				throw new ArgumentException("Error: ActionOption description empty string error");
			if (condition == null)
				condition = TIsNotNull;
			if (performedAction == null)
				throw new ArgumentNullException("Error: ActionOption action null action error");
			if (shortCommand == null)
				throw new ArgumentNullException("Error: ActionOption shortCommand null string error");
			if (shortCommand.Length == 0)
				throw new ArgumentException("Error: ActionOption shortCommand empty string error");
			if (shortCommand.Length > 3)
				throw new ArgumentException("Error: ActionOption shortCommand must be 1 to 3 characters long");
			if (commandAlias == null)
				throw new ArgumentException("Error: ActionOption alternateCommand null string error");
			if (commandAlias.Length > 0 && commandAlias.Length < 3)
				throw new ArgumentException("Error: ActionOption alternateCommand must be 0 or >= 3 characters long");

			this.Name = name;
			this.Description = description;
			this.Condition = condition;
			this.PerformAction = performedAction;
			this.OptionType = optionType;
			this.ShortCommand = shortCommand.ToLower();
			this.CommandAlias = commandAlias.ToLower();
		}

		public bool Matches(string input)
		{
			if (input == null)
				return false;
			input = input.Trim();
			if (input.Length == 0)
				return false;

			return input == ShortCommand || CommandAlias != "" && input == CommandAlias;
		}

		private static bool TIsNotNull(TClass obj)
		{
			return obj != null;
		}

		public override string ToString()
		{
			return Name;
		}

	}

}
