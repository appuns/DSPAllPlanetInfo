using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPAllPlanetInfo
{

    [BepInPlugin("Appun.DSP.plugin.AllPlanetInfo", "DSPAllPlanetInfo", "1.1.2")]
    [BepInProcess("DSPGAME.exe")]



    public class Main : BaseUnityPlugin
    {
        public static ConfigEntry<bool> RequireUniverseExploration4;
        //public static ConfigEntry<bool> UseUniverseExploration5;
        public static ConfigEntry<bool> ShowAdditionalInformationWindow;

        public static StringBuilder SB;



        public static bool PlanetWindowOpen;


        public static PlanetData prePlanet;

        public static int tmpUniverseObserveLevel;
        private Sprite UseUniverseExploration5Icon;

        Vector2 scrollPosition = Vector2.zero;

        public static int fontSize = 20;
        public static int fixedHeight = 30;



        public static int lineMax;
        public static int pageNo;
        public static int lastStationNo;
        public static int[] startStationNo = new int[100];

        public static bool needRefresh = false;


        //string[] selStrings = { "Station", "Factories" };// ,"Miner", };



        public class TransportData
        {
            public string PlanetName;
            public int PlanetId;
            public string StationName;
            public int StationId;
            public int ItemId;
            public int ItemCount;
            public int ItemMax;
            public ELogisticStorage Logic;

        }

        //public static List<string, string> JPDictionary { get; set; }
        public static List<TransportData> transportdata = new List<TransportData>();

        //public TransportData[] td = new TransportData[100];

        public void Start()
        {
            LogManager.Logger = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            //configの設定
            RequireUniverseExploration4 = Config.Bind("Tech", "RequireUniverseExplorationLevel4", false, "Require Universe exploration Level 4");
            ShowAdditionalInformationWindow = Config.Bind("UI", "ShowAdditionalInformationWindow", true, "Show planetary logistics information");




            LoadIcon();
            //if (DependOnObserveLevel.Value == false)
            //{
            //     GameMain.history.universeObserveLevel = 4;
            // }-mbly();

            //if (UseUniverseExploration5.Value)
            //{
            //    LDBTool.PostAddDataAction += AddTech;
            //}

        }




        //アイコンのロード
        private void LoadIcon()
        {
            try
            {
                var assetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("DSPAllPlanetInfo.icon"));
                if (assetBundle == null)
                {
                    LogManager.Logger.LogInfo("asset bundle not loaded.");
                }
                else
                {
                    UseUniverseExploration5Icon = assetBundle.LoadAsset<Sprite>("4105");
                }

            }
            catch (Exception e)
            {
                LogManager.Logger.LogInfo("e.Message " + e.Message);
                LogManager.Logger.LogInfo("e.StackTrace " + e.StackTrace);
            }
        }

        //public static bool CheckStatus()
        //{

        //    if (RequireUniverseExploration4.Value == false)
        //    {
        //        return true;
        //    }
        //    //if (!UseUniverseExploration5.Value && GameMain.history.universeObserveLevel == 4)
        //    //{
        //    //    return true;
        //    //}
        //    //if (UseUniverseExploration5.Value && GameMain.data.history.techStates[4105].unlocked)
        //    //{
        //    //return true;
        //    //}
        //    return false;
        //}











        //惑星情報の表示間隔の調整　30フレーム→100
        [HarmonyPatch(typeof(UIPlanetDetail), "_OnUpdate")]
        class Transpiler_replace_30to100
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                if (codes[1].opcode == OpCodes.Ldc_I4_S)
                {
                    codes[1].operand = 100;
                }
                return codes.AsEnumerable();

            }
        }


        //追加の惑星情報を表示
        //[HarmonyPatch(typeof(UIPlanetDetail), "RefreshDynamicProperties")]
        public static class UIPlanetDetail_RefreshDynamicProperties_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix(UIPlanetDetail __instance)
            {
                //List<TransportData> transportdata = new List<TransportData>();

                //if (CheckStatus())
                //{


                    if (__instance.planet.data == null)
                    {
                        var algorithm = PlanetModelingManager.Algorithm(__instance.planet);
                        __instance.planet.data = new PlanetRawData(__instance.planet.precision);
                        __instance.planet.modData = __instance.planet.data.InitModData(__instance.planet.modData);
                        __instance.planet.data.CalcVerts();
                        __instance.planet.aux = new PlanetAuxData(__instance.planet);
                        algorithm.GenerateTerrain(__instance.planet.mod_x, __instance.planet.mod_y);
                        algorithm.CalcWaterPercent();
                        if (__instance.planet.factory == null)
                        {
                            if (__instance.planet.type != EPlanetType.Gas)
                            {
                                algorithm.GenerateVegetables();
                                algorithm.GenerateVeins(false);
                            }
                        }
                        if (__instance.planet.landPercentDirty)
                        {
                            PlanetAlgorithm.CalcLandPercent(__instance.planet);
                            __instance.planet.landPercentDirty = false;
                        }
                    }
                    if (__instance.planet != null)
                    {

                        if (true) // __instance.planet.loaded)
                        {
                            int i = 0;
                            foreach (UIResAmountEntry uiresAmountEntry in __instance.entries)
                            {
                                ref Text labelText = ref AccessTools.FieldRefAccess<UIResAmountEntry, Text>(uiresAmountEntry, "labelText");

                                if (uiresAmountEntry.refId > 0)
                                {
                                    if (uiresAmountEntry.refId == 7)
                                    {
                                        double num2 = (double)__instance.planet.veinAmounts[uiresAmountEntry.refId] * (double)VeinData.oilSpeedMultiplier;
                                        StringBuilderUtility.WritePositiveFloat(uiresAmountEntry.sb, 0, 8, (float)num2, 2, ' ');
                                        //VeinProto veinProto = LDB.veins.Select(uiresAmountEntry.refId);
                                        //ItemProto itemProto = LDB.items.Select(veinProto.MiningItem);
                                        uiresAmountEntry.valueString = uiresAmountEntry.sb.ToString();
                                    }
                                    else
                                    {
                                        long num3 = __instance.planet.veinAmounts[uiresAmountEntry.refId];
                                        if (num3 < 1000000000L)
                                        {
                                            StringBuilderUtility.WriteCommaULong(uiresAmountEntry.sb, 0, 16, (ulong)num3, 1, ' ');
                                        }
                                        else
                                        {
                                            StringBuilderUtility.WriteKMG(uiresAmountEntry.sb, 15, num3, false);
                                        }
                                        //VeinProto veinProto = LDB.veins.Select(uiresAmountEntry.refId);
                                        //ItemProto itemProto = LDB.items.Select(veinProto.MiningItem);
                                        uiresAmountEntry.valueString = uiresAmountEntry.sb.ToString();
                                    }
                                    //uiresAmountEntry.valueString = uiresAmountEntry.refId.ToString();
                                }
                                else
                                {
                                    if (labelText.text.Contains("适建区域".Translate()))
                                    {
                                        StringBuilderUtility.WritePositiveFloat(uiresAmountEntry.sb, 0, 5, __instance.planet.landPercent * 100f, 1, ' ');
                                        uiresAmountEntry.valueString = uiresAmountEntry.sb.ToString();
                                    }
                                }
                                i += 1;
                            }

                        }
                    }
                    //LogManager.Logger.LogInfo("station info");

                    int stationNo = 0;
                    int lineNo = 0;
                    UI.nextButton.SetActive(false);
                    if (pageNo > 0)
                    {
                        UI.previousButton.SetActive(true);
                    }
                    else
                    {
                        UI.previousButton.SetActive(false);
                    }

                    //LogManager.Logger.LogInfo("startStationNo[" + pageNo + "] : " + startStationNo[pageNo]);


                    //星間物流情報を表示
                    if (UIRoot.instance.uiGame.planetDetail.planet.factory != null)
                    {
                        var planetFactory = UIRoot.instance.uiGame.planetDetail.planet.factory;

                        if (planetFactory.transport.stationCursor > 0)
                            for (int i = startStationNo[pageNo]; i < planetFactory.transport.stationCursor; i++)
                            {

                                if (planetFactory.transport.stationPool[i] != null && planetFactory.transport.stationPool[i].isStellar)
                                {
                                    //行があふれるとき
                                    if ((lineNo + planetFactory.transport.stationPool[i].storage.Length) > lineMax)
                                    {

                                    UI.nextButton.SetActive(true);
                                        lastStationNo = i;
                                        break;
                                        //LogManager.Logger.LogInfo("previousButton != null 2");

                                    }

                                //ステーション変わったのでラインを表示
                                UI.Line[stationNo].transform.localPosition = new Vector3(-125, (float)(-70 - lineNo * 20), 0);
                                UI.Line[stationNo].SetActive(true);
                                //文字列・アイコンは非表示
                                UI.ItemName[lineNo].SetActive(false);
                                UI.ItemIcon[lineNo].SetActive(false);
                                UI.ItemLogic[lineNo].SetActive(false);
                                UI.ItemCount[lineNo].SetActive(false);
                                    stationNo++;
                                    lineNo += 1;

                                    //ストレージの内容を表示
                                    for (int j = 0; j < planetFactory.transport.stationPool[i].storage.Length; j++)
                                    {

                                        if (planetFactory.transport.stationPool[i].storage[j].itemId != 0)
                                        {
                                        //アイテム名
                                        UI.ItemName[lineNo].GetComponent<Text>().text = LDB.items.Select(planetFactory.transport.stationPool[i].storage[j].itemId).name;
                                        UI.ItemName[lineNo].SetActive(true);

                                        //アイテムアイコン
                                        UI.ItemIcon[lineNo].GetComponent<Image>().sprite = LDB.items.Select(planetFactory.transport.stationPool[i].storage[j].itemId).iconSprite;
                                        UI.ItemIcon[lineNo].SetActive(true);

                                        //アイテム数
                                        UI.ItemCount[lineNo].GetComponent<Text>().text = String.Format("{0:#,0}", planetFactory.transport.stationPool[i].storage[j].count);
                                        UI.ItemCount[lineNo].SetActive(true);


                                            //ロジック
                                            if (planetFactory.transport.stationPool[i].storage[j].remoteLogic == ELogisticStorage.Demand)
                                            {
                                            UI.ItemLogic[lineNo].GetComponent<Text>().text = "需求".Translate();
                                            UI.ItemLogic[lineNo].GetComponent<Text>().color = new Color(0.88f, 0.55f, 0.36f, 0.5f);

                                            }
                                            else if (planetFactory.transport.stationPool[i].storage[j].remoteLogic == ELogisticStorage.Supply)
                                            {
                                            UI.ItemLogic[lineNo].GetComponent<Text>().text = "供应".Translate();
                                            UI.ItemLogic[lineNo].GetComponent<Text>().color = new Color(0.24f, 0.55f, 0.65f, 0.5f);
                                            }
                                            else
                                            {
                                            UI.ItemLogic[lineNo].GetComponent<Text>().text = "仓储".Translate();
                                            UI.ItemLogic[lineNo].GetComponent<Text>().color = new Color(1, 1, 1, 0.3f);
                                            }
                                        UI.ItemLogic[lineNo].SetActive(true);

                                            lineNo++;
                                            //LogManager.Logger.LogInfo(LDB.items.Select(planetFactory.transport.stationPool[i].storage[j].itemId).name + " : " + planetFactory.transport.stationPool[i].storage[j].count.ToString());

                                        }
                                        if (lineNo == lineMax)
                                        {
                                            break;

                                        }
                                    }

                                }
                                //LogManager.Logger.LogInfo("ctivate : " + stationNo);
                            }


                    }
                    else
                    {
                    //最初のライン
                    UI.Line[0].transform.localPosition = new Vector3(-125, (float)(-70 - lineNo * 20), 0);
                    UI.Line[0].SetActive(true);
                        stationNo++;
                        lineNo++;

                    UI.ItemName[1].GetComponent<Text>().text = "No Interstellar Logistics Station".Translate();
                    UI.ItemIcon[1].SetActive(false);
                    UI.ItemCount[1].SetActive(false);
                    UI.ItemLogic[1].SetActive(false);


                        lineNo++;


                    }
                //ステーション変わったのでラインを表示
                UI.Line[stationNo].transform.localPosition = new Vector3(-125, (float)(-70 - lineNo * 20), 0);
                UI.Line[stationNo].SetActive(true);
                    stationNo++;


                    //残りのオブジェクトは非表示
                    for (int j = lineNo; j < 108; j++)
                    {
                    UI.ItemName[j].SetActive(false);
                    UI.ItemIcon[j].SetActive(false);
                    UI.ItemCount[j].SetActive(false);
                    UI.ItemLogic[j].SetActive(false);
                    }
                    for (int j = stationNo; j < 50; j++)
                    {
                    UI.Line[j].SetActive(false);
                    }

                //}

            }


        }



        //スターマップを閉じるとき、universeObserveLevel
        [HarmonyPatch(typeof(UIStarmap), "_OnClose")]
        public static class UIStarmap_OnClose_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                 //LogManager.Logger.LogInfo("universeObserveLevel : " + GameMain.history.universeObserveLevel + " => " + tmpUniverseObserveLevel);
                if (GameMain.data.history.techStates[4104].unlocked)
                {
                    GameMain.history.universeObserveLevel = 4;
                }
                else if(GameMain.data.history.techStates[4103].unlocked)
                {
                    GameMain.history.universeObserveLevel = 3;
                }
                else if (GameMain.data.history.techStates[4102].unlocked)
                {
                    GameMain.history.universeObserveLevel = 2;
                }
                else if (GameMain.data.history.techStates[4101].unlocked)
                {
                    GameMain.history.universeObserveLevel = 1;
                }


            }
        }











        private void GUIFactoriesInfo(int windowId)
        {
            ref Text typeText = ref AccessTools.FieldRefAccess<UIPlanetDetail, Text>(UIRoot.instance.uiGame.planetDetail, "typeText");

            int minerCount = 0;
            int assemblerCount = 0;
            int ejectorCount = 0;
            int inserterCount = 0;
            int siloCount = 0;
            int labCount = 0;


            //アイテムアイコンスタイル
            var iconLabelStyle = new GUIStyle(GUI.skin.label);
            iconLabelStyle.fontSize = fontSize;
            iconLabelStyle.font = typeText.font;
            iconLabelStyle.fixedWidth = 40;
            iconLabelStyle.fixedHeight = fixedHeight;
            iconLabelStyle.stretchWidth = false;
            iconLabelStyle.stretchHeight = false;
            iconLabelStyle.alignment = TextAnchor.MiddleCenter;

            //アイテム名スタイル
            var nameLabelStyle = new GUIStyle(GUI.skin.label);
            nameLabelStyle.fontSize = fontSize;
            nameLabelStyle.font = typeText.font;
            //nameLabelStyle.fixedWidth = 100;
            nameLabelStyle.stretchWidth = false;
            nameLabelStyle.stretchHeight = false;
            nameLabelStyle.alignment = TextAnchor.MiddleLeft;

            //アイテム数スタイル
            var numLabelStyle = new GUIStyle(GUI.skin.label);
            numLabelStyle.fontSize = fontSize;
            numLabelStyle.font = typeText.font;
            //nameLabelStyle.fixedWidth = 100;
            numLabelStyle.stretchWidth = false;
            numLabelStyle.stretchHeight = false;
            numLabelStyle.alignment = TextAnchor.MiddleRight;

            var boxStyle = new GUIStyle(GUI.skin.box);

            Texture2D _texture = new Texture2D(1, 1);
            boxStyle.normal.background = _texture;
            Color color = new Color(0.1f, 0.1f, 0.1f, 1);
            _texture.SetPixel(1, 1, color);
            _texture.Apply();



            if (UIRoot.instance.uiGame.planetDetail.planet.factory.gameData.factories != null)
            {
                foreach (var planetFactory in UIRoot.instance.uiGame.planetDetail.planet.factory.gameData.factories)
                {
                    if (planetFactory != null)
                    {
                        {
                            if (UIRoot.instance.uiGame.planetDetail.planet != null)
                            {
                                if (planetFactory.planet == UIRoot.instance.uiGame.planetDetail.planet)
                                {
                                    for (int i = 1; i < planetFactory.factorySystem.minerCursor; i++)
                                    {
                                        if (planetFactory.factorySystem.minerPool[i].id > 0)
                                        {
                                            minerCount++;
                                        }
                                    }
                                    for (int i = 1; i < planetFactory.factorySystem.assemblerCursor; i++)
                                    {
                                        if (planetFactory.factorySystem.assemblerPool[i].id > 0)
                                        {
                                            assemblerCount++;
                                        }
                                    }
                                    for (int i = 1; i < planetFactory.factorySystem.ejectorCursor; i++)
                                    {
                                        if (planetFactory.factorySystem.ejectorPool[i].id > 0)
                                        {
                                            ejectorCount++;
                                        }
                                    }
                                    for (int i = 1; i < planetFactory.factorySystem.inserterCursor; i++)
                                    {
                                        if (planetFactory.factorySystem.inserterPool[i].id > 0)
                                        {
                                            inserterCount++;
                                        }
                                    }
                                    for (int i = 1; i < planetFactory.factorySystem.siloCursor; i++)
                                    {
                                        if (planetFactory.factorySystem.siloPool[i].id > 0)
                                        {
                                            siloCount++;
                                        }
                                    }
                                    for (int i = 1; i < planetFactory.factorySystem.labCursor; i++)
                                    {
                                        if (planetFactory.factorySystem.labPool[i].id > 0)
                                        {
                                            labCount++;
                                        }
                                    }


                                    GUILayout.BeginVertical(boxStyle);

                                    GUILayout.Label("minerCount: " + minerCount);
                                    GUILayout.Label("assemblerCount: " + assemblerCount);
                                    GUILayout.Label("ejectorCount: " + ejectorCount);
                                    GUILayout.Label("inserterCount: " + inserterCount);
                                    GUILayout.Label("siloCount: " + siloCount);
                                    GUILayout.Label("labCount: " + labCount);

                                    GUILayout.EndVertical();



                                    //                                          LogManager.Logger.LogInfo("planetFactory.factorySystem.minerCursor: " + planetFactory.factorySystem..minerCursor);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerPool[i].id: " + planetFactory.factorySystem.minerPool[i].id);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerPool[i].entityId: " + planetFactory.factorySystem.minerPool[i].entityId);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerPool[i].pcId: " + planetFactory.factorySystem.minerPool[i].pcId);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerPool[i].productId: " + planetFactory.factorySystem.minerPool[i].productId);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerPool[i].productCount: " + planetFactory.factorySystem.minerPool[i].productCount);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerPool[i].productCount: " + planetFactory.factorySystem.minerPool[i].productCount);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerPool[i].productId: " + planetFactory.planet..factorySystem.minerPool[i].DetermineState);



                                    //{
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerPool[1].entityId : " + planetFactory.factorySystem.minerPool[1].id);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerPool[2].entityId : " + planetFactory.factorySystem.minerPool[2].id);
                                    //LogManager.Logger.LogInfo("planetFactory.entityNeeds : " + planetFactory.entityNeeds);
                                    //LogManager.Logger.LogInfo("planetFactory.entityNeeds.Length : " + planetFactory.entityNeeds.Length);
                                    //LogManager.Logger.LogInfo("planetFactory.entitySignPool : " + planetFactory.entitySignPool);
                                    //LogManager.Logger.LogInfo("planetFactory.entitySignPool.Length : " + planetFactory.entitySignPool.Length);

                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.assemblerCursor : " + planetFactory.factorySystem.assemblerCursor);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.ejectorCursor : " + planetFactory.factorySystem.ejectorCursor);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.inserterPool.Length : " + planetFactory.factorySystem.inserterPool.Length);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.fractionatePool.Length : " + planetFactory.factorySystem.fractionatePool.Length);
                                    //LogManager.Logger.LogInfo("planetFactory.factorySystem.minerCursor : " + planetFactory.factorySystem.minerCursor);

                                }
                            }

                        }
                    }

                }

            }


        }




        //マウス貫通対策　たぬさんサンクス！
        private bool ResetInput(Rect rect)
        {
            var left = rect.xMin * UnityEngine.GUI.matrix.lossyScale.x;
            var right = rect.xMax * UnityEngine.GUI.matrix.lossyScale.x;
            var top = rect.yMin * UnityEngine.GUI.matrix.lossyScale.x;
            var bottom = rect.yMax * UnityEngine.GUI.matrix.lossyScale.x;
            var inputX = Input.mousePosition.x;
            var inputY = Screen.height - Input.mousePosition.y;
            if (left <= inputX && inputX <= right && top <= inputY && inputY <= bottom)
            {
                int[] zot = { 0, 1, 2 };
                if (zot.Any(Input.GetMouseButton) || Input.mouseScrollDelta.y != 0)
                {
                    Input.ResetInputAxes();
                    return true;
                }
            }
            return false;
        }


    }


    public class LogManager
    {
        public static ManualLogSource Logger;
    }

}