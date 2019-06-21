using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Scoreboard : MonoBehaviour
{

    public List<GameObject> uiPanels;
    public GameObject scoreboardUI;
    public GameObject team1PanelParent;
    public GameObject team2PanelParent;
    public GameObject panelPrefab;
    public GameInfoManager manager;
    public int hertz = 10;

    private void OnEnable()
    {
        StartCoroutine(UpdateAnnhaliationScoreboard());
    }



    private IEnumerator UpdateAnnhaliationScoreboard()
    {
        while (true)
        {

            //Destroy previous score panels
            for (int i = uiPanels.Count; i > 0; i--)
            {
                Destroy(uiPanels[i - 1]);
                uiPanels.RemoveAt(i - 1);
            }

            foreach(GameInfoManager.PlayerInfo info in manager.team1.players.OrderBy(x => x.kills)) {

                //Create panel
                string playerName = info.playerInfo.NickName;
                string kills = info.kills.ToString();
                string deaths = info.deaths.ToString();
                string assists = info.assists.ToString();

                //Instantiate panel and set text
                GameObject newPanel = Instantiate(panelPrefab, team1PanelParent.transform);
                newPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerName;
                newPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = kills;
                newPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = deaths;
                newPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = assists;

                //Add it to the list
                uiPanels.Add(newPanel);
            }

            foreach (GameInfoManager.PlayerInfo info in manager.team2.players.OrderBy(y => y.kills)) {

                //Create panel
                string playerName = info.playerInfo.NickName;
                string kills = info.kills.ToString();
                string deaths = info.deaths.ToString();
                string assists = info.assists.ToString();

                //Instantiate panel and set text
                GameObject newPanel = Instantiate(panelPrefab, team1PanelParent.transform);
                newPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = playerName;
                newPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = kills;
                newPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = deaths;
                newPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = assists;

                //Add it to the list
                uiPanels.Add(newPanel);
            }

            //Wait for update
            yield return new WaitForSeconds((float)hertz / 60);
        }


    }

    private void OnDisable()
    {
        StopCoroutine(UpdateAnnhaliationScoreboard());
    }
}
