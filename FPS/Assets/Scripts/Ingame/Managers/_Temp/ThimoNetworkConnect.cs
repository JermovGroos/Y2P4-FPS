using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThimoNetworkConnect : Photon.MonoBehaviour {
    [Header ("Networking")]
    public string gameVersion = "0.1";
    public string roomName = "ThimoTest";
    public int maxPlayers = 4;

    [Header ("Players")]
    public string playerResourceName;
    public Transform[] spawnPoints;

    // Start is called before the first frame update
    void Start () {
        Setup ();
    }

    void Setup () {
        StartCoroutine (Connect ());
    }

    IEnumerator Connect () {

        //Connect to main Photon server
        print ("Connecting...");
        PhotonNetwork.ConnectUsingSettings (gameVersion);

        //Wait until you are connected to the Photon server
        while (!PhotonNetwork.connectedAndReady) {
            yield return null;
        }

        //Connect to room
        RoomOptions roomOptions = new RoomOptions ();
        roomOptions.MaxPlayers = (byte) maxPlayers;
        PhotonNetwork.JoinOrCreateRoom (roomName, roomOptions, TypedLobby.Default);

        //Wait until you are connected to the room
        while (!PhotonNetwork.inRoom) {
            yield return null;
        }

        //Start the scene essentials
        print ("Connected!");
        UpNext ();

        yield break;
    }

    void UpNext () {
        SpawnPlayer();
    }

    void SpawnPlayer () {

        //Choose random spawn point
        int index = Random.Range (0, spawnPoints.GetLength (0) - 1);

        //Instantiate the player
        PhotonNetwork.Instantiate(playerResourceName, spawnPoints[index].position, spawnPoints[index].rotation, 0);
    }

}