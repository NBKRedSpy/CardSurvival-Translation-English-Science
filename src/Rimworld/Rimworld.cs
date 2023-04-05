using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Rimworld;

[BepInPlugin("Plugin.Rimworld", "Rimworld", "1.0.0")]
public class Rimworld : BaseUnityPlugin
{
	private static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();

	private static List<CardData> kilnBps = new List<CardData>();

	public static int 花桌数量 = 1;

	private static List<CardData> 找出液体s = new List<CardData>();

	private void Awake()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		花桌数量 = ((BaseUnityPlugin)this).Config.Bind<int>("Rimworld Setting", "HuaZhuoShuLiang", 1, "花桌每次的取出数量").Value;
		Harmony val = new Harmony(((BaseUnityPlugin)this).Info.Metadata.GUID);
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		try
		{
			HarmonyMethod val2 = new HarmonyMethod(typeof(Rimworld).GetMethod("SomePatch"));
			MethodInfo method = typeof(GameLoad).GetMethod("LoadMainGameData", bindingAttr);
			if (method == null)
			{
				method = typeof(GameLoad).GetMethod("LoadGameData", bindingAttr);
			}
			if (method != null)
			{
				val.Patch((MethodBase)method, (HarmonyMethod)null, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarningFormat("{0} {1}", new object[2]
			{
				"GameLoadLoadOptionsPostfix",
				ex.ToString()
			});
		}
		try
		{
			HarmonyMethod val3 = new HarmonyMethod(typeof(Rimworld).GetMethod("ARPatch"));
			MethodInfo method2 = typeof(GameManager).GetMethod("ActionRoutine", bindingAttr);
			if (method2 != null)
			{
				val.Patch((MethodBase)method2, (HarmonyMethod)null, val3, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
		catch (Exception ex2)
		{
			Debug.LogWarningFormat("{0} {1}", new object[2]
			{
				"ARPatch",
				ex2.ToString()
			});
		}
		((BaseUnityPlugin)this).Logger.LogInfo((object)"Plugin Rimworld is loaded!");
	}

	private void Update()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyUp((KeyCode)290))
		{
			GraphicsManager value = Traverse.Create((object)MBSingleton<GameManager>.Instance).Field("GameGraphics").GetValue<GraphicsManager>();
			CardData currentResearch = value.BlueprintModelsPopup.CurrentResearch;
			if ((Object)(object)currentResearch != (Object)null)
			{
				Debug.Log((object)("你按了F9，现在研究的是：" + LocalizedString.op_Implicit(currentResearch.CardName)));
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
					Debug.Log((object)"研究结束了");
				}
			}
			else
			{
				Debug.Log((object)"你按了F9，但现在没有在研究的项目");
			}
		}
		if (Input.GetKeyUp((KeyCode)287))
		{
			Debug.Log((object)Guid.NewGuid().ToString("N"));
		}
	}

	public static IEnumerator ARPatch(IEnumerator results, CardAction _Action, InGameCardBase _ReceivingCard, bool _FastMode, bool _ModifiersAlreadyCollected = false)
	{
		if (LocalizedString.op_Implicit(_Action.ActionName) == "研读蓝图" && _Action.ActionName.LocalizationKey == "Guil-Lantu")
		{
			Debug.Log((object)"你研读了蓝图");
			GraphicsManager guil = Traverse.Create((object)MBSingleton<GameManager>.Instance).Field("GameGraphics").GetValue<GraphicsManager>();
			CardData guil3 = guil.BlueprintModelsPopup.CurrentResearch;
			if ((Object)(object)guil3 != (Object)null)
			{
				GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("1c299faa61bd4479acce2820782b4518"), false);
				Dictionary<CardData, int> BlueprintResearchTimes = MBSingleton<GameManager>.Instance.BlueprintResearchTimes;
				if (!BlueprintResearchTimes.ContainsKey(guil3))
				{
					BlueprintResearchTimes.Add(guil3, 0);
				}
				Debug.Log((object)("你研读了蓝图，执行了研究，现在的研究时间是：" + (BlueprintResearchTimes[guil3] += 2)));
				if (BlueprintResearchTimes[guil3] >= GameManager.DaysToTicks(guil3.BlueprintUnlockSunsCost))
				{
					MBSingleton<GameManager>.Instance.FinishedBlueprintResearch = guil3;
					guil.BlueprintModelsPopup.FinishBlueprintResearch();
				}
			}
			else
			{
				GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("b157a8efec8f42af9de3838dc0e94ff7"), false);
			}
		}
		if (LocalizedString.op_Implicit(_Action.ActionName) == "精读蓝图" && _Action.ActionName.LocalizationKey == "Guil-Lantu")
		{
			Debug.Log((object)"你精读了蓝图");
			GraphicsManager guil2 = Traverse.Create((object)MBSingleton<GameManager>.Instance).Field("GameGraphics").GetValue<GraphicsManager>();
			CardData guil4 = guil2.BlueprintModelsPopup.CurrentResearch;
			if ((Object)(object)guil4 != (Object)null)
			{
				GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("1c299faa61bd4479acce2820782b4518"), false);
				Dictionary<CardData, int> BlueprintResearchTimes2 = MBSingleton<GameManager>.Instance.BlueprintResearchTimes;
				if (!BlueprintResearchTimes2.ContainsKey(guil4))
				{
					BlueprintResearchTimes2.Add(guil4, 0);
				}
				Debug.Log((object)("你精读了蓝图，执行了研究，现在的研究时间是：" + (BlueprintResearchTimes2[guil4] += 8)));
				if (BlueprintResearchTimes2[guil4] >= GameManager.DaysToTicks(guil4.BlueprintUnlockSunsCost))
				{
					MBSingleton<GameManager>.Instance.FinishedBlueprintResearch = guil4;
					guil2.BlueprintModelsPopup.FinishBlueprintResearch();
				}
			}
			else
			{
				GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("b157a8efec8f42af9de3838dc0e94ff7"), false);
			}
		}
		if (LocalizedString.op_Implicit(_Action.ActionName) == "进入木屋" && _Action.ActionName.LocalizationKey == "Guil_Muwu")
		{
			string preEnvGUID = ((UniqueIDScriptable)MBSingleton<GameManager>.Instance.CurrentEnvironmentCard.CardModel).UniqueID;
			if (preEnvGUID != null)
			{
				GameStat 电力2 = UniqueIDScriptable.GetFromID<GameStat>("c10e8c1add174a96987acf8684e3126d");
				InGameStat 电力4 = MBSingleton<GameManager>.Instance.StatsDict[电力2];
				StalenessData stalenessData = default(StalenessData);
				stalenessData.ModifierSource = "guil" + preEnvGUID;
				电力4.StalenessValues.Add(stalenessData);
			}
		}
		if (LocalizedString.op_Implicit(_Action.ActionName) == "离开木屋" && _Action.ActionName.LocalizationKey == "Guil_Muwu")
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
			CardDrop[] DroppedCards1 = Traverse.Create((object)_Action.ProducedCards[0]).Field("DroppedCards").GetValue<CardDrop[]>();
			DroppedCards1[0].DroppedCard = UniqueIDScriptable.GetFromID<CardData>(st);
			((Vector2Int)(ref DroppedCards1[0].Quantity)).x = 1;
			((Vector2Int)(ref DroppedCards1[0].Quantity)).y = 1;
			Traverse.Create((object)_Action.ProducedCards[0]).Field("DroppedCards").SetValue((object)DroppedCards1);
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

	private static float 消耗电力(CardData card)
	{
		string name = ((Object)card).name;
		string key = name.Replace("-通电", "");
		if (card_dict.TryGetValue(key, out var value))
		{
			if (value.CardInteractions.Length == 0)
			{
				return -1f;
			}
			CardOnCardAction[] cardInteractions = value.CardInteractions;
			foreach (CardOnCardAction val in cardInteractions)
			{
				if (((CardAction)val).ActionName.DefaultText == "接入电网")
				{
					return 0f - ((CardAction)val).StatModifications[0].ValueModifier.x;
				}
			}
		}
		return -1f;
	}

	public static void 制冰(string 烹饪前guid, string 烹饪后guid)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Invalid comparison between Unknown and I4
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Invalid comparison between Unknown and I4
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		CardData val = utc(烹饪前guid);
		CardData val2 = utc(烹饪后guid);
		if (!(((Object)(object)val == (Object)null) | ((Object)(object)val2 == (Object)null)))
		{
			CookingRecipe val3 = new CookingRecipe();
			val3.CannotCookText.DefaultText = "水不够！";
			val3.Conditions.RequiredDurabilityRanges.LiquidQuantityRange = new Vector2(280f, 9999f);
			val3.ActionName.DefaultText = "冷冻";
			val3.ActionName.LocalizationKey = "Guil-更多水果_冷冻";
			Array.Resize(ref val3.CompatibleCards, 1);
			val3.CompatibleCards[0] = val;
			if ((int)val.CardType == 9)
			{
				val3.IngredientChanges.ModifyLiquid = true;
				val3.IngredientChanges.LiquidQuantityChange = new Vector2(-300f, -300f);
				val3.IngredientChanges.ModType = (CardModifications)1;
			}
			if ((int)val.CardType == 0)
			{
				val3.IngredientChanges.ModType = (CardModifications)3;
			}
			CardDrop val4 = default(CardDrop);
			val4.DroppedCard = val2;
			val4.Quantity = new Vector2Int(1, 1);
			Array.Resize(ref val3.Drops, 1);
			val3.Drops[0] = val4;
			Traverse.Create((object)val3).Field("Duration").SetValue((object)2);
			CardData val5 = utc("99ed3ea66d954a6d8d193c52a5301912");
			Array.Resize(ref val5.CookingRecipes, val5.CookingRecipes.Length + 1);
			val5.CookingRecipes[val5.CookingRecipes.Length - 1] = val3;
		}
	}

	public static void SomePatch()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Invalid comparison between Unknown and I4
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Expected O, but got Unknown
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Expected O, but got Unknown
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f5: Invalid comparison between Unknown and I4
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Invalid comparison between Unknown and I4
		//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0729: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c8: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < GameLoad.Instance.DataBase.AllData.Count; i++)
		{
			if (GameLoad.Instance.DataBase.AllData[i] is CardData)
			{
				Dictionary<string, CardData> dictionary = card_dict;
				string name = ((Object)GameLoad.Instance.DataBase.AllData[i]).name;
				UniqueIDScriptable obj = GameLoad.Instance.DataBase.AllData[i];
				dictionary[name] = (CardData)(object)((obj is CardData) ? obj : null);
			}
		}
		CardData val = utc("f124e68738466c44b99b660ff303e7c1");
		CardData val2 = utc("ed1faa3c968011ed8285047c16184f06");
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val2))
		{
			val2.CookingRecipes = val.CookingRecipes;
			val2.CookingConditions = val.CookingConditions;
			val2.InventoryFilter = val.InventoryFilter;
		}
		val2 = utc("f1311b8f6cce4bbbacac3a7ed52c9583");
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val2))
		{
			val2.CookingRecipes = val.CookingRecipes;
			val2.CookingConditions = val.CookingConditions;
			val2.InventoryFilter = val.InventoryFilter;
		}
		val = utc("a65185c5152ecc644b503090be695008");
		val2 = utc("21a8ed94976c11ed9a84047c16184f06");
		CardData val3 = utc("429b9c74a51b11ed8c87c475ab46ec3f");
		CardData val4 = utc("3e72851fbb2a42a98d54dc4d47e7efbf");
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val2) && Object.op_Implicit((Object)(object)val3) && Object.op_Implicit((Object)(object)val4))
		{
			val2.CookingRecipes = val.CookingRecipes;
			val2.CookingConditions = val.CookingConditions;
			val2.InventoryFilter = val.InventoryFilter;
			int num = val3.CookingRecipes.Length;
			int num2 = val2.CookingRecipes.Length;
			Array.Resize(ref val2.CookingRecipes, num + num2);
			val3.CookingRecipes.CopyTo(val2.CookingRecipes, num2);
			val4.CookingRecipes = val2.CookingRecipes;
		}
		if (card_dict.TryGetValue("Guil-科技至上_花桌", out val) && 花桌数量 != 1)
		{
			for (int j = 0; j <= 2; j++)
			{
				CardDrop[] value = Traverse.Create((object)((CardAction)val.DismantleActions[j]).ProducedCards[0]).Field("DroppedCards").GetValue<CardDrop[]>();
				((Vector2Int)(ref value[0].Quantity)).x = 花桌数量;
				((Vector2Int)(ref value[0].Quantity)).y = 花桌数量;
				Traverse.Create((object)((CardAction)val.DismantleActions[j]).ProducedCards[0]).Field("DroppedCards").SetValue((object)value);
				float num3 = (float)花桌数量 / 100f;
				if (num3 <= 0f)
				{
					num3 = 0.01f;
				}
				switch (j)
				{
				case 0:
					((OptionalFloatValue)((CardAction)val.DismantleActions[j]).RequiredReceivingDurabilities.RequiredSpecial1Percent).FloatValue = num3;
					((CardAction)val.DismantleActions[j]).ReceivingCardChanges.Special1Change.x = -花桌数量;
					((CardAction)val.DismantleActions[j]).ReceivingCardChanges.Special1Change.y = -花桌数量;
					break;
				case 1:
					((OptionalFloatValue)((CardAction)val.DismantleActions[j]).RequiredReceivingDurabilities.RequiredSpecial2Percent).FloatValue = num3;
					((CardAction)val.DismantleActions[j]).ReceivingCardChanges.Special2Change.x = -花桌数量;
					((CardAction)val.DismantleActions[j]).ReceivingCardChanges.Special2Change.y = -花桌数量;
					break;
				case 2:
					((OptionalFloatValue)((CardAction)val.DismantleActions[j]).RequiredReceivingDurabilities.RequiredSpecial3Percent).FloatValue = num3;
					((CardAction)val.DismantleActions[j]).ReceivingCardChanges.Special3Change.x = -花桌数量;
					((CardAction)val.DismantleActions[j]).ReceivingCardChanges.Special3Change.y = -花桌数量;
					break;
				}
			}
		}
		foreach (KeyValuePair<string, CardData> item in card_dict)
		{
			CardData value2 = item.Value;
			if (Object.op_Implicit((Object)(object)value2) && (int)value2.CardType == 7 && value2.CardsOnBoard.Count > 0)
			{
				for (int k = 0; k < value2.CardsOnBoard.Count; k++)
				{
					CardData card = value2.CardsOnBoard[k].Card;
					if ((Object)(object)card == (Object)null)
					{
						Debug.Log((object)("no card2:" + ((Object)value2).name));
						break;
					}
					if (((UniqueIDScriptable)card).UniqueID == "d7d2831f33ccf184e9b09f8411339948")
					{
						CardOnBoardSubObjective val5 = new CardOnBoardSubObjective();
						val5.Card = utc("21a8ed94976c11ed9a84047c16184f06");
						((SubObjective)val5).CompletionWeight = 1;
						((SubObjective)val5).ObjectiveName = ((SubObjective)value2.CardsOnBoard[k]).ObjectiveName;
						val5.Quantity = 1;
						value2.CardsOnBoard.Add(val5);
						break;
					}
					if (((UniqueIDScriptable)card).UniqueID == "a65185c5152ecc644b503090be695008")
					{
						CardOnBoardSubObjective val6 = new CardOnBoardSubObjective();
						val6.Card = utc("21a8ed94976c11ed9a84047c16184f06");
						((SubObjective)val6).CompletionWeight = 1;
						((SubObjective)val6).ObjectiveName = ((SubObjective)value2.CardsOnBoard[k]).ObjectiveName;
						val6.Quantity = 1;
						value2.CardsOnBoard.Add(val6);
						break;
					}
				}
			}
			if (Object.op_Implicit((Object)(object)value2) && (int)value2.CardType == 9)
			{
				找出液体s.Add(value2);
			}
			if ((int)value2.CardType != 2 || !((Object)value2).name.StartsWith("Guil-科技至上_") || value2.DismantleActions.Count <= 0)
			{
				continue;
			}
			foreach (DismantleCardAction dismantleAction in value2.DismantleActions)
			{
				if (!(((CardAction)dismantleAction).ActionName.DefaultText == "拆除"))
				{
					continue;
				}
				((CardAction)dismantleAction).ConfirmPopup = true;
				if (((Object)value2).name.EndsWith("-通电"))
				{
					float num4 = 消耗电力(value2);
					if (num4 > 0f)
					{
						StatModifier val7 = default(StatModifier);
						val7.Stat = UniqueIDScriptable.GetFromID<GameStat>("15b71d9593ec11edbce7047c16184f06");
						val7.ValueModifier.x = num4;
						val7.ValueModifier.y = num4;
						Array.Resize(ref ((CardAction)dismantleAction).StatModifications, ((CardAction)dismantleAction).StatModifications.Length + 1);
						((CardAction)dismantleAction).StatModifications[((CardAction)dismantleAction).StatModifications.Length - 1] = val7;
					}
				}
			}
		}
		CardData fromID = UniqueIDScriptable.GetFromID<CardData>("01742fd8a0d04d0082ae05ca0d969ba6");
		for (int l = 0; l < 找出液体s.Count; l++)
		{
			CardData val8 = 找出液体s[l];
			Array.Resize(ref val8.PassiveEffects, val8.PassiveEffects.Length + 1);
			val8.PassiveEffects[val8.PassiveEffects.Length - 1] = fromID.PassiveEffects[0];
		}
		制冰("425259cb06b869d45be2e7f1b5b54aff", "b1d69e1b95fd4244b66b1c89dd59f65b");
		制冰("5481d599322f41d3b88249442ec4e8c0", "d196f43d14014712a42d371145d586d5");
		制冰("eb1c2d24e3a74870af79ba7fd8ba2868", "7ef0c448b98d4466945cfa4ba5cf7e69");
	}
}
