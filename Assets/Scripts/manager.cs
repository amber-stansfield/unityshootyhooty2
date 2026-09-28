using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Collections;
using static System.Net.Mime.MediaTypeNames;


namespace HelloWorld
{


    public class HelloWorldManager : NetworkBehaviour
    {
        [SerializeField] TextMeshProUGUI textMoment;
        [SerializeField] TMP_InputField dudeTextField;
        private NetworkManager m_NetworkManager;
        [SerializeField] TMP_InputField usernameInput;
        [SerializeField] TextMeshProUGUI inGameUserList;

        [SerializeField] GameObject startButton;
        [SerializeField] GameObject playerPrefab;
        [SerializeField] GameObject mainMenuHolder;
        [SerializeField] GameObject subMenuHolder;

        public bool raceInProgress;
        public GameObject raceStartDude;
        public GameObject raceEndDude;

        public NativeArray<FixedString32Bytes> names = new NativeArray<FixedString32Bytes>(8,Allocator.Persistent);
        private void Awake()
        {
            m_NetworkManager = GetComponent<NetworkManager>();

        }

  

        private async void Start()
        {
            DontDestroyOnLoad(this);
            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F))
            {
                
            }
        }

        public async void JoinRelay()
        {
            if (usernameInput.text == null || usernameInput.text.Length < 3 || usernameInput.text.Length > 10)
            {
                return;
            }
            await StartClientWIthRelay(dudeTextField.text);
            mainMenuHolder.SetActive(false);
            subMenuHolder.SetActive(true);
            
            //AddToUserListServerRpc();
        }


        public async void StartRelay()
        {
            if (usernameInput.text == null || usernameInput.text.Length < 3 || usernameInput.text.Length > 10)
            {
                return;
            }
            string joinCode = await StartHostWithRelay();
            textMoment.text = joinCode;
            mainMenuHolder.SetActive(false);
            startButton.SetActive(true);
            subMenuHolder.SetActive(true);
        }

        private async Task<string> StartHostWithRelay(int maxConnections = 3)
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            var relayServerData = allocation.ToRelayServerData("udp");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            await AuthenticationService.Instance.UpdatePlayerNameAsync(usernameInput.text);
            string gamers = AuthenticationService.Instance.PlayerName.Substring(0, AuthenticationService.Instance.PlayerName.Length - 5);
            StartCoroutine(callTheDude(gamers));

            return NetworkManager.Singleton.StartHost() ? joinCode : null;
        }


        private async Task<bool> StartClientWIthRelay(string joinCode)
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = joinAllocation.ToRelayServerData("udp");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData((relayServerData));
            mainMenuHolder.SetActive(false);
            subMenuHolder.SetActive(true);
            await AuthenticationService.Instance.UpdatePlayerNameAsync(usernameInput.text);
            FixedString32Bytes gamers = (FixedString32Bytes)AuthenticationService.Instance.PlayerName.Substring(0, AuthenticationService.Instance.PlayerName.Length - 5);
            StartCoroutine(callTheDude(gamers));
            //askforConnectedClientsServerRpc(name);
            //AddPlayerToReg((ulong)NetworkManager.ConnectedClients.Count + 3, gamers);

            return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();

        }
        void AddPlayerToReg(ulong playerIndex, FixedString32Bytes playerName)
        {
            names[(int)playerIndex] = playerName;
        }


        [ServerRpc(RequireOwnership = false)]
        void askforConnectedClientsServerRpc(FixedString32Bytes name)
        {
            int count = NetworkManager.ConnectedClients.Count;
            returnConnectedClientRpc(count, name);
        }

        [ClientRpc]
        void returnConnectedClientRpc(int count, FixedString32Bytes name)
        {
            names[(int)((ulong)count + 2)] = name;
            Debug.Log($" utter gaming {names[(int)((ulong)count + 2)]}");
        }


        IEnumerator callTheDude(FixedString32Bytes input)
        {
            if (IsServer || IsHost)
            {
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                yield return new WaitForSeconds(3);
            }

            askforConnectedClientsServerRpc(input);
            AddToUserListServerRpc(input);
        }

        [ServerRpc(RequireOwnership = false)]
        void AddToUserListServerRpc(FixedString32Bytes gamer)
        {
            inGameUserList.text += $"\n {gamer}";
            UpdateClientUserListClientRpc(inGameUserList.text,names);
        }

        [ClientRpc]
        void UpdateClientUserListClientRpc(string input, NativeArray<FixedString32Bytes> serverNames)
        {
            if (IsServer)
            {
                return;
            }
            names = serverNames;
            inGameUserList.text = input;

        }



        [ServerRpc(RequireOwnership = false)]
        public void BeginRaceServerRpc()
        {
            raceInProgress = true;
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                client.Value.PlayerObject.GetComponentInChildren<moveyment>().raceStartTime = Time.time;
                NetworkObjectReference dudeRef = client.Value.PlayerObject;
                BeginRaceClientRpc(dudeRef);
            }

        }

        [ClientRpc]
        public void BeginRaceClientRpc(NetworkObjectReference dudeRef)
        {
            if (!IsOwner) { return; }
            if (dudeRef.TryGet(out NetworkObject dude))
            {
                dude.GetComponentInChildren<moveyment>().raceStartTime = Time.time;
                
                Debug.Log($"gaming hath commenced at {Time.time}");
            }
            
        }





        public void gamerPre()
        {
            StartCoroutine(gamer());
        }

        private IEnumerator gamer()
        {
            loadingSceneClientRpc();
            yield return new WaitForSeconds(0.01f);
            spawnDudesServerRpc();

        }

        [ServerRpc]
        void spawnDudesServerRpc()
        {
            int i = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                GameObject playerInstance = Instantiate(playerPrefab);
                NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
                networkObject.SpawnAsPlayerObject(client.Key);
                Debug.Log(names[(int)((ulong)i + 3)]);
                

                int plap = Random.Range(0, 1);
                if (plap == 0)
                {
                    networkObject.transform.position = new Vector3(2.81999993f, 1.07000005f, -23.75f);
                }
                else
                {
                    networkObject.transform.position = new Vector3(6.11999989f, 1.39999998f, -22.3899994f);
                }

                networkObject.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = names[(int)((ulong)i + 3)].ToString();
                Debug.Log($" what poppin {networkObject.transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text}");
                i++;
            }

            GameObject raceStartObj = Instantiate(raceStartDude);
            raceStartObj.GetComponent<NetworkObject>().Spawn();

            GameObject raceEndObj = Instantiate(raceEndDude);
            raceEndObj.GetComponent<NetworkObject>().Spawn();
            //StartCoroutine(SetNamePlateCaller());
        }

        IEnumerator SetNamePlateCaller()
        {
            yield return new WaitForSeconds(3);
            SetNamePlatesServerRpc();
        }

        [ServerRpc]
        void SetNamePlatesServerRpc()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                //client.Value.PlayerObject.GetComponentInChildren<TMP_Text>().text = names[(ulong)i + 3];

                //client.Value.PlayerObject
                client.Value.PlayerObject.transform.position = new Vector3(1145, 1521, 12);
                
                //.text = names[(ulong)i + 3];
            }
        }

        [ClientRpc]
        void loadingSceneClientRpc()
        {
            SceneManager.LoadScene("Lab 1");
        }
        private void StartButtons()
        {
            if (GUILayout.Button("Host")) StartRelay();
        }

        //private IEnumerator loadDudes()
        //{
            
        //}
        private void StatusLabels()
        {
            var mode = m_NetworkManager.IsHost ?
                "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        



        //private void SubmitNewPosition()
        //{

        //    if (m_NetworkManager.IsServer && !m_NetworkManager.IsClient)
        //    {
        //        foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
        //        m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<dude>().Move();
        //    }
        //    else
        //    {

        //        var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
        //        var player = playerObject.GetComponent<shmoovement>();
        //        player.Move();


        //    }


        //}
    }
}