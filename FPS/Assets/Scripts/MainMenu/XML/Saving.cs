using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.UI;

public class Saving : MonoBehaviour
{
    [Header("SaveInfo")]
    public string fileName;
    public SaveDataBase data;
    [Header("UI")]
    public GameObject nameInput, UI;
    public Text playerName;

    public int currentGamemodeIndex;

    //StartFunction
    ///Makes it so the object wont be destroyed && checks if there is a savefile by checking the name
    public void Start()
    {
        DontDestroyOnLoad(gameObject);
        LoadData();
        if (data.playerName == "")
            nameInput.SetActive(true);
        else
        {
            playerName.text = "Welcome " + data.playerName;
            PhotonNetwork.playerName = data.playerName;
            UI.SetActive(true);
        }
    }

    //QuitFunction
    ///Saves your data when you quit
    public void OnApplicationQuit()
    {
        SaveData();
    }

    //NameInputCheck
    ///Checks if you put in a name, and saves your name if you did
    public void EnteredName(InputField input)
    {
        if (input.text != "")
        {
            data.playerName = input.text;
            SaveData();
            nameInput.SetActive(false);
            playerName.text = "Welcome " + data.playerName;
            PhotonNetwork.playerName = data.playerName;
            UI.SetActive(true);
        }
    }

    //LoadData
    ///Loads your XML save file
    public void LoadData()
    {
        if (File.Exists(Application.persistentDataPath + "/" + fileName + ".xml"))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SaveDataBase));
            FileStream stream = new FileStream(Application.persistentDataPath + "/" + fileName + ".xml", FileMode.Open);
            data = (SaveDataBase)serializer.Deserialize(stream);
            stream.Close();
        }
        else
        {
            data = new SaveDataBase();
            data.lastLoadout = new WeaponCustomizer.WeaponLoadoutSlot();
            data.lastLoadout.weapon1 = new WeaponCustomizer.WeaponClassData();
            data.lastLoadout.weapon2 = new WeaponCustomizer.WeaponClassData();
            data.lastLoadout.weapon2.currentWeapon = 1;
        }
    }

    //SaveData
    ///Saves your data as a XML file
    public void SaveData()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SaveDataBase));
        FileStream stream = new FileStream(Application.persistentDataPath + "/" + fileName + ".xml", FileMode.Create);
        serializer.Serialize(stream, data);
        stream.Close();
    }

    //SaveInfo
    ///This is the info that will get saved
    [System.Serializable]
    public class SaveDataBase
    {
        public string playerName = "";
        public WeaponCustomizer.WeaponLoadoutSlot lastLoadout;
    }
}
