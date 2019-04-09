using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomButtonCode : MonoBehaviour
{
    public RoomInfo info;
    [Header("UI")]
    public Text roomNameInput;
    public Text roomPlayerAmountInput;

    //SetInfo
    ///Calling this void will set the info of the room and fill in the nameInput and playeramountInput
    public void SetInfo(RoomInfo _info)
    {
        info = _info;
        roomNameInput.text = info.Name;
        roomPlayerAmountInput.text = "Players: " + info.PlayerCount + "/" + info.MaxPlayers;
    }
    //JoinRoom
    ///Joins a room if there is space
    public void OnButtonPressed()
    {
        if(info.PlayerCount != info.MaxPlayers)
            PhotonNetwork.JoinRoom(info.Name);
    }
}
