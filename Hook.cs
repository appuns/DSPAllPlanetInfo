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


        //整地モード：cursorSizeの変更
        //[HarmonyPatch(typeof(UIStarDetail), "OnStarDataSet")]
        class Transpiler_replace1
        {
            [HarmonyTranspiler]

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                codes[30].opcode = OpCodes.Nop;
                codes[31].opcode = OpCodes.Nop;
                codes[32].opcode = OpCodes.Nop; 
                codes[33].opcode = OpCodes.Nop;
                codes[34].opcode = OpCodes.Nop;
                codes[35].opcode = OpCodes.Ldc_I4_1;

                return codes.AsEnumerable();
                //}

            }
        }




        [HarmonyPrefix, HarmonyPatch(typeof(UIStarDetail), "OnStarDataSet")]
        public static bool UIStarDetail_OnStarDataSet_Prefix(UIStarDetail __instance)
        {
            LogManager.Logger.LogInfo("UIStarDetail_OnStarDataSet_Prefix");
            for (int i = 0; i < __instance.entries.Count; i++)
            {
                UIResAmountEntry uiresAmountEntry = __instance.entries[i];
                uiresAmountEntry.SetEmpty();
                __instance.pool.Add(uiresAmountEntry);
            }
            __instance.entries.Clear();
            __instance.tipEntry = null;
            if (__instance.star != null)
            {
                if (!__instance.star.loaded)
                {
                    //PlanetModelingManager.RequestLoadStar(__instance.star);
                    //__instance.star.Load();
                    //惑星情報を作成
                    //LogManager.Logger.LogInfo("start Creating Planets");
                    for (int i = 0; i < __instance.star.planetCount; i++)
                    {
                        PlanetData planetData = __instance.star.planets[i];
                        //PlanetCreater.Create(planetData);
                        IEnumerator coroutine = PlanetCreater.Create(planetData);
                        coroutine.MoveNext();
                    }
                    //LogManager.Logger.LogInfo("finished Creating Planets");
                }
                    //double magnitude = (__instance.star.uPosition - GameMain.mainPlayer.uPosition).magnitude;
                    //int num = (__instance.star == GameMain.localStar) ? 2 : ((magnitude < 14400000.0) ? 3 : 4);
                    //bool flag = GameMain.history.universeObserveLevel >= num;
                    if (!__instance.nameInput.isFocused)
                    {
                        __instance.nameInput.text = __instance.star.displayName;
                    }
                    __instance.typeText.text = __instance.star.typeString;
                    __instance.massValueText.text = __instance.star.mass.ToString("0.000") + " M    ";
                    __instance.spectrValueText.text = __instance.star.spectr.ToString();
                    __instance.radiusValueText.text = __instance.star.radius.ToString("0.00") + " R    ";
                    double num2 = (double)__instance.star.dysonLumino;
                    __instance.luminoValueText.text = num2.ToString("0.000") + " L    ";
                    __instance.temperatureValueText.text = __instance.star.temperature.ToString("#,##0") + " K";
                    if (Localization.isKMG)
                    {
                        __instance.ageValueText.text = (__instance.star.age * __instance.star.lifetime).ToString("#,##0 ") + "百万亿年".Translate();
                    }
                    else
                    {
                        __instance.ageValueText.text = (__instance.star.age * __instance.star.lifetime * 0.01f).ToString("#,##0.00 ") + "百万亿年".Translate();
                    }
                    int num3 = 0;
                    for (int j = 1; j < 15; j++)
                    {
                        int num4 = j;
                        VeinProto veinProto = LDB.veins.Select(num4);
                        ItemProto itemProto = LDB.items.Select(veinProto.MiningItem);
                        bool flag2 = __instance.star.GetResourceAmount(j) > 0L || j < 7;
                        if (veinProto != null && itemProto != null && flag2)
                        {
                            UIResAmountEntry entry = __instance.GetEntry();
                            __instance.entries.Add(entry);
                            entry.SetInfo(num3, itemProto.name, veinProto.iconSprite, veinProto.description, j >= 7, false, (j == 7) ? "         /s" : "                ");
                            entry.refId = num4;
                            num3++;
                        }
                    }
                    for (int k = 0; k < __instance.star.planetCount; k++)
                    {
                        int waterItemId = __instance.star.planets[k].waterItemId;
                        string label = "无".Translate();
                        if (waterItemId > 0)
                        {
                            ItemProto itemProto2 = LDB.items.Select(waterItemId);
                            if (itemProto2 != null)
                            {
                                Sprite iconSprite = itemProto2.iconSprite;
                                label = itemProto2.name;
                                UIResAmountEntry entry2 = __instance.GetEntry();
                                __instance.entries.Add(entry2);
                                entry2.SetInfo(num3, label, iconSprite, itemProto2.description, itemProto2 != null && waterItemId != 1000, false, "");
                                entry2.valueString = "海洋".Translate();
                                num3++;
                            }
                        }
                    }
                    for (int l = 0; l < __instance.star.planetCount; l++)
                    {
                        PlanetData planetData = __instance.star.planets[l];
                        if (planetData.type == EPlanetType.Gas && planetData.gasItems != null)
                        {
                            for (int m = 0; m < planetData.gasItems.Length; m++)
                            {
                                ItemProto itemProto3 = LDB.items.Select(planetData.gasItems[m]);
                                if (itemProto3 != null)
                                {
                                    UIResAmountEntry entry3 = __instance.GetEntry();
                                    __instance.entries.Add(entry3);
                                    entry3.SetInfo(num3, itemProto3.name, itemProto3.iconSprite, itemProto3.description, false, false, "        /s");
                                    StringBuilderUtility.WritePositiveFloat(entry3.sb, 0, 7, planetData.gasSpeeds[m], 2, ' ');
                                    entry3.DisplayStringBuilder();
                                    entry3.SetObserved(true);
                                    num3++;
                                }
                            }
                        }
                    }
                __instance.SetResCount(num3);
                __instance.RefreshDynamicProperties();
            }
            return false;
        }

        //星系情報表示をフック
        [HarmonyPrefix, HarmonyPatch(typeof(UIStarDetail), "RefreshDynamicProperties")]
        public static bool UIStarDetail_RefreshDynamicProperties_Prefix(UIStarDetail __instance)
        {
            LogManager.Logger.LogInfo("UIStarDetail_RefreshDynamicProperties_Prefix");

            bool isInfiniteResource = GameMain.data.gameDesc.isInfiniteResource;
            if (__instance.star != null)
            {
                //if (!__instance.star.loaded)
                //{
                //    return false;
                //}
                double magnitude = (__instance.star.uPosition - GameMain.mainPlayer.uPosition).magnitude;
                int num = (__instance.star == GameMain.localStar) ? 2 : ((magnitude < 14400000.0) ? 3 : 4);
                bool flag = true; //GameMain.history.universeObserveLevel >= num;
                foreach (UIResAmountEntry uiresAmountEntry in __instance.entries)
                {
                    if (uiresAmountEntry.refId > 0)
                    {
                        if (flag)
                        {
                            long num2 = __instance.star.loaded ? __instance.star.GetResourceAmount(uiresAmountEntry.refId) : ((long)__instance.star.GetResourceSpots(uiresAmountEntry.refId));
                            if (uiresAmountEntry.refId == 7)
                            {
                                double num3 = (double)num2 * (double)VeinData.oilSpeedMultiplier;
                                if (__instance.star.loaded)
                                {
                                    StringBuilderUtility.WritePositiveFloat(uiresAmountEntry.sb, 0, 8, (float)num3, 2, ' ');
                                    uiresAmountEntry.DisplayStringBuilder();
                                }
                                else
                                {
                                    uiresAmountEntry.valueString = ((num2 > 0L) ? "探测到信号" : "无").Translate();
                                }
                            }
                            else if (__instance.star.loaded)
                            {
                                if (num2 < 1000000000L)
                                {
                                    StringBuilderUtility.WriteCommaULong(uiresAmountEntry.sb, 0, 16, (ulong)num2, 1, ' ');
                                }
                                else if (isInfiniteResource)
                                {
                                    StringBuilderUtility.WriteKMG(uiresAmountEntry.sb, 15, (num2 + 500000000L) / 1000000000L, false);
                                }
                                else
                                {
                                    StringBuilderUtility.WriteKMG(uiresAmountEntry.sb, 15, num2, false);
                                }
                                uiresAmountEntry.DisplayStringBuilder();
                            }
                            else
                            {
                                uiresAmountEntry.valueString = ((num2 > 0L) ? "探测到信号" : "无").Translate();
                            }
                            uiresAmountEntry.SetObserved(true);
                        }
                        else
                        {
                            uiresAmountEntry.valueString = "未知".Translate();
                            if (uiresAmountEntry.refId > 7)
                            {
                                uiresAmountEntry.overrideLabel = "未知珍奇信号".Translate();
                            }
                            if (uiresAmountEntry.refId > 7)
                            {
                                uiresAmountEntry.SetObserved(false);
                            }
                            else
                            {
                                uiresAmountEntry.SetObserved(true);
                            }
                        }
                    }
                }
                if (__instance.tipEntry != null)
                {
                    if (!flag)
                    {
                        __instance.tipEntry.valueString = "宇宙探索等级".Translate() + num.ToString();
                    }
                    else
                    {
                        __instance.tipEntry.valueString = "";
                    }
                    __instance.SetResCount(flag ? (__instance.entries.Count - 1) : __instance.entries.Count);
                }
            }
            return false;
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
        //[HarmonyPatch(typeof(UIPlanetDetail), "_OnOpen")]
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
                    Main.lineMax = (DSPGame.globalOption.uiLayoutHeight / 20) - 5;
                    //LogManager.Logger.LogInfo("lineMax : " + lineMax);
                    //アイコンの変更
                    UI.infoWindow.transform.Find("icon").GetComponent<Image>().sprite = LDB.techs.Select(1605).iconSprite;

                }
            }
        }

        //UIplanetwindowのcloseと同期
        //[HarmonyPatch(typeof(UIPlanetDetail), "_OnClose")]
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
                Main.pageNo = 0;
                Main.startStationNo[0] = 1;
                //LogManager.Logger.LogInfo("pageNo = 0" );
            }

        }

    }
}
