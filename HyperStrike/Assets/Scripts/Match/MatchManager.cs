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
    WON,
    LOOSE,
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
       
    [Header("VFX References")]
    [SerializeField] private GameObject localGoalVFX;
    [SerializeField] private GameObject visitantGoalVFX;

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
            GameManager.Instance.allowMovement.Value = false;
        }
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (NetworkManager.Singleton?.ConnectedClientsList.Count > 1) SetMatchState(MatchState.CHARACTER_SELECTION);
    }

    public void SceneManager_OnSynchronizeComplete(ulong clientId)
    {
        if (NetworkManager.Singleton?.ConnectedClientsList.Count > 1) SetMatchState(MatchState.CHARACTER_SELECTION);
    }

    void MatchStateBehavior()
    {
        switch (State.Value)
        {
            case MatchState.NONE:

                break;
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

                        CharacterSelected.Add((byte)Characters.NONE);
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
                    GameManager.Instance.allowMovement.Value = true;

                    if (initTimerCoroutine != null)
                        StopCoroutine(initTimerCoroutine);

                    // Starts timer
                    if (matchTimerCoroutine != null)
                        StartCoroutine(matchTimerCoroutine);

                    // LET PLAYERS AND BALL MOVE
                    // HANDLE PLAY BEHAVIOR
                    // Score go to WAIT and reset positions
                    //if (scores)
                    //{
                    //    OnUpdateMatchScore.Invoke();
                    //    SetMatchState(MatchState.WAIT);
                    //}
                }
                break;
            case MatchState.WON:
                // STOP THE PLAYERS AND BALL
                // Show win UI to the players
                break;
            case MatchState.LOOSE:
                // STOP THE PLAYERS AND BALL
                // Show loose UI to the players
                break;
            default:
                break;
        }
    }

    public void SetMatchState(MatchState state)
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

    }

    private void EndMatch()
    {
        // Stop the match timer coroutine
        if (matchTimerCoroutine != null)
        {
            StopCoroutine(matchTimerCoroutine);
        }

        // Set the match state to WON or LOOSE based on the score after finishing the time
        MatchState st = localGoals.Value > visitantGoals.Value ? MatchState.WON : MatchState.LOOSE;
        SetMatchState(st);

        Debug.Log("Match Ended!");
        Debug.Log($"Final Score: Local {localGoals} - {visitantGoals} Visitant");
    }

    private IEnumerator MatchTimer()
    {
        while (currentMatchTime.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            currentMatchTime.Value--;
            //UpdateTimerUI();
        }

        EndMatch();
    }

    void UpdateMatchScore()
    {
        OnUpdateMatchScore.Invoke();
        Debug.Log("Match score updated!");
    }

    private void TriggerGoalVFX(GameObject vfxPrefab, Vector3 goalPosition)
    {
        if (vfxPrefab != null)
        {
            // Instantiate the VFX at the goal's position
            GameObject vfxInstance = Instantiate(vfxPrefab, goalPosition, Quaternion.identity);

            Destroy(vfxInstance, 3f);
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
}
