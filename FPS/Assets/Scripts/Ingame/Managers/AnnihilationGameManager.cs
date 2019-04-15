using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnihilationGameManager : GameInfoManager
{
    [Header("CustomGameModeVariables")]
    public int nextRoundDelayTime;
    public int pointsToWin;

    public override void Start()
    {
        base.Start();
    }

    [HideInInspector]
    [PunRPC]
    public override void PlayerKilled(string killed, string killer, float[] damageDone, string[] damageingPlayers)
    {
        base.PlayerKilled(killed, killer, damageDone, damageingPlayers);


        if (currentRoundType == RoundType.Round)
            CheckIfSOmeoneWon();
    }


    [HideInInspector]
    [PunRPC]
    public override void GiveJoiningInformation(string asker, int seconds, int roundTime)
    {
        base.GiveJoiningInformation(asker, seconds, roundTime);
        if (currentRoundType == RoundType.RoundDelay)
            StartCoroutine(NextRoundTimer(seconds));
    }

    //NextRoundTimer
    ///Delay before the next round starts, this gives time to send a message for who won that round.
    public IEnumerator NextRoundTimer(int remainingTime)
    {
        if (!CheckIfGameWon())
        {
            currentRoundType = RoundType.RoundDelay;
            basic.currentRound.text = "Round: " + currentRoundNumber.ToString();
            for (int i = 0; i < remainingTime; i++)
            {
                waitingTime = nextRoundDelayTime - i - (nextRoundDelayTime - remainingTime);
                basic.CalculateTime(waitingTime, basic.time);
                yield return new WaitForSeconds(1);
            }

            RoundReset();

            StartCoroutine(RoundTimer(roundTime));
            currentRoundType = RoundType.Round;
        }
        else if(PhotonNetwork.isMasterClient)
        {
            int teamWinner = (team1.teamwins == pointsToWin) ? 1 : 2;
            photonView.RPC("GameWon", PhotonTargets.All, teamWinner);
        }
    }

    [HideInInspector]
    [PunRPC]
    public override void GameWon(int winnerIndex)
    {
        basic.DisplayWinningTeam(winnerIndex);
    }

    public override IEnumerator RoundTimer(int remainingTime)
    {
        currentRoundNumber++;
        basic.currentRound.text = "Round: " + currentRoundNumber.ToString();
        for (int i = 0; i < remainingTime; i++)
        {
            if (stopRoundTimer)
                break;
            waitingTime = roundTime - i - (roundTime - remainingTime);
            basic.CalculateTime(waitingTime, basic.time);
            yield return new WaitForSeconds(1);
        }

        StartCoroutine(NextRoundTimer(nextRoundDelayTime));
        if (!stopRoundTimer && PhotonNetwork.isMasterClient)
            photonView.RPC("SendRoundEnding", PhotonTargets.All, 3);

        SerializeMatchData();
    }

    //CheckIfWon
    ///Checks if a team is dead so it can see if someone won
    public void CheckIfSOmeoneWon()
    {
        bool team1Defeated = true;
        foreach (PlayerInfo player in team1.players)
            if (!player.isDead)
                team1Defeated = false;

        bool team2Defeated = true;
        foreach (PlayerInfo player in team2.players)
            if (!player.isDead)
                team2Defeated = false;

        if (team1Defeated)
            photonView.RPC("SendRoundEnding", PhotonTargets.All, 2);
        else if (team2Defeated)
            photonView.RPC("SendRoundEnding", PhotonTargets.All, 1);
        else
            SerializeMatchData();
    }

    public override bool CheckIfGameWon()
    {
        if (team1.teamwins == pointsToWin || team2.teamwins == pointsToWin)
            return true;

        return false;
    }

    //SendRoundEnding
    ///This is called by the MasterClient to giev everyone a message that the round is won or tied
    [HideInInspector]
    [PunRPC]
    public void SendRoundEnding(int roundEndingCase)
    {

        stopRoundTimer = true;
        switch (roundEndingCase)
        {
            //Team1Wins
            case 1:
                team1.teamwins++;
                Debug.Log("Team1Wins");
                break;
            //Team2Wins
            case 2:
                team2.teamwins++;
                Debug.Log("Team2Wins");
                break;
            //Tie
            case 3:
                Debug.Log("Tie");
                break;
        }
        CheckIfGameWon();
        SerializeMatchData();
    }
}
