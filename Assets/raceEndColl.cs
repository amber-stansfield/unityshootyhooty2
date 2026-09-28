using HelloWorld;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class raceEndColl : NetworkBehaviour
{

    private int numberFinished;
    HelloWorldManager manager;
    [SerializeField] GameObject textChap;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("netHomie").GetComponent<HelloWorldManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (manager.raceInProgress && other.CompareTag("Player"))
        {
            NetworkObjectReference dudeRef = other.GetComponentInParent<NetworkObject>();
            CheckIfAllPlayersFinishedServerRpc(dudeRef);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void CheckIfAllPlayersFinishedServerRpc(NetworkObjectReference dudeRef)
    {
        if (dudeRef.TryGet(out NetworkObject dude))
        {
            dude.GetComponentInChildren<moveyment>().completeTime = Time.time - dude.GetComponentInChildren<moveyment>().raceStartTime;
        }
        numberFinished++;
        Debug.Log($"hmmmhm {numberFinished}");
        if (numberFinished >= NetworkManager.Singleton.ConnectedClients.Count)
        {
            
            numberFinished = 0;
            manager.raceInProgress = false;
            float bestTime = Mathf.Infinity;
            NetworkObject bestDude = null;
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                var gaming = client.Value.PlayerObject.GetComponentInChildren<moveyment>();
                
                Debug.Log(client.Value.PlayerObject.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text);
                Debug.Log(gaming.completeTime);
                if (gaming.completeTime < bestTime)
                {
                    bestTime = gaming.completeTime;
                    bestDude = client.Value.PlayerObject;
                    Debug.Log(bestDude.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text);
                }
            }

            textChap.GetComponent<TMP_Text>().text = bestDude.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text;
            textChap.GetComponent<TMP_Text>().text += ($"\n{Math.Round(bestTime,2).ToString()}");

            NetworkObjectReference bestDudeRef = bestDude;
            UpdateEndBoardClientRpc(bestTime, textChap.GetComponent<TMP_Text>().text);



            Debug.Log($"gaminger {bestTime}");

        }
        
    }

    [ClientRpc]
    void UpdateEndBoardClientRpc(float bestTime,string dudeName)
    {
            textChap.GetComponent<TMP_Text>().text = dudeName;
            //textChap.GetComponent<TMP_Text>().text += ($"\n{bestTime.ToString()}");
        
    }

}
