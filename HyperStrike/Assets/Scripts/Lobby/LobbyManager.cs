using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LobbyState : byte
{
    NONE = 0,
    CONNECTING,
    WAIT,
    COMPLETED
}

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public NetworkVariable<LobbyState> State { get; private set; } = new NetworkVariable<LobbyState>(LobbyState.NONE);

    public NetworkVariable<float> currentWaitTime = new NetworkVariable<float>(5.0f);

    float waitTime = 10f;

    private IEnumerator completedTimerCoroutine;

    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private List<GameObject> charactersPrefabs;

    public override void OnNetworkSpawn()
    {
        if (IsServer) NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if (!IsServer) return;

        GameObject ball = Instantiate(ballPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        ball.GetComponent<NetworkObject>().Spawn(true);

        SetLobbyState(LobbyState.CONNECTING);
    }

    void OnClientConnected(ulong clientId)
    {
        Characters[] enumValues = (Characters[])System.Enum.GetValues(typeof(Characters));
        var character = (byte)UnityEngine.Random.Range(0, (enumValues.Length - 1));
        //var character = (byte)Characters.CRASHWALL;

        if(character != (byte)Characters.NONE)
        {
            GameObject player = Instantiate(charactersPrefabs[character], Vector3.zero, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 6)
        {
            SetLobbyState(LobbyState.WAIT);
        }
        else if (NetworkManager.Singleton.ConnectedClientsIds.Count > 6)
        {
            NetworkManager.DisconnectClient(clientId, "Server is full");
        }
    }
    
    void OnClientDisconnected(ulong clientId)
    {
        if (IsClient)
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 6)
        {
            SetLobbyState(LobbyState.CONNECTING);
        }
    }

    void Awake()
    {
        // Destroy, we dont want it to be in the other scenes, not needed
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && State.Value != LobbyState.WAIT)
        {
            SetLobbyState(LobbyState.WAIT);
        }
    }

    void LobbyStateBehaviour()
    {
        switch (State.Value)
        {
            case LobbyState.NONE:
            case LobbyState.CONNECTING:
                {
                    if (completedTimerCoroutine != null)
                        StopCoroutine(completedTimerCoroutine);

                    currentWaitTime.Value = waitTime;

                    completedTimerCoroutine = LobbyCompletedTimer();
                }
                break;
            case LobbyState.WAIT:
                {
                    // Once all 6 players are in the lobby, init Coroutine with timer, then change scene in server.
                    StartCoroutine(completedTimerCoroutine);
                }
                break;
            case LobbyState.COMPLETED:
                {
                    var status = NetworkManager.Singleton.SceneManager.LoadScene("ArenaRoom", LoadSceneMode.Single);
                    if (status != SceneEventProgressStatus.Started)
                    {
                        Debug.LogWarning($"Failed to load Arena Room Scene" +
                              $"with a {nameof(SceneEventProgressStatus)}: {status}");
                    }
                }
                break;
            default:
                break;
        }
    }

    public void SetLobbyState(LobbyState state)
    {
        if (IsServer && State.Value != state)
        {
            State.Value = state;
            LobbyStateBehaviour();
        }
    }

    private IEnumerator LobbyCompletedTimer()
    {
        while (currentWaitTime.Value >= 0.0f)
        {
            yield return new WaitForSeconds(1f);
            currentWaitTime.Value--;
        }

        SetLobbyState(LobbyState.COMPLETED);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}
