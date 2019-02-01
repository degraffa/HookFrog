using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LoggingManager : MonoBehaviour
{
    public static LoggingManager instance;

    /// <summary>
    /// Examples of Calling Logging Methods
    /// </summary>
    //LoggingManager.instance.Initialize(14454, 1, false);
    //LoggingManager.instance.RecordPageLoad();
    //LoggingManager.instance.RecordLevelStart(11111f,"info about quest");
    //LoggingManager.instance.RecordEvent(22222);
    //LoggingManager.instance.RecordLevelEnd();
   
    // Initialize variables
    private bool isDebugging = false; // A convenience parameter which, when set to TRUE, disables logging. 
                                      // Make sure you set this to FALSE before you submit your game online!
    private int gameId = 99999; // The game's specific ID number
    private int versionId = 0; // Your game's current version number. You should change this number between releases, 
                               // and after very large changes to your logging methods.

    private bool isLevelStarted = false; // Semaphore for assertion

    private string userId = null;

    private string sessionId = null;

    private string dynamicQuestId = null;

    private float questId = 1f;

    private int sessionSeqId = 1;

    private int QuestSeqId = 1;

    private int abstoredValue = -1;

    private bool abValueSet = false;

    /**
     * Two internal classes for JSON deserialsation
     */
    private class PageLoadData
    {
        public string error_code;
        public string message;
        public string user_id;
        public string session_id;
    }

    private class PlayerQuestData
    {
        public string error_code;
        public string message;
        public string dynamic_quest_id;
    }

    private string pageHost = "http";

    private string phpPath = "://gdiac.cis.cornell.edu/logging/cs4154/fall2018/";

    private string pageLoadPath = "page_load.php";

    private string playerActionPath = "player_action.php";

    private string playerQuestPath = "player_quest.php";

    private string playerQuestEndPath = "player_quest_end.php";

    private string playerABTestPath = "record_abtest.php";

    public void Initialize(int gameId, int versionId, bool isDebugging)
    {
        this.gameId = gameId;
        this.versionId = versionId;
        this.isDebugging = isDebugging;
    }

    public void RecordEvent(int actionId, string actionDetail = "")
    {
        if (isDebugging)
        {
            return;
        }

        TestInitialization();
        Debug.Assert(isLevelStarted, "Cannot record a player's action before a level start.");
        StartCoroutine(GetPlayerAction(actionId, actionDetail));
        sessionSeqId += 1;
        QuestSeqId += 1;
    }

    private IEnumerator GetPlayerAction(int actionId, string actionDetail = "")
    {

        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        long nowMilliSec = (long)(System.DateTime.UtcNow - epochStart).TotalSeconds * 1000;

        string requestData = "?game_id=" + gameId + "&version_id=" + versionId + "&client_timestamp=" + nowMilliSec + "&session_id=" + sessionId +
            "&session_seq_id=" + sessionSeqId + "&user_id=" + userId + "&quest_id=" + questId + "&quest_seq_id=" + QuestSeqId + "&action_id=" + actionId +
            "&dynamic_quest_id=" + dynamicQuestId + "&action_detail=" + actionDetail;

        UnityWebRequest www = UnityWebRequest.Get(pageHost + phpPath + playerActionPath + requestData);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string logReturnedString = www.downloadHandler.text;
            Debug.Log(logReturnedString);
        }
    }

    public void RecordLevelEnd()
    {
        if (isDebugging)
        {
            return;
        }

        TestInitialization();
        Debug.Assert(isLevelStarted, "Cannot end a level that has not started.");
        StartCoroutine(GetPlayerQuestEnd());
        sessionSeqId += 1;
        QuestSeqId = 1;
        isLevelStarted = false;
    }

    private IEnumerator GetPlayerQuestEnd()
    {
        string requestData = "?dynamic_quest_id=" + dynamicQuestId;

        UnityWebRequest www = UnityWebRequest.Get(pageHost + phpPath + playerQuestEndPath + requestData);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string logReturnedString = www.downloadHandler.text;
            Debug.Log(logReturnedString);
        }
    }

    public void RecordLevelStart(float questId, string questDetail = "")
    {
        if (isDebugging)
        {
            return;
        }

        TestInitialization();
        this.questId = questId;
        //Debug.Assert(!isLevelStarted, "Cannot start a level before the last one ends.");
        StartCoroutine(GetPlayerQuest(questId, questDetail));
        sessionSeqId += 1;
        QuestSeqId = 1;
        isLevelStarted = true;
    }

    private IEnumerator GetPlayerQuest(float questId, string questDetail = "")
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        long nowMilliSec = (long)(System.DateTime.UtcNow - epochStart).TotalSeconds * 1000;

        string requestData = "?game_id=" + gameId + "&version_id=" + versionId + "&client_timestamp=" + nowMilliSec + "&session_id=" + sessionId +
            "&session_seq_id=" + sessionSeqId + "&user_id=" + userId + "&quest_id=" + questId + "&quest_detail=" + questDetail;

        UnityWebRequest www = UnityWebRequest.Get(pageHost + phpPath + playerQuestPath + requestData);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string logReturnedString = www.downloadHandler.text;
            Debug.Log(logReturnedString);
            PlayerQuestData pageLoadData = JsonUtility.FromJson<PlayerQuestData>(logReturnedString);
            dynamicQuestId = pageLoadData.dynamic_quest_id;
            Debug.Log(dynamicQuestId);
        }

    }

    public void RecordPageLoad(string userInfo = "")
    {
        if (isDebugging)
        {
            return;
        }

        TestInitialization();
        StartCoroutine(GetUserSessionId(gameId, versionId, userInfo));
    }

    private IEnumerator GetUserSessionId(int gameId, int versionId, string userInfo = "")
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        long nowMilliSec = (long)(System.DateTime.UtcNow - epochStart).TotalSeconds * 1000;

        string requestData = "?game_id=" + gameId + "&version_id=" + versionId + "&client_timestamp=" + nowMilliSec
            + "&user_info=" + userInfo;

        if(PlayerPrefs.HasKey("user_id"))
        {
            userId = PlayerPrefs.GetString("user_id");
            requestData += "&user_id=" + userId;
        }

        UnityWebRequest www = UnityWebRequest.Get(pageHost + phpPath + pageLoadPath + requestData);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string logReturnedString = www.downloadHandler.text;
            PageLoadData pageLoadData = JsonUtility.FromJson<PageLoadData>(logReturnedString);
            userId = pageLoadData.user_id;
            sessionId = pageLoadData.session_id;
            PlayerPrefs.SetString("user_id", userId);
            Debug.Log("userID: " + userId);
            Debug.Log("sessionID: " + sessionId);
        }

    }

    public int AssignABTestValue(int candidate)
    {
        if (!isDebugging)
        {
            Debug.Assert(userId != null, "Call Page load before assignABTestValue");
            if (PlayerPrefs.HasKey("ab_test_value"))
            {
                abstoredValue = PlayerPrefs.GetInt("ab_test_value");
            }
            else
            {
                abstoredValue = candidate;
                PlayerPrefs.SetInt("ab_test_value", abstoredValue);
            }

            abValueSet = true;  //true if this method is called;
            return abstoredValue;
        }
        else
        {
            return candidate;
        }
    }

    public void RecordABTestValue()
    {
        if (isDebugging)
        {
            return;
        }

        TestInitialization();
        Debug.Assert(abValueSet, "recordABTestValue: You must call assignABTestValue before recording the A/B test value.");
        StartCoroutine(GetABTestRecordRequest());
    }

    private IEnumerator GetABTestRecordRequest()
    {
        yield return new WaitUntil(() => (userId != null));

        string requestData = "?game_id=" + gameId + "&user_id=" + userId + "&abvalue=" + abstoredValue;

        UnityWebRequest www = UnityWebRequest.Get(pageHost + phpPath + playerABTestPath + requestData);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string logReturnedString = www.downloadHandler.text;
            Debug.Log(logReturnedString);
        }
    }

    private void TestInitialization()
    {
        Debug.Assert(gameId != -1, "Call initialize() / Initialize in the Editor mode before recording.");
    }

    private void Awake()
    {
        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;
            //If instance already exists and it's not this:
        }
        else if (instance != this)
        {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }
        
        if (!isDebugging)
        {
            if (Application.absoluteURL.Contains("https"))
            {
                pageHost = "https";
            }
        }
    }
}