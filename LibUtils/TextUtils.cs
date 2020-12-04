using System;
using System.Text;
using System.Collections.Generic;

namespace DStults.Utils
{

	public static class TextUtils
	{

		public static List<string> UncapitalizedWords = new List<string>{"the", "a", "of", "and", "or", "to", "from", "at"};

		public static int? ParseInt(string input, int min, int max, bool verbose = false)
		{
			int? result = null;
			try
			{
				result = int.Parse(input);
				if (result < min || result > max)
				{
					if (verbose) Console.WriteLine($"Cancelled: Input must be an integer between {min} and {max} !");
					result = null;
				}
			}
			catch (FormatException)
			{
				if (verbose) Console.WriteLine($"Failed: Input must be an integer between {min} and {max} !");
				result = null;
			}
			catch (OverflowException)
			{
				if (verbose) Console.WriteLine($"Failed: Input must be an integer between {min} and {max} !");
				result = null;
			}
			return result;
		}

		public static string GetStringInput(string prompt, int minLength, int maxLength, bool forceInput = true)
		{
			string myString = "";
			do
			{
				Console.Write($"{prompt} ({minLength}-{maxLength} characters) > ");
				myString = Console.ReadLine();
				myString = myString.Trim();
				if (myString.Length >= minLength || myString.Length <= maxLength)
					return myString;
				else
					myString = "";
			} while (forceInput);
			return myString;
		}

		public static char GetKeyInput(List<char> allowedKeys)
		{
			char myKey;
			while (true)
			{
				if (Console.KeyAvailable)
				{
					myKey = Console.ReadKey(true).KeyChar;
					if (allowedKeys.Contains(myKey))
					{
						return myKey;
					}
				}
			}
		}

		public static string Repeat(char letter, int count)
		{
			if (count < 0) // || count > 100)
				throw new ArgumentOutOfRangeException("Repeat count should be between (incl) 0 and 100");

			StringBuilder sb = new StringBuilder(count);
			for (int i = 0; i < count; i++)
			{
				sb.Insert(i, letter);
			}
			return sb.ToString();
		}

		public static string GetOptionsMenuString<TClass, TCategory>(List<ActionOption<TClass, TCategory>> options, int nameWidth = -1, int descriptionWidth = -1) where TCategory : Enum
		{
			lock (options) // static method must be threadsafe, this gets called by multiple tasks
			{
				if (options == null)
					throw new ArgumentNullException("Error: TextUtils.GetOptionsMenuString null options list");
				if (options.Count == 0)
					return "";
				if (nameWidth != -1 && nameWidth < 7)
					throw new ArgumentOutOfRangeException("Error: TextUtils.GetOptionsMenuString nameMaxWidth must be -1 (unset) or >= 7 chars long");
				if (descriptionWidth != -1 && descriptionWidth < 15)
					throw new ArgumentOutOfRangeException("Error: TextUtils.GetOptionsMenuString descriptionMaxWidth must be -1 (unset) or >= 15 chars long");

				if (nameWidth == -1)
				{
					string[] sa = new string[options.Count];
					int i = 0;
					options.ForEach(o => { sa[i] = o.Name; i++; });
					nameWidth = GetWidth(sa);
				}
				if (descriptionWidth == -1)
				{
					string[] sa = new string[options.Count];
					int i = 0;
					options.ForEach(o => { sa[i] = o.Description; i++; });
					descriptionWidth = GetWidth(sa);
				}
				int width = nameWidth + descriptionWidth + " -- ".Length + "000) ".Length;
				string result = "";

				foreach (TCategory thisType in Enum.GetValues(typeof(TCategory)))
				{
					List<ActionOption<TClass, TCategory>> optionsOfThisType = options.FindAll(p => p.OptionType.Equals(thisType));
					if (optionsOfThisType.Count > 0)
					{
						result += $"====== {thisType}: {Repeat('=', width - thisType.ToString().Length)}=====\n";
						foreach (ActionOption<TClass, TCategory> option in optionsOfThisType)
						{
							result += $" {option.ShortCommand,3}) {option.Name.PadRight(nameWidth).Substring(0, nameWidth)} -- {option.Description.PadRight(descriptionWidth).Substring(0, descriptionWidth)}\n";
						}
					}
				}
				return result;
			}
		}

		public static string Borderize(string text, int width = -1, int margin = 0, bool headless = false, bool footless = false, bool sideless = false)
		{
			if (text == null)
				return "";
			if (text.Length == 0)
				return "";
			if (width < -1 || (width >= 0 && width < 10) || width > 120)
				throw new ArgumentOutOfRangeException("Error: TextUtils.Borderize width must be either -1 (default) or between 10 and 120");
			if (margin < 0 || margin > 40)
				throw new ArgumentOutOfRangeException("Error: TextUtils.Borderize margin must be between 0 and 40 (inclusive)");

			string[] textArray = text.Split("\n");
			int actualWidth = GetWidth(textArray);
			if (width == -1)
			{
				width = actualWidth;
			}
			else if (actualWidth > width)
			{
				text = WordWrap(text, width);
				textArray = text.Split("\n");
			}

			string result = "";
			//result += $"+-{Repeat('-', width)}-+ w={width}\n";
			if (!headless) result += $"{Repeat(' ', margin)}+-{Repeat('-', width)}-+\n";
			foreach (string line in textArray)
			{
				result += $"{Repeat(' ', margin)}| {line.PadRight(width)} |\n";
			}
			if (!footless) result += $"{Repeat(' ', margin)}+-{Repeat('-', width)}-+";
			return result;
		}

