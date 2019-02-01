using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

// Manages shared state and keeps our game in tip top shape. 
// https://www.youtube.com/watch?v=ZdGrC9S4PYA
public class GameManager : MonoBehaviour
{
    //Static instance of GameManager which allows it to be accessed by any other script. 
    public static GameManager instance = null;

    // Player A/B Test
    public int abValue;

    public int currentSceneId = 0;
    public int collectibleTotal = 0; 

    // Key Entities to Keep Track Of:
    public GameObject frog;

    // Checkpoints and Death: 
    public Checkpoint levelStart; 

    private Checkpoint currentCheckpoint;
    
    public Checkpoint CurrentCheckpoint
    {
        get
        {
            return currentCheckpoint;
        }
        set
        {
            if(currentCheckpoint == value) return; 
            currentCheckpoint = value;

            var payload = new CheckpointSetPayload();
            payload.checkpointName = currentCheckpoint.gameObject.name;
            payload.respawnPoint = currentCheckpoint.respawnPoint.position;
            payload.enterPosition = frog.transform.position;
            var payloadJSON = JsonUtility.ToJson(payload);

            //Print.Log("[GM] Setting Checkpoint " + payloadJSON);
            LoggingManager.instance.RecordEvent(Actions.CHECKPOINT_SET, payloadJSON); 
        }
    }

    // Player position logging
    private float lastInputTime;
    private float loggingDelta = 2f;
    private float ceil;
    private List<PlayerPositionPayload> positionData;

    public void addCollectable(Collectible c){
        //Print.Log("[GM] Got Collectible!");
        collectibleTotal++;

        var payload = new CollectiblePayload();
        payload.worldPos = c.transform.position; 
        payload.newScore = collectibleTotal; 
        var payloadJSON = JsonUtility.ToJson(payload);

        //Print.Log("[GM] Setting Checkpoint " + payloadJSON);
        LoggingManager.instance.RecordEvent(Actions.COLLECTIBLE_OBTAINED, payloadJSON);

        AudioManager.instance.Play("collectible");
        SetScoreText();
    }

    void SetScoreText(){
        if(ScoreDisplay.instance != null){
            ScoreDisplay.instance.GetComponent<Text>().text = "Score: " + collectibleTotal;
        }
    }

    void FetchReferences(){
        currentCheckpoint = null; 
        InitializePositionLogging();
        SetScoreText();
    }

    //Awake is always called before any Start functions
    void Awake()
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
            //Then destroy this. This enforces our singleton pattern,
            //meaning there can only ever be one instance of a GameManager.

            // Hopefully this doesn't cause terrible things to happen
            instance.frog = this.frog; 
            instance.levelStart = this.levelStart; 

            Destroy(gameObject);
        }
    }

    void Start()
    {

        // Initialize Logging
        LoggingManager.instance.Initialize(
            Constants.GAME_ID,
            Constants.VERSION_ID, 
            Constants.IS_DEBUGGING
        );

        
        // Set AB Test Value
        // abValue 1: SwingForce 8.5  
        // abValue 2: SwingForce multiplier * swingforce
        int randVal = Random.Range(1,3);
        Print.Log("random value" + randVal);
        abValue = LoggingManager.instance.AssignABTestValue(randVal);
        LoggingManager.instance.RecordABTestValue();
        frog.GetComponent<PlayerController>().setABValue();

        // Get References to Game Objects in Scene
        FetchReferences();

        // Okay the game has loaded:
        LoggingManager.instance.RecordPageLoad(); 
        LoggingManager.instance.RecordLevelStart(currentSceneId);

        // Start the MUSIC!
        AudioManager.instance.Play("bg");
    }

    public void InitializePositionLogging(){
         // Initialize position logging
        lastInputTime = Time.time;
        ceil = 0f;
        positionData = new List<PlayerPositionPayload>();
    }

    public void UploadPositionPayload(){
        // Log position data and flush buffer
        // TODO: JSON CONVERSION OF SHAME 
        var payloadJSON = "[" + 
            System.String.Join(",", positionData.Select(payload => JsonUtility.ToJson(payload)).ToArray())
            + "]";
        LoggingManager.instance.RecordEvent(Actions.PLAYER_WORLD_POS, payloadJSON.ToString());
        InitializePositionLogging();
    }
    
    public void Update(){
        // Logging player position
        if(Input.anyKeyDown){
            // Update latest time of player pressing any button
            lastInputTime = Time.time;
            //Print.Log("[GM]Input detected.");
        }
        float timePassed = Time.time - lastInputTime;
        float newCeil = Mathf.Ceil(loggingDelta * timePassed/Mathf.Pow(timePassed,0.3f))/loggingDelta;
        // magic numbers!
        //Print.Log(timePassed + "," + ceil + "," + newCeil); 
        if(timePassed <= 30f && ceil < newCeil) {
            var payload = new PlayerPositionPayload();
            payload.worldPos = frog.transform.position; 
            payload.time = Time.time;
            positionData.Add(payload);
            ceil = newCeil;
        }
    }

    // Game Lifecycle: 
    public void RespawnPlayer(string type = "")
    {   
        // Death audio
        AudioManager.instance.Play("lose");

        var payload = new PlayerDeathPayload();
        payload.location = frog.transform.position; 
        payload.type = type; 

        var payloadJSON = JsonUtility.ToJson(payload);
        //Print.Log("[GM] Respawning Player...");
        //Print.Log(payloadJSON); 

        // Reset Frog Position
        IEnumerator respawn = frog.GetComponent<PlayerController>().Respawn();
        StartCoroutine(respawn);

        LoggingManager.instance.RecordEvent(Actions.PLAYER_DEATH, payloadJSON);

        // Log position data and flush buffer
        UploadPositionPayload();
    }

    public void LoadScene(int sceneId = -1) {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    private IEnumerator LoadSceneAsync(int sceneId)
    {   
        yield return new WaitForSecondsRealtime(1);
        Print.Log("[GM] Loading Scene..."); 

        if(currentSceneId == -1 && !Constants.IS_DEBUGGING){
            var storedId = PlayerPrefs.GetInt("sceneID", sceneId); 
            // Avoid Main Menu Loops, check to make sure the player isn't a hotshot
            if(storedId != -1){
                sceneId = storedId; 
                collectibleTotal = PlayerPrefs.GetInt("collectibleTotal", 0);
            }
        }

        string sceneName;
        // Get Scene Name

        if(Constants.LEVEL_DIRECTORY.TryGetValue(sceneId, out sceneName)){
            // Yay we got it :) 
        }else{
           Print.Log("[GM] Could not load sceneId (" + sceneId + "), aborting..."); 
           yield break;
        }

        Print.Log("[GM] Loading Scene {" + sceneId + "," + sceneName +"}"); 
        
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Send the player's final position information up to the mothership 
        UploadPositionPayload();
        FetchReferences();

        if(!(Constants.IS_DEBUGGING)){
            // Store the Player's Current Scene
            PlayerPrefs.SetInt("sceneID", sceneId); 
            PlayerPrefs.SetInt("collectibleTotal", collectibleTotal); 
        }

        // Okay we're in, emit the correct logging events: 
        LoggingManager.instance.RecordLevelEnd();
        LoggingManager.instance.RecordLevelStart(sceneId); 
        currentSceneId = sceneId;
    }
}