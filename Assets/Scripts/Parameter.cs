using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 複数のシーンで共有する情報を管理するクラス
public static class Parameter
{
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