		private static int GetWidth(string[] text)
		{
			if (text == null)
				throw new ArgumentNullException("Error: TextUtils.GetWidth null text array error");
			if (text.Length == 0)
				throw new ArgumentException("Error: TextUtils.GetWidth empty text array error");

			int width = -1;
			foreach (string line in text)
			{
				if (line.Length > width)
					width = line.Length;
			}
			return width;
		}

		public static string WordWrap(string text, int maxWidth)
		{
			if (text == null)
				throw new ArgumentNullException("Error: TextUtils.WordWrap null text input");
			if (text.Length == 0)
				throw new ArgumentException("Error: TextUtils.WordWrap empty text input");
			if (maxWidth < 10 && maxWidth > 60)
				throw new ArgumentOutOfRangeException("Error: TextUtils.Columnize centerMargin not between 10 and 60 (inclusive)");

			string[] lines = text.Split("\n");
			int width = GetWidth(lines);
			string residualString = "";
			string newLine = "";
			StringBuilder sb = new StringBuilder();
			foreach (string line in lines)
			{
				residualString = line;
				while (residualString != "")
				{
					sb.Append(newLine);
					if (residualString.Length > maxWidth)
					{
						int lastSpace = residualString.Substring(0, maxWidth + 1).LastIndexOf(' ');
						if (lastSpace > 0)
						{
							sb.Append(residualString.Substring(0, lastSpace));
							residualString = residualString.Substring(lastSpace + 1).Trim();
						}
						else
						{
							sb.Append(residualString.Substring(0, maxWidth));
							residualString = residualString.Substring(maxWidth).Trim();
						}
					}
					else
					{
						sb.Append(residualString);
						residualString = "";
					}
					newLine = "\n";
				}
			}
			return sb.ToString();
		}

		public static List<string> GetCustomListFromNamedList<T>(List<T> sourceList, bool numbered = true, bool lowered = true) where T : INameable
		{
			List<string> myList = new List<string>();
			int i = 0;
			string nextWord;
			foreach (INameable listItem in sourceList)
			{
				if (lowered)
					nextWord = listItem.Name.ToString().ToLower();
				else
					nextWord = listItem.Name.ToString().ToLower();

				if (numbered)
					myList.Add($"{++i}) {nextWord}");
				else
					myList.Add(nextWord);
			}
			return myList;
		}

		public static List<string> GetStringListFromEnum<E>(bool numbered = false, bool lowered = false) where E : Enum
		{
			List<string> myList = new List<string>();
			int i = 0;
			string nextWord;
			foreach (E word in Enum.GetValues(typeof(E)))
			{
				if (lowered)
					nextWord = word.ToString().ToLower();
				else
					nextWord = word.ToString().ToLower();

				if (numbered)
					myList.Add($"{++i}) {nextWord}");
				else
					myList.Add(nextWord);
			}
			return myList;
		}

		public static Dictionary<string, E> GetInvertedStringDictionaryFromEnum<E>(bool numbered = true, bool lowered = true) where E : Enum
		{
			Dictionary<string, E> myDictionary = new Dictionary<string, E>();
			string nextWord;
			int i = 0;
			foreach (E word in Enum.GetValues(typeof(E)))
			{
				if (lowered)
					nextWord = word.ToString().ToLower();
				else
					nextWord = word.ToString().ToLower();

				if (numbered)
					myDictionary.Add($"{++i}) {nextWord}", word);
				else
					myDictionary.Add(nextWord, word);
			}
			return myDictionary;
		}

		public static string Columnize(string[] text, int centerMargin = 1)
		{
			if (text == null)
				throw new ArgumentNullException("Error: TextUtils.Columnize null text array input");
			// if (text.Count == 0) THROW TO MAIN HANDLER
			// if (centerMarin... THROW TO MAIN HANDLER

			return Columnize(new List<string>(text), centerMargin);
		}

		public static string Columnize(List<string> text, int centerMargin = 1)
		{
			if (text == null)
				throw new ArgumentNullException("Error: TextUtils.Columnize null text list input");
			if (text.Count == 0)
				return "";
			else if (text.Count == 1)
				return text[0];

			int leftLength = (int)(Math.Ceiling((double)text.Count / 2));
			//int rightLength = (int)(Math.Floor((double)text.Count / 2));
			StringBuilder leftText = new StringBuilder();
			StringBuilder rightText = new StringBuilder();
			int i = 0;
			foreach (string line in text)
			{
				if (i < leftLength - 1)
					leftText.Append(line).Append("\n");
				else if (i < leftLength)
					leftText.Append(line);
				else if (i < text.Count - 1)
					rightText.Append(line).Append("\n");
				else
					rightText.Append(line);
				i++;
			}
			return Columnize(leftText.ToString(), rightText.ToString(), centerMargin);
		}

