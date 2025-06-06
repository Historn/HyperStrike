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

    #region "Coroutines"
    private IEnumerator characterSelectTimerCoroutine;
    private IEnumerator initTimerCoroutine;
    private IEnumerator matchTimerCoroutine;
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
    float waitTime = 6f; // Wait for 5 seconds
    public NetworkVariable<float> currentWaitTime = new NetworkVariable<float>(6.0f);
    #endregion

    #region "MATCH VARIABLES"
    [Header("Match Conditions")]
    public float maxTime = 300f; // 300 = 5 minutes in seconds
    public NetworkVariable<float> currentMatchTime = new NetworkVariable<float>(300.0f);
    public NetworkVariable<int> localGoals = new NetworkVariable<int>(0);
    public NetworkVariable<int> visitantGoals = new NetworkVariable<int>(0);
    #endregion

    [SerializeField] private GameObject ballPrefab;

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

        // SYNCHRONIZATION EVENT PROCESS
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;

        // Init Enumerators
        characterSelectTimerCoroutine = CharacterSelectionTimer();
        matchTimerCoroutine = MatchTimer();
        initTimerCoroutine = PlayMatch();

        CharacterSelected = new NetworkList<byte>();
        LocalPlayersID = new NetworkList<ulong>();
        VisitantPlayersID = new NetworkList<ulong>();
        if (IsServer)
        {
            State.Value = MatchState.NONE;
            allowMovement.Value = false;
            localGoals.OnValueChanged += OnGoalScored; // SOLO SE LLAMA EN CLIENTES
            visitantGoals.OnValueChanged += OnGoalScored;
        }
    }

    void OnGoalScored(int previous, int current)
    {
        if (State.Value == MatchState.PLAY) SetMatchState(MatchState.GOAL);
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (NetworkManager.Singleton?.ConnectedClientsList.Count > 1)
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
                    // ASSIGN PLAYERS TO A TEAM HARDCODED NOW
                    for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
                    {
                        if (i < 3 && LocalPlayersID.Count < 3)
                        {
                            LocalPlayersID.Add(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
                        }
                        else if (i >= 3 && VisitantPlayersID.Count < 3)
                        {
                            VisitantPlayersID.Add(NetworkManager.Singleton.ConnectedClientsList[i].ClientId);
                        }

                        if (CharacterSelected.Count < 6) CharacterSelected.Add((byte)Characters.NONE);
                    }

                    // DISPLAY CHARACTER SELECTION WHEN ALL PLAYERS ARE CONNECTED AND SYNCED
                    OnDisplayCharacterSelection?.Invoke();

                    if (characterSelectTimerCoroutine != null)
                        StartCoroutine(characterSelectTimerCoroutine);
                }
                break;
            case MatchState.RESET:
                {
                    if (characterSelectTimerCoroutine != null)
                        StopCoroutine(characterSelectTimerCoroutine);

                    /*DESPAWN EACH TEAM PLAYER PREFABS TO SPAWN POSITIONS*/
                    foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
                    {
                        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.Despawn();
                    }

                    /*SPAWN EACH TEAM PLAYER PREFABS TO SPAWN POSITIONS*/
                    for (int i = 0; i < CharacterSelected.Count; i++)
                    {
                        var character = CharacterSelected[i];
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

                    if (initTimerCoroutine != null)
                        StartCoroutine(initTimerCoroutine);
                }
                break;
            case MatchState.PLAY:
                {
                    allowMovement.Value = true;

                    if (initTimerCoroutine != null)
                        StopCoroutine(initTimerCoroutine);

                    // Starts timer
                    if (matchTimerCoroutine != null)
                        StartCoroutine(matchTimerCoroutine);

                    // HANDLE PLAY BEHAVIOR --> GOALS
                }
                break;
            case MatchState.GOAL:
                {
                    Debug.Log($"Local: {localGoals.Value} - {visitantGoals.Value} :Visitant");
                    OnUpdateMatchScore.Invoke();
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

    private void SetMatchState(MatchState state)
    {
        if (IsServer)
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
                Debug.Log(character.ToString());
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
        // Stop the match timer coroutine
        if (matchTimerCoroutine != null)
        {
            StopCoroutine(matchTimerCoroutine);
        }

        // Reset Match vars
        localGoals.Value = 0;
        visitantGoals.Value = 0;
        currentWaitTime.Value = waitTime;
        currentMatchTime.Value = maxTime;
    }

    void ResetPositions()
    {
        allowMovement.Value = false;
        GameObject ball = Instantiate(ballPrefab, new Vector3(0, 5, 0), Quaternion.identity);
        ball.GetComponent<NetworkObject>().Spawn(true);
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

    void UpdateMatchScore()
    {
        OnUpdateMatchScore.Invoke();
        Debug.Log("Match score updated!");
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
}
