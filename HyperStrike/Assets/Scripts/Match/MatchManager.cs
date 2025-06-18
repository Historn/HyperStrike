using HyperStrike;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MatchState : byte
{
    NONE = 0,
    CHARACTER_SELECTION,
    RESET,
    WAIT,
    INIT,
    PLAY,
    GOAL,
    FINALIZED
}

public enum Characters : byte
{
    SPEED,
    CRASHWALL,
    NANOFLOW,
    NONE
}

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance { get; private set; }

    public event Action OnDisplayCharacterSelection;
    public event Action OnUpdateMatchScore;

    public NetworkVariable<MatchState> State { get; private set; } = new NetworkVariable<MatchState>(MatchState.NONE);
    public NetworkVariable<bool> allowMovement = new NetworkVariable<bool>(false);
    public NetworkList<ulong> LocalPlayersID;
    public NetworkList<ulong> VisitantPlayersID;

    public NetworkVariable<float> closeMatchGameTime = new NetworkVariable<float>(30.0f);

    #region "Coroutines"
    private IEnumerator characterSelectTimerCoroutine;
    private IEnumerator initTimerCoroutine;
    private IEnumerator matchTimerCoroutine;
    private IEnumerator closeMatchGameTimerCoroutine;
    #endregion

    #region "Character Selection"
    [Header("Character Selection")]
    public List<GameObject> charactersPrefabs;
    [SerializeField] private List<Transform> spawnPositions;
    public NetworkList<byte> CharacterSelected;
    public NetworkVariable<float> characterSelectionTime = new NetworkVariable<float>(90.0f);
    #endregion

    #region "WAIT TIME"
    [Header("Wait Conditions")]
    float waitTime = 5f; // Wait for 5 seconds
    public NetworkVariable<float> currentWaitTime = new NetworkVariable<float>(5.0f);
    #endregion

    #region "MATCH VARIABLES"
    [Header("Match Conditions")]
    public float maxTime = 300f; // 300 = 5 minutes in seconds
    public NetworkVariable<float> currentMatchTime = new NetworkVariable<float>(300.0f);
    public NetworkVariable<int> localGoals = new NetworkVariable<int>(0);
    public NetworkVariable<int> visitantGoals = new NetworkVariable<int>(0);
    #endregion

    [SerializeField] private GameObject ballPrefab;
    private GameObject currentBall;

    public override void OnNetworkSpawn()
    {
        // SYNCHRONIZATION EVENT PROCESS
        if (IsServer) NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
        else if (IsClient) NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

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

        CharacterSelected = new NetworkList<byte>();
        LocalPlayersID = new NetworkList<ulong>();
        VisitantPlayersID = new NetworkList<ulong>();

        if (IsServer)
        {
            State.Value = MatchState.NONE;
            allowMovement.Value = false;
        }
    }

    private void OnClientDisconnected(ulong obj)
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
#if UNITY_EDITOR
        if (NetworkManager.Singleton?.ConnectedClientsList.Count > 0)
        {
            SetMatchState(MatchState.CHARACTER_SELECTION);
        }
