using System;
using System.Text;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{

	public enum Gender { Unset, Male, Female, Genderless }

	public partial class Player : INameable
	{
		static protected Random rng = new Random();

		// --------------------------------------------------------------------------------------------------------------------
		//  Stats
		// --------------------------------------------------------------------------------------------------------------------

		public Enum UnitType { get; private set; }
		public string Name { get; private set; }
		public Gender Gender { get; private set; }
		public Faction Faction { get; private set; }
		public Race Race { get; private set; }
		public Location Location { get; private set; }
		public List<Enum> Flags { get; private set; }
		public bool IsHumanoid => Race.IsHumanoid;

		public int HP { get; private set; }
		public int MaxHP { get; private set; }
		public bool IsAlive => HP > 0;
		protected List<StatusEffect> PersonalEffects;
		public StatusEffect DecayStatus => PersonalEffects.Find(se => se.EffectClass.Equals(EffectClass.Decay));
		public bool CanAct => IsAlive && GetStatusEffects().Find(se => se.DisablesActions) == null;

		protected Weapon weapon = null;
		public bool IsArmed => weapon != null;
		public Weapon Weapon => IsArmed ? weapon : Prefabs.Unarmed;
		protected Armor armor = null;
		public bool IsArmored => armor != null;
		public Armor Armor => IsArmored ? armor : Prefabs.Unarmored;

		// --------------------------------------------------------------------------------------------------------------------
		//  Volatiles and Metadata
		// --------------------------------------------------------------------------------------------------------------------

		public GameClient Client { get; protected set; }
		public bool HasClient => Client != null;
		public ActionOption<Player, MenuOptionType> ChosenAction { get; protected set; }
		public ActionOption<Player, MenuOptionType> DefaultAction => CanAct ? PlayerActions.Idle : null;
		protected List<ActionOption<Player, MenuOptionType>> actions;
		public ulong TimeToAct { get; private set; }
		protected string actionArg2;
		public DateTime LastNotifyTime { get; private set; }
		protected List<TimeText> notifications;
		public Dictionary<Player, int> HatedUnits { get; private set; }
		protected string causeOfDeath;

		// --------------------------------------------------------------------------------------------------------------------
		//  Constructors
		// --------------------------------------------------------------------------------------------------------------------

		public Player(Enum template, string name, int maxHP, Gender gender, Faction faction, Race race, Location location,
			Enum weaponTemplate = null, Enum armorTemplate = null)
		{
			if (name == null)
				throw new ArgumentNullException("Error: Player constructor null name error");
			if (name.Length == 0)
				throw new ArgumentException("Error: Player constructor empty name error");
			if (maxHP <= 0)
				throw new ArgumentOutOfRangeException("Error: Player constructor maxHP must be > 0");
			if (weaponTemplate == null)
				weaponTemplate = ItemType.Unarmed;
			if (armorTemplate == null)
				armorTemplate = ItemType.Unarmored;

			this.UnitType = template;
			this.Name = name;
			this.MaxHP = maxHP;
			this.HP = MaxHP;
			this.Gender = gender;
			this.Faction = faction;
			this.Race = race;
			this.Location = location;

			TimeToAct = GameEngine.WorldTime + (ulong)rng.Next(GameEngine.MSTimePerPCAction);

			if (!weaponTemplate.Equals(ItemType.Unarmed))
				this.weapon = (Weapon)Prefabs.NewItem(weaponTemplate);
			if (!armorTemplate.Equals(ItemType.Unarmored))
				this.armor = (Armor)Prefabs.NewItem(armorTemplate);

			this.Flags = new List<Enum>();
			PersonalEffects = new List<StatusEffect>();
			actions = new List<ActionOption<Player, MenuOptionType>>();
			notifications = new List<TimeText>();
			HatedUnits = new Dictionary<Player, int>();
			causeOfDeath = "";
		}

		// --------------------------------------------------------------------------------------------------------------------
		//  Unit Life and Death
		// --------------------------------------------------------------------------------------------------------------------

		public void Refresh()
		{
			HP = MaxHP;
		}

		public void ChangeMaxHP(int newMax)
		{
			if (newMax <= 0)
				throw new ArgumentOutOfRangeException("Error: Player.ChangeMaxHP must be > 0");

			MaxHP = newMax;
		}

		public void HealHP(int healValue)
		{
			if (healValue <= 0)
			{
				GameEngine.SayToLocation(Location, $"{Name} HP: Heal ineffective!");
				return;
			}
			if (HP == MaxHP)
				return;
			HP += healValue;
			if (HP > MaxHP) HP = MaxHP;
			GameEngine.SayToLocation(Location, $"{Name} HP: +{healValue} => {HP}");
		}

		public void DamageHP(int damageValue, string causeOfDamage, Player sourcePlayer)
		{
			if (causeOfDamage == null)
				throw new ArgumentNullException("Error: Player.DamageHP causeOfDamage null string");
			if (causeOfDamage == null)
				throw new ArgumentException("Error: Player.DamageHP causeOfDamage null string");
			// sourcePlayer == null OK

			if (damageValue <= 0)
			{
				if (sourcePlayer != null)
					AddHate(sourcePlayer, 3);
				GameEngine.SayToLocation(Location, $"{Name} HP: NO DAMAGE! ({causeOfDamage})");
				return;
			}

			if (sourcePlayer != null)
				AddHate(sourcePlayer, 8);

			HP -= damageValue;
			if (HP < 0 && sourcePlayer != null) HP = 0;
			if (HP < 1 && sourcePlayer == null) HP = 1;
			GameEngine.SayToLocation(Location, $"{Name} HP: -{damageValue} => {HP} ({causeOfDamage})");
			if (HP == 0)
			{
				if (IsPlayerReallyDefeated())
				{
					this.causeOfDeath = causeOfDamage;
					GameEngine.SayToLocation(Location, $"{Name} has fallen to {causeOfDeath}!");
					PersonalEffects.Add(new StatusEffect(EffectClass.Decay, turnTickEvent: StatusEvents.CorpseRot, effectValue: 12));
					Weapon killingWeapon = sourcePlayer.Weapon;
				}
			}
		}

		public List<StatusEffect> GetStatusEffects()
		{
			List<StatusEffect> myEffects = new List<StatusEffect>();
			foreach (StatusEffect se in PersonalEffects)
			{
				myEffects.Add(se);
			}
			foreach (StatusEffect se in Weapon.GetStatusEffects())
			{
				myEffects.Add(se);
			}
			foreach (StatusEffect se in Armor.GetStatusEffects())
			{
				myEffects.Add(se);
			}
			return myEffects;
		}

		public void RemoveExpiredStatusEffects()
		{
			PersonalEffects.RemoveAll(st => st.IsExpired(this, st));
			Weapon.RemoveExpiredStatusEffects(this);
			Armor.RemoveExpiredStatusEffects(this);
		}

		public void AddEffect(StatusEffect statusEffect)
		{
			StatusEffect matchingEffect = PersonalEffects.Find(se => se.Matches(statusEffect));
			if (matchingEffect == null)
			{
				PersonalEffects.Add(statusEffect);
			}
			else
			{
				matchingEffect.Combine(statusEffect);
			}
		}

		public bool HasState(Enum effectClass)
		{
			return GetStatusEffects().Find(se => se.EffectClass.Equals(effectClass)) != null;
		}

		// --------------------------------------------------------------------------------------------------------------------
		//  Locality and Questing
		// --------------------------------------------------------------------------------------------------------------------

		public void Relocate(Location location) => Location = location;

		public bool HasFlag(Enum flag) => Flags.Contains(flag);

		public void AddFlag(Enum flag) => Flags.Add(flag);		 // addquestflag setflag setquestflag

		public void RemoveFlag(Enum flag) => Flags.Remove(flag); // removequestflag dropflag dropquestflag

		// --------------------------------------------------------------------------------------------------------------------
		//  Events & Reactions (automatic)
		// --------------------------------------------------------------------------------------------------------------------

		public void DoWorldTickPassiveUpdate()
		{
			if (IsAlive)
			{
				// Hate diminish over time
				List<Player> tempList = new List<Player>();
				foreach (KeyValuePair<Player, int> hatedPlayer in HatedUnits)
				{
					tempList.Add(hatedPlayer.Key);
				}
				foreach (Player p in tempList)
				{
					HatedUnits[p]--;
					if (HatedUnits[p] <= 0)
						HatedUnits.Remove(p);
				}

				// Handle status effects
				GetStatusEffects().ForEach(st => st.OnTurnTick(this, st));
				RemoveExpiredStatusEffects();
			}
			else
			{
				DecayStatus.OnTurnTick(this, DecayStatus);
				if (DecayStatus.IsExpired(this, DecayStatus))
				{
					this.PersonalEffects.Remove(DecayStatus);
					GameEngine.Players.Remove(this);
					if (HasClient)
						Client.KickClient("You died!");
				}
			}
		}

		protected bool IsPlayerReallyDefeated()
		{
			Item resurrectItem = GetItem(ItemType.HolyAnkh);
			if (resurrectItem != null)
			{
				RemoveItem(resurrectItem);
				GameEngine.SayToLocation(Location, $"{this.Name}'s Holy Ankh shatters as life energy flows back into {Genderize("him", "her", "it")}!");
				this.Refresh();
				return false;
			}
			else if (HasFlag(QuestFlags.StoryCritical))
			{
				GameEngine.SayToLocation(Location, $"{this.Name} struggles to survive!");
				this.HP = 1;
				return false;
			}
			return true;
		}

		// --------------------------------------------------------------------------------------------------------------------
		//  Volatile Management
		// --------------------------------------------------------------------------------------------------------------------

		public void AddHate(Player sourcePlayer, int hateValue)
		{
			if (sourcePlayer == null)
				throw new ArgumentNullException("Error: Player.AddHate null sourcePlayer error");
			if (!HatedUnits.ContainsKey(sourcePlayer))
				HatedUnits.Add(sourcePlayer, 0);
			if (hateValue < 0)
				throw new ArgumentOutOfRangeException("Error: Player.AddHate hateValue must be >= 0");

			HatedUnits[sourcePlayer] += hateValue;
		}

		public bool IsHostileToward(Player p2)
		{
			if (p2 == null)
				throw new ArgumentNullException("Error: Player IsHostileToward null player");
			if (this == p2)
				return false;

			if (Lore.FactionsHostile(this.Faction, p2.Faction))
				return true;
			if (HatedUnits.ContainsKey(p2) && HatedUnits[p2] > 0)
				return true;
			return false;
		}

		public void AssignClient(GameClient client)
		{
			if (client == null)
				throw new ArgumentNullException("Error: Player.AssignClient null client assignment, consider using RemoveClient(client) instead");

			Client = client;
			Console.WriteLine($"SERVER < [{this}] now being controlled by [{client}]"); // SAY EVEN WHEN SERVER IS PLAYING
		}

		public bool RemoveClient(GameClient client)
		{
			if (Client == client)
			{
				Console.WriteLine($"SERVER < [{this}]'s client is no longer set to [{client}]"); // SAY EVEN WHEN SERVER IS PLAYING
				Client = null;
				return true;
			}
			else
			{
				Console.WriteLine($"SERVER < [{this}]'s client is [{Client}] and had no binding changes"); // SAY EVEN WHEN SERVER IS PLAYING
				return false;
			}
		}

		public void Notify(string text) // addnotification add notification
		{
			if (text == null)
				throw new ArgumentNullException("Error: Player.Notify null text error");
			if (text.Length == 0)
				throw new ArgumentException("Error: Player.Notify empty text error");

			LastNotifyTime = DateTime.Now;
			notifications.Add(new TimeText(GameEngine.WorldTime + GameEngine.NotificationLifeSpan, text));
			if (notifications.Count > GameEngine.MaxNotifications)
				notifications.Remove(notifications[0]);
		}

		private object actionLocker = new object();

		public void TryAssignAction(string input, GameClient assigningClient)
		{
			lock (actionLocker) // CALLED BY (2 - CLIENT LISTENER)
			{
				if (input == null || input.Length == 0)
				{
					ChosenAction = DefaultAction;
					actionArg2 = "";
				}
				else
				{
					string cmd = "", arg2 = "";
					input = TextUtils.SanitizeInput(input);
					cmd = TextUtils.GetArg1(input).ToLower();
					arg2 = TextUtils.GetArg2(input);

					ChosenAction = actions.Find(ao => ao.Matches(cmd));
					if (ChosenAction == null)
					{
						ChosenAction = DefaultAction;
						actionArg2 = "";
					}
					else if (arg2 != null)
					{
						actionArg2 = arg2;
					}
				}

				if (ChosenAction != null)
				{
					string arg2out = (actionArg2.Length > 0) ? $" '{actionArg2}'" : "";
					GameEngine.SayToServer($"{Name} assigned action: {ChosenAction.Name}{arg2out}\n");
				}
			}
		}

		public void RunAI()
		{
			lock (actionLocker) // CALLED BY (1 - MAIN WORKER)
			{
				AssignActions();
				if (actions.Count > 0)
					ChosenAction = actions[rng.Next(actions.Count)];
				else
					ChosenAction = null;
			}
		}

		public bool IsReadyToAct()
		{
			if (TimeToAct <= GameEngine.WorldTime)
			{
				if (this.HasClient)
				{
					return this.ChosenAction != null;
				}
				else
				{
					this.RunAI();
					return true;
				}
			}
			return false;
		}

		public void DoTurn() // ActPlayerTurn RunTurn DoTurn RunPlayerTurn TakeTurn
		{
			lock (actionLocker) // Called by: (1 - MAIN WORKER) prepped unit when TTA goes live; AND (2 - CLIENT LISTENER) client assigning task when ready to act
			{
				if (IsReadyToAct() && ChosenAction != null)
				{
					ChosenAction.PerformAction(this);
					if (this.HasClient)
						this.TimeToAct = GameEngine.WorldTime + GameEngine.MSTimePerPCAction;
					else
						this.TimeToAct = GameEngine.WorldTime + GameEngine.MSTimePerNPCAction;
					ChosenAction = null;
				}
				else if (ChosenAction == null)
				{
					this.TimeToAct = GameEngine.WorldTime + (ulong)rng.Next(GameEngine.MSTimePerPCAction) + GameEngine.MSTimePerPCAction / 2;
				}
			}
		}

		public void ResetTimeToAct() // use after forced actions
		{
			if (this.HasClient)
				this.TimeToAct = GameEngine.WorldTime + GameEngine.MSTimePerPCAction;
			else
				this.TimeToAct = GameEngine.WorldTime + GameEngine.MSTimePerNPCAction;
		}

	}
}
