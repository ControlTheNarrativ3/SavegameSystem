using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour, ISerializable
{
    public int hP; // The health of the enemy
    public string type; // The type of the enemy

    private void Start()
    {
        SaveGameManager.Instance.RegisterPersistence(this); // Register the enemy with the SaveGameManager
    }

    public string GetId()
    {
        return name; // Return the name of the enemy as the id
    }

    public JObject Serialize()
    {
        string jsonString = JsonUtility.ToJson(this); // Serialize the enemy with the JsonUtility
        return JObject.Parse(jsonString); // Parse the json string into a JObject and return it
    }

    public void Deserialize(string jsonString)
    {
        JsonUtility.FromJsonOverwrite(jsonString, this); // Deserialize the enemy with the JsonUtility and overwrite the current enemy
    }

    private void OnDestroy()
    {
        SaveGameManager.Instance.UnregisterPersistence(this); // Unregister the enemy with the SaveGameManager when the enemy is destroyed
    }
}
