using System;
using static System.Console;
using System.Threading;
using System.Threading.Tasks;
using DStults.Utils;
using DStults.SocketClient;
using DStults.SocketServer;

namespace DazzleADV
{
	internal class Program
	{

		private static Task GameTask;
		private static bool done;

		private static void Main(string[] args)
		{
			Thread.CurrentThread.Name = "Server TUI Thread";
			if (args.Length > 0)
				GameTask = Task.Factory.StartNew(() => LaunchGameWithThreadName());

			ShowMenu();
			while (!done)
			{
				if (KeyAvailable)
				{
					HandleKeyInput(ReadKey(true).Key);
				}
			}
			WriteLine("--------------------------------------------------------------------");
			WriteLine();
			WriteLine("Program gracefully terminated.");
		}

		private static void LaunchGameWithThreadName(bool enableListener = true)
		{
			GameEngine.InitializeGame(enableListener);
		}

		private static void ShowMenu()
		{
			Thread.Sleep(1000);
			if (!GameEngine.Running)
			{
				WriteLine("--------------------------------------------------------------------");
				WriteLine();
				WriteLine("              Menu:");
				WriteLine();
				WriteLine("[S] Start Server              [C] Join Other Server as Client");
				WriteLine("[L] Local Play (w/ Network)   [D] Join Public Server: dstults.net");
				WriteLine("[K] Local Play (No Network)");
				WriteLine();
				WriteLine("           [X][Q] Quit");
				WriteLine();
				WriteLine("--------------------------------------------------------------------");
			}
			else
			{
				WriteLine("--------------------------------------------------------------------");
				WriteLine();
				WriteLine("              Menu:");
				WriteLine();
				WriteLine("      [G] Game Information");
				WriteLine();
				WriteLine("[S] Spawn Creature            [I] Spawn Item");
				WriteLine("[V] View Player Screen        [O] Possess Creature");
				WriteLine();
				WriteLine("[P] Poll Clients              [D] Detailed Connection Data");
				WriteLine();
				if (!GameEngine.Paused)
				{
					WriteLine("      [Z] Game Unpaused, Pause Game");
				}
				else
				{
					WriteLine("      [Z] Game PAUSED, Unpause Game");
				}
				WriteLine();
				WriteLine("           [X][Q] Quit");
				WriteLine();
				WriteLine("--------------------------------------------------------------------");
			}
		}

		private static void HandleKeyInput(ConsoleKey key)
		{
			bool subdueMenuRepeat = false;

			if (!GameEngine.Running)
			{
				switch (key)
				{
					case ConsoleKey.X:
					case ConsoleKey.Q:
						WriteLine("Exiting...");
						done = true;
						break;
					case ConsoleKey.S:
						GameTask = Task.Factory.StartNew(() => LaunchGameWithThreadName());
						while (!GameEngine.Running) Thread.Sleep(100);
						break;
					case ConsoleKey.L:
						GameTask = Task.Factory.StartNew(() => LaunchGameWithThreadName());
						while (!GameEngine.Running) Thread.Sleep(100);
						GameEngine.PlayAsServer();
						break;
					case ConsoleKey.K:
						GameTask = Task.Factory.StartNew(() => LaunchGameWithThreadName(false));
						while (!GameEngine.Running) Thread.Sleep(100);
						GameEngine.PlayAsServer();
						break;
					case ConsoleKey.C:
						StandaloneClient.RunClient();
						break;
					case ConsoleKey.D:
						StandaloneClient.RunClient("dstults.net", 11111);
						break;
					default:
						subdueMenuRepeat = true;
						break;
				}
			}
			else
			{
				switch (key)
				{
					case ConsoleKey.X:
					case ConsoleKey.Q:
						WriteLine("Exiting...");
						GameEngine.Shutdown();
						GameTask.Wait();
						done = true;
						break;
					case ConsoleKey.S:
						WriteLine(WorldMap.GetLocationList());
						Write($" Where? > ");						
						Location unitLoc = WorldMap.GetLocation(Console.ReadLine());
						if (unitLoc == null)
						{
							WriteLine("Invalid location name");
 							break;
						}
						WriteLine(Prefabs.GetPrefabUnitList());
						Write($" What unit? > ");
						Player myUnit = Prefabs.SpawnUnitAtLocation(Console.ReadLine(), unitLoc);
						if (myUnit != null)
						{
							WriteLine($"Playable unit [{myUnit.Name}] spawned at [{unitLoc.Name}].");
						}
						else
						{
							WriteLine($"Invalid unit type or incomplete unit creation.");
						}
						break;
					case ConsoleKey.I:
						WriteLine(WorldMap.GetLocationList());
						Write($" Where? > ");						
						Location itemLoc = WorldMap.GetLocation(Console.ReadLine());
						if (itemLoc == null)
						{
							WriteLine("Invalid location name");
 							break;
						}
						WriteLine(Prefabs.GetPrefabItemList());
						Write($" What item? > ");
						Item myItem = Prefabs.SpawnItemAtLocation(Console.ReadLine(), itemLoc);
						if (myItem != null)
						{
							WriteLine($"Item [{myItem.Title}] spawned at [{itemLoc.Name}].");
							GameEngine.SayToLocation(itemLoc, $"All of the sudden {myItem.SetName()} appears!");
						}
						else
						{
							WriteLine($"Invalid item name.");
						}
						break;
					case ConsoleKey.P:
						WriteLine("Polling current clients:");
						WriteLine(NetworkServer.GetConnectionPolling(verbose: true, detailed: false));
						break;
					case ConsoleKey.D:
						WriteLine("Polling current clients (detailed):");
						WriteLine(NetworkServer.GetConnectionPolling(verbose: true, detailed: true));
						break;
					case ConsoleKey.G:
						WriteLine(GameEngine.GameInfo());
						break;
					case ConsoleKey.V:
						WriteLine(TextUtils.Borderize(TextUtils.Columnize(TextUtils.GetCustomListFromNamedList(GameEngine.Players, numbered: true, lowered: true))));
						Write($" ? (1-{GameEngine.Players.Count}) > ");
						Player viewPlayer = GameEngine.GetPlayer(Console.ReadLine());
						if (viewPlayer != null)
						{
							WriteLine("\n================================================================");
							WriteLine(viewPlayer.GetFullInGameView());
						}
						else
						{
							WriteLine("No such player.");
						}
						break;
					case ConsoleKey.O:
						WriteLine(TextUtils.Borderize(TextUtils.Columnize(TextUtils.GetCustomListFromNamedList(GameEngine.Players, numbered: true, lowered: true))));
						Write($" ? (1-{GameEngine.Players.Count}) > ");
						Player possessPlayer = GameEngine.GetPlayer(Console.ReadLine());
						if (possessPlayer != null)
						{
							GameEngine.PlayAsServer(possessPlayer);
						}
						else
						{
							WriteLine("No such player.");
						}
						break;
					case ConsoleKey.Z:
						GameEngine.TogglePause();
						break;
					default:
						subdueMenuRepeat = true;
						break;
				}
			}

			if (!subdueMenuRepeat && !done)
				ShowMenu();
		}

	}
}
