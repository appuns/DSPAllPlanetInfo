using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using static UnityEngine.GUILayout;
using UnityEngine.Rendering;
using Steamworks;
using rail;
using xiaoye97;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPAllPlanetInfo
{

    [BepInPlugin("Appun.DSP.plugin.AllPlanetInfo", "DSPAllPlanetInfo", "1.1.2")]
    [BepInProcess("DSPGAME.exe")]



    public class DSPAllPlanetInfo : BaseUnityPlugin
    {
        public static ConfigEntry<bool> RequireUniverseExploration4;
        //public static ConfigEntry<bool> UseUniverseExploration5;
        public static ConfigEntry<bool> ShowAdditionalInformationWindow;

        public static StringBuilder SB;


        public static int windowWidth = 550;
        public static Rect windowRectNemu = new Rect(0, 50, 400, 0);
        public static Rect windowRectStation = new Rect(0, 120, windowWidth, 100);
        public static Rect windowRectMiner = new Rect(0, 120, windowWidth, 0);
        public static Rect windowRectFactories = new Rect(0, 120, windowWidth, 0);

        public static bool PlanetWindowOpen;

        public static bool MiniLabPanelOn;

        public static PlanetData prePlanet;

        public static int tmpUniverseObserveLevel;
        private Sprite UseUniverseExploration5Icon;

        Vector2 scrollPosition = Vector2.zero;

        public static int fontSize = 20;
        public static int fixedHeight = 30;

        public static GameObject infoWindow;
        public static RectTransform rectInfoWindow;


        //public static GameObject planetdetailwindow;

        public static GameObject[] ItemName = new GameObject[108];
        public static GameObject[] ItemIcon = new GameObject[108];
        public static GameObject[] ItemCount = new GameObject[108];
        public static GameObject[] ItemLogic = new GameObject[108];
        public static GameObject[] Line = new GameObject[50];
        public static GameObject WindowTitle;
        public static GameObject page;
        public static GameObject previousButton;
        public static GameObject nextButton;

        public static Button previousButtonButton;
        public static Button nextButtonButton;


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
            //UseUniverseExploration5 = Config.Bind("Tech", "UseUniverseExplorationLevel5", false, "Use and require Universe Exploration Level 5");
            ShowAdditionalInformationWindow = Config.Bind("UI", "ShowAdditionalInformationWindow", true, "Show planetary logistics information");

            //UI解像度計算
            int UIheight = DSPGame.globalOption.uiLayoutHeight;
            int UIwidth = UIheight * Screen.width / Screen.height;

            //情報表示用のウインドウを作成
            GameObject planetdetailwindow = GameObject.Find("UI Root/Overlay Canvas/In Game/Planet & Star Details/planet-detail-ui");
            infoWindow = Instantiate(planetdetailwindow) as GameObject;
            infoWindow.name = "infoWindow";
            infoWindow.transform.SetParent(planetdetailwindow.transform.parent, true);
            infoWindow.transform.localPosition = new Vector3(260 - UIwidth, 150, 0);
            infoWindow.transform.localScale = planetdetailwindow.transform.localScale;

            rectInfoWindow = infoWindow.GetComponent<RectTransform>();
            rectInfoWindow.sizeDelta = new Vector2(rectInfoWindow.sizeDelta.x, UIheight);


            Destroy(infoWindow.transform.Find("res-group/res-entry/icon").GetComponentInChildren<UIButton>());

            //ボタンの追加
            previousButton = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Assembler Window/produce/copy-button"), infoWindow.transform) as GameObject;
            previousButton.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(100, 20);
            previousButton.GetComponentInChildren<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            previousButton.transform.localPosition = new Vector3(-125, -40, 0);
            previousButton.GetComponentInChildren<Text>().text = "Previous Page".Translate();
            Destroy(previousButton.transform.Find("Text").GetComponentInChildren<Localizer>());
            Destroy(previousButton.GetComponentInChildren<UIButton>());
            previousButton.name = "previousButton".Translate();
            previousButton.SetActive(true);
            previousButtonButton = previousButton.GetComponentInChildren<Button>();

            nextButton = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Assembler Window/produce/copy-button"), infoWindow.transform) as GameObject;
            nextButton.GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(100, 20);
            nextButton.GetComponentInChildren<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            nextButton.transform.localPosition = new Vector3(-125, 40 - UIheight, 0);
            nextButton.GetComponentInChildren<Text>().text = "Next Page".Translate();
            Destroy(nextButton.transform.Find("Text").GetComponentInChildren<Localizer>());
            Destroy(nextButton.GetComponentInChildren<UIButton>());
            nextButton.name = "nextButton".Translate();
            nextButton.SetActive(true);
            nextButtonButton = nextButton.GetComponentInChildren<Button>();

            //ウインドウのタイトル追加
            WindowTitle = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
            WindowTitle.transform.localPosition = new Vector3(-200, -5, 0);
            WindowTitle.GetComponentInChildren<Text>().text = "Interstellar Logistics".Translate();
            WindowTitle.GetComponentInChildren<Text>().verticalOverflow = VerticalWrapMode.Overflow;
            WindowTitle.GetComponentInChildren<Text>().resizeTextMaxSize = 20;
            //WindowTitle.GetComponentInChildren<Text>().fontSize = 23;
            WindowTitle.SetActive(true);

            //ページ
            //page = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
            //page.transform.localPosition = new Vector3(-20, -40, 0);

            for (int j = 0; j < 108; j++)
            {
                infoWindow.transform.Find("bg").gameObject.SetActive(false);
                infoWindow.transform.Find("res-group").gameObject.SetActive(false);

                //アイテム名
                ItemProto itemProto = LDB.items.Select(1001);
                ItemName[j] = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
                ItemName[j].name = "ItemName " + j;
                ItemName[j].transform.localPosition = new Vector3(-245, (float)(-60 - j * 20), 0);
                ItemName[j].GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(150, 20);
                //ItemName[j].GetComponentInChildren<Text>().text = "";
                ItemName[j].GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;
                ItemName[j].SetActive(false);

                //アイテムアイコン
                ItemIcon[j] = Instantiate(infoWindow.transform.Find("res-group/res-entry/icon").gameObject, infoWindow.transform) as GameObject;
                ItemIcon[j].name = "ItemIcon " + j;
                ItemIcon[j].transform.localPosition = new Vector3(-85, (float)(-60 - j * 20), 0);
                //ItemIcon[j].GetComponentInChildren<Image>().sprite = null;
                ItemIcon[j].GetComponentInChildren<Image>().enabled = true;
                ItemIcon[j].SetActive(false);
                Destroy(ItemIcon[j].GetComponentInChildren<UIButton>());


                //アイテム数
                ItemCount[j] = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
                ItemCount[j].name = "ItemCount " + j;
                ItemCount[j].transform.localPosition = new Vector3(-80, (float)(-60 - j * 20), 0);
                ItemCount[j].GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(40, 20);
                //ItemCount[j].GetComponentInChildren<Text>().text = "";
                ItemCount[j].GetComponentInChildren<Text>().alignment = TextAnchor.MiddleRight;
                ItemCount[j].SetActive(false);

                //ロジック情報
                ItemLogic[j] = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
                ItemLogic[j].name = "ItemLogic " + j;
                ItemLogic[j].transform.localPosition = new Vector3(-30, (float)(-60 - j * 20), 0);
                ItemLogic[j].GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(40, 20);
                //ItemLogic[j].GetComponentInChildren<Text>().text = "";
                ItemLogic[j].GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;
                ItemLogic[j].SetActive(false);


            }
            for (int j = 0; j < 50; j++)
            {
                Line[j] = Instantiate(infoWindow.transform.Find("res-group/line").gameObject, infoWindow.transform) as GameObject;
                Line[j].SetActive(false);


            }

            //不要なオブジェクトの削除
            //Destroy(StationWindow.transform.Find("icon").gameObject);
            Destroy(infoWindow.transform.Find("name-input").gameObject);
            Destroy(infoWindow.transform.Find("type-text").gameObject);
            Destroy(infoWindow.transform.Find("param-group").gameObject);

            //ボタンクリックイベントの追加
            previousButtonButton.onClick.AddListener(OnClickPreviousButton);
            nextButtonButton.onClick.AddListener(OnClickNextButton);



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

        //ボタンイベント
        public void OnClickPreviousButton()
        {
            pageNo--;
            //LogManager.Logger.LogInfo("pageNo--");
            UIRoot.instance.uiGame.planetDetail.RefreshDynamicProperties();
            //needRefresh = true;
        }

        public void OnClickNextButton()
        {
            pageNo++;
            //LogManager.Logger.LogInfo("pageNo++");
            startStationNo[pageNo] = lastStationNo;
            //LogManager.Logger.LogInfo("lastStationNo = " +lastStationNo);
            UIRoot.instance.uiGame.planetDetail.RefreshDynamicProperties();
            //needRefresh = true;
        }


        //オプション変更後の情報ウインドウ位置修正
        [HarmonyPatch(typeof(UIOptionWindow), "ApplyOptions")]
        public static class UIOptionWindowa_ApplyOptions_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix(UIOptionWindow __instance)
            {
                //UI解像度計算
                int UIheight = DSPGame.globalOption.uiLayoutHeight;
                int UIwidth = UIheight * Screen.width / Screen.height;
                //位置とサイズの調整
                infoWindow.transform.localPosition = new Vector3(260 - UIwidth, 150, 0);
                rectInfoWindow.sizeDelta = new Vector2(rectInfoWindow.sizeDelta.x, UIheight);
                previousButton.transform.localPosition = new Vector3(-125, -40, 0);
                nextButton.transform.localPosition = new Vector3(-125, 40 - UIheight, 0);

            }
        }

        //UIplanetwindowのopenと同期
        [HarmonyPatch(typeof(UIPlanetDetail), "_OnOpen")]
        public static class UIPlanetDetail_OnOpen_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (ShowAdditionalInformationWindow.Value)
                {
                    if (GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Mini Lab Panel").activeSelf == true)
                    {
                        GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Mini Lab Panel").SetActive(false);
                        MiniLabPanelOn = true;
                    }
                    else
                    {
                        MiniLabPanelOn = false;
                    }
                    //最大行数の設定
                    infoWindow.SetActive(true);
                    lineMax = (DSPGame.globalOption.uiLayoutHeight / 20) - 5;
                    //LogManager.Logger.LogInfo("lineMax : " + lineMax);
                    //アイコンの変更
                    infoWindow.transform.Find("icon").GetComponentInChildren<Image>().sprite = LDB.techs.Select(1605).iconSprite;

                }
            }
        }

        //UIplanetwindowのcloseと同期
        [HarmonyPatch(typeof(UIPlanetDetail), "_OnClose")]
        public static class UIPlanetDetail_OnClose_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                infoWindow.SetActive(false);
                if (MiniLabPanelOn)
                {
                    GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Mini Lab Panel").SetActive(true);
                }
            }
        }


        [HarmonyPatch(typeof(UIPlanetDetail), "_OnOpen")]
        public static class UIPlanetDetail_OnCreate_PostPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                pageNo = 0;
                startStationNo[0] = 1;
                //LogManager.Logger.LogInfo("pageNo = 0" );
            }

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

        public static bool CheckStatus()
        {

            if (RequireUniverseExploration4.Value == false)
            {
                return true;
            }
            //if (!UseUniverseExploration5.Value && GameMain.history.universeObserveLevel == 4)
            //{
            //    return true;
            //}
            //if (UseUniverseExploration5.Value && GameMain.data.history.techStates[4105].unlocked)
            //{
            //return true;
            //}
            return false;
        }




        //星系情報
        [HarmonyPatch(typeof(UIStarDetail), "RefreshDynamicProperties")]
        public static class UIStarDetail_RefreshDynamicProperties_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix(UIStarDetail __instance)
            {
                if (CheckStatus())
                {
                    int startNum = 0;

                    //惑星情報を作成
                    for (int i = 0; i < __instance.star.planetCount; i++)
                    {
                        PlanetData planetData = __instance.star.planets[i];
                        if (planetData.data == null)
                        {
                            var algorithm = PlanetModelingManager.Algorithm(planetData);
                            planetData.data = new PlanetRawData(planetData.precision);
                            planetData.modData = planetData.data.InitModData(planetData.modData);
                            planetData.data.CalcVerts();
                            planetData.aux = new PlanetAuxData(planetData);
                            algorithm.GenerateTerrain(planetData.mod_x, planetData.mod_y);
                            algorithm.CalcWaterPercent();
                            if (planetData.factory == null)
                            {
                                if (planetData.type != EPlanetType.Gas)
                                {
                                    algorithm.GenerateVegetables();
                                    algorithm.GenerateVeins(false);
                                }
                            }
                            if (planetData.landPercentDirty)
                            {
                                PlanetAlgorithm.CalcLandPercent(planetData);
                                planetData.landPercentDirty = false;
                            }
                        }
                        if (planetData.type == EPlanetType.Gas && planetData.gasItems != null)
                        {
                            for (int m = 0; m < planetData.gasItems.Length; m++)
                            {
                                ItemProto itemProto3 = LDB.items.Select(planetData.gasItems[m]);

                                //LogManager.Logger.LogInfo(itemProto3.Name + " : " + planetData.gasSpeeds[m]);

                                //ガス惑星情報を上書き
                                for (int n = startNum; i < __instance.entries.Count; n++)
                                {
                                    ref Text labelText = ref AccessTools.FieldRefAccess<UIResAmountEntry, Text>(__instance.entries[n], "labelText");
                                    if (labelText.text.Contains(itemProto3.Name.Translate()) && __instance.entries[n].refId == 0)
                                    {
                                        StringBuilderUtility.WritePositiveFloat(__instance.entries[n].sb, 0, 7, planetData.gasSpeeds[m], 2, ' ');
                                        __instance.entries[n].valueString = __instance.entries[n].sb.ToString();
                                        //LogManager.Logger.LogInfo(n + " : " + itemProto3.Name.Translate() + " : " + __instance.entries[n].sb.ToString());
                                        startNum = n + 1;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    //表示行ごとに数値を上書き
                    foreach (UIResAmountEntry uiresAmountEntry in __instance.entries)
                    {
                        ref Text labelText = ref AccessTools.FieldRefAccess<UIResAmountEntry, Text>(uiresAmountEntry, "labelText");

                        if (uiresAmountEntry.refId > 0)
                        {
                            long num2 = __instance.star.GetResourceAmount(uiresAmountEntry.refId);
                            if (uiresAmountEntry.refId == 0)
                            {




                            }
                            else if (uiresAmountEntry.refId == 7)
                            {
                                double num3 = (double)num2 * (double)VeinData.oilSpeedMultiplier;
                                StringBuilderUtility.WritePositiveFloat(uiresAmountEntry.sb, 0, 8, (float)num3, 2, ' ');
                                //VeinProto veinProto = LDB.veins.Select(uiresAmountEntry.refId);
                                //ItemProto itemProto = LDB.items.Select(veinProto.MiningItem);
                                uiresAmountEntry.valueString = uiresAmountEntry.sb.ToString();
                            }
                            else
                            {
                                if (num2 < 1000000000L)
                                {
                                    StringBuilderUtility.WriteCommaULong(uiresAmountEntry.sb, 0, 16, (ulong)num2, 1, ' ');
                                }
                                else
                                {
                                    StringBuilderUtility.WriteKMG(uiresAmountEntry.sb, 15, num2, false);
                                }
                                //VeinProto veinProto = LDB.veins.Select(uiresAmountEntry.refId);
                                //ItemProto itemProto = LDB.items.Select(veinProto.MiningItem);
                                uiresAmountEntry.valueString = uiresAmountEntry.sb.ToString();
                            }

                        }
                        if (uiresAmountEntry.valueString != null)
                        {
                            //uiresAmountEntry.valueString = uiresAmountEntry.sb.ToString();
                            //LogManager.Logger.LogInfo(uiresAmountEntry.refId + " : " + labelText.text + " : " + uiresAmountEntry.valueString);
                        }
                    }
                    //LogManager.Logger.LogInfo("universeObserveLevel : " + GameMain.history.universeObserveLevel + " => " + tmpUniverseObserveLevel);
                    //GameMain.history.universeObserveLevel = tmpUniverseObserveLevel;

                }
            }

        }


        //星間物流ステーション情報の表示
        //[HarmonyPatch(typeof(UIPlanetDetail), "_OnUpdate")]
        public static class UIPlanetDetail_OnUpdate_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix(UIPlanetDetail __instance)
            {
                if (Time.frameCount % 30 == 0 && CheckStatus())
                {
                    if (__instance.planet.data == null)
                    {





                    }

                }


            }

        }


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
        [HarmonyPatch(typeof(UIPlanetDetail), "RefreshDynamicProperties")]
        public static class UIPlanetDetail_RefreshDynamicProperties_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix(UIPlanetDetail __instance)
            {
                //List<TransportData> transportdata = new List<TransportData>();

                if (CheckStatus())
                {


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
                    nextButton.SetActive(false);
                    if (pageNo > 0)
                    {
                        previousButton.SetActive(true);
                    }
                    else
                    {
                        previousButton.SetActive(false);
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

                                        nextButton.SetActive(true);
                                        lastStationNo = i;
                                        break;
                                        //LogManager.Logger.LogInfo("previousButton != null 2");

                                    }

                                    //ステーション変わったのでラインを表示
                                    Line[stationNo].transform.localPosition = new Vector3(-125, (float)(-70 - lineNo * 20), 0);
                                    Line[stationNo].SetActive(true);
                                    //文字列・アイコンは非表示
                                    ItemName[lineNo].SetActive(false);
                                    ItemIcon[lineNo].SetActive(false);
                                    ItemLogic[lineNo].SetActive(false);
                                    ItemCount[lineNo].SetActive(false);
                                    stationNo++;
                                    lineNo += 1;

                                    //ストレージの内容を表示
                                    for (int j = 0; j < planetFactory.transport.stationPool[i].storage.Length; j++)
                                    {

                                        if (planetFactory.transport.stationPool[i].storage[j].itemId != 0)
                                        {
                                            //アイテム名
                                            ItemName[lineNo].GetComponentInChildren<Text>().text = LDB.items.Select(planetFactory.transport.stationPool[i].storage[j].itemId).name;
                                            ItemName[lineNo].SetActive(true);

                                            //アイテムアイコン
                                            ItemIcon[lineNo].GetComponentInChildren<Image>().sprite = LDB.items.Select(planetFactory.transport.stationPool[i].storage[j].itemId).iconSprite;
                                            ItemIcon[lineNo].SetActive(true);

                                            //アイテム数
                                            ItemCount[lineNo].GetComponentInChildren<Text>().text = String.Format("{0:#,0}", planetFactory.transport.stationPool[i].storage[j].count);
                                            ItemCount[lineNo].SetActive(true);


                                            //ロジック
                                            if (planetFactory.transport.stationPool[i].storage[j].remoteLogic == ELogisticStorage.Demand)
                                            {
                                                ItemLogic[lineNo].GetComponentInChildren<Text>().text = "需求".Translate();
                                                ItemLogic[lineNo].GetComponentInChildren<Text>().color = new Color(0.88f, 0.55f, 0.36f, 0.5f);

                                            }
                                            else if (planetFactory.transport.stationPool[i].storage[j].remoteLogic == ELogisticStorage.Supply)
                                            {
                                                ItemLogic[lineNo].GetComponentInChildren<Text>().text = "供应".Translate();
                                                ItemLogic[lineNo].GetComponentInChildren<Text>().color = new Color(0.24f, 0.55f, 0.65f, 0.5f);
                                            }
                                            else
                                            {
                                                ItemLogic[lineNo].GetComponentInChildren<Text>().text = "仓储".Translate();
                                                ItemLogic[lineNo].GetComponentInChildren<Text>().color = new Color(1, 1, 1, 0.3f);
                                            }
                                            ItemLogic[lineNo].SetActive(true);

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
                        Line[0].transform.localPosition = new Vector3(-125, (float)(-70 - lineNo * 20), 0);
                        Line[0].SetActive(true);
                        stationNo++;
                        lineNo++;

                        ItemName[1].GetComponentInChildren<Text>().text = "No Interstellar Logistics Station".Translate();
                        ItemIcon[1].SetActive(false);
                        ItemCount[1].SetActive(false);
                        ItemLogic[1].SetActive(false);


                        lineNo++;


                    }
                    //ステーション変わったのでラインを表示
                    Line[stationNo].transform.localPosition = new Vector3(-125, (float)(-70 - lineNo * 20), 0);
                    Line[stationNo].SetActive(true);
                    stationNo++;


                    //残りのオブジェクトは非表示
                    for (int j = lineNo; j < 108; j++)
                    {
                        ItemName[j].SetActive(false);
                        ItemIcon[j].SetActive(false);
                        ItemCount[j].SetActive(false);
                        ItemLogic[j].SetActive(false);
                    }
                    for (int j = stationNo; j < 50; j++)
                    {
                        Line[j].SetActive(false);
                    }

                }

            }


        }



        //スターマップを閉じるとき、universeObserveLevelを戻す。
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