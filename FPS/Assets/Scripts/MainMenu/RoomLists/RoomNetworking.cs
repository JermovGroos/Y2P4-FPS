using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomNetworking : Photon.MonoBehaviour
{
    public string version;
    public string loadSeneName;
    public string savingTag;

    [Header("RoomsInfo")]
    public GameObject roomButton;
    public Transform roomButtonLayout;
    public string[] roomTypes;

    [Header("CustomRoom")]
    public InputField roomNameInput;
    public Slider playerAmount;
    public Text playerAmountValue;

    //ConnectForRooms
    ///Connects to a certain room type to get a certain type of room
    public void ConnectToRoomType(int roomType)
    {
        GameObject.FindWithTag(savingTag).GetComponent<Saving>().currentGamemodeIndex = roomType;
        Debug.Log("Connecting to: " + version + "_" + roomTypes[roomType]);
        PhotonNetwork.ConnectUsingSettings(version + "_" + roomTypes[roomType]);
        PhotonNetwork.automaticallySyncScene = true;
        foreach (Transform child in roomButtonLayout)
            Destroy(child.gameObject);
    }

    //Back
    ///Dissconnects you from photon and sends you to the main menu
    public void BackToMainMenu()
    {
        Debug.Log("Disconnected");
        PhotonNetwork.Disconnect();
    }

    //RoomList
    ///It Will clear the previous rooms and set new panels with the correct info in the layout
    ///This void is automatically called by photon
    public void OnReceivedRoomListUpdate()
    {
        foreach (Transform child in roomButtonLayout)
            Destroy(child.gameObject);
        foreach(RoomInfo room in PhotonNetwork.GetRoomList())
        {
            GameObject g = Instantiate(roomButton, roomButtonLayout);
            g.GetComponent<RoomButtonCode>().SetInfo(room);
        }
    }

    //SliderValueChange
    ///Changes the UIText to the slider value
    public void SliderChangedValue(int index)
    {
        playerAmountValue.text = playerAmount.value.ToString();
    }

    //CreateRoom
    ///Checks if you filled in a name and then creates a room
    public void CreateRoom()
    {
        if(roomNameInput.text != "")
        {
            RoomOptions ro = new RoomOptions() { MaxPlayers = (byte)playerAmount.value };
            PhotonNetwork.CreateRoom(roomNameInput.text, ro, TypedLobby.Default);
            PhotonNetwork.LoadLevelAsync(loadSeneName);
        }
    }
}
