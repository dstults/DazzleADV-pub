using System;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{

	public static partial class Prefabs
	{

		public static void AddGoblinLoot(Player goblin)
		{
			switch (rng.Next(3))
			{
				case 0:
					goblin.AddItem(ItemType.BoneClub);
					break;
				case 1:
					goblin.AddItem(ItemType.GoblinRing);
					break;
				case 2:
					goblin.AddItem(ItemType.GoblinTrinket);
					break;
			}
		}

		public static void AddCultistLoot(Player cultist)
		{
			switch (rng.Next(3))
			{
				case 0:
					cultist.AddItem(ItemType.CultistKris);
					break;
				case 1:
					cultist.AddItem(ItemType.Robes);
					break;
				case 2:
					cultist.AddItem(ItemType.BlackPowder);
					break;
			}
		}

		public static void AddSpiderlingLoot(Player spiderling)
		{
			switch (rng.Next(4))
			{
				case 0:
					spiderling.AddLoot(ItemType.SpiderFang);
					break;
				case 1:
					spiderling.AddLoot(ItemType.SpiderLeg);
					break;
			}
		}

		public static void AddSpiderLoot(Player spider)
		{
			switch (rng.Next(2))
			{
				case 0:
					spider.AddLoot(ItemType.VenomSac);
					break;
			}
			switch (rng.Next(3))
			{
				case 1:
					spider.AddLoot(ItemType.SpiderFang);
					break;
				case 2:
					spider.AddLoot(ItemType.SpiderLeg);
					break;
			}
		}

		public static void AddBroodMotherLoot(Player broodMother)
		{
			switch (rng.Next(2))
			{
				case 0:
					broodMother.AddLoot(ItemType.BroodMotherEye);
					break;
			}
			switch (rng.Next(2))
			{
				case 0:
					broodMother.AddLoot(ItemType.VenomSac);
					break;
			}
			switch (rng.Next(4))
			{
				case 0:
				case 1:
				case 2:
					broodMother.AddLoot(ItemType.SpiderFang);
					break;
			}
			switch (rng.Next(4))
			{
				case 0:
				case 1:
				case 2:
					broodMother.AddLoot(ItemType.SpiderLeg);
					break;
			}
		}

	}
}