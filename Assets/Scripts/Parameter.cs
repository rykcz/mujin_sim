using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//複数のシーンで共有する情報を管理するクラス
//MonoBehaviourは継承しないことに注意
public static class Parameter
{
    // public static List<int> eventList = new List<int>(){};
    // public static List<int> tmpEventList = new List<int>(){}; //通過した異変数表示用。タブ閉じない限り初期化しない。
    // public static bool mouseUnlockBool = false; //これをLockMouseCursor.csでInput.GetMouseButtonDown(0) && playerObj.GetComponent<PlayerController>().hasDiedに置き換えてPCでのマウス表示させたい
    public static int money = 1000;
    public static int dayCostMoney = 600;
    public static int day = 1;
    public static int limitDay = 5;
    public static int soldVegetableCount = 0;
    public static float tmpFloat;
    public static List<int> tmpList = new List<int>(){};
    public static bool tmpBool = false;

    // パラメータを初期化するメソッド
    public static void ResetParameters()
    {
        money = 1000;
        dayCostMoney = 600;
        day = 1;
        limitDay = 5;
        soldVegetableCount = 0;
        tmpFloat = 0f;
        tmpList.Clear();
        tmpBool = false;
    }

}