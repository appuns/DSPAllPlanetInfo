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

    [BepInPlugin("Appun.DSP.plugin.AllPlanetInfo", "DSPAllPlanetInfo", "1.1.10")]
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
        public static Sprite UseUniverseExploration5Icon;

        public static Sprite planetInfoIcon;

        Vector2 scrollPosition = Vector2.zero;

        public static int fontSize = 20;
        public static int fixedHeight = 30;

        //public static List<int> loadedStar = new List<int>();


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
            //RequireUniverseExploration4 = Config.Bind("Tech", "RequireUniverseExplorationLevel4", false, "Require Universe exploration Level 4");
            ShowAdditionalInformationWindow = Config.Bind("UI", "ShowAdditionalInformationWindow", true, "Show planetary logistics information");

            LoadIcon();
            UI.Create();

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
                    planetInfoIcon = assetBundle.LoadAsset<Sprite>("PlanetInfo");
                }

            }
            catch (Exception e)
            {
                LogManager.Logger.LogInfo("e.Message " + e.Message);
                LogManager.Logger.LogInfo("e.StackTrace " + e.StackTrace);
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
                else if (GameMain.data.history.techStates[4103].unlocked)
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
                }else
                {
                    GameMain.history.universeObserveLevel = 0;
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