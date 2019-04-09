using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfoManager : MonoBehaviour
{
    

    [System.Serializable]
    public class PlayerInfo
    {
        public bool isDead;
        public int kills;
        public int assists;
        public int deaths;
    }

    [System.Serializable]
    public class TeamInfo
    {
        public int teamwins;
        public List<PlayerInfo> players;
    }
}
