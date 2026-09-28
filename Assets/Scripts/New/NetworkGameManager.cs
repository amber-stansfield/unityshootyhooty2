using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
public class NetworkGameManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text testField;

    private void Update()
    {

        testField.text = NetworkManager.Singleton.ConnectedClients.Count.ToString();
    }


    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    public void StopConnection()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();
    }

    public string GetPlayerName()
    {
        
        return playerNameInput.text;
    }



}