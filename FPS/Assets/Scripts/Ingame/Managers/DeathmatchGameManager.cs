using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathmatchGameManager : GameInfoManager
{
    public List<PlayerInfo> players;
    public Transform[] spawnPoints;
    public float spawnRange;
    public LayerMask spawnLayer;

   [PunRPC, HideInInspector]
    public override void GameWon(int winnerIndex)
    {
        
    }

    public override IEnumerator RoundTimer(int remainingTime)
    {
        Respawn();
        if (PhotonNetwork.isMasterClient)
            photonView.RPC("SendRoundMessage", PhotonTargets.All, "Round has started.");
        basic.currentRound.text = "Score: 0";
        for (int i = 0; i < remainingTime; i++)
        {
            waitingTime = roundTime - i - (roundTime - remainingTime);
            basic.CalculateTime(waitingTime, basic.time);
            yield return new WaitForSeconds(1);

        }

        SerializeMatchData();
        CheckIfGameWon();
    }

    public override bool CheckIfGameWon()
    {
        int mostKills = 0;
        for (int i = 0; i < players.Count; i++)
            if (players[i].kills >= players[mostKills].kills)
                mostKills = i;
        photonView.RPC("GameWon", PhotonTargets.All, mostKills);
        return true;
    }

    public override void Start()
    {
        base.Start();
        basic.teams.SetActive(false);
        basic.team1Wins.gameObject.SetActive(false);
        basic.team1Alive.gameObject.SetActive(false);
        basic.team2Wins.gameObject.SetActive(false);
        basic.team2Alive.gameObject.SetActive(false);
    }
    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.isMasterClient)
            photonView.RPC("AskForJoiningInformation", PhotonTargets.MasterClient, PhotonNetwork.playerName);
        else
        {
            PlayerInfo inf = new PlayerInfo();
            inf.playerInfo = PhotonNetwork.player;
            players.Add(inf);
            StartCoroutine(CheckForEnoughPlayers());
        }
        Respawn();
    }

    public void OnPhotonPlayerConnected(PhotonPlayer joinedPlayer)
    {
        if (PhotonNetwork.isMasterClient)
        {
            PlayerInfo inf = new PlayerInfo();
            inf.playerInfo = joinedPlayer;
            players.Add(inf);
            SerializeMatchData();
        }
    }

    public override void Respawn()
    {
        if (yourPlayer)
            PhotonNetwork.Destroy(yourPlayer);
        int index = 0;
        while (true)
        {
            index = Random.Range(0, spawnPoints.Length);
            if (!Physics.CheckSphere(spawnPoints[index].transform.position, spawnRange, spawnLayer))
            {
                Transform spawnPos = spawnPoints[index];
                yourPlayer = PhotonNetwork.Instantiate(basic.playerObject, spawnPos.position, spawnPos.rotation, 0);
                break;
            }
        }
    }

    public override IEnumerator CheckForEnoughPlayers()
    {
        bool b = true;
        Debug.Log("StartedCheckingForPlaying");
        while (b)
        {
            if (players.Count >= playersNeededATeam)
                b = false;
            yield return null;
        }
        photonView.RPC("StartWarmup", PhotonTargets.All);
        currentRoundType = RoundType.Warmup;
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        for (int i = 0; i < players.Count; i++)
            if(players[i].playerInfo.NickName == player.NickName)
            {
                players.RemoveAt(i);
                break;
            }
    }

    [PunRPC,HideInInspector]
    public override void PlayerKilled(string killed, string killer, float[] damageDone, string[] damageingPlayers)
    {
        //Checks if there was an assist
        bool isAssisted = false;
        List<float> damageDoneList = new List<float>(damageDone);
        List<string> damageingPlayersList = new List<string>(damageingPlayers);
        if (damageingPlayers.Length > 1)
        {
            isAssisted = true;
            for (int i = 0; i < damageingPlayersList.Count; i++)
                if (damageingPlayersList[i] == killer)
                {
                    damageDoneList.RemoveAt(i);
                    damageingPlayersList.RemoveAt(i);
                    break;
                }
        }

        string assisterName = "";
        if (isAssisted)
        {
            int bestDamage = 0;
            for (int i = 0; i < damageDoneList.Count; i++)
                if (damageDoneList[i] >= damageDoneList[bestDamage])
                    bestDamage = i;
            assisterName = damageingPlayersList[bestDamage];
        }

        foreach(PlayerInfo player in players)
            if (player.playerInfo.NickName == killed)
                player.deaths++;
            else if (player.playerInfo.NickName == killer)
                player.kills++;
            else if (isAssisted && player.playerInfo.NickName == assisterName)
                player.assists++;

        string message = killer + (isAssisted ? " + " + assisterName : "") + " Killed " + killed;
        photonView.RPC("KillFeed", PhotonTargets.All, message);

        SerializeMatchData();
    }

    public override void SerializeMatchData()
    {
        if (PhotonNetwork.isMasterClient)
            photonView.RPC("DeserializeDeathMatchData", PhotonTargets.All, players.ToArray());
    }

    [PunRPC,HideInInspector]
    public void DeserializeDeathMatchData(PlayerInfo[] playerInfos)
    {
        players = new List<PlayerInfo>(playerInfos);

        foreach (PlayerInfo player in players)
            if (player.playerInfo.NickName == PhotonNetwork.playerName)
            {
                basic.currentRound.text = "Score: " + player.kills;
                break;
            }
    }
}
