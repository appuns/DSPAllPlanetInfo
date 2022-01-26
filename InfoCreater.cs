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
    internal class InfoCreater
    {
        public static int lineMax;
        public static int pageNo;
        public static int lastStationNo;
        public static int[] startStationNo = new int[100];


        //追加の惑星情報を表示
        public static void StationInfo(UIPlanetDetail __instance)
        {

            if (__instance.planet.loaded) // __instance.planet.loaded)
            {
                LogManager.Logger.LogInfo("station info");

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
                  //LogManager.Logger.LogInfo("Planet Name : " + UIRoot.instance.uiGame.planetDetail.planet.name + "    stationCursor : " + UIRoot.instance.uiGame.planetDetail.planet.factory.transport.stationCursor);
                  var planetFactory = UIRoot.instance.uiGame.planetDetail.planet.factory;

                    if (planetFactory.transport.stationCursor > 0)
                    {
                        for (int i = startStationNo[pageNo]; i < planetFactory.transport.stationCursor; i++)
                        {
                            //LogManager.Logger.LogInfo("i : " + i);

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

                }
                //factory情報がない場合
                else
                {
                    //LogManager.Logger.LogInfo("no factory");

                    for (int j = 1; j < 108; j++)
                    {
                        UI.ItemName[j].SetActive(false);
                        UI.ItemIcon[j].SetActive(false);
                        UI.ItemCount[j].SetActive(false);
                        UI.ItemLogic[j].SetActive(false);
                    }
                    for (int j = 1; j < 50; j++)
                    {
                        UI.Line[j].SetActive(false);
                    }
                    UI.ItemName[1].SetActive(true);
                    UI.ItemName[1].GetComponent<Text>().text = "No Interstellar Logistics Station".Translate();

                }




            }


        }

    }
}
