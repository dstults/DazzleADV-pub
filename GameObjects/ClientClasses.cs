using System;
using System.Text;
using System.Net.Sockets;
using DStults.Utils;
using DStults.SocketServer;

namespace DazzleADV
{
	public enum MenuState { Normal, Error, Exiting, InGame }

	public abstract class GameClient
	{

		public Player Avatar { get; private set; }
		public string Nickname { get; protected set; }
		public bool HasUpdates => Avatar == null ? false : lastUpdateTime < Avatar.LastNotifyTime;

		private DateTime lastUpdateTime;

		public GameClient()
		{
			playerName = "";
			playerGender = Gender.Unset;
			MenuState = MenuState.Normal;
		}

		public void AssignAvatar(Player playerUnit)
		{
			if (playerUnit == null)
				throw new ArgumentNullException("Error: AssignAvatar player assignment null");

			this.Avatar = playerUnit;
			//GameEngine.SayToAll($"{Nickname()} has joined the game, controlling {Avatar.Name}!");
			System.Console.WriteLine($"SERVER < [{this}] assigned control over [{playerUnit}]");
		}

		public void RemoveAvatar()
		{
			System.Console.WriteLine($"SERVER < [{this}] yielding control over [{this.Avatar}]");
			this.Avatar = null;
		}

		public string GetImmediateReplyForClient()
		{
			if (MenuState == MenuState.InGame && Avatar != null)
			{
				return $"Player action set to: {Avatar.ChosenAction}\n  Action? >> ";
			}
			else if (MenuState == MenuState.Exiting)
			{
				MenuState = MenuState.InGame;
				return GetFullUpdateForClient();
			}
			else
			{
				return GetMenu();
			}
		}

		public string GetFullUpdateForClient()
		{
			lastUpdateTime = DateTime.Now;
			if (Avatar != null)
			{
				return $"{Avatar.GetFullInGameView()}\n  Action? >> ";
			}
			else
			{
				return GetMenu();
			}
		}

		public abstract void SendUpdate(string text);

		public void HandleInputFromClientToServer(string userInput)
		{
			if (userInput == null)
				throw new ArgumentNullException("Error: GameClient.HandleInput null userInput.");
			// if (userInput.Length == 0) OK

			if (Avatar != null)
			{
				Avatar.TryAssignAction(userInput, this);
			}
			else
			{
				SetMenu(userInput);
			}
		}

		public abstract void KickClient(string reason);

		// --------------------------------- MENUS ---------------------------------
		public MenuState MenuState { get; set; }
		private string playerName;
		private Gender playerGender;

		private string GetMenu()
		{
			string result = "";
			if (playerGender == Gender.Unset)
			{
				result = "CHOOSE A GENDER: 'M' / 'F'";
				if (MenuState == MenuState.Error)
					result += "\nPLEASE TYPE 'M' FOR MALE AND 'F' FOR FEMALE.";
				result = $"\n{TextUtils.Borderize(result)}\n >> ";
				return result;
			}
			else if (playerName == "")
			{
				result = "CHOOSE A PLAYER NAME";
				if (MenuState == MenuState.Error)
					result += $"\nPLAYER NAME MUST BE {GameEngine.MinimumNameLength} - {GameEngine.MaximumNameLength} CHARACTERS LONG.";
				result = $"\n{TextUtils.Borderize(result)}\n >> ";
				return result;
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				result = $"Welcome, {playerName} to the world of Fuchar Gonzol (name subject to copyright)! Your goal is to get inside the town. Explore and try not to die! Good luck!";
				sb.Append(Environment.NewLine).Append(TextUtils.Borderize(result, 80));
				sb.Append($"\n\n    Some commands have arguments:");
				sb.Append($"\nTo chat with people nearby, input 'c how are you' and the game will output 'How are you?'!");
				sb.Append("\nTo pick up an item, type 'get [item name substring]' or 'g [item name substring]'\n    Examples:  'get stick' 'get sti' 'g s'");
				sb.Append("\nTo drop an item, you can use To drop an item it's 'drop [item name substring]' or 'd [item name substring]'\n    Examples:  'drop stick' 'drop ck' 'd sti'");
				sb.Append("\nThe game will check player inventory in this order: item bag first, then weapon, then armor.");
				sb.Append("\n\n    REAL-TIME TURN-BASED MULTIPLAYER ON CONSOLE:");
				sb.Append("\nText will be streamed to your console even as you are typing. Despite this, anything");
				sb.Append("\nyou type will still be remembered by the console and you can backspace through it even");
				sb.Append("\nif it is no longer 'there'. Likewise, despite it not being visible, when you submit your");
				sb.Append("\naction, even if you don't see it, it will be submitted along with whatever text you typed");
				sb.Append("\nfollowing the most recent screen refresh(es).\n\n");
				result = $"The resulting experience should still be amazing and I really hope you enjoy your visit!";
				result = TextUtils.Borderize(result, 80);
				sb.Append(result);
				if (Plugins.AllPlugins.Count > 0)
				{
					sb.Append("\n\nLoaded plugins:\n");
					foreach (IPluggable plugin in Plugins.AllPlugins)
					{
						sb.Append(TextUtils.Borderize($"{plugin.Name}: {plugin.Description}", 80)).Append(Environment.NewLine);
					}
				}
				sb.Append("\n\n  >> OK");
				return sb.ToString();
			}
		}

