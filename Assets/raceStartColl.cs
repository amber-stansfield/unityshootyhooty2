using HelloWorld;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class raceStartColl : NetworkBehaviour
{

    HelloWorldManager manager;
    private HashSet<GameObject> playerList = new HashSet<GameObject>();

    [SerializeField] GameObject timerDisplay;
    private bool routineRunning;
    // Start is called before the first frame update
    void Start()
    {
        manager =  GameObject.Find("netHomie").GetComponent<HelloWorldManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }


    [ServerRpc(RequireOwnership = false)]
    void CheckToStartServerRpc()
    {

        if (playerList.Count == NetworkManager.Singleton.ConnectedClients.Count && !routineRunning)
        {
            StartCoroutine(TimerToStartRace(NetworkManager.Singleton.ConnectedClients.Count));
        }
    }


    IEnumerator TimerToStartRace(int count)
    {
        routineRunning = true;
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(1.2f);
            timerDisplay.GetComponent<TMP_Text>().text = (3 - i).ToString();
            updateStartTimerClientRpc(timerDisplay.GetComponent<TMP_Text>().text);
            if (playerList.Count != count)
            {
                routineRunning = false;
                yield break;
            }
        }
        timerDisplay.GetComponent<TMP_Text>().text = "gooooooo";
        updateStartTimerClientRpc(timerDisplay.GetComponent<TMP_Text>().text);
        manager.BeginRaceServerRpc();
        routineRunning = false;
    }

    [ClientRpc]
    void updateStartTimerClientRpc(string text)
    {
        timerDisplay.GetComponent<TMP_Text>().text = text;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerList.Add(other.gameObject);
        }
        CheckToStartServerRpc();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerList.Remove(other.gameObject);
        }
    }

}
