﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInfoManager : Photon.MonoBehaviour
{
    [Header("TeamInfo")]
    public TeamInfo team1;
    public TeamInfo team2;

    [Header("SpawnPoints")]
    public Transform[] team1SpawnPoints;
    public Transform[] team2SpawnPoints;
    public string playerObject;
    GameObject yourPlayer;

    [Header("BasicInfo")]
    public int warmupTime;
    public int playersNeededATeam;
    public int roundTime;
    int currentRoundNumber;
    int waitingTime;
    int yourTeam;
    RoundType currentRoundType;
    public enum RoundType { Waiting,Warmup,Round}

    [Header("UI")]
    public Text time;
    public Text currentRound;
    public Text team1Wins, team2Wins;
    public Text team1Alive, team2Alive;
    public GameObject teams;

    public void Respawn()
    {
        Transform position;
        if (yourTeam == 1)
            position = team1SpawnPoints[Random.Range(0, team1SpawnPoints.Length)];
        else
            position = team2SpawnPoints[Random.Range(0, team2SpawnPoints.Length)];

        yourPlayer = PhotonNetwork.Instantiate(playerObject, position.position, position.rotation, 0);
    }

    [PunRPC]
    public void AskForJoiningInformation(string asker)
    {
        SerializeMatchData();
        photonView.RPC("GiveJoiningInformation", PhotonTargets.All, asker, waitingTime - 1, (currentRoundType == RoundType.Waiting) ? 1 : (currentRoundType == RoundType.Warmup) ? 2 : 3);
    }

    [PunRPC]
    public void GiveJoiningInformation(string asker, int seconds, int roundTime)
    {
        if(asker == PhotonNetwork.playerName)
        {
            currentRoundType = (roundTime == 1) ? RoundType.Waiting : (roundTime == 2) ? RoundType.Warmup : RoundType.Round;
            switch (currentRoundType)
            {
                case RoundType.Round:
                    StartCoroutine(CheckAlive(seconds));
                    break;
                case RoundType.Warmup:
                    StartCoroutine(WarmupTimer(seconds));
                    break;
            }
        }
    }
    
    //JoinTeamButton
    ///Calls the RPC and closes the teamPicking UI
    public void JoinTeamButton(int index)
    {
        photonView.RPC("JoinTeam", PhotonTargets.MasterClient, index, PhotonNetwork.playerName);
        yourTeam = index;
        teams.SetActive(false);
        Respawn();
    }

    //JoinTeamRPC
    ///Looks for the right Photonplayer and then saves it in the right team
    [HideInInspector]
    [PunRPC]
    public void JoinTeam(int index, string playerName)
    {
        PhotonPlayer[] players = PhotonNetwork.playerList;
        PlayerInfo inf = new PlayerInfo();
        foreach (PhotonPlayer player in players)
            if (player.NickName == playerName)
            {
                inf.playerInfo = player;
                break;
            }

        switch (index)
        {
            case 1:
                team1.players.Add(inf);
                break;
            case 2:
                team2.players.Add(inf);
                break;
        }

        SerializeMatchData();
    }

    //Start
    ///Start the coroutine if the player is the master client
    ///and it ads the player info to serialization
    public void Start()
    {
        if (PhotonNetwork.isMasterClient)
            StartCoroutine(CheckForEnoughPlayers());
        NetworkingPeer.RegisterType(typeof(PlayerInfo), 24, PlayerInfo.Serialize, PlayerInfo.Deserialize);
        currentRoundType = RoundType.Waiting;
    }

    //OnJoinedRoom
    ///Calls the startFunction
    ///Sometimes the player isnt connected while he is in the scene, this is a failsave
    public void OnJoinedRoom()
    {
        Start();
        if (!PhotonNetwork.isMasterClient)
            photonView.RPC("AskForJoiningInformation", PhotonTargets.MasterClient, PhotonNetwork.playerName);
    }

    //EnoughPlayerCheck
    ///Checks if there are enough players in both teams
    public IEnumerator CheckForEnoughPlayers()
    {
        bool b = true;
        while (b)
        {
            if (team1.players.Count >= playersNeededATeam && team2.players.Count >= playersNeededATeam)
                b = false;
            yield return null;
        }
        photonView.RPC("StartWarmup", PhotonTargets.All);
        currentRoundType = RoundType.Warmup;
    }

    //StartWarmupRPC
    ///Starts the warmup for each player
    [HideInInspector]
    [PunRPC]
    public void StartWarmup()
    {
        StartCoroutine(WarmupTimer(warmupTime));
    }

    //WarmupCountdown
    ///timer before the game starts
    public IEnumerator WarmupTimer(int remainingTime)
    {
        currentRound.text = "Warmup";
        for (int i = 0; i < remainingTime; i++)
        {
            waitingTime = warmupTime - i - (warmupTime - remainingTime);
            CalculateTime(waitingTime,time);
            yield return new WaitForSeconds(1);
        }
        StartGame();
    }

    //StartGameVoid
    ///Starts the first round
    public void StartGame()
    {
        StartCoroutine(CheckAlive(roundTime));
        currentRoundType = RoundType.Round;
    }

    public IEnumerator CheckAlive(int remainingTime)
    {
        currentRoundNumber++;
        currentRound.text = "Round: " + currentRoundNumber.ToString();
        for (int i = 0; i < remainingTime; i++)
        {
            waitingTime = roundTime - i - (roundTime - remainingTime);
            CalculateTime(waitingTime, time);
            yield return new WaitForSeconds(1);
        }
    }

    //CalculateTime
    ///Filters out the minutes from the given seconds
    public void CalculateTime(int seconds, Text input)
    {
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.RoundToInt(seconds - (min * 60f));
        input.text = min.ToString() + ":" + ((sec < 10) ? "0" : "") + sec.ToString();
    }

    //SerializeMatchData
    ///This is called if the master client wants to serialize the game data to all connected players
    public void SerializeMatchData()
    {
        List<PlayerInfo> team1Players = new List<PlayerInfo>();
        List<PhotonPlayer> team1PhotonPlayers = new List<PhotonPlayer>();
        foreach (PlayerInfo player in team1.players)
        {
            team1Players.Add(player);
            team1PhotonPlayers.Add(player.playerInfo);
        }

        List<PlayerInfo> team2Players = new List<PlayerInfo>();
        List<PhotonPlayer> team2PhotonPlayers = new List<PhotonPlayer>();
        foreach (PlayerInfo player in team2.players)
        {
            team2Players.Add(player);
            team2PhotonPlayers.Add(player.playerInfo);
        }

        photonView.RPC("DeserializeMatchData", PhotonTargets.All, team1Players.ToArray(), team1PhotonPlayers.ToArray(), team2Players.ToArray(), team2PhotonPlayers.ToArray(), team1.teamwins, team2.teamwins, currentRoundNumber);
    }

    //DeserializeMatchData
    ///This is what players recieve to get the right match info
    [HideInInspector]
    [PunRPC]
    public void DeserializeMatchData(PlayerInfo[] _team1Players, PhotonPlayer[] _team1Photon, PlayerInfo[] _team2Players, PhotonPlayer[] _team2Photon, int _team1Wins, int _team2Wins, int round)
    {
        team1.players = new List<PlayerInfo>();
        team1.teamwins = _team1Wins;
        for (int i = 0; i < _team1Players.Length; i++)
        {
            PlayerInfo info = _team1Players[i];
            info.playerInfo = _team1Photon[i];
            team1.players.Add(info);
        }

        team2.players = new List<PlayerInfo>();
        team2.teamwins = _team1Wins;
        for (int i = 0; i < _team2Players.Length; i++)
        {
            PlayerInfo info = _team2Players[i];
            info.playerInfo = _team2Photon[i];
            team2.players.Add(info);
        }

        //How many team1 players are alive
        int alive = 0;
        foreach (PlayerInfo player in team1.players)
            if (!player.isDead)
                alive++;
        team1Alive.text = alive.ToString() + " / " + team1.players.Count.ToString();
        //how many team2 players are alive
        alive = 0;
        foreach (PlayerInfo player in team2.players)
            if (!player.isDead)
                alive++;
        team2Alive.text = alive.ToString() + " / " + team2.players.Count.ToString();
        //Round?
        currentRoundNumber = round;
    }

    //PlayerInfo
    ///Stats for each player
    [System.Serializable]
    public class PlayerInfo
    {
        public bool isDead;
        public int kills;
        public int assists;
        public int deaths;
        public PhotonPlayer playerInfo;

        //Serialize
        ///To be able to sent this through photon you need to serialize it to a byte[]
        public static byte[] Serialize(object info)
        {
            PlayerInfo playerInf = (PlayerInfo)info;
            List<byte> bytes = new List<byte>();

            bytes.Add((byte)(playerInf.isDead? 1 : 2));
            bytes.Add((byte)playerInf.kills);
            bytes.Add((byte)playerInf.assists);
            bytes.Add((byte)playerInf.deaths);

            return bytes.ToArray();
        }

        //Deserialize
        ///Gets the values out of an byte[]
        public static PlayerInfo Deserialize(byte[] info)
        {
            PlayerInfo playerInf = new PlayerInfo();
            playerInf.isDead = (info[0] == 1);
            playerInf.kills = info[1];
            playerInf.assists = info[2];
            playerInf.deaths = info[3];
            return playerInf;
        }
    }

    //TeamInfo
    ///TeamStats and players
    [System.Serializable]
    public class TeamInfo
    {
        public int teamwins;
        public List<PlayerInfo> players;
    }
}