		public static string Columnize(string leftText, string rightText, int centerMargin = 1)
		{
			if (leftText == null)
				throw new ArgumentNullException("Error: TextUtils.Columnize null leftText input");
			// if (leftText.Length == 0) OK
			if (rightText == null)
				throw new ArgumentNullException("Error: TextUtils.Columnize null rightText input");
			// if (rightText.Length == 0) OK
			if (centerMargin < 0 && centerMargin > 100)
				throw new ArgumentOutOfRangeException("Error: TextUtils.Columnize centerMargin not between 0 and 100 (inclusive)");

			string[] leftLines = leftText.Split('\n');
			string[] rightLines = rightText.Split('\n');
			int maxRows = leftLines.Length > rightLines.Length ? leftLines.Length : rightLines.Length;
			int leftWidth = GetWidth(leftLines);
			string newLine = "";
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < maxRows; i++)
			{
				sb.Append(newLine);
				if (leftLines.Length > i)
					sb.Append(leftLines[i].PadRight(leftWidth));
				else
					sb.Append("".PadRight(leftWidth));
				sb.Append(Repeat(' ', centerMargin));
				if (rightLines.Length > i)
					sb.Append(rightLines[i]);
				newLine = "\n";
			}
			return sb.ToString();
		}

		public static string PrettyPrint(string[] items, bool writeAnd = false)
		{
			if (items == null)
				throw new ArgumentNullException("Error: TextUtils.PrettyPrint null items array");
			if (items.Length == 0)
				return "";

			return PrettyPrint(new List<string>(items), writeAnd);
		}
		public static string PrettyPrint(List<string> items, bool writeAnd = false)
		{
			if (items == null)
				throw new ArgumentNullException("Error: TextUtils.PrettyPrint null items list");
			if (items.Count == 0)
				return "";

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < items.Count; i++)
			{
				if (!writeAnd && i > 0 || writeAnd && i > 0 && i <= items.Count - 2) sb.Append(", ");
				if (writeAnd && i > 0 && i == items.Count - 1) sb.Append(" and ");
				sb.Append(items[i]);
			}
			return sb.ToString();
		}

		public static string SanitizeInput(string input)
		{
			if (input == null)
				throw new ArgumentNullException("Error: TextUtils.SanitizeInput null input string");
			if (input.Length == 0)
				return "";

			if (input == null)
				input = "";
			input = input.Trim();
			return input;
		}

		public static string GetArg1(string input)
		{
			if (input == null)
				throw new ArgumentNullException("Error: TextUtils.GetArg1 null input string");
			if (input.Length == 0)
				return "";

			return input.Split(' ')[0];
		}

		public static string GetArg2(string input)
		{
			if (input == null)
				throw new ArgumentNullException("Error: TextUtils.GetArg2 null input string");
			if (input.Length == 0)
				return "";

			int start = input.IndexOf(' ');
			if (start > -1)
				return input.Substring(start + 1);
			return "";
		}

		public static string CapitalizeFirstLetter(string input)
		{
			if (input == null)
				throw new ArgumentNullException("Error: TextUtils.CapitalizeFirstLetter null input string");
			if (input.Length == 0)
				return "";

			return input.Substring(0, 1).ToUpper() + input.Substring(1);
		}

		public static string FormatName(string input)
		{
			if (input == null)
				throw new ArgumentNullException("Error: TextUtils.CapitalizeFirstLetter null input string");
			if (input.Length == 0)
				return "";

			List<string> parts = new List<string>(input.Split(' '));
			StringBuilder sb = new StringBuilder();
			string space = "";
			for (int i = 0; i < parts.Count; i++)
			{
				parts[i] = parts[i].ToLower();
				if (i == 0 || !UncapitalizedWords.Contains(parts[i]))
					parts[i] = parts[i].Substring(0, 1).ToUpper() + parts[i].Substring(1);
				sb.Append(space).Append(parts[i]);
				space = " ";
			}
			return sb.ToString();
		}

		public static string CheckForPunctuation(string input)
		{
			if (input == null)
				throw new ArgumentNullException("Error: TextUtils.CheckForPunctuation null input string");
			if (input.Length == 0)
				return "";

			string lastChar = input.Substring(input.Length - 1);
			if (lastChar != "." && lastChar != "!" && lastChar != "?")
			{
				if (input.Length >= 3)
				{
					string firstThree = input.Substring(0, 3).ToLower();
					if (firstThree == "how" || firstThree == "who" || firstThree == "wha" || firstThree == "whe" || firstThree == "why")
						input += '?';
					else
						input += '.';
				}
				else
				{
					input += '.';
				}
			}
			return input;
		}

		public static string CurateDialogue(string input)
		{
			if (input == null)
				return "";
			input = input.Trim();
			if (input.Length == 0)
				return "";

			input = CapitalizeFirstLetter(input);
			input = CheckForPunctuation(input);
			return input;
		}

	}

}