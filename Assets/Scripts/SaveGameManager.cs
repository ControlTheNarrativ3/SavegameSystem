using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SaveGameManager : MonoBehaviour // This class is used to manage the saving and loading of the game and the persistence of the objects
{
    public static SaveGameManager Instance { get; private set; } // The instance of the SaveGameManager

    private List<ISerializable> savedObjects = new List<ISerializable>(); // The list of all the save objects

    public UnityEvent OnSave; // the unity event that is called when the game is saved

    public UnityEvent OnLoad; // the unity event that is called when the game is loaded

    private void Awake() // Awake is called when the script instance is being loaded
    {
        // If there is no instance of the SaveGameManager then set this instance to the instance of the SaveGameManager
        // and don't destroy it when loading a new scene
        if (!Instance && Instance != this)
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            JObject savedGameObject = new JObject(); // The object that will contain all the saved objects data
            for (int i = 0; i < savedObjects.Count; i++) // Loop through all the saved objects
            {
                var curObjects = savedObjects[i]; // The current object
                JObject serializedEnemy = curObjects.Serialize(); // The serialized object data that hold the Enemy.cs script
                savedGameObject.Add(curObjects.GetId(), serializedEnemy); // Add the serialized object data to the saved game object with the id as the key and the serialized object data as the value
            }
            SaveGame(savedGameObject.ToString()); // Save the game with the saved game object data as a string (I.E: the json string)
        }

        //load game
        if (Input.GetKeyDown(KeyCode.L))
        {
            string loadedGame = LoadGame(); // Load the game and get the saved game object data as a string (I.E: the json string)
            JObject loadedJObject = JObject.Parse(loadedGame); // Parse the json string into a JObject

            for (int i = 0; i < savedObjects.Count; i++) // Loop through all the saved objects
            {
                var curObjects = savedObjects[i]; // The current object
                if (loadedJObject.ContainsKey(curObjects.GetId())) // If the saved game object data contains the id of the current object
                {
                    curObjects.Deserialize(loadedJObject[curObjects.GetId()].ToString()); // Deserialize the current object with the saved game object data of the current object
                }
            }
        }
    }

    public void RegisterPersistence(ISerializable serializableGameObject) // This method is used to register the object with the SaveGameManager
    {
        if (!savedObjects.Contains(serializableGameObject)) // If the saved objects list doesn't contain the object
        {
            savedObjects.Add(serializableGameObject); // Add the object to the saved objects list
        }
    }

    public void UnregisterPersistence(ISerializable serializableGameObject) // This method is used to unregister the object with the SaveGameManager
    {
        if (savedObjects.Contains(serializableGameObject)) // If the saved objects list contains the object
        {
            savedObjects.Remove(serializableGameObject); // Remove the object from the saved objects list
        }
    }

    private void SaveGame(string jsonString) // the save game methed that saves the game
    {
        // create a string with the save file path
        string filePath = Application.persistentDataPath + "/saveGame.sav";
        // log the file path to the console
        Debug.Log("saving to " + filePath + "\n" + jsonString);
        // create an array of bytes from the json string
        // and Encrypt the save game data before saving it to the file
        byte[] saveGame = Encrypt(jsonString);
        File.WriteAllBytes(filePath, saveGame); // write the array of bytes to the file path
        OnSave?.Invoke(); // invoke the OnSave unity event
    }

    private string LoadGame() // the load game method that loads the game
    {
        string filePath = Application.persistentDataPath + "/saveGame.sav"; // create a string with the save file path
        Debug.Log("loading from " + filePath); // log the file path to the console
        byte[] encryptedSaveGame = File.ReadAllBytes(filePath); // read the file at the file path and create an array of bytes from it
        return Decrypt(encryptedSaveGame); // return the decrypted array of bytes as a string
        OnLoad?.Invoke(); // invoke the OnLoad unity event
    }

    private byte[] _key =
    {
        0x01,
        0x02,
        0x03,
        0x04,
        0x05,
        0x06,
        0x07,
        0x08,
        0x09,
        0x10,
        0x11,
        0x12,
        0x13,
        0x14,
        0x15,
        0x16
    };

    private byte[] _initializationVector =
    {
        0x01,
        0x02,
        0x03,
        0x04,
        0x05,
        0x06,
        0x07,
        0x08,
        0x09,
        0x10,
        0x11,
        0x12,
        0x13,
        0x14,
        0x15,
        0x16
    };

    /*---------------------------------------------------This method is used to encrypt the save game data ---------------------------------------------------------*/
    // this method returns an Encrypted array of bytes from the string we pass to it (the save game file)
    byte[] Encrypt(string message)
    {
        AesManaged aesManaged = new AesManaged(); // create a new instance of the AesManaged class
        ICryptoTransform encryptor = aesManaged.CreateEncryptor(_key, _initializationVector); // create an encryptor from the AesManaged class
        MemoryStream memoryStream = new MemoryStream(); // create a new instance of the MemoryStream class
        CryptoStream cryptoStream = new CryptoStream(
            memoryStream,
            encryptor,
            CryptoStreamMode.Write
        ); // create a new instance of the CryptoStream class with the memory stream, encryptor and CryptoStreamMode.Write as parameters
        StreamWriter streamWriter = new StreamWriter(cryptoStream); // create a new instance of the StreamWriter class with the crypto stream as a parameter
        streamWriter.Write(message); // write the message to the stream writer
        streamWriter.Close(); // close the stream writer
        cryptoStream.Close(); // close the crypto stream
        memoryStream.Close(); // close the memory stream
        return memoryStream.ToArray(); // return the memory stream as an array of bytes
    }

    /*---------------------------------------------------This method is used to decrypt the save game data ---------------------------------------------------------*/
    // this methos a Decrypted string of Encrypted byte array we pass to it (the array of bytes from the save game file)
    string Decrypt(byte[] message)
    {
        AesManaged aesManaged = new AesManaged(); // create a new instance of the AesManaged class
        ICryptoTransform decrypter = aesManaged.CreateDecryptor(_key, _initializationVector); // create a decrypter from the AesManaged class
        MemoryStream memoryStream = new MemoryStream(message); // create a new instance of the MemoryStream class with the message as a parameter
        CryptoStream cryptoStream = new CryptoStream(
            memoryStream,
            decrypter,
            CryptoStreamMode.Read
        ); // create a new instance of the CryptoStream class with the memory stream, decrypter and CryptoStreamMode.Read as parameters
        StreamReader streamReader = new StreamReader(cryptoStream); // create a new instance of the StreamReader class with the crypto stream as a parameter
        var decryptedMessage = streamReader.ReadToEnd(); // read the stream reader to the end and store it in a string
        streamReader.Close(); // close the stream reader
        cryptoStream.Close(); // close the crypto stream
        memoryStream.Close(); // close the memory stream
        return decryptedMessage; // return the decrypted message as a string
    }
}
