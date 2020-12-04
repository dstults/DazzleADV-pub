using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using DStults.Utils;
using DStults.SocketServer;

namespace DazzleADV
{
	public static class GameEngine
	{

		public static string Name { get; private set; } = "SERVER";

		public static int WorldTick = 0;
		public static ulong WorldTime { get; private set; } = 0;
		public const int WorldTickInterval = 10000; // 10 seconds per worldtick
		public const int NotificationLifeSpan = 20000; // 20 seconds per notification
		public const int MaxNotifications = 12;
		public const int MSTimePerPCAction = 4 * 1000; // 4 seconds per normal player action
		public const int MSTimePerNPCAction = (int)1.2 * MSTimePerPCAction; // 20% slower (4.8 seconds) for npc action
		public const int MinimumNameLength = 3;
		public const int MaximumNameLength = 20;

		public static List<Player> Players { get; private set; } = new List<Player>();

		public static bool Running { get; private set; }
		public static CancellationToken CancelToken { get; private set; }
		public static bool Paused { get; private set; }
		private static LocalClient serverClient;
		public static bool ServerIsPlayer => serverClient != null;

		public static void InitializeGame(bool enableListeners)
		{
			Thread.CurrentThread.Name = "Main Game Worker";

			SayToServer("\nGame initializing:\n");

			SayToServer("\nStarting PHASE 1 \"Core Startup\" - Basic worldgen and prefab testing:\n");
			WorldMap.Generate();
			Prefabs.TestGenerationDatabase();
			SayToServer("PHASE 1 completed.\n");

			SayToServer("\nStarting PHASE 2 \"Plugin Startup\" - Run Mapper and Spawner Plugin Startup Procedures:\n");
			Plugins.LoadPlugins();
			Plugins.RunStartupPlugins();
			SayToServer("PHASE 2 completed.\n");

			SayToServer("\nStarting PHASE 3 \"Finalization\" - Syncing plugins w/ core (rehashing dictionaries), cleaning garbage, starting listener (if enabled):\n");
			WorldMap.PopulateLookupDictionary();
			GarbageCollect();
			if (enableListeners) NetworkServer.CreateAsyncListener<NetworkClient>();
			SayToServer("PHASE 3 completed.\n");

			SayToServer("\nGame has been initialized!\n\n");
			Running = true;

			RunGame(CancelToken);

			SayToServer("\nGame engine shutdown detected!\n\n");
			if (enableListeners)
				NetworkServer.CloseAsyncListener();
		}

		public static void GarbageCollect()
		{
			SayToServer(" - Garbage collecting...");
			System.GC.Collect();
			SayToServer("done.\n");
		}

		private static async void RunGame(CancellationToken stoppingToken)
		{
			// MAIN LOOP
			const int pollInterval = 1000; // ms
			int timeToNextPoll = pollInterval;
			int timeToNextWorldTick = WorldTickInterval; // 1 worldTick = 10 seconds
			const int sleepTime = 100;
			const ulong sleepTimeLong = (ulong)sleepTime;
			while (!stoppingToken.IsCancellationRequested && Running)
			{
				// Timing control
				await Task.Delay(sleepTime, stoppingToken);
				if (!Paused)
				{
					WorldTime += sleepTimeLong;
					timeToNextPoll -= sleepTime;
					timeToNextWorldTick -= sleepTime;
				}

				// Check for and remove undetectable disconnected clients
				if (timeToNextPoll <= 0)
				{
					NetworkServer.GetConnectionPolling();
					timeToNextPoll += pollInterval;
				}

				// World Tick
				if (timeToNextWorldTick <= 0)
				{
					GameEngine.RunWorldTick();
					timeToNextWorldTick += WorldTickInterval;
				}

				// Player Turns
				GetActors().ForEach(p => p.DoTurn());

				// Update Changes to Clients
				Players.FindAll(p => p.HasClient && p.Client.HasUpdates).ForEach(p => p.Client.SendUpdate(p.Client.GetFullUpdateForClient()));
			}
		}

		public static void TogglePause()
		{
			Paused = !Paused;
			if (Paused)
				SayToAll($"[{DateTime.Now}] GAME PAUSED!");
			else
				SayToAll($"[{DateTime.Now}] GAME UNPAUSED!");
		}

		public static void RunWorldTick()
		{
			WorldTick++;

			lock (Players)
			{
				foreach (Location location in WorldMap.Locations)
				{
					location.DoWorldTickActions();
				}
				for (int i = Players.Count - 1; i >= 0; i--) // may delete players, no foreach
				{
					Players[i].DoWorldTickPassiveUpdate();
				}
			}

		}

