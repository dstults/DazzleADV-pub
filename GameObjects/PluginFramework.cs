// References:
// https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
// https://www.c-sharpcorner.com/article/introduction-to-building-a-plug-in-architecture-using-C-Sharp/
// https://www.youtube.com/watch?v=lOcJ2z-tgu0&t=557s
// https://www.youtube.com/watch?v=r5dtl9Uq9V0&t=1489s

using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace DazzleADV
{

	public interface IPluggable
	{

		string Name { get; }
		string Description { get; }
		List<string> Dependencies { get; }

	}

	public interface IMappable : IPluggable
	{
		void ExpandMap();
	}


	public interface ISpawnable : IPluggable
	{
		void SpawnAtStart();
	}

	public interface IPlayerModifiable : IPluggable
	{
		Player ModifyNewPlayer(Player player);
	}

	public interface ICanOverrideAttackMethod : IPluggable
	{
		void AttackOverride(Player p1, Player p2);
	}

	public interface ISpeakable : IPluggable
	{
		bool HandleDialogue(Player p1, Player p2, string speech, string speechLower);
	}

	public interface IFabricable : IPluggable
	{
		Item NewItem(Enum myEnum);
		Player NewUnit(Enum myEnum);
	}

	internal static class Plugins
	{

		public static List<IPluggable> AllPlugins = new List<IPluggable>();
		public static ICanOverrideAttackMethod AttackMod = null;
		public static List<IMappable> Mappers = new List<IMappable>();
		public static List<ISpawnable> Spawners = new List<ISpawnable>();
		public static List<IPlayerModifiable> PlayerMods = new List<IPlayerModifiable>();
		public static List<ISpeakable> SpeechMods = new List<ISpeakable>();
		public static List<IFabricable> TemplateMods = new List<IFabricable>();

		public static void LoadPlugins()
		{
			GameEngine.SayToServer(" - Scanning for plugins...");
			foreach (string dll in Directory.GetFiles(".", "*.dll"))
			{
				// Prevent snagging on self when running as deployed executable
				if (dll.Contains(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name)) continue;
				
				Assembly asm = Assembly.LoadFrom(dll);
				foreach (Type type in asm.GetTypes())
				{
					if (type.GetInterface("IPluggable") == typeof(IPluggable))
					{
						IPluggable thisPlugin = Activator.CreateInstance(type) as IPluggable;
						GameEngine.SayToServer($"{thisPlugin.Name}...");
						AllPlugins.Add(thisPlugin);
						if (type.GetInterface("IMappable") == typeof(IMappable))
						{
							IMappable mapPlugin = Activator.CreateInstance(type) as IMappable;
							Mappers.Add(mapPlugin);
						}
						if (type.GetInterface("ISpawnable") == typeof(ISpawnable))
						{
							ISpawnable spawnPlugin = Activator.CreateInstance(type) as ISpawnable;
							Spawners.Add(spawnPlugin);
						}
						if (type.GetInterface("IPlayerModifiable") == typeof(IPlayerModifiable))
						{
							IPlayerModifiable playerPlugin = Activator.CreateInstance(type) as IPlayerModifiable;
							PlayerMods.Add(playerPlugin);
						}
						if (type.GetInterface("ICanOverrideAttackMethod") == typeof(ICanOverrideAttackMethod))
						{
							ICanOverrideAttackMethod combatPlugin = Activator.CreateInstance(type) as ICanOverrideAttackMethod;
							AttackMod = combatPlugin;
						}
						if (type.GetInterface("ISpeakable") == typeof(ISpeakable))
						{
							ISpeakable speechMod = Activator.CreateInstance(type) as ISpeakable;
							SpeechMods.Add(speechMod);
						}
						if (type.GetInterface("IFabricable") == typeof(IFabricable))
						{
							IFabricable templateMod = Activator.CreateInstance(type) as IFabricable;
							TemplateMods.Add(templateMod);
						}
					}
				}
			}
			GameEngine.SayToServer("done.\n");
		}

		public static void RunStartupPlugins()
		{
			if (AllPlugins.Count > 0)
			{
				if (Mappers.Count > 0)
				{
					GameEngine.SayToServer(" - Map-building plugins:\n");
					foreach (IMappable mapper in Mappers)
					{
						mapper.ExpandMap();
					}
				}
				if (Spawners.Count > 0)
				{
					GameEngine.SayToServer(" - Spawner plugins:\n");
					foreach (ISpawnable spawner in Spawners)
					{
						spawner.SpawnAtStart();
					}
				}
			}
			else
			{
				GameEngine.SayToServer(" - No plugins detected.\n");
			}
		}

	}

}