using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathmatchGameManager : GameInfoManager
{
    public List<PlayerInfo> players;
    public Transform[] spawnPoints;
    public float spawnRange;

    public override void GameWon(int winnerIndex)
    {
        
    }

    public override IEnumerator RoundTimer(int remainingTime)
    {
        if (PhotonNetwork.isMasterClient)
            photonView.RPC("SendRoundMessage", PhotonTargets.All, "Round has started.");
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
        //GameWonCheck
        return false;
    }

    public override void Start()
    {
        base.Start();
        basic.teams.SetActive(false);
        if (PhotonNetwork.isMasterClient)
        {
            PlayerInfo inf = new PlayerInfo();
            inf.playerInfo = PhotonNetwork.player;
            players.Add(inf);
        }
    }
    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.isMasterClient)
            photonView.RPC("AskForJoiningInformation", PhotonTargets.MasterClient, PhotonNetwork.playerName);
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
            if (!Physics.CheckSphere(spawnPoints[index].transform.position, spawnRange))
            {
                Transform spawnPos = spawnPoints[index];
                yourPlayer = PhotonNetwork.Instantiate(basic.playerObject, spawnPos.position, spawnPos.rotation, 0);
                break;
            }
        }
    }
}
