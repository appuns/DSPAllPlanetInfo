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
        [HarmonyPatch(typeof(UIStarDetail), "OnStarDataSet")]
        class UIStarDetail_OnStarDataSet_Transpiler
        {
            [HarmonyTranspiler]

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeMatcher matcher = new CodeMatcher(instructions);
                matcher.
                    MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_magnitude"),
                        new CodeMatch(OpCodes.Stloc_2),
                         new CodeMatch(OpCodes.Ldarg_0)).
                         //new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_star")).
                    RemoveInstructions(19).
                    InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1));
                return matcher.InstructionEnumeration();


                //var codes = new List<CodeInstruction>(instructions);
                //codes[49].opcode = OpCodes.Ldc_I4_0;
                //codes[51].opcode = OpCodes.Ldc_I4_0;
                //codes[53].opcode = OpCodes.Ldc_I4_0;
                //return codes.AsEnumerable();
            }
        }

        //フラグのごまかし２
        [HarmonyPatch(typeof(UIStarDetail), "RefreshDynamicProperties")]
        class UIStarDetail_RefreshDynamicProperties_Transpiler
        {
            [HarmonyTranspiler]

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeMatcher matcher = new CodeMatcher(instructions);
                matcher.
                    MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_magnitude"),
                        new CodeMatch(OpCodes.Stloc_1),
                         new CodeMatch(OpCodes.Ldarg_0)).
                         //new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_star")).
                    RemoveInstructions(19).
                    InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1));
                return matcher.InstructionEnumeration();

                //var codes = new List<CodeInstruction>(instructions);
                //codes[24].opcode = OpCodes.Ldc_I4_0;
                //codes[26].opcode = OpCodes.Ldc_I4_0;
                //codes[28].opcode = OpCodes.Ldc_I4_0; 
                //return codes.AsEnumerable();

            }
        }

        //フラグのごまかし３
        [HarmonyPatch(typeof(UIPlanetDetail), "OnPlanetDataSet")]
        class UIPlanetDetail_OnStarDataSet_Transpiler
        {
            [HarmonyTranspiler]

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeMatcher matcher = new CodeMatcher(instructions);
                matcher.
                    MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_planet"),
                        new CodeMatch(OpCodes.Brfalse),
                        new CodeMatch(OpCodes.Ldarg_0)).
                    RemoveInstructions(15).
                    InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1)).
                    InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_3));
                return matcher.InstructionEnumeration();
                //var codes = new List<CodeInstruction>(instructions);
                //codes[36].opcode = OpCodes.Ldc_I4_0;
                //return codes.AsEnumerable();
                //}

            }
        }

        //フラグのごまかし４
        [HarmonyPatch(typeof(UIPlanetDetail), "RefreshDynamicProperties")]
        class UIPlanetDetail_RefreshDynamicProperties_Transpiler
        {
            [HarmonyTranspiler]

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeMatcher matcher = new CodeMatcher(instructions);
                matcher.
                    MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_planet"),
                        new CodeMatch(OpCodes.Brfalse),
                        new CodeMatch(OpCodes.Ldarg_0)).
                    RemoveInstructions(15).
                    InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1)).
                    InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_2));
                return matcher.InstructionEnumeration();
                //var codes = new List<CodeInstruction>(instructions);
                //codes[11].opcode = OpCodes.Ldc_I4_0;
                //return codes.AsEnumerable();
            }
        }

        //フラグのごまかし５
        [HarmonyPostfix, HarmonyPatch(typeof(UIStarmap), "OnCursorFunction2Click")]
        public static void UIStarmap_OnCursorFunction2Click_Postfix(UIStarmap __instance)
        {
            if (__instance.focusStar != null)
            {

                __instance.SetViewStar(__instance.focusStar.star, true);
            }
        }


        //フラグのごまかし６
        [HarmonyPostfix, HarmonyPatch(typeof(UIStarmap), "OnStarClick")]
        public static void UIStarmap_OnStarClick_Postfix(UIStarmap __instance, UIStarmapStar star)
        {
            if (__instance.focusStar == star)
            {

                __instance.SetViewStar(__instance.focusStar.star, true);
            }
        }


        //星系情報が作成されていないときは作成
        [HarmonyPrefix, HarmonyPatch(typeof(UIStarDetail), "OnStarDataSet")]
        public static bool UIStarDetail_OnStarDataSet_Prefix(UIStarDetail __instance)
        {
            if (__instance.star != null)
            {
                if (!__instance.star.loaded)
                {
                    //惑星情報を作成
                    for (int i = 0; i < __instance.star.planetCount; i++)
                    {
                        PlanetData planetData = __instance.star.planets[i];
                        PlanetCreater.Create(planetData);
                    }
                }
            }
            return true;
        }

        //星系情報が作成されていないときは表示をスキップ
        [HarmonyPrefix, HarmonyPatch(typeof(UIStarDetail), "RefreshDynamicProperties")]
        public static bool UIStarDetail_RefreshDynamicProperties_Prefix(UIStarDetail __instance)
        {
            if (__instance.star != null)
            {
                if (!__instance.star.loaded)
                {
                    return false;
                }
            }
            return true;
        }


        //追加の惑星情報を表示
        [HarmonyPostfix,HarmonyPatch(typeof(UIPlanetDetail), "RefreshDynamicProperties")]
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
                UI.infoWindow.transform.localPosition = new Vector3(260 - UIwidth, 150, 0);
                UI.rectInfoWindow.sizeDelta = new Vector2(UI.rectInfoWindow.sizeDelta.x, UIheight);
                UI.previousButton.transform.localPosition = new Vector3(-125, -40, 0);
                UI.nextButton.transform.localPosition = new Vector3(-125, 40 - UIheight, 0);

            }
        }

        //UIplanetwindowのopenと同期
        [HarmonyPatch(typeof(UIPlanetDetail), "_OnOpen")]
        public static class UIPlanetDetail_OnOpen_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (Main.ShowAdditionalInformationWindow.Value)
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
                    UI.infoWindow.SetActive(true);
                    InfoCreater.lineMax = (DSPGame.globalOption.uiLayoutHeight / 20) - 5;
                    //LogManager.Logger.LogInfo("lineMax : " + lineMax);
                    //アイコンの変更
                    UI.infoWindow.transform.Find("icon").GetComponent<Image>().sprite = LDB.techs.Select(1605).iconSprite;

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
