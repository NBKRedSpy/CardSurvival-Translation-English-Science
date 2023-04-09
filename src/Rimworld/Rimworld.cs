using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LocalizationUtilities;
using UnityEngine;

namespace Rimworld;

[BepInPlugin("Plugin.Guil-Science", "Guil-Science", "1.1.0")]
public class Rimworld : BaseUnityPlugin
{
	private static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();

	private static List<CardData> kilnBps = new List<CardData>();

    /// <summary>
	/// Number of flower tables
	/// </summary>
    public static int 花桌数量 = 1;

    /// <summary>
    /// Find the liquids
    /// </summary>
    private static List<CardData> 找出液体s = new List<CardData>();

	public static ManualLogSource ManualLogger { get; private set; }
	private void Awake()
	{

        LocalizationStringUtility.Init(
            Config.Bind<bool>("Debug", "LogCardInfo", false,
                "If true, will output the localization keys for the cards. 如果为真，将输出卡片的本地化密钥。").Value,
            Info.Location,
            Logger
        );

		ManualLogger = Logger;

        花桌数量 = base.Config.Bind("Rimworld Setting", "HuaZhuoShuLiang", 1, "花桌每次的取出数量").Value;
		Harmony harmony = new Harmony(base.Info.Metadata.GUID);
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		try
		{
			HarmonyMethod postfix = new HarmonyMethod(typeof(Rimworld).GetMethod("SomePatch"));
			MethodInfo method = typeof(GameLoad).GetMethod("LoadMainGameData", bindingAttr);
			if (method == null)
			{
				method = typeof(GameLoad).GetMethod("LoadGameData", bindingAttr);
			}
			if (method != null)
			{
				harmony.Patch(method, null, postfix);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarningFormat("{0} {1}", "GameLoadLoadOptionsPostfix", ex.ToString());
		}
		try
		{
			HarmonyMethod postfix2 = new HarmonyMethod(typeof(Rimworld).GetMethod("ARPatch"));
			MethodInfo method2 = typeof(GameManager).GetMethod("ActionRoutine", bindingAttr);
			if (method2 != null)
			{
				harmony.Patch(method2, null, postfix2);
			}
		}
		catch (Exception ex2)
		{
			Debug.LogWarningFormat("{0} {1}", "ARPatch", ex2.ToString());
		}
		base.Logger.LogInfo("Plugin Rimworld is loaded!");
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.F9))
		{
			GraphicsManager value = Traverse.Create(MBSingleton<GameManager>.Instance).Field("GameGraphics").GetValue<GraphicsManager>();
			CardData currentResearch = value.BlueprintModelsPopup.CurrentResearch;
			if (currentResearch != null)
			{
				Debug.Log("你按了F9，现在研究的是：" + currentResearch.CardName);
				Dictionary<CardData, int> blueprintResearchTimes = MBSingleton<GameManager>.Instance.BlueprintResearchTimes;
				if (!blueprintResearchTimes.ContainsKey(currentResearch))
				{
					blueprintResearchTimes.Add(currentResearch, 0);
				}
				int value2 = blueprintResearchTimes[currentResearch] + 960;
				blueprintResearchTimes[currentResearch] = value2;
				if (blueprintResearchTimes[currentResearch] >= GameManager.DaysToTicks(currentResearch.BlueprintUnlockSunsCost))
				{
					MBSingleton<GameManager>.Instance.FinishedBlueprintResearch = currentResearch;
					value.BlueprintModelsPopup.FinishBlueprintResearch();
					Debug.Log("研究结束了");
				}
			}
			else
			{
				Debug.Log("你按了F9，但现在没有在研究的项目");
			}
		}
		if (Input.GetKeyUp(KeyCode.F6))
		{
			Debug.Log(Guid.NewGuid().ToString("N"));
		}
	}

	public static IEnumerator ARPatch(IEnumerator results, CardAction _Action, InGameCardBase _ReceivingCard, bool _FastMode, bool _ModifiersAlreadyCollected = false)
	{
        //研读蓝图 = Read the blueprint
        //Change:  Changed to LocalizationKey since the the action name is always "研读蓝图" for this key.
        if (_Action.ActionName.LocalizationKey == "Guil-Lantu_The")
		{
			Debug.Log("你研读了蓝图");
			GraphicsManager guil = Traverse.Create(MBSingleton<GameManager>.Instance).Field("GameGraphics").GetValue<GraphicsManager>();
			CardData guil3 = guil.BlueprintModelsPopup.CurrentResearch;
			if (guil3 != null)
			{
				GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("1c299faa61bd4479acce2820782b4518"), _Complete: false);
				Dictionary<CardData, int> BlueprintResearchTimes = MBSingleton<GameManager>.Instance.BlueprintResearchTimes;
				if (!BlueprintResearchTimes.ContainsKey(guil3))
				{
					BlueprintResearchTimes.Add(guil3, 0);
				}
				Debug.Log("你研读了蓝图，执行了研究，现在的研究时间是：" + (BlueprintResearchTimes[guil3] += 2));
				if (BlueprintResearchTimes[guil3] >= GameManager.DaysToTicks(guil3.BlueprintUnlockSunsCost))
				{
					MBSingleton<GameManager>.Instance.FinishedBlueprintResearch = guil3;
					guil.BlueprintModelsPopup.FinishBlueprintResearch();
				}
			}
			else
			{
				GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("b157a8efec8f42af9de3838dc0e94ff7"), _Complete: false);
			}
		}

        //Read a blueprint
        //精读蓝图 == "Guil-Lantu_A"
        if (_Action.ActionName.LocalizationKey == "Guil-Lantu_A")
		{
			Debug.Log("你精读了蓝图");
			GraphicsManager guil2 = Traverse.Create(MBSingleton<GameManager>.Instance).Field("GameGraphics").GetValue<GraphicsManager>();
			CardData guil4 = guil2.BlueprintModelsPopup.CurrentResearch;
			if (guil4 != null)
			{
				GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("1c299faa61bd4479acce2820782b4518"), _Complete: false);
				Dictionary<CardData, int> BlueprintResearchTimes2 = MBSingleton<GameManager>.Instance.BlueprintResearchTimes;
				if (!BlueprintResearchTimes2.ContainsKey(guil4))
				{
					BlueprintResearchTimes2.Add(guil4, 0);
				}
				Debug.Log("你精读了蓝图，执行了研究，现在的研究时间是：" + (BlueprintResearchTimes2[guil4] += 8));
				if (BlueprintResearchTimes2[guil4] >= GameManager.DaysToTicks(guil4.BlueprintUnlockSunsCost))
				{
					MBSingleton<GameManager>.Instance.FinishedBlueprintResearch = guil4;
					guil2.BlueprintModelsPopup.FinishBlueprintResearch();
				}
			}
			else
			{
				GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("b157a8efec8f42af9de3838dc0e94ff7"), _Complete: false);
			}
		}

        //Enter the wooden house. 
        //"进入木屋" == "Guil_Muwu_Enter"
        if (_Action.ActionName.LocalizationKey == "Guil_Muwu_Enter")
		{
			string preEnvGUID = MBSingleton<GameManager>.Instance.CurrentEnvironmentCard.CardModel.UniqueID;
			if (preEnvGUID != null)
			{
				GameStat 电力2 = UniqueIDScriptable.GetFromID<GameStat>("c10e8c1add174a96987acf8684e3126d");
				InGameStat 电力4 = MBSingleton<GameManager>.Instance.StatsDict[电力2];
				StalenessData stalenessData = default(StalenessData);
				stalenessData.ModifierSource = "guil" + preEnvGUID;
				电力4.StalenessValues.Add(stalenessData);
			}
		}

        //离开木屋 = Leave the wooden house
        if (_Action.ActionName.LocalizationKey == "Guil_Muwu_Exit")
		{
			GameStat 电力 = UniqueIDScriptable.GetFromID<GameStat>("c10e8c1add174a96987acf8684e3126d");
			InGameStat 电力3 = MBSingleton<GameManager>.Instance.StatsDict[电力];
			string st = "";
			foreach (StalenessData x in 电力3.StalenessValues)
			{
				if (x.ModifierSource.StartsWith("guil"))
				{
					st = x.ModifierSource.Substring(4);
				}
			}
			if (st == null)
			{
				st = "99d50c5820b3fda4db985e85f5995977";
			}
			CardDrop[] DroppedCards1 = Traverse.Create(_Action.ProducedCards[0]).Field("DroppedCards").GetValue<CardDrop[]>();
			DroppedCards1[0].DroppedCard = UniqueIDScriptable.GetFromID<CardData>(st);
			DroppedCards1[0].Quantity.x = 1;
			DroppedCards1[0].Quantity.y = 1;
			Traverse.Create(_Action.ProducedCards[0]).Field("DroppedCards").SetValue(DroppedCards1);
		}
		while (results.MoveNext())
		{
			yield return results.Current;
		}
	}

	private static CardData utc(string uniqueID)
	{
		return UniqueIDScriptable.GetFromID<CardData>(uniqueID);
	}

    /// <summary>
	/// Consumption
	/// </summary>
	/// <param name="card"></param>
	/// <returns></returns>
    private static float 消耗电力(CardData card)
	{
		string text = card.name;


        //-power ups
        string key = text.Replace("-通电", "");
		if (card_dict.TryGetValue(key, out var value))
		{
			if (value.CardInteractions.Length == 0)
			{
				return -1f;
			}
			CardOnCardAction[] cardInteractions = value.CardInteractions;
			foreach (CardOnCardAction cardOnCardAction in cardInteractions)
			{
                //Access to the grid.  Actions are only in .json.  Use translation key.
				//key of: "T-byXKiGBqDMaPAczTSvHnH4hk5Ck=" == "接入电网" from there.
                if (cardOnCardAction.ActionName.LocalizationKey == "T-byXKiGBqDMaPAczTSvHnH4hk5Ck=")
				{
					return 0f - cardOnCardAction.StatModifications[0].ValueModifier.x;
				}
			}
		}
		return -1f;
	}

    //Make ice
    public static void 制冰(string 烹饪前guid, string 烹饪后guid)
	{
		CardData cardData = utc(烹饪前guid);
		CardData cardData2 = utc(烹饪后guid);
		if (!((cardData == null) | (cardData2 == null)))
		{
			CookingRecipe cookingRecipe = new CookingRecipe();
			cookingRecipe.CannotCookText.DefaultText = "水不够！";
			cookingRecipe.CannotCookText.SetLocalizationInfo();

            cookingRecipe.Conditions.RequiredDurabilityRanges.LiquidQuantityRange = new Vector2(280f, 9999f);
			cookingRecipe.ActionName.DefaultText = "冷冻";
			cookingRecipe.ActionName.LocalizationKey = "Guil-更多水果_冷冻";
			Array.Resize(ref cookingRecipe.CompatibleCards, 1);
			cookingRecipe.CompatibleCards[0] = cardData;
			if (cardData.CardType == CardTypes.Liquid)
			{
				cookingRecipe.IngredientChanges.ModifyLiquid = true;
				cookingRecipe.IngredientChanges.LiquidQuantityChange = new Vector2(-300f, -300f);
				cookingRecipe.IngredientChanges.ModType = CardModifications.DurabilityChanges;
			}
			if (cardData.CardType == CardTypes.Item)
			{
				cookingRecipe.IngredientChanges.ModType = CardModifications.Destroy;
			}
			CardDrop cardDrop = default(CardDrop);
			cardDrop.DroppedCard = cardData2;
			cardDrop.Quantity = new Vector2Int(1, 1);
			Array.Resize(ref cookingRecipe.Drops, 1);
			cookingRecipe.Drops[0] = cardDrop;
			Traverse.Create(cookingRecipe).Field("Duration").SetValue(2);
			CardData cardData3 = utc("99ed3ea66d954a6d8d193c52a5301912");
			Array.Resize(ref cardData3.CookingRecipes, cardData3.CookingRecipes.Length + 1);
			cardData3.CookingRecipes[cardData3.CookingRecipes.Length - 1] = cookingRecipe;
		}
	}

	public static void SomePatch()
	{
		for (int i = 0; i < GameLoad.Instance.DataBase.AllData.Count; i++)
		{
			if (GameLoad.Instance.DataBase.AllData[i] is CardData)
			{
				card_dict[GameLoad.Instance.DataBase.AllData[i].name] = GameLoad.Instance.DataBase.AllData[i] as CardData;
			}
		}
		CardData cardData = utc("f124e68738466c44b99b660ff303e7c1");
		CardData cardData2 = utc("ed1faa3c968011ed8285047c16184f06");
		if ((bool)cardData && (bool)cardData2)
		{
			cardData2.CookingRecipes = cardData.CookingRecipes;
			cardData2.CookingConditions = cardData.CookingConditions;
			cardData2.InventoryFilter = cardData.InventoryFilter;
		}
		cardData2 = utc("f1311b8f6cce4bbbacac3a7ed52c9583");
		if ((bool)cardData && (bool)cardData2)
		{
			cardData2.CookingRecipes = cardData.CookingRecipes;
			cardData2.CookingConditions = cardData.CookingConditions;
			cardData2.InventoryFilter = cardData.InventoryFilter;
		}
		cardData = utc("a65185c5152ecc644b503090be695008");
		cardData2 = utc("21a8ed94976c11ed9a84047c16184f06");
		CardData cardData3 = utc("429b9c74a51b11ed8c87c475ab46ec3f");
		CardData cardData4 = utc("3e72851fbb2a42a98d54dc4d47e7efbf");
		if ((bool)cardData && (bool)cardData2 && (bool)cardData3 && (bool)cardData4)
		{
			cardData2.CookingRecipes = cardData.CookingRecipes;
			cardData2.CookingConditions = cardData.CookingConditions;
			cardData2.InventoryFilter = cardData.InventoryFilter;
			int num = cardData3.CookingRecipes.Length;
			int num2 = cardData2.CookingRecipes.Length;
			Array.Resize(ref cardData2.CookingRecipes, num + num2);
			cardData3.CookingRecipes.CopyTo(cardData2.CookingRecipes, num2);
			cardData4.CookingRecipes = cardData2.CookingRecipes;
		}
		//TODO: If card name is affected, this has to be addressed.
		if (card_dict.TryGetValue("Guil-科技至上_花桌", out cardData) && 花桌数量 != 1)
		{
			for (int j = 0; j <= 2; j++)
			{
				CardDrop[] value = Traverse.Create(cardData.DismantleActions[j].ProducedCards[0]).Field("DroppedCards").GetValue<CardDrop[]>();
				value[0].Quantity.x = 花桌数量;
				value[0].Quantity.y = 花桌数量;
				Traverse.Create(cardData.DismantleActions[j].ProducedCards[0]).Field("DroppedCards").SetValue(value);
				float num3 = (float)花桌数量 / 100f;
				if (num3 <= 0f)
				{
					num3 = 0.01f;
				}
				switch (j)
				{
				case 0:
					cardData.DismantleActions[j].RequiredReceivingDurabilities.RequiredSpecial1Percent.FloatValue = num3;
					cardData.DismantleActions[j].ReceivingCardChanges.Special1Change.x = -花桌数量;
					cardData.DismantleActions[j].ReceivingCardChanges.Special1Change.y = -花桌数量;
					break;
				case 1:
					cardData.DismantleActions[j].RequiredReceivingDurabilities.RequiredSpecial2Percent.FloatValue = num3;
					cardData.DismantleActions[j].ReceivingCardChanges.Special2Change.x = -花桌数量;
					cardData.DismantleActions[j].ReceivingCardChanges.Special2Change.y = -花桌数量;
					break;
				case 2:
					cardData.DismantleActions[j].RequiredReceivingDurabilities.RequiredSpecial3Percent.FloatValue = num3;
					cardData.DismantleActions[j].ReceivingCardChanges.Special3Change.x = -花桌数量;
					cardData.DismantleActions[j].ReceivingCardChanges.Special3Change.y = -花桌数量;
					break;
				}
			}
		}
		foreach (KeyValuePair<string, CardData> item in card_dict)
		{
			CardData value2 = item.Value;
			if ((bool)value2 && value2.CardType == CardTypes.Blueprint && value2.CardsOnBoard.Count > 0)
			{
				for (int k = 0; k < value2.CardsOnBoard.Count; k++)
				{
					CardData card = value2.CardsOnBoard[k].Card;
					if (card == null)
					{
						Debug.Log("no card2:" + value2.name);
						break;
					}
					if (card.UniqueID == "d7d2831f33ccf184e9b09f8411339948")
					{
						CardOnBoardSubObjective cardOnBoardSubObjective = new CardOnBoardSubObjective();
						cardOnBoardSubObjective.Card = utc("21a8ed94976c11ed9a84047c16184f06");
						cardOnBoardSubObjective.CompletionWeight = 1;
						cardOnBoardSubObjective.ObjectiveName = value2.CardsOnBoard[k].ObjectiveName;
						cardOnBoardSubObjective.Quantity = 1;
						value2.CardsOnBoard.Add(cardOnBoardSubObjective);
						break;
					}
					if (card.UniqueID == "a65185c5152ecc644b503090be695008")
					{
						CardOnBoardSubObjective cardOnBoardSubObjective2 = new CardOnBoardSubObjective();
						cardOnBoardSubObjective2.Card = utc("21a8ed94976c11ed9a84047c16184f06");
						cardOnBoardSubObjective2.CompletionWeight = 1;
						cardOnBoardSubObjective2.ObjectiveName = value2.CardsOnBoard[k].ObjectiveName;
						cardOnBoardSubObjective2.Quantity = 1;
						value2.CardsOnBoard.Add(cardOnBoardSubObjective2);
						break;
					}
				}
			}
			if ((bool)value2 && value2.CardType == CardTypes.Liquid)
			{
				找出液体s.Add(value2);
			}
			//Todo:  card name translation question.
			if (value2.CardType != CardTypes.Location || !value2.name.StartsWith("Guil-科技至上_") || value2.DismantleActions.Count <= 0)
			{
				continue;
			}
			foreach (DismantleCardAction dismantleAction in value2.DismantleActions)
			{
                //tear down
                //  "拆除" == "T-5Vbc1DoG2wO5Cu6T4mTWzns33Yg="
                // Note - filtered above with StartsWith("Guil-科技至上_") check.
                if (!(dismantleAction.ActionName.LocalizationKey == "T-5Vbc1DoG2wO5Cu6T4mTWzns33Yg="))
				{
					continue;
				}

				dismantleAction.ConfirmPopup = true;
				//-power ups
                if (value2.name.EndsWith("-通电"))
				{
					float num4 = 消耗电力(value2);
					if (num4 > 0f)
					{
						StatModifier statModifier = default(StatModifier);
						statModifier.Stat = UniqueIDScriptable.GetFromID<GameStat>("15b71d9593ec11edbce7047c16184f06");
						statModifier.ValueModifier.x = num4;
						statModifier.ValueModifier.y = num4;
						Array.Resize(ref dismantleAction.StatModifications, dismantleAction.StatModifications.Length + 1);
						dismantleAction.StatModifications[dismantleAction.StatModifications.Length - 1] = statModifier;
					}
				}
			}

        }
		CardData fromID = UniqueIDScriptable.GetFromID<CardData>("01742fd8a0d04d0082ae05ca0d969ba6");
		for (int l = 0; l < 找出液体s.Count; l++)
		{
			CardData cardData5 = 找出液体s[l];
			Array.Resize(ref cardData5.PassiveEffects, cardData5.PassiveEffects.Length + 1);
			cardData5.PassiveEffects[cardData5.PassiveEffects.Length - 1] = fromID.PassiveEffects[0];
		}
        //Make ice
        制冰("425259cb06b869d45be2e7f1b5b54aff", "b1d69e1b95fd4244b66b1c89dd59f65b");
		制冰("5481d599322f41d3b88249442ec4e8c0", "d196f43d14014712a42d371145d586d5");
		制冰("eb1c2d24e3a74870af79ba7fd8ba2868", "7ef0c448b98d4466945cfa4ba5cf7e69");
	}
}
