﻿using BepInEx;
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
    internal class UI : MonoBehaviour
    {


        public static int windowWidth = 550;
        public static Rect windowRectNemu = new Rect(0, 50, 400, 0);
        public static Rect windowRectStation = new Rect(0, 120, windowWidth, 100);
        public static Rect windowRectMiner = new Rect(0, 120, windowWidth, 0);
        public static Rect windowRectFactories = new Rect(0, 120, windowWidth, 0);


        public static GameObject infoWindow;
        //public static RectTransform rectInfoWindow;

        //public static GameObject planetdetailwindow;

        public static GameObject[] ItemName = new GameObject[108];
        public static GameObject[] ItemIcon = new GameObject[108];
        public static GameObject[] ItemCount = new GameObject[108];
        public static GameObject[] ItemLogic = new GameObject[108];
        public static GameObject[] Line = new GameObject[50];
        public static GameObject WindowTitle;
        //public static GameObject page;
        public static GameObject previousButton;
        public static GameObject nextButton;

        public static Button previousButtonButton;
        public static Button nextButtonButton;
        public static Button planetInfoButtonButton;

        public static GameObject planetInfoButton;

        public static bool infoWindowEnable = true;

        public static float posY = -40;

        public static void Create()

        {

            //メインアイコンの作成
            GameObject lockNorthButton = UIRoot.instance.uiGame.starmap.lockNorthButton.gameObject;
            planetInfoButton = Instantiate(lockNorthButton, lockNorthButton.transform.parent) as GameObject;
            planetInfoButtonButton = planetInfoButton.GetComponent<Button>();
            planetInfoButton.GetComponent<UIButton>().tips.offset = new Vector2(-300, 500);
            planetInfoButton.transform.Find("icon").GetComponent<Image>().sprite = Main.planetInfoIcon;
            planetInfoButton.transform.Find("icon").GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);

            //アイコンの読み込み

            //Destroy(planetInfoButtonButton.GetComponent<UIButton>().tip.gameObject);
            //Destroy(planetInfoButtonButton.GetComponent<UIButton>());


            //情報表示用のウインドウを作成
            GameObject planetdetailwindow = UIRoot.instance.uiGame.planetDetail.gameObject;
            infoWindow = Instantiate(planetdetailwindow) as GameObject;
            infoWindow.name = "infoWindow";
            infoWindow.transform.SetParent(planetdetailwindow.transform.parent, true);
            infoWindow.transform.localScale = planetdetailwindow.transform.localScale;

            Destroy(infoWindow.transform.Find("detail_group/res-group/res-entry/icon").GetComponent<UIButton>());
            Destroy(infoWindow.GetComponent<UIPlanetDetail>());

            //LogManager.Logger.LogInfo("情報表示用のウインドウを作成");


            //next&previousボタンの追加
            previousButton = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Assembler Window/produce/copy-button"), infoWindow.transform) as GameObject;
            previousButton.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 20);
            previousButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            //previousButton.transform.localPosition = new Vector3(-135, -40, 0);
            previousButton.transform.Find("Text").GetComponent<Text>().text = "Previous Page".Translate();
            Destroy(previousButton.transform.Find("Text").GetComponent<Localizer>());
            Destroy(previousButton.GetComponent<UIButton>());
            previousButton.name = "previousButton";
            previousButton.SetActive(true);
            previousButtonButton = previousButton.GetComponent<Button>();

            nextButton = Instantiate(previousButton.gameObject, infoWindow.transform) as GameObject;
            //nextButton.transform.localPosition = new Vector3(-15, -40, 0);
            nextButton.transform.Find("Text").GetComponent<Text>().text = "Next Page".Translate();
            nextButton.name = "nextButton";
            nextButton.SetActive(true);
            nextButtonButton = nextButton.GetComponent<Button>();


            //LogManager.Logger.LogInfo("ボタンの追加");

            //ウインドウのタイトル追加
            WindowTitle = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
            Destroy(WindowTitle.transform.Find("tip-btn").gameObject);
            WindowTitle.transform.localPosition = new Vector3(-250, 15, 0);
            WindowTitle.GetComponent<Text>().text = "Interstellar Logistics".Translate();
            WindowTitle.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;
            WindowTitle.GetComponent<Text>().resizeTextMaxSize = 20;
            //WindowTitle.GetComponent<Text>().fontSize = 23;
            WindowTitle.SetActive(true);

            //LogManager.Logger.LogInfo("ウインドウのタイトル追加");

            UI.infoWindow.transform.Find("icon").gameObject.SetActive(false);

            //ページ
            //page = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
            //page.transform.localPosition = new Vector3(-20, -40, 0);

            //表示行を作成
            for (int j = 0; j < 108; j++)
            {
                infoWindow.transform.Find("bg").gameObject.SetActive(false);
                infoWindow.transform.Find("detail_group/res-group").gameObject.SetActive(false);

                //アイテム名
                ItemProto itemProto = LDB.items.Select(1001);
                ItemName[j] = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
                Destroy(ItemName[j].transform.Find("tip-btn").gameObject);

                ItemName[j].name = "ItemName " + j;
                ItemName[j].transform.localPosition = new Vector3(-245, (float)(posY - j * 20), 0);
                ItemName[j].GetComponent<RectTransform>().sizeDelta = new Vector2(150, 20);
                //ItemName[j].GetComponent<Text>().text = "";
                ItemName[j].GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                ItemName[j].SetActive(false);

                //アイテムアイコン
                ItemIcon[j] = Instantiate(infoWindow.transform.Find("detail_group/res-group/res-entry/icon").gameObject, infoWindow.transform) as GameObject;
                ItemIcon[j].name = "ItemIcon " + j;
                ItemIcon[j].transform.localPosition = new Vector3(-85, (float)(posY - j * 20), 0);
                //ItemIcon[j].GetComponent<Image>().sprite = null;
                ItemIcon[j].GetComponent<Image>().enabled = true;
                ItemIcon[j].SetActive(false);
                Destroy(ItemIcon[j].GetComponent<UIButton>());


                //アイテム数
                ItemCount[j] = Instantiate(infoWindow.transform.Find("detail_group/res-group/res-entry/value-text").gameObject, infoWindow.transform) as GameObject;
                ItemCount[j].name = "ItemCount " + j;
                ItemCount[j].transform.localPosition = new Vector3(-35, (float)(posY - j * 20), 0);
                ItemCount[j].GetComponent<RectTransform>().sizeDelta = new Vector2(40, 20);
                //ItemCount[j].GetComponent<Text>().text = "";
                ItemCount[j].GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                ItemCount[j].SetActive(false);

                //ロジック情報
                ItemLogic[j] = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
                Destroy(ItemLogic[j].transform.Find("tip-btn").gameObject);
                ItemLogic[j].name = "ItemLogic " + j;
                ItemLogic[j].transform.localPosition = new Vector3(-30, (float)(posY - j * 20), 0);
                ItemLogic[j].GetComponent<RectTransform>().sizeDelta = new Vector2(40, 20);
                //ItemLogic[j].GetComponent<Text>().text = "";
                ItemLogic[j].GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                ItemLogic[j].SetActive(false);


            }

            for (int j = 0; j < 50; j++)
            {
                Line[j] = Instantiate(infoWindow.transform.Find("detail_group/res-group/line").gameObject, infoWindow.transform) as GameObject;
                Destroy(Line[j].transform.Find("display-combo").gameObject);
                Line[j].SetActive(false);


            }
            //LogManager.Logger.LogInfo("表示行を作成");

            //不要なオブジェクトの削除
            Destroy(infoWindow.transform.Find("mask").gameObject);
            //Destroy(infoWindow.transform.Find("icon").gameObject);
            Destroy(infoWindow.transform.Find("name-input").gameObject);
            Destroy(infoWindow.transform.Find("type-text").gameObject);
            Destroy(infoWindow.transform.Find("detail_group").gameObject);

            //ボタンクリックイベントの追加
            previousButtonButton.onClick.AddListener(OnClickPreviousButton);
            nextButtonButton.onClick.AddListener(OnClickNextButton);
            planetInfoButtonButton.onClick.AddListener(OnClickplanetInfoButton);

            setWindowPos();

        }

        //ウインドウ位置調整
        public static void setWindowPos()
        {
            int UIheight = DSPGame.globalOption.uiLayoutHeight;
            int UIwidth = UIheight * Screen.width / Screen.height;

            planetInfoButton.transform.localPosition = new Vector3(0 - UIwidth / 2 + 50, UIheight / 2 - 30, 0); // 1080 => -910 530 0

            infoWindow.transform.localPosition = new Vector3(260 - UIwidth, UIheight / 2 - 370, 0);
            infoWindow.GetComponent<RectTransform>().sizeDelta = new Vector2(UI.infoWindow.GetComponent<RectTransform>().sizeDelta.x, UIheight - 130);

            previousButton.transform.localPosition = new Vector3(-125, -20, 0);
            nextButton.transform.localPosition = new Vector3(-20, -20, 0);
        }


        //ボタンイベント
        public static void OnClickPreviousButton()
        {
            InfoCreater.pageNo--;
            //LogManager.Logger.LogInfo("pageNo--");
            UIRoot.instance.uiGame.planetDetail.RefreshDynamicProperties();
            //needRefresh = true;
        }

        public static void OnClickNextButton()
        {
            InfoCreater.pageNo++;
            //LogManager.Logger.LogInfo("pageNo++");
            InfoCreater.startStationNo[InfoCreater.pageNo] = InfoCreater.lastStationNo;
            //LogManager.Logger.LogInfo("lastStationNo = " +lastStationNo);
            UIRoot.instance.uiGame.planetDetail.RefreshDynamicProperties();
            //needRefresh = true;
        }

        public static void OnClickplanetInfoButton()
        {
            infoWindowEnable = !infoWindowEnable;
            infoWindow.SetActive(infoWindowEnable);
            planetInfoButtonButton.GetComponent<UIButton>().highlighted = infoWindowEnable;

            UIRoot.instance.uiGame.dfMonitor.gameObject.SetActive(!infoWindowEnable);
        }

        



    }
}
