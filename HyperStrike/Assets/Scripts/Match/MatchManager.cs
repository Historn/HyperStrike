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
    public event Action<Team> OnMatchEnded;

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
    [SerializeField] private List<Transform> localSpawnPositions;
    [SerializeField] private List<Transform> visitantSpawnPositions;
    public NetworkList<byte> LocalCharacterSelected;
    public NetworkList<byte> VisitantCharacterSelected;
    public NetworkVariable<float> currentCharacterSelectionTime = new NetworkVariable<float>(90.0f);
    public float characterSelectionTime = 90f;

    bool localsReady = false;
    bool visitantsReady = false;
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
    public NetworkVariable<Team> winnerTeam = new NetworkVariable<Team>(0);
    #endregion

    [Header("Ball")]
    [SerializeField] private GameObject ballPrefab;
    private GameObject currentBall;
    bool first = true; 
    public override void OnNetworkSpawn()
    {
        // SYNCHRONIZATION EVENT PROCESS
        if (NetworkManager.Singleton == null) return;

        if (IsServer)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApproval;
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
            //LocalCharacterSelected.OnListChanged += OnCharacterSelectedReadyCheck;
            //VisitantCharacterSelected.OnListChanged += OnCharacterSelectedReadyCheck;
        }
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

        LocalCharacterSelected = new NetworkList<byte>();
        VisitantCharacterSelected = new NetworkList<byte>();
        LocalPlayersID = new NetworkList<ulong>();
        VisitantPlayersID = new NetworkList<ulong>();

        if (IsServer)
        {
            State.Value = MatchState.NONE;
            allowMovement.Value = false;
        }
    }

    private void ConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name == "ArenaRoom")
        {
            response.Approved = false;
        }
        else
        {
            response.Approved = true;
        }
    }

    private void OnClientDisconnected(ulong obj)
    {
        StartCoroutine(LoadMenuDelayed());
    }

    private IEnumerator LoadMenuDelayed()
    {
        yield return new WaitForSeconds(1.0f);
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
        if (NetworkManager.Singleton?.ConnectedClientsList.Count > 5 && first)
        {
            SetMatchState(MatchState.CHARACTER_SELECTION);
            first = false;
        }
#endif
    }

    void Update()
    {
#if DEV_CLIENT
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (State.Value == MatchState.CHARACTER_SELECTION)
            {
                LowerCharacterSelectionTimeRpc();
            }
            else if (State.Value != MatchState.CHARACTER_SELECTION)
            {
                SetMatchStateRpc(MatchState.CHARACTER_SELECTION);
            }
        }
