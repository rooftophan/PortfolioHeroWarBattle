using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopDeployDefinitions
{
	public enum UserDeckKeyType
	{
		StoryDeck = 0,
		CompanyMissionDeck,
		UserArenaDeck,
		UserArenaDefenseDeck,
		UserArenaDefensePracticeDeck,
		SearchPointDeck,
        SearchPointBonusStageDeck,
		TraceDeck,
		TrainingCenterDeck,
		TrainingArenaDeck,
        AloneMissionDeck,
		ExpansionDeck,
        PartyMissionDeck,
        DailyDungeon,
        UndergroundLaboratoryDeck,
        HeroChallenge,
        StoryEliteDeck,
        GuildRaidDeck,
        GuildRaidDeckBoss,
        GuildWarDeck,
        Max,
	}

	public enum DeckUnitType
	{
		Empty						= 0,
		UserHave 					= 1,
		FixedUnit					= 2,
		ForcedUnit					= 3,
		StandUnit					= 4,
		PreSetSpawnUnit				= 5,
		PreSetReinforcementUnit		= 6,
		PreSetNPCUnit				= 7,
		FriendUnit					= 8,
        TutorialUnit                = 9,
        FixedUnitControllable = 10,
	}
}
