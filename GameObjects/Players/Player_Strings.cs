using System;
using System.Text;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{

	public partial class Player
	{
		// --------------------------------------------------------------------------------------------------------------------
		//  Stringifiers
		// --------------------------------------------------------------------------------------------------------------------

		public string Genderize(string s1, string s2, string s3)
		{
			if (s1 == null)
				throw new ArgumentNullException("Error: player genderize string 1 null string error");
			if (s1.Length == 0)
				throw new ArgumentException("Error: player genderize string 1 empty string error");
			if (s2 == null)
				throw new ArgumentNullException("Error: player genderize string 2 null string error");
			if (s2.Length == 0)
				throw new ArgumentException("Error: player genderize string 2 empty string error");
			if (s3 == null)
				throw new ArgumentNullException("Error: player genderize string 3 null string error");
			if (s3.Length == 0)
				throw new ArgumentException("Error: player genderize string 3 empty string error");

			if (Gender == Gender.Male)
				return s1;
			else if (Gender == Gender.Female)
				return s2;
			else
				return s3;
		}

		public string GetNotifications()
		{
			string result = "", newline = "";
			notifications.RemoveAll(tt => tt.HasExpired());
			foreach (TimeText tt in notifications)
			{
				result += $"{newline}{tt.Text}";
				newline = "\n";
			}
			if (result == "")
				result = "(No updates this turn.)";
			return result;
		}

		protected string GuiStatusHorizontal()
		{
			string result = "", comma = "";
			if (IsAlive)
			{
				foreach (StatusEffect se in GetStatusEffects())
				{
					if (se.OnTurnTick != null && se.OnTurnTick != StatusEvents.DoNothing)
						result += $"{comma}{se}";
					comma = ", ";
				}
				if (result == "") result = "Healthy (no negative status effects)";
			}
			else
			{
				if (DecayStatus != null)
					result = $"Died of {causeOfDeath} (Despawn in {DecayStatus.Value} turns)";
				else
					result = $"Died of {causeOfDeath}";
			}
			return result;
		}

		public string GetFullInGameView()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Environment.NewLine).Append(TextUtils.Borderize(GuiStringHorizontal(), margin: 2));
			sb.Append(Environment.NewLine).Append(TextUtils.Columnize(
				TextUtils.Borderize(Location.Name, 20),
				TextUtils.WordWrap(Location.GetDescription(this), 48),
				centerMargin: 4
			));
			sb.Append(Environment.NewLine);
			AssignActions();
			if (actions.Count > 0)
				sb.Append(Environment.NewLine).Append(TextUtils.GetOptionsMenuString(actions));
			sb.Append(Environment.NewLine).Append($"Notifications:\n{TextUtils.Borderize(TextUtils.WordWrap(GetNotifications(), 72), 72)}");
			sb.Append(Environment.NewLine);
			return sb.ToString();
		}

		public override string ToString()
		{
			return Name;
		}

		public string LookString()
		{
			StringBuilder sb = new StringBuilder();
			if (IsAlive)
				sb.Append($"  *  {Name} has {FlavorText.AmountOfAComparedtoB(HP, MaxHP)} {Genderize("his", "her", "its")} HP left.\n");
			else
				sb.Append($"  *  {Genderize("He", "She", "It")} fell to {causeOfDeath}.\n");
			if (IsHumanoid)
			{
				if (IsArmed && IsArmored)
					sb.Append($"  *  {Genderize("He", "She", "It")} is equipped with {Weapon.SetName()} and {Armor.SetName()}.\n");
				else if (IsArmed)
					sb.Append($"  *  {Genderize("He", "She", "It")} is armed with {Weapon.SetName()}.\n");
				else if (IsArmored)
					sb.Append($"  *  {Genderize("He", "She", "It")} is wearing {Armor.SetName()}.\n");
				else
					sb.Append($"  *  {Genderize("He", "She", "It")} is not equipped with anything.\n");
				if (!IsAlive && (IsArmed || IsArmored))
					sb.Append($"  *  {Genderize("His", "Her", "Its")} equipment's too damaged to salvage.\n");
				/*
					if (Inventory.GetCount() > 0)
						sb.Append($"  *  {Genderize("He", "She", "It")} is carrying {Inventory}.\n");
					else
						sb.Append($"  *  {Genderize("He", "She", "It")} doesn't seem to be carrying anything.\n");
				*/
			}
			else
			{
				if (IsArmed && IsArmored)
					sb.Append($"  *  {Genderize("He", "She", "It")} has {Weapon.SetName()} and {Armor.SetName()}.\n");
				else if (IsArmed)
					sb.Append($"  *  {Genderize("He", "She", "It")} has {Weapon.SetName()}.\n");
				else if (IsArmored)
					sb.Append($"  *  {Genderize("He", "She", "It")} has {Armor.SetName()}.\n");

			}
			string ailments = "", comma = "";
			foreach (StatusEffect se in GetStatusEffects())
			{
				if (!se.EffectClass.Equals(EffectClass.Decay) && se.OnTurnTick != null && se.OnTurnTick != StatusEvents.DoNothing)
				{
					ailments += $"{comma}{se.EffectClass.ToString().ToLower()}";
					comma = ", ";
				}
			}
			if (ailments == "")
				sb.Append($"  *  {Genderize("He", "She", "It")} doesn't seem to be suffering from anything.\n");
			else
				sb.Append($"  *  {Genderize("He", "She", "Tt")} seems to be suffering from {ailments}.\n");
			if (!IsAlive)
			{
				sb.Replace(" is ", " was ");
				sb.Replace(" be ", " have been ");
				sb.Replace(" has ", " had ");
			}
			return sb.ToString();
		}

		public string GuiStringHorizontal()
		{
			int itemMaxLength = 20;
			StringBuilder sb = new StringBuilder();
			sb.Append($"Name:   {Name.PadRight(itemMaxLength).Substring(0, itemMaxLength)} | Health: {HP,6:n0} / {MaxHP,-6:n0}\n");
			sb.Append($"Weapon: {Weapon.GuiString().PadRight(itemMaxLength).Substring(0, itemMaxLength)} | Armor: {Armor.GuiString().PadRight(itemMaxLength).Substring(0, itemMaxLength)}\n");
			if (IsHumanoid)
				sb.Append($"{TextUtils.Columnize("Items:", TextUtils.WordWrap(Inventory.GuiStringHorizontal(), 60), 2)}\n");
			sb.Append($"Status: {GuiStatusHorizontal()}");
			return sb.ToString();
		}


	}
}