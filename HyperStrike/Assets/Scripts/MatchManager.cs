using HyperStrike;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance { get; private set; }

    public Action OnUpdateMatchScore;

    public NetworkVariable<MatchState> State { get; private set; } = new NetworkVariable<MatchState>(MatchState.NONE);

    private IEnumerator characterSelectTimerCoroutine;
    private IEnumerator initTimerCoroutine;
    private IEnumerator matchTimerCoroutine;

    [Header("Character Selection")]
    public NetworkVariable<float> characterSelectionTime = new NetworkVariable<float>(90.0f);

    [Header("Wait Conditions")]
    float waitTime = 5f; // Wait for 5 seconds
    public NetworkVariable<float> currentWaitTime = new NetworkVariable<float>(5.0f);

    [Header("Match Conditions")]
    public float maxTime = 300f; // 300 = 5 minutes in seconds
    public NetworkVariable<float> currentMatchTime = new NetworkVariable<float>(300.0f);
    public NetworkVariable<int> localGoals = new NetworkVariable<int>(0);
    public NetworkVariable<int> visitantGoals = new NetworkVariable<int>(0);

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

        // Init Enumerators
        characterSelectTimerCoroutine = CharacterSelectionTimer();
        matchTimerCoroutine = MatchTimer();
        initTimerCoroutine = PlayMatch();

        if (IsServer) State.Value = MatchState.NONE;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton?.ConnectedClientsList.Count > 0 && State.Value == MatchState.NONE) SetMatchState(MatchState.RESET);
    }

    void MatchStateBehavior()
    {
        switch (State.Value)
        {
            case MatchState.NONE:
                SetMatchState(MatchState.RESET);
                break;
            case MatchState.CHARACTER_SELECTION:
                {
                    // DISPLAY CHARACTER SELECTION WHEN ALL PLAYERS ALL CONNECTED
                    // Look NetworkSceneManager client sync to check if all are synced
                }
                break;
            case MatchState.RESET:
                {
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

                    // CHECK CONNECTION STATUS // Look NetworkSceneManager client sync to check if all are synced


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
        while (currentWaitTime.Value >= 0.0f)
        {
            yield return new WaitForSeconds(1f);
            currentWaitTime.Value--;
        }

        SetMatchState(MatchState.PLAY);
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

    public float GetCurrentWaitTime() 
    {
        return currentWaitTime.Value; 
    }
    
    public float GetCurrentMatchTime() 
    {
        return currentMatchTime.Value; 
    }
}
