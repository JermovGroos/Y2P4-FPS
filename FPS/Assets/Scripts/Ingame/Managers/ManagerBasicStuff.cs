using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerBasicStuff : Photon.MonoBehaviour
{
    [Header("SpawnPoints")]
    public Transform[] team1SpawnPoints;
    public Transform[] team2SpawnPoints;
    public string playerObject;
    public Transform[] victoryScreenSpawnPoints;

    [Header("UI")]
    public Text time;
    public Text currentRound;
    public Text team1Wins, team2Wins;
    public Text team1Alive, team2Alive;
    public GameObject teams;
    public Text victoryTeamTextBar;

    [Header("Killfeed")]
    public GameObject killfeedPanel;
    public Transform killfeedLayout;
    public float killFeedMessageLifetime;

    [Header("Other")]
    public GameInfoManager manager;
    public string savingTag;
    public GameObject victoryScreenCamera;
    
    [Header("RoundInfoMessage")]
    public GameObject roundInfoPanel;
    public Text roundInfoText;
    public float roundInfoVisibleTime;
    public AudioClip roundMessageSound;
    public AudioSource audioSource;

    [HideInInspector]
    [PunRPC]
    public void SendRoundMessage(string message)
    {
        StartCoroutine(DisplayRoundMessage(message));
    }

    public IEnumerator DisplayRoundMessage(string _message)
    {
        roundInfoText.text = _message;
        roundInfoPanel.SetActive(true);
        audioSource.PlayOneShot(roundMessageSound);
        yield return new WaitForSeconds(roundInfoVisibleTime);
        roundInfoPanel.SetActive(false);
    }

    public void DisplayWinningTeam(int winningTeam)
    {
        victoryScreenCamera.SetActive(true);
        victoryTeamTextBar.gameObject.SetActive(true);
        victoryTeamTextBar.text = "Team" + winningTeam.ToString() + " Won";
        GameInfoManager.TeamInfo team = (winningTeam == 1)? manager.team1 : manager.team2;
        for (int i = 0; i < team.players.Count; i++)
            if (team.players[i].playerInfo.NickName == PhotonNetwork.playerName)
            {
                if (manager.yourPlayer)
                    PhotonNetwork.Destroy(manager.yourPlayer);
                manager.yourPlayer = PhotonNetwork.Instantiate(playerObject, victoryScreenSpawnPoints[i].position, victoryScreenSpawnPoints[i].rotation, 0);
                manager.yourPlayer.GetComponent<Player>().movementAllowed = false;
                break;
            }
    }
    
    //Killfeed
    ///This is called when someone has been killed so everyone knows it
    [HideInInspector]
    [PunRPC]
    public void KillFeed(string message)
    {
        GameObject g = Instantiate(killfeedPanel, killfeedLayout);
        g.GetComponent<Text>().text = message;
        Destroy(g, killFeedMessageLifetime);
    }

    public void Start()
    {
        GameInfoManager[] gamemodes = gameObject.GetComponents<GameInfoManager>();
        Saving save = GameObject.FindWithTag(savingTag).GetComponent<Saving>();
        for (int i = 0; i < gamemodes.Length; i++)
        {
            if (i != save.currentGamemodeIndex)
                Destroy(gamemodes[i]);
            else
                manager = gamemodes[i];
        }
    }

    //JoinTeamButton
    ///Calls the RPC and closes the teamPicking UI
    public void JoinTeamButton(int index)
    {
        photonView.RPC("JoinTeam", PhotonTargets.MasterClient, index, PhotonNetwork.playerName);
        manager.yourTeam = index;
        manager.basic.teams.SetActive(false);
        manager.Respawn();
    }

    //CalculateTime
    ///Filters out the minutes from the given seconds
    public void CalculateTime(int seconds, Text input)
    {
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.RoundToInt(seconds - (min * 60f));
        input.text = min.ToString() + ":" + ((sec < 10) ? "0" : "") + sec.ToString();
    }
}
