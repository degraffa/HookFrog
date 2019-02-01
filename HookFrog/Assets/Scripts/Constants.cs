using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {

    // DB Logging Information
    public const int GAME_ID = 439; 
    public const int VERSION_ID = 201;
    // 101 - Newground release 11/9/18 

    // Only enable debugging if we're in the web build 
    #if UNITY_EDITOR
        public const bool IS_DEBUGGING = true;
    #else 
        public const bool IS_DEBUGGING = false;
    #endif

    // log debug output in unity editor
    public const bool PRINT_DEBUG = false;

    // Map of Level (Quest) numbers to Scene Locations:
    public static readonly IDictionary<int, string> LEVEL_DIRECTORY = new Dictionary<int, string>(){
        // UI SCREENS
        {1, "Scenes/Main Menu"},
        // LEVELSs
        // forest
        {100, "Scenes/Developer/Beta-wAssets"},
        {101, "Scenes/tutorial/tutorial1.meta"},
        {102, "Scenes/tutorial/tutoriallevel2.meta"},
        {103, "Scenes/tutorial/tutoriallevel3.meta"},
        {104, "Scenes/tutorial/tutoriallevel4.meta"},
        {105, "Scenes/tutorial/tutoriallevel5.meta"},
        {106, "Scenes/tutorial/tutoriallevel6.meta"},
        {107, "Scenes/Forest/forestlevel1"},
        {108, "Scenes/Forest/forestlevel2"},
        {109, "Scenes/Forest/forestlevel3"},
        {110, "Scenes/Forest/forestlevel4"},
        {111, "Scenes/Forest/forestlevel5"},

        // cave
        {201, "Scenes/Cave/cavelevel1"},
        {202, "Scenes/Cave/cavelevel2"},
        {203, "Scenes/Cave/cavelevel3"},
        {205, "Scenes/Cave/cavelevel5"},
        {206, "Scenes/Cave/cavelevel6"},

        // floating
        {301, "Scenes/Floating/Floating-1"},
        {302, "Scenes/Floating/Floating-2"},
        {303, "Scenes/Floating/Floating-3"},
        {304, "Scenes/Floating/Floating-4"},
        {305, "Scenes/Floating/Floating-5"},
        {306, "Scenes/Floating/Floating-6"},
        {307, "Scenes/Floating/Floating-7"},

        // misc
        {666, "Scenes/You Win"}
    };
}
