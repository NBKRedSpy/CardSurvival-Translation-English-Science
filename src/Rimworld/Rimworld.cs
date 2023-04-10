using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Rimworld{

    [BepInPlugin("Plugin.Rimworld", "Rimworld", "2.1.0")]
    public class Rimworld : BaseUnityPlugin
    {
        static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();
        static List<CardData> kilnBps = new List<CardData>();

        /// <summary>
	    /// Number of flower tables
	    /// </summary>
        public static int 花桌数量 = 1;

        public static ManualLogSource ManualLogger { get; private set; }

        public ConfigEntry<bool> EnableDebugKeys { get; set; }
        private void Awake()
        {

            EnableDebugKeys = Config.Bind<bool>("Debug", nameof(EnableDebugKeys), false,
                "If true, will enable the F6 and F9 debug keys.  如果为真，将启用F6和F9调试键。");


            ManualLogger = Logger;

            Rimworld.花桌数量 = base.Config.Bind<int>("Rimworld Setting", "HuaZhuoShuLiang", 1, "花桌每次的取出数量").Value;
            //手动Patch
            var harmony = new Harmony(this.Info.Metadata.GUID);
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            try
            {
                var load = new HarmonyMethod(typeof(Rimworld).GetMethod("SomePatch"));
                var method = typeof(GameLoad).GetMethod("LoadMainGameData", bindingFlags);
                if (method == null)
                {
                    method = typeof(GameLoad).GetMethod("LoadGameData", bindingFlags);
                }

                if (method != null)
                {
                    harmony.Patch(method, postfix: load);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarningFormat("{0} {1}", "GameLoadLoadOptionsPostfix", ex.ToString());
            }

            try
            {
                var load = new HarmonyMethod(typeof(Rimworld).GetMethod("ARPatch"));
                var method = typeof(GameManager).GetMethod("ActionRoutine", bindingFlags);
                if (method != null)
                    harmony.Patch(method, postfix: load);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarningFormat("{0} {1}", "ARPatch", ex.ToString());
            }
            //Harmony.CreateAndPatchAll(typeof(Rimworld));
            Logger.LogInfo("Plugin Rimworld is loaded!");
        }
    
        static List<CardData> 找出液体s = new List<CardData> { };
        
        private void Update()
	    {
		    if(!EnableDebugKeys.Value)
		    {
			    return;
		    }

            if (Input.GetKeyUp(KeyCode.F9))
            {
                GraphicsManager guil01 = Traverse.Create(MBSingleton<GameManager>.Instance).Field("GameGraphics").GetValue<GraphicsManager>();
                CardData guil03 = guil01.BlueprintModelsPopup.CurrentResearch;
                if (guil03 != null)
                {
                    Debug.Log("你按了F9，现在研究的是：" + guil03.CardName);
                    //这里之上实现了，可以读取在研究的卡
                    //查找这东西是否有研究进度
                    Dictionary<CardData, int> BlueprintResearchTimes = MBSingleton<GameManager>.Instance.BlueprintResearchTimes;
                    if (!BlueprintResearchTimes.ContainsKey(guil03))
                    {
                        BlueprintResearchTimes.Add(guil03, 0);
                        //Debug.Log("添加了这个研究的东西");
                        //MBSingleton<GameManager>.Instance.BlueprintResearchTimes = BlueprintResearchTimes;
                    }

                    //执行研究                   
                    int num = BlueprintResearchTimes[guil03] + 960;
                    BlueprintResearchTimes[guil03] = num;
                    //Debug.Log("执行了研究，现在的研究时间是：" + num.ToString());

                    //研究结束
                    if (BlueprintResearchTimes[guil03] >= GameManager.DaysToTicks(guil03.BlueprintUnlockSunsCost))
                    {
                        MBSingleton<GameManager>.Instance.FinishedBlueprintResearch = guil03;
                        guil01.BlueprintModelsPopup.FinishBlueprintResearch();
                        Debug.Log("研究结束了");
                    }
                }
                else
                {
                    Debug.Log("你按了F9，但现在没有在研究的项目");
                }
            }
			if (Input.GetKeyUp(KeyCode.F6)) {
				Debug.Log(System.Guid.NewGuid().ToString("N"));
			}
	    }

	    public static IEnumerator ARPatch(IEnumerator results, CardAction _Action, InGameCardBase _ReceivingCard, bool _FastMode, bool _ModifiersAlreadyCollected = false)
	    {
            //研读蓝图 = Read the blueprint
            //Change:  Changed to LocalizationKey since the the action name is always "研读蓝图" for this key.
            //获取名字成功
            if (_Action.ActionName == "研读蓝图" && _Action.ActionName.LocalizationKey == "Guil-Lantu")
            {
                Debug.Log("你研读了蓝图");
                //执行推进科技
                GraphicsManager guil01 = Traverse.Create(MBSingleton<GameManager>.Instance).Field("GameGraphics").GetValue<GraphicsManager>();
                CardData guil03 = guil01.BlueprintModelsPopup.CurrentResearch;
                if (guil03 != null)
                {
                    GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("1c299faa61bd4479acce2820782b4518"), false);
                    
                    //Debug.Log("你研读了蓝图，现在研究的是：" + guil03.CardName);
                    //这里之上实现了，可以读取在研究的卡

                    //查找这东西是否有研究进度
                    Dictionary<CardData, int> BlueprintResearchTimes = MBSingleton<GameManager>.Instance.BlueprintResearchTimes;
                    if (!BlueprintResearchTimes.ContainsKey(guil03))
                    {
                        BlueprintResearchTimes.Add(guil03, 0);
                        //Debug.Log("你研读了蓝图，添加了这个研究的东西");
                        //MBSingleton<GameManager>.Instance.BlueprintResearchTimes = BlueprintResearchTimes;
                    }

                    //执行研究                   
                    int num = BlueprintResearchTimes[guil03] + 2;
                    BlueprintResearchTimes[guil03] = num;
                    Debug.Log("你研读了蓝图，执行了研究，现在的研究时间是：" + num.ToString());

                    //研究结束
                    if (BlueprintResearchTimes[guil03] >= GameManager.DaysToTicks(guil03.BlueprintUnlockSunsCost))
                    {
                        MBSingleton<GameManager>.Instance.FinishedBlueprintResearch = guil03;
                        guil01.BlueprintModelsPopup.FinishBlueprintResearch();
                        //Debug.Log("你研读了蓝图，研究结束了");
                    }
			}
                else
                {
                    GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("b157a8efec8f42af9de3838dc0e94ff7"), false);
                }
            }

            //Read a blueprint
            //精读蓝图 == "Guil-Lantu_A"
            if (_Action.ActionName.LocalizationKey == "Guil-Lantu_A")
		    {
			    Debug.Log("你精读了蓝图");
                //执行推进科技
                GraphicsManager guil01 = Traverse.Create(MBSingleton<GameManager>.Instance).Field("GameGraphics").GetValue<GraphicsManager>();
                CardData guil03 = guil01.BlueprintModelsPopup.CurrentResearch;
                if (guil03 != null)
                {
                    GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("1c299faa61bd4479acce2820782b4518"), false);

                    //查找这东西是否有研究进度
                    Dictionary<CardData, int> BlueprintResearchTimes = MBSingleton<GameManager>.Instance.BlueprintResearchTimes;
                    if (!BlueprintResearchTimes.ContainsKey(guil03))
                    {
                        BlueprintResearchTimes.Add(guil03, 0);
				    }

                    //执行研究                   
                    int num = BlueprintResearchTimes[guil03] + 8;
                    BlueprintResearchTimes[guil03] = num;
                    Debug.Log("你精读了蓝图，执行了研究，现在的研究时间是：" + num.ToString());

                    //研究结束
                    if (BlueprintResearchTimes[guil03] >= GameManager.DaysToTicks(guil03.BlueprintUnlockSunsCost))
                    {
                        MBSingleton<GameManager>.Instance.FinishedBlueprintResearch = guil03;
                        guil01.BlueprintModelsPopup.FinishBlueprintResearch();
                    }
                }
                else
                {
                    GameManager.GiveCard(UniqueIDScriptable.GetFromID<CardData>("b157a8efec8f42af9de3838dc0e94ff7"), false);
			    }
		    }

            //Enter the wooden house. 
            //"进入木屋" == "Guil_Muwu_Enter"
            if (_Action.ActionName.LocalizationKey == "Guil_Muwu_Enter")
            {
                //存储先前环境卡到para
                string preEnvGUID = GameManager.Instance.CurrentEnvironmentCard.CardModel.UniqueID;
                if (preEnvGUID != null)
                {
                    GameStat 电力;
                    InGameStat 电力2;
                    电力 = UniqueIDScriptable.GetFromID<GameStat>("c10e8c1add174a96987acf8684e3126d");
                    电力2 = GameManager.Instance.StatsDict[电力];
                    StalenessData stalenessData = new StalenessData();
                    stalenessData.ModifierSource = "guil" + preEnvGUID;
                    电力2.StalenessValues.Add(stalenessData);

                    /*
                   StreamWriter sw0 = new StreamWriter(Directory.GetCurrentDirectory() + "\\para.txt");
                   try
                   {
                       sw0.WriteLine(preEnvGUID);
                       sw0.Close();                    
                   }
                   catch
                   {
                       sw0.Close();
                   }
                   */

                }
            }

            //离开木屋 = Leave the wooden house
            if (_Action.ActionName.LocalizationKey == "Guil_Muwu_Exit")
		    {
                    GameStat 电力;
                    InGameStat 电力2;
                    电力 = UniqueIDScriptable.GetFromID<GameStat>("c10e8c1add174a96987acf8684e3126d");
                    电力2 = GameManager.Instance.StatsDict[电力];
                    string st = "";
                    foreach (StalenessData x in 电力2.StalenessValues)
                    {
                        if (x.ModifierSource.StartsWith("guil"))
                        {
					    st = x.ModifierSource.Substring(4);
				    }
			    }
                //空值处理
                if (st == null)
			    {
				    st = "99d50c5820b3fda4db985e85f5995977";
			    }
                //写入producecard
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
        
        private static CardData utc(String uniqueID)
        {
            return UniqueIDScriptable.GetFromID<CardData>(uniqueID);
        }

	    /// Consumption
	    /// </summary>
	    /// <param name="card"></param>
        private static float 消耗电力(CardData card)
        {
            string name = card.name;
            string origin_name = name.Replace("-通电", "");
            CardData origin_card;
            if (card_dict.TryGetValue(origin_name, out origin_card))
            {
                if (origin_card.CardInteractions.Length == 0)
                {
                    return -1f;
                }
                foreach (CardOnCardAction action in origin_card.CardInteractions)
                {
                    //Access to the grid.  Actions are only in .json.  Use translation key.
                    //key of: "T-byXKiGBqDMaPAczTSvHnH4hk5Ck=" == "接入电网" from there.
                    if (action.ActionName.LocalizationKey == "T-byXKiGBqDMaPAczTSvHnH4hk5Ck=")
                    {
                        return -(action.StatModifications[0].ValueModifier.x);
                    }
                }
            }
	        return -1f;
	    }

		public static void 制冰(String 烹饪前guid, string 烹饪后guid)
		{
			CardData 烹饪前 = utc(烹饪前guid);
			CardData 烹饪后 = utc(烹饪后guid);
            if(烹饪前 == null | 烹饪后 ==  null) { return; }

			CookingRecipe cr = new CookingRecipe();
			cr.CannotCookText.DefaultText = "水不够！";
			cr.CannotCookText.LocalizationKey = "T-DYdH3B7N25bX3Yv5kgB307rJU+w=";

			cr.Conditions.RequiredDurabilityRanges.LiquidQuantityRange = new Vector2(280f, 9999f);
			cr.ActionName.DefaultText = "冷冻";
			cr.ActionName.LocalizationKey = "Guil-更多水果_冷冻";
			Array.Resize(ref cr.CompatibleCards, 1);
			cr.CompatibleCards[0] = 烹饪前;

            if(烹饪前.CardType == CardTypes.Liquid) {
				cr.IngredientChanges.ModifyLiquid = true;
				cr.IngredientChanges.LiquidQuantityChange = new Vector2(-300f, -300f);
				cr.IngredientChanges.ModType = CardModifications.DurabilityChanges;
			}
            if (烹饪前.CardType == CardTypes.Item) {
                cr.IngredientChanges.ModType = CardModifications.Destroy;

            }


	        CardDrop cd = new CardDrop();
			cd.DroppedCard = 烹饪后;
			cd.Quantity = new Vector2Int(1, 1);
			Array.Resize(ref cr.Drops, 1);
			cr.Drops[0] = cd;

			Traverse.Create(cr).Field("Duration").SetValue(2);

			CardData 冰箱 = utc("99ed3ea66d954a6d8d193c52a5301912");
            Array.Resize(ref 冰箱.CookingRecipes, 冰箱.CookingRecipes.Length + 1);
			冰箱.CookingRecipes[冰箱.CookingRecipes.Length - 1] = cr;
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
        
            //从火炉拷贝cooking到烹饪台
            CardData a = utc("f124e68738466c44b99b660ff303e7c1"); //火炉
            CardData b = utc("ed1faa3c968011ed8285047c16184f06");//烹饪台           
                   
            if (a && b){
                b.CookingRecipes = a.CookingRecipes;
                b.CookingConditions = a.CookingConditions;
                b.InventoryFilter = a.InventoryFilter; 
            }
			b = utc("f1311b8f6cce4bbbacac3a7ed52c9583");//烹饪台-通电
			if (a && b) {
				b.CookingRecipes = a.CookingRecipes;
				b.CookingConditions = a.CookingConditions;
				b.InventoryFilter = a.InventoryFilter;
			}

			//从高级窑炉拷贝cooking到烤制台
			a = utc("a65185c5152ecc644b503090be695008");//高级窑炉
            b = utc("21a8ed94976c11ed9a84047c16184f06");//烤制台
            CardData c = utc("429b9c74a51b11ed8c87c475ab46ec3f");//烤制台-添加
            CardData d = utc("3e72851fbb2a42a98d54dc4d47e7efbf");//烤制台-通电
			if (a && b && c && d) {
                b.CookingRecipes = a.CookingRecipes;
                b.CookingConditions = a.CookingConditions;
                b.InventoryFilter = a.InventoryFilter;
				//添加烤制台添加部分
				int x = c.CookingRecipes.Length;
				int y = b.CookingRecipes.Length;
				Array.Resize<CookingRecipe>(ref b.CookingRecipes, x + y);
				c.CookingRecipes.CopyTo(b.CookingRecipes, y);
				//拷贝烤制台到烤制台通电
				d.CookingRecipes = b.CookingRecipes;
			}

			//修正花桌取出数量
			if (card_dict.TryGetValue("Guil-科技至上_花桌", out a) && 花桌数量 != 1)
            {
                /*
                List<String> dls = new List<string>
                {
                    "Manure",
                    "Guano",
                    "RottenRemains"
                };
                */
                for (int m = 0; m <= 2; m++)
                {
                    var DroppedCards1 = Traverse.Create(a.DismantleActions[m].ProducedCards[0]).Field("DroppedCards").GetValue<CardDrop[]>();
                    DroppedCards1[0].Quantity.x = 花桌数量;
                    DroppedCards1[0].Quantity.y = 花桌数量;
                    Traverse.Create(a.DismantleActions[m].ProducedCards[0]).Field("DroppedCards").SetValue(DroppedCards1);

                    float u = (float)花桌数量 / 100f;
                    if (u <= 0) { u = 0.01f; }

                    switch (m)
                    {
                        case 0:
                            a.DismantleActions[m].RequiredReceivingDurabilities.RequiredSpecial1Percent.FloatValue = u;
                            a.DismantleActions[m].ReceivingCardChanges.Special1Change.x = -花桌数量;
                            a.DismantleActions[m].ReceivingCardChanges.Special1Change.y = -花桌数量;
                            break;
                        case 1:
                            a.DismantleActions[m].RequiredReceivingDurabilities.RequiredSpecial2Percent.FloatValue = u;
                            a.DismantleActions[m].ReceivingCardChanges.Special2Change.x = -花桌数量;
                            a.DismantleActions[m].ReceivingCardChanges.Special2Change.y = -花桌数量;
                            break;
                        case 2:
                            a.DismantleActions[m].RequiredReceivingDurabilities.RequiredSpecial3Percent.FloatValue = u;
                            a.DismantleActions[m].ReceivingCardChanges.Special3Change.x = -花桌数量;
                            a.DismantleActions[m].ReceivingCardChanges.Special3Change.y = -花桌数量;
                            break;
                        default:
                            break;
                    }
                }
			}


            foreach (KeyValuePair<string, CardData> kvp in card_dict) {
                CardData card = kvp.Value;
                //烤制台蓝图修正
                if (card && card.CardType == CardTypes.Blueprint)
                {
                    if (card.CardsOnBoard.Count > 0)
                    {
                        for (int m = 0; m < card.CardsOnBoard.Count; m++)
                        {
                            CardData card2 = card.CardsOnBoard[m].Card;
                            if (card2 == null)
                            {
                                Debug.Log("no card2:" + card.name);
                                break;
                            }
                            if (card2.UniqueID == "d7d2831f33ccf184e9b09f8411339948")//Kiln 窑炉
                            {
                                CardOnBoardSubObjective guil1 = new CardOnBoardSubObjective();
                                guil1.Card = utc("21a8ed94976c11ed9a84047c16184f06");
                                guil1.CompletionWeight = 1;
                                guil1.ObjectiveName = card.CardsOnBoard[m].ObjectiveName;
                                guil1.Quantity = 1;
                                card.CardsOnBoard.Add(guil1);
                                break;
                            }
                            if (card2.UniqueID == "a65185c5152ecc644b503090be695008")//KilnAdvanced 高级窑炉
                            {
                                CardOnBoardSubObjective guil1 = new CardOnBoardSubObjective();
                                guil1.Card = utc("21a8ed94976c11ed9a84047c16184f06");
                                guil1.CompletionWeight = 1;
                                guil1.ObjectiveName = card.CardsOnBoard[m].ObjectiveName;
                                guil1.Quantity = 1;
                                card.CardsOnBoard.Add(guil1);
                                break;
                            }
                        }
                    }
                }
                //遍历找出液体，然后放入包装
                if (card && card.CardType == CardTypes.Liquid) {
                    找出液体s.Add(card);
                }
				//添加拆除确认
                if (card.CardType == CardTypes.Location && card.name.StartsWith("Guil-科技至上_")) {
                    if (card.DismantleActions.Count > 0) {
                        foreach (DismantleCardAction action in card.DismantleActions) {
                            //tear down
                            //  "拆除" == "T-5Vbc1DoG2wO5Cu6T4mTWzns33Yg="
                            // Note - filtered above with StartsWith("Guil-科技至上_") check.
                            if (action.ActionName.LocalizationKey == "T-5Vbc1DoG2wO5Cu6T4mTWzns33Yg=")
				            {
                                action.ConfirmPopup = true;
                                //电力返还
                                if (card.name.EndsWith("-通电")) {
                                    //找接入电网时消耗的电
                                    float dl = 消耗电力(card);
                                    if (dl > 0) {
                                        StatModifier DLFH = new StatModifier();
                                        DLFH.Stat = UniqueIDScriptable.GetFromID<GameStat>("15b71d9593ec11edbce7047c16184f06");
                                        DLFH.ValueModifier.x = dl; DLFH.ValueModifier.y = dl;

                                        Array.Resize(ref action.StatModifications, action.StatModifications.Length + 1);
                                        action.StatModifications[action.StatModifications.Length - 1] = DLFH;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //保鲜罐 液体改良
            CardData 液体模板 = UniqueIDScriptable.GetFromID<CardData>("01742fd8a0d04d0082ae05ca0d969ba6");
            for (int i = 0; i < 找出液体s.Count; i++){
                CardData yeti = 找出液体s[i];
                //Debug.Log(yeti.CardName);
                Array.Resize(ref yeti.PassiveEffects, yeti.PassiveEffects.Length + 1);
                yeti.PassiveEffects[yeti.PassiveEffects.Length - 1] = 液体模板.PassiveEffects[0];
            }
            //Make ice
            制冰("425259cb06b869d45be2e7f1b5b54aff", "b1d69e1b95fd4244b66b1c89dd59f65b");
		    制冰("5481d599322f41d3b88249442ec4e8c0", "d196f43d14014712a42d371145d586d5");
		    制冰("eb1c2d24e3a74870af79ba7fd8ba2868", "7ef0c448b98d4466945cfa4ba5cf7e69");
	    }
    }
}