#else
        if (NetworkManager.Singleton?.ConnectedClientsList.Count > 5)
        {
            SetMatchState(MatchState.CHARACTER_SELECTION);
        }
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && State.Value != MatchState.CHARACTER_SELECTION)
        {
            SetMatchState(MatchState.CHARACTER_SELECTION);
        }
    }

    void MatchStateBehavior()
    {
        switch (State.Value)
        {
            case MatchState.NONE:
            case MatchState.CHARACTER_SELECTION:
                {
                    /*DESPAWN EACH TEAM PLAYER PREFABS TO SPAWN POSITIONS*/
                    foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
                    {
                        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject?.Despawn();
                    }

                    // ASSIGN PLAYERS TO A TEAM HARDCODED NOW
                    for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
                    {
                        if ((i % 2 == 0) && LocalPlayersID.Count < 3)
                        {
                            LocalPlayersID.Add(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
                        }
                        else if ((i % 2 != 0) && VisitantPlayersID.Count < 3)
                        {
                            VisitantPlayersID.Add(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
                        }

                        if (CharacterSelected.Count < 6) CharacterSelected.Add((byte)Characters.NONE);
                    }

                    // DISPLAY CHARACTER SELECTION WHEN ALL PLAYERS ARE CONNECTED AND SYNCED
                    OnDisplayCharacterSelection?.Invoke();

                    characterSelectTimerCoroutine = CharacterSelectionTimer();

                    if (characterSelectTimerCoroutine != null)
                        StartCoroutine(characterSelectTimerCoroutine);
                }
                break;
            case MatchState.RESET:
                {
                    if (characterSelectTimerCoroutine != null)
                        StopCoroutine(characterSelectTimerCoroutine);

                    /*SPAWN EACH TEAM PLAYER PREFABS TO SPAWN POSITIONS*/
                    for (int i = 0; i < CharacterSelected.Count; i++)
                    {
                        var character = CharacterSelected[i];

                        Characters[] enumValues = (Characters[])System.Enum.GetValues(typeof(Characters));

                        if (character == (byte)Characters.NONE && i < NetworkManager.Singleton.ConnectedClientsList.Count)
                            character = (byte)UnityEngine.Random.Range(0, (enumValues.Length - 2));

                        if (character != (byte)Characters.NONE && charactersPrefabs[character] != null && spawnPositions[i] != null)
                        {
                            GameObject player = Instantiate(charactersPrefabs[character], spawnPositions[i].position, spawnPositions[i].rotation);
                            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsList[i].ClientId, true);
                        }
                    }

                    ResetMatch();
                    SetMatchState(MatchState.WAIT);
                }
                break;
            case MatchState.WAIT:
                {
                    if (matchTimerCoroutine != null)
                        StopCoroutine(matchTimerCoroutine);

                    // PUT PLAYERS AND BALL TO INIT POS
                    ResetPositions();

                    // AND WAIT SOME OF SECONDS
                    SetMatchState(MatchState.INIT);
                }
                break;
            case MatchState.INIT:
                {
                    currentWaitTime.Value = waitTime;

                    initTimerCoroutine = PlayMatch(); // recreate the IEnumerator

                    if (initTimerCoroutine != null)
                        StartCoroutine(initTimerCoroutine);
                }
                break;
            case MatchState.PLAY:
                {
                    for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
                    {
                        var player = NetworkManager.Singleton.SpawnManager.PlayerObjects[i];
                        player.GetComponent<Rigidbody>().isKinematic = false;
                    }

                    allowMovement.Value = true;

                    currentBall.GetComponent<Rigidbody>().isKinematic = false;

                    if (initTimerCoroutine != null)
                        StopCoroutine(initTimerCoroutine);

                    matchTimerCoroutine = MatchTimer();

                    // Starts timer
                    if (matchTimerCoroutine != null)
                        StartCoroutine(matchTimerCoroutine);

                    // HANDLE PLAY BEHAVIOR --> GOALS
                }
                break;
            case MatchState.GOAL:
                {
                    Debug.Log($"Local: {localGoals.Value} - {visitantGoals.Value} :Visitant");
                    OnUpdateMatchScore?.Invoke();
                    SetMatchState(MatchState.WAIT);
                }
                break;
            case MatchState.FINALIZED:
                {
                    // Stop the match timer coroutine
                    if (matchTimerCoroutine != null)
                    {
                        StopCoroutine(matchTimerCoroutine);
                    }

                    allowMovement.Value = false;

                    closeMatchGameTime.Value = 3f;
                    closeMatchGameTimerCoroutine = CloseMatchGameTimer();

                    if (closeMatchGameTimerCoroutine != null)
                        StartCoroutine(closeMatchGameTimerCoroutine);

                    Debug.Log("Match Ended!");
                    Debug.Log($"Final Score: Local {localGoals.Value} - {visitantGoals.Value} Visitant");
                    // STOP THE PLAYERS AND BALL
                    // Show UI to the players depending if they won or lost
                }
                break;
            default:
                break;
        }
    }

    public void SetMatchState(MatchState state)
    {
        if (IsServer && State.Value != state)
        {
            State.Value = state;
            MatchStateBehavior();
        }
    }

    private IEnumerator CharacterSelectionTimer()
    {
        while (characterSelectionTime.Value >= 0.0f)
        {
            yield return new WaitForSeconds(1f);
            characterSelectionTime.Value--;
        }

        SetMatchState(MatchState.RESET);
    }

    public void SelectCharacter(Characters character)
    {
        SelectCharacterRpc(character, NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    public void SelectCharacterRpc(Characters character, ulong clientID)
    {
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        {
            if (NetworkManager.Singleton.ConnectedClientsList[i].ClientId.Equals(clientID))
            {
                CharacterSelected[i] = (byte)character;
            }
        }
    }

    private IEnumerator PlayMatch()
    {
        while (currentWaitTime.Value >= 0.0f)
        {
            yield return new WaitForSeconds(1f);
            currentWaitTime.Value--;
        }

        SetMatchState(MatchState.PLAY);
    }

    void ResetMatch()
    {
        // Reset Match vars
        localGoals.Value = 0;
        visitantGoals.Value = 0;
        currentWaitTime.Value = waitTime;
        currentMatchTime.Value = maxTime;
    }

    void ResetPositions()
    {
        allowMovement.Value = false;

        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
        {
            var player = NetworkManager.Singleton.SpawnManager.PlayerObjects[i];
            Rigidbody rb = player.GetComponent<Rigidbody>();
            
            rb.isKinematic = true;
            rb.position = spawnPositions[i].position;
            rb.rotation = Quaternion.LookRotation(spawnPositions[i].forward);
        }

        GameObject ball = Instantiate(ballPrefab, new Vector3(0, 5, 0), Quaternion.identity);
        ball.transform.localScale = new Vector3(3, 3, 3);
        ball.GetComponent<NetworkObject>().Spawn(true);
        ball.GetComponent<Rigidbody>().isKinematic = true;
        currentBall = ball;
    }

    private IEnumerator MatchTimer()
    {
        while (currentMatchTime.Value > 0 || localGoals.Value == visitantGoals.Value)
        {
            yield return new WaitForSeconds(1f);
            currentMatchTime.Value--;
        }

        SetMatchState(MatchState.FINALIZED);
    }

    private IEnumerator CloseMatchGameTimer()
    {
        while (closeMatchGameTime.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            closeMatchGameTime.Value--;
        }

        NetworkManager.Singleton.Shutdown();

        var status = NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load Main Menu Scene" +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
    }

    public float GetCurrentCharSelectTime()
    {
        return characterSelectionTime.Value;
    }

    public float GetCurrentWaitTime()
    {
        return currentWaitTime.Value;
    }

    public float GetCurrentMatchTime()
    {
        return currentMatchTime.Value;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoaded;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}