#endif
    }

    void MatchStateBehavior()
    {
        switch (State.Value)
        {
            case MatchState.NONE:
            case MatchState.CHARACTER_SELECTION:
                {
                    StopAllCoroutines();

                    LocalCharacterSelected.Clear();
                    VisitantCharacterSelected.Clear();
                    LocalPlayersID.Clear();
                    VisitantPlayersID.Clear();

                    /*DESPAWN EACH TEAM PLAYER PREFABS TO SPAWN POSITIONS*/
                    foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
                    {
                        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject?.Despawn(true);
                    }

                    if (currentBall != null)
                    {
                        currentBall.GetComponent<NetworkObject>().Despawn(true);
                    }

                    GameObject ball = Instantiate(ballPrefab, new Vector3(0, 5, 0), Quaternion.identity);
                    ball.transform.localScale = new Vector3(3, 3, 3);
                    ball.GetComponent<NetworkObject>().Spawn(true);
                    ball.GetComponent<Rigidbody>().isKinematic = true;
                    currentBall = ball;

                    // DISPLAY CHARACTER SELECTION WHEN ALL PLAYERS ARE CONNECTED AND SYNCED
                    OnDisplayCharacterSelection?.Invoke();

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

                        if (LocalCharacterSelected.Count < 3) LocalCharacterSelected.Add((byte)Characters.NONE);
                        if (VisitantCharacterSelected.Count < 3) VisitantCharacterSelected.Add((byte)Characters.NONE);
                    }

                    currentCharacterSelectionTime.Value = characterSelectionTime;
                    characterSelectTimerCoroutine = CharacterSelectionTimer();

                    if (characterSelectTimerCoroutine != null)
                        StartCoroutine(characterSelectTimerCoroutine);
                }
                break;
            case MatchState.RESET:
                {
                    if (characterSelectTimerCoroutine != null)
                        StopCoroutine(characterSelectTimerCoroutine);

                    for (int i = 0; i < LocalCharacterSelected.Count; i++)
                    {
                        var character = LocalCharacterSelected[i];

                        Characters[] enumValues = (Characters[])System.Enum.GetValues(typeof(Characters));

                        if (character == (byte)Characters.NONE && i < LocalPlayersID.Count)
                            character = (byte)UnityEngine.Random.Range(0, (enumValues.Length - 1));

                        if (character != (byte)Characters.NONE && charactersPrefabs[character] != null)
                        {
                            ulong id = LocalPlayersID[i];

                            GameObject playerGO = Instantiate(charactersPrefabs[character], localSpawnPositions[i].position, localSpawnPositions[i].rotation);
                            if (playerGO.TryGetComponent<Player>(out Player player))
                            {
                                player.Team.Value = Team.LOCAL;
                            }
                            playerGO.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                        }
                    }

                    for (int i = 0; i < VisitantCharacterSelected.Count; i++)
                    {
                        var character = VisitantCharacterSelected[i];

                        Characters[] enumValues = (Characters[])System.Enum.GetValues(typeof(Characters));

                        if (character == (byte)Characters.NONE && i < VisitantPlayersID.Count)
                            character = (byte)UnityEngine.Random.Range(0, (enumValues.Length - 1));

                        if (character != (byte)Characters.NONE && charactersPrefabs[character] != null)
                        {
                            ulong id = VisitantPlayersID[i];

                            GameObject playerGO = Instantiate(charactersPrefabs[character], visitantSpawnPositions[i].position, visitantSpawnPositions[i].rotation);
                            if (playerGO.TryGetComponent<Player>(out Player player))
                            {
                                player.Team.Value = Team.VISITANT;
                            }
                            playerGO.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
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
                    Debug.Log("Init the Match with Wait Timer");
                    if (initTimerCoroutine != null)
                        StartCoroutine(initTimerCoroutine);
                }
                break;
            case MatchState.PLAY:
                {
                    foreach (var player in NetworkManager.Singleton.SpawnManager.PlayerObjects)
                    {
                        if (player.TryGetComponent<Rigidbody>(out var rb))
                        {
                            rb.isKinematic = false;
                        }
                    }

                    allowMovement.Value = true;

                    if (currentBall != null && currentBall.TryGetComponent<Rigidbody>(out var ballRb))
                    {
                        ballRb.isKinematic = false;
                    }

                    if (initTimerCoroutine != null)
                        StopCoroutine(initTimerCoroutine);

                    matchTimerCoroutine = MatchTimer();

                    if (matchTimerCoroutine != null)
                        StartCoroutine(matchTimerCoroutine);
                }
                break;
            case MatchState.GOAL:
                {
                    OnUpdateMatchScore?.Invoke();
                    if (localGoals.Value != visitantGoals.Value && currentMatchTime.Value < 0.0f)
                    {
                        SetMatchState(MatchState.FINALIZED);
                    }
                    else
                    {
                        SetMatchState(MatchState.WAIT);
                    }
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

                    ResetPositions();

                    if (visitantGoals.Value > localGoals.Value) winnerTeam.Value = Team.VISITANT;

                    closeMatchGameTime.Value = 5f;
                    closeMatchGameTimerCoroutine = CloseMatchGameTimer();

                    if (closeMatchGameTimerCoroutine != null)
                        StartCoroutine(closeMatchGameTimerCoroutine);

                    OnMatchEnded?.Invoke(winnerTeam.Value);
                    Debug.Log("Match Ended!");
                    Debug.Log($"Final Score: Local {localGoals.Value} - {visitantGoals.Value} Visitant");
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

    [Rpc(SendTo.Server)]
    public void SetMatchStateRpc(MatchState state)
    {
        SetMatchState(state);
    }

    [Rpc(SendTo.Server)]
    public void LowerCharacterSelectionTimeRpc()
    {
        currentCharacterSelectionTime.Value = 10f;
    }

    private IEnumerator CharacterSelectionTimer()
    {
        while (currentCharacterSelectionTime.Value >= 0.0f)
        {
            yield return new WaitForSeconds(1f);
            currentCharacterSelectionTime.Value--;
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
        for (int i = 0; i < LocalPlayersID.Count; i++)
        {
            if (LocalPlayersID[i].Equals(clientID))
            {
                LocalCharacterSelected[i] = (byte)character;
            }
        }

        for (int i = 0; i < VisitantPlayersID.Count; i++)
        {
            if (VisitantPlayersID[i].Equals(clientID))
            {
                VisitantCharacterSelected[i] = (byte)character;
            }
        }
    }

    private void OnCharacterSelectedReadyCheck(NetworkListEvent<byte> changeEvent)
    {
        if (State.Value != MatchState.CHARACTER_SELECTION || !IsServer) return;

        if (!LocalCharacterSelected.Contains((byte)Characters.NONE)) localsReady = true;
        else localsReady = false;

        if (!VisitantCharacterSelected.Contains((byte)Characters.NONE)) visitantsReady = true;
        else visitantsReady = false;

        if (localsReady && visitantsReady && currentCharacterSelectionTime.Value > 10f)
        {
            if (characterSelectTimerCoroutine != null)
                StopCoroutine(characterSelectTimerCoroutine);

            currentCharacterSelectionTime.Value = 10f;

            characterSelectTimerCoroutine = CharacterSelectionTimer();

            if (characterSelectTimerCoroutine != null)
                StartCoroutine(characterSelectTimerCoroutine);
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

        int spawnIndexLocal = 0;
        int spawnIndexVisitant = 0;

        foreach (var playerNO in NetworkManager.Singleton.SpawnManager.PlayerObjects)
        {
            if (playerNO.TryGetComponent<Player>(out Player p) && playerNO.TryGetComponent<Rigidbody>(out Rigidbody rb) && playerNO.TryGetComponent<PlayerController>(out PlayerController playerController))
            {
                if (p.Team.Value == Team.LOCAL && spawnIndexLocal < localSpawnPositions.Count)
                {
                    rb.isKinematic = true;
                    rb.position = localSpawnPositions[spawnIndexLocal].position;
                    playerController.cinemachineCamera.transform.rotation = Quaternion.LookRotation(localSpawnPositions[spawnIndexLocal].forward);
                    rb.rotation = Quaternion.LookRotation(localSpawnPositions[spawnIndexLocal].forward);
                    spawnIndexLocal++;
                }
                else if (p.Team.Value == Team.VISITANT && spawnIndexVisitant < visitantSpawnPositions.Count)
                {
                    rb.isKinematic = true;
                    rb.position = visitantSpawnPositions[spawnIndexVisitant].position;
                    playerController.cinemachineCamera.transform.rotation = Quaternion.LookRotation(visitantSpawnPositions[spawnIndexVisitant].forward);
                    rb.rotation = Quaternion.LookRotation(visitantSpawnPositions[spawnIndexVisitant].forward);
                    spawnIndexVisitant++;
                }
            }
        }

        if (currentBall != null && currentBall.TryGetComponent<Rigidbody>(out var ballRb))
        {
            ballRb.isKinematic = true;
            currentBall.transform.SetPositionAndRotation(new Vector3(0, 5, 0), Quaternion.identity);
        }
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
        return currentCharacterSelectionTime.Value;
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

        if (IsServer)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApproval;
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoaded;
            //LocalCharacterSelected.OnListChanged -= OnCharacterSelectedReadyCheck;
            //VisitantCharacterSelected.OnListChanged -= OnCharacterSelectedReadyCheck;
        }
        else if (IsClient) NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}
