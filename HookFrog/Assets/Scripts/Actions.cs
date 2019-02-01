using System; 
using UnityEngine;

public static class Actions {
    public const int PLAYER_DEATH = 0;
    public const int CHECKPOINT_SET = 1;
    public const int PLAYER_WORLD_POS = 2;
    public const int COLLECTIBLE_OBTAINED = 3; 
}

// Payloads 
// JSON payload sent to the server for each action

[Serializable]
public struct PlayerDeathPayload{
    public Vector3 location;
    public string type; 
}

[Serializable]
public struct CheckpointSetPayload{
    public string checkpointName;
    public Vector3 respawnPoint; 
    public Vector3 enterPosition; 
}

[Serializable]
public struct PlayerPositionPayload{
    public Vector3 worldPos;
    public float time; 
}

[Serializable]
public struct CollectiblePayload{
    public Vector3 worldPos; 
    public int newScore;
}