		public static List<Player> GetActors()
		{
			List<Player> actorList = Players.FindAll(p => p.IsReadyToAct());
			if (actorList.Count == 0) return actorList;

			if (actorList.Count > 1)
			{
				// Randomize simultaneously acting actors
				Random rng = new Random();
				for (int i = 0; i < actorList.Count; i++)
				{
					int j = rng.Next(i, actorList.Count);
					Player p = actorList[i];
					actorList[i] = actorList[j];
					actorList[j] = p;
				}
			}

			return actorList;
		}

		public static void Shutdown()
		{
			SayToAll("Server shutting down! Goodbye!");
			// save data goes here
			Running = false;
		}

		public static void AddPlayer(Player player)
		{
			if (player == null)
				throw new NullReferenceException("GameServer/AddPlayer: player null error");

			Players.Add(player);
		}

		public static Player GetPlayer(string input)
		{
			if (input == null)
				throw new ArgumentNullException("Error: GameEngine.GetPlayer null input string");
			if (input.Length == 0)
				return null;

			input = input.Trim().ToLower();
			int? getIndex = TextUtils.ParseInt(input, 1, Players.Count);
			if (getIndex != null)
			{
				int resultingIndex = (int)getIndex - 1;
				return GameEngine.Players[resultingIndex];
			}
			foreach (Player player in Players)
			{
				if (player.Name.ToLower().Contains(input))
					return player;
			}
			return null;
		}

		public static void SayToServer(string text)
		{
			if (!ServerIsPlayer)
				Console.Write(text);
		}

		public static void SayToAll(string text)
		{
			if (text == null)
				throw new ArgumentNullException("Cannot SayToAll with a null string");
			if (text.Length == 0)
				throw new ArgumentException("Cannot SayToAll with empty string");

			SayToServer($"SYSTEM: {text}\n");
			foreach (Player iPlayer in Players)
			{
				iPlayer.Notify($"SYSTEM: {text}");
			}
		}

		public static void SayToLocation(Location location, string text)
		{
			if (location == null)
				throw new ArgumentNullException("Cannot SayToLocation to a null location");
			if (text == null)
				throw new ArgumentNullException("Cannot SayToLocation with a null string");
			if (text.Length == 0)
				throw new ArgumentException("Cannot SayToLocation with empty string");

			foreach (Player iPlayer in Players.FindAll(p => p.Location == location))
			{
				iPlayer.Notify(text);
			}
		}

		public static void SayToAdjacent(Location location, string text)
		{
			if (location == null)
				throw new ArgumentNullException("Cannot SayToAdjacent to a null location");
			if (text == null)
				throw new ArgumentNullException("Cannot SayToAdjacent with a null string");
			if (text.Length == 0)
				throw new ArgumentException("Cannot SayToAdjacent with empty string");

			foreach (LocationConnector nearby in location.Connections.Values)
			{
				GameEngine.SayToLocation(nearby.Destination, text);
			}
		}

		public static string GameInfo()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append($"Game Turn: {WorldTick}...");
			string pString = "";
			if (Players.Count > 0)
			{
				List<string> playerNames = new List<string>();
				for (int i = 0; i < Players.Count; i++)
				{
					playerNames.Add(Players[i].Name);
					if (Players[i].HasClient)
						playerNames[i] += $" ({Players[i].Client.Nickname}-Act:[{Players[i].ChosenAction}])";
				}
				pString = TextUtils.PrettyPrint(playerNames);
			}
			else
			{
				pString = "(none)";
			}
			sb.Append($"Players: {pString}");
			return sb.ToString();
		}

		public static void PlayAsServer(Player player = null)
		{
			// if (player == null) OK
			if (!Running)
				while (!Running) Thread.Sleep(100);

			Thread.Sleep(100);
			Console.WriteLine("\n\nSERVER < You are joining the game as a local client.");
			Console.WriteLine("\n\nSERVER < Type '/exit' to return here.");
			serverClient = new LocalClient();
			if (player != null)
			{
				serverClient.AssignAvatar(player);
				serverClient.MenuState = MenuState.Exiting;
				if (!player.HasClient)
					player.AssignClient(serverClient);
			}
			//Prefabs.NewPlayer("Server", Gender.Genderless, serverClient);
			while (ServerIsPlayer)
			{
				Console.Write(serverClient.GetImmediateReplyForClient());
				string input = Console.ReadLine();
				if (ServerIsPlayer && !HandledMetaInput(input))
					serverClient.HandleInputFromClientToServer(input);
			}
			System.Console.WriteLine("SERVER < You are no longer a local client.");
		}

		private static bool HandledMetaInput(string input)
		{
			if (input.Length == 0)
				return false;

			input = input.Trim().ToLower();
			if (input.Substring(0, 1) == "/")
			{
				if (input.Contains("/exit"))
				{
					if (serverClient.Avatar != null)
					{
						serverClient.Avatar.RemoveClient(serverClient);
						serverClient.RemoveAvatar();
					}
					serverClient = null;
					return true;
				}
			}

			return false;

		}

		public static void DisconnectLocalPlayer()
		{
			serverClient = null;
		}

	}
}