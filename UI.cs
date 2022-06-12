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
    internal class UI : MonoBehaviour
    {


        public static int windowWidth = 550;
        public static Rect windowRectNemu = new Rect(0, 50, 400, 0);
        public static Rect windowRectStation = new Rect(0, 120, windowWidth, 100);
        public static Rect windowRectMiner = new Rect(0, 120, windowWidth, 0);
        public static Rect windowRectFactories = new Rect(0, 120, windowWidth, 0);


        public static GameObject infoWindow;
        public static RectTransform rectInfoWindow;

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

        public static void Create()

        {
            //UI解像度計算
            int UIheight = DSPGame.globalOption.uiLayoutHeight;
            int UIwidth = UIheight * Screen.width / Screen.height;


            //情報表示用のウインドウを作成
            GameObject planetdetailwindow = GameObject.Find("UI Root/Overlay Canvas/In Game/Planet & Star Details/planet-detail-ui");
            infoWindow = Instantiate(planetdetailwindow) as GameObject;
            infoWindow.name = "infoWindow";
            infoWindow.transform.SetParent(planetdetailwindow.transform.parent, true);
            infoWindow.transform.localPosition = new Vector3(260 - UIwidth, 250, 0);
            infoWindow.transform.localScale = planetdetailwindow.transform.localScale;

            infoWindow.GetComponent<RectTransform>().sizeDelta = new Vector2(infoWindow.GetComponent<RectTransform>().sizeDelta.x, UIheight);

            Destroy(infoWindow.transform.Find("res-group/res-entry/icon").GetComponent<UIButton>());
            Destroy(infoWindow.GetComponent<UIPlanetDetail>());

            //LogManager.Logger.LogInfo("情報表示用のウインドウを作成");


            //next&previousボタンの追加
            previousButton = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Assembler Window/produce/copy-button"), infoWindow.transform) as GameObject;
            previousButton.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 20);
            previousButton.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            previousButton.transform.localPosition = new Vector3(-135, -40, 0);
            previousButton.transform.Find("Text").GetComponent<Text>().text = "Previous Page".Translate();
            Destroy(previousButton.transform.Find("Text").GetComponent<Localizer>());
            Destroy(previousButton.GetComponent<UIButton>());
            previousButton.name = "previousButton";
            previousButton.SetActive(true);
            previousButtonButton = previousButton.GetComponent<Button>();

            nextButton = Instantiate(previousButton.gameObject, infoWindow.transform) as GameObject;
            nextButton.transform.localPosition = new Vector3(-15, -40, 0);
            nextButton.transform.Find("Text").GetComponent<Text>().text = "Next Page".Translate();
            nextButton.name = "nextButton";
            nextButton.SetActive(true);
            nextButtonButton = nextButton.GetComponent<Button>();


            //LogManager.Logger.LogInfo("ボタンの追加");

            //ウインドウのタイトル追加
            WindowTitle = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
            WindowTitle.transform.localPosition = new Vector3(-200, -5, 0);
            WindowTitle.GetComponent<Text>().text = "Interstellar Logistics".Translate();
            WindowTitle.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;
            WindowTitle.GetComponent<Text>().resizeTextMaxSize = 20;
            //WindowTitle.GetComponent<Text>().fontSize = 23;
            WindowTitle.SetActive(true);

            //LogManager.Logger.LogInfo("ウインドウのタイトル追加");

            //ページ
            //page = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
            //page.transform.localPosition = new Vector3(-20, -40, 0);

            //表示行を作成
            for (int j = 0; j < 108; j++)
            {
                infoWindow.transform.Find("bg").gameObject.SetActive(false);
                infoWindow.transform.Find("res-group").gameObject.SetActive(false);

                //アイテム名
                ItemProto itemProto = LDB.items.Select(1001);
                ItemName[j] = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
                ItemName[j].name = "ItemName " + j;
                ItemName[j].transform.localPosition = new Vector3(-245, (float)(-60 - j * 20), 0);
                ItemName[j].GetComponent<RectTransform>().sizeDelta = new Vector2(150, 20);
                //ItemName[j].GetComponent<Text>().text = "";
                ItemName[j].GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                ItemName[j].SetActive(false);

                //アイテムアイコン
                ItemIcon[j] = Instantiate(infoWindow.transform.Find("res-group/res-entry/icon").gameObject, infoWindow.transform) as GameObject;
                ItemIcon[j].name = "ItemIcon " + j;
                ItemIcon[j].transform.localPosition = new Vector3(-85, (float)(-60 - j * 20), 0);
                //ItemIcon[j].GetComponent<Image>().sprite = null;
                ItemIcon[j].GetComponent<Image>().enabled = true;
                ItemIcon[j].SetActive(false);
                Destroy(ItemIcon[j].GetComponent<UIButton>());


                //アイテム数
                ItemCount[j] = Instantiate(infoWindow.transform.Find("res-group/res-entry/value-text").gameObject, infoWindow.transform) as GameObject;
                ItemCount[j].name = "ItemCount " + j;
                ItemCount[j].transform.localPosition = new Vector3(-35, (float)(-60 - j * 20), 0);
                ItemCount[j].GetComponent<RectTransform>().sizeDelta = new Vector2(40, 20);
                //ItemCount[j].GetComponent<Text>().text = "";
                ItemCount[j].GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                ItemCount[j].SetActive(false);

                //ロジック情報
                ItemLogic[j] = Instantiate(infoWindow.transform.Find("type-text").gameObject, infoWindow.transform) as GameObject;
                ItemLogic[j].name = "ItemLogic " + j;
                ItemLogic[j].transform.localPosition = new Vector3(-30, (float)(-60 - j * 20), 0);
                ItemLogic[j].GetComponent<RectTransform>().sizeDelta = new Vector2(40, 20);
                //ItemLogic[j].GetComponent<Text>().text = "";
                ItemLogic[j].GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                ItemLogic[j].SetActive(false);


            }
            for (int j = 0; j < 50; j++)
            {
                Line[j] = Instantiate(infoWindow.transform.Find("res-group/line").gameObject, infoWindow.transform) as GameObject;
                Line[j].SetActive(false);


            }
            //LogManager.Logger.LogInfo("表示行を作成");


            //不要なオブジェクトの削除
            //Destroy(StationWindow.transform.Find("icon").gameObject);
            Destroy(infoWindow.transform.Find("name-input").gameObject);
            Destroy(infoWindow.transform.Find("type-text").gameObject);
            Destroy(infoWindow.transform.Find("param-group").gameObject);

            //ボタンクリックイベントの追加
            previousButtonButton.onClick.AddListener(OnClickPreviousButton);
            nextButtonButton.onClick.AddListener(OnClickNextButton);


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



    }
}
