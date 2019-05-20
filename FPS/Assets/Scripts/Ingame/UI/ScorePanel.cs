using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorePanel : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI score;
    public TextMeshProUGUI kills;
    public TextMeshProUGUI deaths;
    public TextMeshProUGUI assists;
    public TextMeshProUGUI ping;

    // public GameInfoManager.PlayerInfo playerInfo;



    void SetInfo(GameInfoManager.PlayerInfo info)
    {

        //Set info of the player
        playerName.text = info.playerInfo.NickName;
        score.text = "SCORE"; 
        kills.text = info.kills.ToString();
        deaths.text = info.deaths.ToString();
        assists.text = info.assists.ToString();

    }
}
