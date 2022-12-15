using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using System.Linq;

public class ExperienceBattleSetOnlyTroopInfoTransit : SetOnlyTroopInfoBaseTransit 
{
	
	ExperienceBattleModel _model;
    int experienceHeroIndex;
    private int skin;

    public ExperienceBattleSetOnlyTroopInfoTransit( int experienceHeroIndex, int skin ) : base()
    {
        
        this.experienceHeroIndex = experienceHeroIndex;
        this.skin = skin;
    }

    protected override void OnInit()
    {
        base.OnInit();
        BackKeyService.UnRegister(this);
    }

    public override void InitSetOnlyTroopInfo( )
	{
		base.InitSetOnlyTroopInfo ();
        
		_model = System.Data.ExperienceBattleModel;
        _model.experienceHeroIndex = experienceHeroIndex;
    }
        
	protected override void SetAllyDeckUnitInfo()
    {
        for( int i = 0; i < _deployInfoManager.UserDeckData.UserBattleDeckFormations.Count; i++ )
        {
            int waveIndex = i;
            UserBattleDeckFormation userBattleDeck = _deployInfoManager.UserDeckData.UserBattleDeckFormations[waveIndex];
            for( int j = 0; j < _deployInfoManager.UserDeckData.UserBattleDecks.Count; j++ )
            {
                int posIndex = j;
                UserBattleDeck battleDeck = _deployInfoManager.UserDeckData.UserBattleDecks[posIndex];
                HeroModel userHeroModel = GetUserHeroModel( posIndex );
                battleDeck.DeckUnitType = TroopDeployDefinitions.DeckUnitType.TutorialUnit;
                battleDeck.UnitInfo = userHeroModel.Info;
                battleDeck.UnitInfo.FormationIndex = (sbyte)posIndex;
                battleDeck.UnitID = userHeroModel.ID;
            }
        }

        //var isTutorial = _model.IsTutorial;

        //if (isTutorial) {
        //	for (int i = 0; i < _deployInfoManager.UserDeckData.UserBattleDeckFormations.Count; i++) {
        //		int waveIndex = i;
        //		UserBattleDeckFormation userBattleDeck = _deployInfoManager.UserDeckData.UserBattleDeckFormations [waveIndex];
        //		for (int j = 0; j < _deployInfoManager.UserDeckData.UserBattleDecks.Count; j++) {
        //			int posIndex = j;
        //			UserBattleDeck battleDeck = _deployInfoManager.UserDeckData.UserBattleDecks [posIndex];
        //			HeroModel userHeroModel = GetUserHeroModel (posIndex);
        //			battleDeck.DeckUnitType = TroopDeployDefinitions.DeckUnitType.UserHave;
        //			battleDeck.UnitInfo = userHeroModel.Info;
        //			battleDeck.UnitInfo.FormationIndex = (sbyte)posIndex;
        //			battleDeck.UnitID = userHeroModel.ID;
        //		}
        //	}
        //}
    }

	protected HeroModel GetUserHeroModel(int posIndex)
	{
        var stageRow = _model.currentStageRow;
        HeroInfo unitInfo = null;
        if( posIndex == 0 )
        {
             unitInfo = new HeroInfo(  GameSystem.Instance.Data.BattleContext, experienceHeroIndex);
             unitInfo.Skin = skin;
        }
        else
        {
            unitInfo =  new HeroInfo(  GameSystem.Instance.Data.BattleContext, stageRow.ForceHero[posIndex], stageRow.ForceHeroLevel[posIndex]);
        }

        //HeroInfo unitInfo = new HeroInfo(  GameSystem.Instance.Data.BattleContext, experienceHeroIndex);
        HeroModel hero = new HeroModel (System.Data, unitInfo);

        return hero;
    }

	protected override void InitTroopListData()
	{
		base.InitTroopListData ();

        _troopList = _model.currentStageRow.TroopIndex;
        //_troopList = new int[] {experienceHeroIndex,experienceHeroIndex,experienceHeroIndex};
	}

	protected override int[] GetCurTroopHeroes(int troopIndex)
	{
		return System.Data.Sheet.SheetExperienceBattleStageTroop.Hero[troopIndex];
        //return new int[] { experienceHeroIndex };
	}

	protected override int[] GetCurTroopUnitLevels(int troopIndex)
	{
        //return new int[] {1,1,1};
		return System.Data.Sheet.SheetExperienceBattleStageTroop.Level[troopIndex];;
	}

	protected override int[] GetCurTroopUnitDifficultyTypes(int troopIndex)
	{
        //return new int[] {601,601,601};
		return System.Data.Sheet.SheetExperienceBattleStageTroop.DifficultyType[troopIndex];;
	}

	protected override string GetAllyFormationPath( int troopIndex )
	{        
		//return "AllyFormation_Default_22";//System.Data.Sheet.SheetTutorialBattleStageTroop.AllyFormationPath [troopIndex];
        return System.Data.Sheet.SheetExperienceBattleStageTroop.AllyFormationPath [troopIndex];
	}

	protected override string GetEnemyFormationPath( int troopIndex )
	{
        //return "EnemyFormation_TutorialBattle_22";
		return System.Data.Sheet.SheetExperienceBattleStageTroop.FormationPath [troopIndex];
	}

	protected override string GetBattleEventPath( int troopIndex )
	{
        //return "";
		return System.Data.Sheet.SheetExperienceBattleStageTroop.BattleEventPath [troopIndex];
	}

	
}
