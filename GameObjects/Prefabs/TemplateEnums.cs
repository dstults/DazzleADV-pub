using System;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{

	public enum ItemType
	{

		// ---------------------- ITEMS ----------------------
		RabbitsFoot, GoblinRing, BlackPowder, SpiderLeg, VenomSac,
		SpiderFang, BroodMotherEye,
		GoblinTrinket, Emerald,
		HolyAnkh,

		// ---------------------- WEAPONS ----------------------
		Unarmed,
		Stick, Knife, BoneClub, CultistKris,
		RustySword, SpikedClub, Longsword,
		MythrilSword, HolySpear,
		Excalibur,

		// ---------------------- ARMORS ----------------------
		Unarmored,
		Cloth, Robes, Leather,
		ChainMail, PlateArmor,
		MythrilChainMail,

		// ---------------------- NATURAL BODY PARTS ----------------------
		WeakPoisonFangs, PoisonFangs, StrongPoisonFangs,
		HardBone, Chitin, ThickChitin
	}

	public enum UnitType
	{
		PlayerCharacter,
		WeakGoblin,
		NorthGateGuard, SouthGateGuard, KeepGuard, Chancellor, KingQueen,
		Skeleton, Cultist,
		Spiderling, Spider, BroodMother
	}

}