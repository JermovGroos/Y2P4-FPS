using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class GameInfoManager : Photon.MonoBehaviour
{
    [Header("TeamInfo")]
    public TeamInfo team1;
    public TeamInfo team2;

    [Header("BasicInfo")]
    public int warmupTime;
    public int playersNeededATeam;
    public int roundTime;
    public bool allowRespawn;
    [HideInInspector]
    public ManagerBasicStuff basic;
    GameObject yourPlayer;

    [Header("NonInspectorVeriables")]
    [HideInInspector]public int currentRoundNumber;
    [HideInInspector] public int waitingTime;
    [HideInInspector] public int yourTeam;
    public RoundType currentRoundType;
    public enum RoundType { Waiting,Warmup,Round,RoundDelay}
    [HideInInspector] public bool stopRoundTimer;

    //PlayerKilled
    //!!ONLY SEND THIS TO THE MASTER CLIENT!!
    ///Call this when a player is killed.
    ///You need to give the player that has been killed and the killers name, aswell as all the damage done to the player to give an assist.
    [HideInInspector]
    [PunRPC]
    public virtual void PlayerKilled(string killed, string killer, float[] damageDone, string[] damageingPlayers)
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
        //Checks in what team the killed person was
        bool killedInTeam1 = false;
        foreach(PlayerInfo player in team1.players)
            if(player.playerInfo.NickName == killed)
            {
                player.deaths++;
                if(currentRoundType == RoundType.Round)
                {
                    player.isDead = true;
                    killedInTeam1 = true;
                }
                break;
            }
        if(!killedInTeam1)
            foreach (PlayerInfo player in team2.players)
                if (player.playerInfo.NickName == killed)
                {
                    player.deaths++;
                    if (currentRoundType == RoundType.Round)
                        player.isDead = true;
                    break;
                }
        //Gets the assisters name if there was an assist
        string assisterName = "";
        if (isAssisted)
        {
            int bestDamage = 0;
            for (int i = 0; i < damageDoneList.Count; i++)
                if (damageDoneList[i] >= damageDoneList[bestDamage])
                    bestDamage = i;
            assisterName = damageingPlayersList[bestDamage];
        }
        //Gives the kills and assists to the right player
        List<PlayerInfo> enemyTeam = killedInTeam1 ? team2.players : team1.players;
        foreach(PlayerInfo player in enemyTeam)
        {
            if (player.playerInfo.NickName == killer)
                player.kills++;
            if (isAssisted && assisterName == player.playerInfo.NickName)
                player.assists++;
        }

        if (killedInTeam1)
            team2.players = enemyTeam;
        else
            team1.players = enemyTeam;

        string message = killer + (isAssisted ? " + " + assisterName : "") + " Killed " + killed;
        photonView.RPC("KillFeed", PhotonTargets.All, message);
    }

    //Respawn
    ///Respawns you on the right team
    public void Respawn()
    {
        if (yourPlayer)
            PhotonNetwork.Destroy(yourPlayer);
        Transform position;
        if (yourTeam == 1)
            position = basic.team1SpawnPoints[Random.Range(0, basic.team1SpawnPoints.Length)];
        else
            position = basic.team2SpawnPoints[Random.Range(0, basic.team2SpawnPoints.Length)];

        yourPlayer = PhotonNetwork.Instantiate(basic.playerObject, position.position, position.rotation, 0);
    }

    //AskForJoinInfo
    ///This is called when a player joins to get the information to the player
    [HideInInspector]
    [PunRPC]
    public void AskForJoiningInformation(string asker)
    {
        SerializeMatchData();
        photonView.RPC("GiveJoiningInformation", PhotonTargets.All, asker, waitingTime - 1, (currentRoundType == RoundType.Waiting) ? 1 : (currentRoundType == RoundType.Warmup) ? 2 : (currentRoundType == RoundType.Round)? 3 : 4);
    }

    //GiveJoinInfo
    ///Send the information to the other Player
    [HideInInspector]
    [PunRPC]
    public virtual void GiveJoiningInformation(string asker, int seconds, int roundTime)
    {
        if(asker == PhotonNetwork.playerName)
        {
            currentRoundType = (roundTime == 1) ? RoundType.Waiting : (roundTime == 2) ? RoundType.Warmup : (roundTime == 3)? RoundType.Round : RoundType.RoundDelay;
            switch (currentRoundType)
            {
                case RoundType.Round:
                    StartCoroutine(RoundTimer(seconds));
                    break;
                case RoundType.Warmup:
                    StartCoroutine(WarmupTimer(seconds));
                    break;
            }
        }
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
    public virtual void Start()
    {
        basic = gameObject.GetComponent<ManagerBasicStuff>();
        if (PhotonNetwork.isMasterClient)
            StartCoroutine(CheckForEnoughPlayers());
        NetworkingPeer.RegisterType(typeof(PlayerInfo), 24, PlayerInfo.Serialize, PlayerInfo.Deserialize);
        currentRoundType = RoundType.Waiting;
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        if (PhotonNetwork.isMasterClient)
        {
            for (int i = 0; i < team1.players.Count; i++)
                if(team1.players[i].playerInfo.NickName == player.NickName)
                {
                    team1.players.RemoveAt(i);
                    break;
                }
            for (int i = 0; i < team2.players.Count; i++)
                if (team2.players[i].playerInfo.NickName == player.NickName)
                {
                    team1.players.RemoveAt(i);
                    break;
                }
            SerializeMatchData();
        }
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
        stopRoundTimer = false;
        basic.currentRound.text = "Warmup";
        for (int i = 0; i < remainingTime; i++)
        {
            waitingTime = warmupTime - i - (warmupTime - remainingTime);
            basic.CalculateTime(waitingTime, basic.time);
            yield return new WaitForSeconds(1);
        }
        StartGame();
    }

    //StartGameVoid
    ///Starts the first round
    public void StartGame()
    {
        if (yourTeam != 0)
            Respawn();
        allowRespawn = true;
        StartCoroutine(RoundTimer(roundTime));
        currentRoundType = RoundType.Round;
    }

    //RoundReset
    ///Resets the round veriables and respawns your player
    public void RoundReset()
    {
        stopRoundTimer = false;

        if (PhotonNetwork.isMasterClient)
        {
            foreach (PlayerInfo player in team1.players)
                player.isDead = false;
            foreach (PlayerInfo player in team2.players)
                player.isDead = false;
        }

        if (yourTeam != 0)
            Respawn();

        SerializeMatchData();
    }

    //RoundTimer
    ///The timer for a round
    ///This also calls the tied round ending if no team won
    public abstract IEnumerator RoundTimer(int remainingTime);

    //SerializeMatchData
    ///This is called if the master client wants to serialize the game data to all connected players
    public void SerializeMatchData()
    {
        if (PhotonNetwork.isMasterClient)
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

            photonView.RPC("DeserializeMatchData", PhotonTargets.All, team1Players.ToArray(), team1PhotonPlayers.ToArray(), team2Players.ToArray(), team2PhotonPlayers.ToArray(), team1.teamwins, team2.teamwins, currentRoundNumber, stopRoundTimer);
        }
    }

    //DeserializeMatchData
    ///This is what players recieve to get the right match info
    [HideInInspector]
    [PunRPC]
    public void DeserializeMatchData(PlayerInfo[] _team1Players, PhotonPlayer[] _team1Photon, PlayerInfo[] _team2Players, PhotonPlayer[] _team2Photon, int _team1Wins, int _team2Wins, int round, bool _stopTimer)
    {
        Debug.Log("Deserialized");
        stopRoundTimer = _stopTimer;
        team1.players = new List<PlayerInfo>();
        team1.teamwins = _team1Wins;
        for (int i = 0; i < _team1Players.Length; i++)
        {
            PlayerInfo info = _team1Players[i];
            info.playerInfo = _team1Photon[i];
            team1.players.Add(info);
        }

        team2.players = new List<PlayerInfo>();
        team2.teamwins = _team2Wins;
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
        basic.team1Alive.text = alive.ToString() + " / " + team1.players.Count.ToString();
        //how many team2 players are alive
        alive = 0;
        foreach (PlayerInfo player in team2.players)
            if (!player.isDead)
                alive++;
        basic.team2Alive.text = alive.ToString() + " / " + team2.players.Count.ToString();
        //Round?
        currentRoundNumber = round;
        basic.team1Wins.text = team1.teamwins.ToString();
        basic.team2Wins.text = team2.teamwins.ToString();
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