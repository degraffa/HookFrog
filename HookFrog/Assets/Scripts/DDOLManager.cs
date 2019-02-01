using UnityEngine;
using System.Collections;

public class DDOLManager : MonoBehaviour
{
    //Awake is always called before any Start functions
    void Awake()
    {
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }
}