		private void SetMenu(string userInput)
		{
			if (playerGender == Gender.Unset)
			{
				userInput = userInput.Trim();
				if (userInput.Length > 0 && userInput.Substring(0, 1).ToLower() == "m")
				{
					playerGender = Gender.Male;
					MenuState = MenuState.Normal;
				}
				else if (userInput.Length > 0 && userInput.Substring(0, 1).ToLower() == "f")
				{
					playerGender = Gender.Female;
					MenuState = MenuState.Normal;
				}
				else
				{
					MenuState = MenuState.Error;
				}
			}
			else if (playerName == "")
			{
				userInput = userInput.Trim();
				if (userInput.Length >= GameEngine.MinimumNameLength && userInput.Length <= GameEngine.MaximumNameLength)
				{
					playerName = TextUtils.FormatName(userInput);
					MenuState = MenuState.Normal;
				}
				else
				{
					MenuState = MenuState.Error;
				}
			}
			else
			{
				Prefabs.NewPlayer(playerName, playerGender, client: this);
				MenuState = MenuState.Exiting;
			}
		}

		public override string ToString()
		{
			return this.Nickname;
		}

	}

	internal class LocalClient : GameClient
	{

		public LocalClient() : base()
		{
			Nickname = "Local Client";
		}

		public override void KickClient(string reason)
		{
			string result = "";
			result = $"\n\nMESSAGE FROM SERVER:\n\nYou have been disconnected for the following reason(s): {reason}\n\n";
			result += "Thank you for your support! Please come back and try again!\n\n";
			Console.WriteLine(result);
			GameEngine.DisconnectLocalPlayer();
		}

		public override void SendUpdate(string text)
		{
			Console.Write(text);
		}

	}

	internal class NetworkClient : GameClient, IClientConnectable
	{

		public Socket Socket { get; private set; }
		public byte[] Buffer { get; private set; }
		public string ExtraInfo => Avatar == null ? " - (New Player Menu)" : $" - Player: {Avatar.Name}";

		public NetworkClient(Socket socket, int serialNo) : base()
		{
			this.Socket = socket;
			this.Nickname = $"Client #{serialNo}";
			Buffer = new byte[NetworkServer.BufferSize];
		}

		public override void KickClient(string reason)
		{
			string result = "";
			result = $"\n\nMESSAGE FROM SERVER:\n\nYou have been disconnected for the following reason(s): {reason}\n\n";
			result += "Thank you for your support! Please come back and try again!\n\n";
			Console.WriteLine(result);
			NetworkServer.DisconnectClient(this, result, reason);
		}

		public override void SendUpdate(string text)
		{
			NetworkServer.TryTransmitToClient(this, text);
		}

	}
}