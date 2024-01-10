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

namespace DSPAllPlanetInfo
{
    [HarmonyPatch]
    internal class Hook
    {
        public static bool MiniLabPanelOn;

        //フラグのごまかし１
        //フラグのごまかし２
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStarDetail), "OnStarDataSet")]
        [HarmonyPatch(typeof(UIStarDetail), "RefreshDynamicProperties")]
        [HarmonyPatch(typeof(UIPlanetDetail), "OnPlanetDataSet")]
        [HarmonyPatch(typeof(UIPlanetDetail), "RefreshDynamicProperties")]
        [HarmonyPatch(typeof(UIStarmap), "OnCursorFunction2Click")]
        [HarmonyPatch(typeof(UIStarmap), "OnStarClick")]

        public static bool UIStarDetail_Prefix(ref int __state)
        {
            __state = GameMain.history.universeObserveLevel;
            GameMain.history.universeObserveLevel = 4;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIStarDetail), "OnStarDataSet")]
        [HarmonyPatch(typeof(UIStarDetail), "RefreshDynamicProperties")]
        [HarmonyPatch(typeof(UIPlanetDetail), "OnPlanetDataSet")]
        [HarmonyPatch(typeof(UIPlanetDetail), "RefreshDynamicProperties")]
        [HarmonyPatch(typeof(UIStarmap), "OnCursorFunction2Click")]
        [HarmonyPatch(typeof(UIStarmap), "OnStarClick")]
        public static void UIStarDetail_Postfix(int __state)
        {
            GameMain.history.universeObserveLevel = __state;

        }


        //星系情報が作成されていないときは表示をスキップ
        [HarmonyPrefix, HarmonyPatch(typeof(UIStarDetail), "RefreshDynamicProperties")]
        public static bool UIStarDetail_RefreshDynamicProperties_Prefix(UIStarDetail __instance)
        {
            if (__instance.star != null )
            {
                if (!__instance.star.calculated)
                {
                    __instance.loadingTextGo.SetActive(true);
                    __instance.star.RunCalculateThread();

                    return false;
                }
            }
            return true;
        }


        //追加の惑星情報を表示
        [HarmonyPostfix, HarmonyPatch(typeof(UIPlanetDetail), "RefreshDynamicProperties")]
        public static void UIPlanetDetail_RefreshDynamicProperties_Postfix(UIPlanetDetail __instance)
        {
            InfoCreater.StationInfo(__instance);
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




        ////オプション変更後の情報ウインドウ位置修正
        //[HarmonyPatch(typeof(UIOptionWindow), "ApplyOptions")]
        //public static class UIOptionWindowa_ApplyOptions_Postfix
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(UIOptionWindow __instance)
        //    {
        //        //位置とサイズの調整
        //        UI.setWindowPos();
        //    }
        //}

        //UIplanetwindowのopenと同期
        [HarmonyPatch(typeof(UIPlanetDetail), "_OnOpen")]
        public static class UIPlanetDetail_OnOpen_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                //位置とサイズの調整
                UI.setWindowPos();

                if (Main.ShowAdditionalInformationWindow.Value && UIGame.viewMode != EViewMode.Globe)
                {
                    if (UIRoot.instance.uiGame.mechaLab.gameObject.activeSelf == true)
                    {
                        UIRoot.instance.uiGame.mechaLab.gameObject.SetActive(false);
                        MiniLabPanelOn = true;
                    }
                    else
                    {
                        MiniLabPanelOn = false;
                    }
                    //最大行数の設定
                    UI.infoWindow.SetActive(UI.infoWindowEnable);
                    UI.planetInfoButtonButton.GetComponent<UIButton>().highlighted = UI.infoWindowEnable;

                    InfoCreater.lineMax = (DSPGame.globalOption.uiLayoutHeight / 20) - 8;
                    //LogManager.Logger.LogInfo("lineMax : " + lineMax);
                    //アイコンの変更
                    //UI.infoWindow.transform.Find("icon").GetComponent<Image>().sprite = LDB.techs.Select(1605).iconSprite;

                    UIRoot.instance.uiGame.dfMonitor.gameObject.SetActive(!UI.infoWindowEnable);

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
                UI.infoWindow.SetActive(false);
                if (MiniLabPanelOn)
                {
                    GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Mini Lab Panel").SetActive(true);
                }
                UIRoot.instance.uiGame.dfMonitor.gameObject.SetActive(true);
            }
        }


        [HarmonyPatch(typeof(UIPlanetDetail), "_OnOpen")]
        public static class UIPlanetDetail_OnCreate_PostPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                InfoCreater.pageNo = 0;
                InfoCreater.startStationNo[0] = 1;
                //LogManager.Logger.LogInfo("pageNo = 0" );
            }

        }

    }
}
