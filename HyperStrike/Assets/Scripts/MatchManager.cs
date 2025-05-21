using HyperStrike;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum MatchState
{
    NONE = 0,
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

    public MatchState State { get; private set; }

    private IEnumerator initTimerCoroutine;
    private IEnumerator matchTimerCoroutine;

    [Header("Wait Conditions")]
    float waitTime = 5f; // Wait for 5 seconds
    float currentWaitTime; // Wait for 5 seconds

    [Header("Match Conditions")]
    public float maxTime = 300f; // 300 = 5 minutes in seconds
    private float currentMatchTime;
    [HideInInspector] public int localGoals;
    [HideInInspector] public int visitantGoals;

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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Init Enumerators
        matchTimerCoroutine = MatchTimer();
        initTimerCoroutine = PlayMatch();

        // Set initial Match state
        //SetMatchState(MatchState.RESET);
    }

    // Update is called once per frame
    void Update()
    {
        //
        if (NetworkManager.Singleton?.ConnectedClientsList.Count > 1) SetMatchState(MatchState.RESET);
    }

    void MatchStateBehavior()
    {
        switch (State)
        {
            case MatchState.NONE:
                SetMatchState(MatchState.RESET);
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

                    // CHECK CONNECTION STATUS AND WAIT A COUPLE OF SECONDS
                    SetMatchState(MatchState.INIT);
                }
                break;
            case MatchState.INIT:
                {
                    currentWaitTime = waitTime;

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
        State = state;
        MatchStateBehavior();
    }

    private IEnumerator PlayMatch()
    {
        while (currentWaitTime >= 0)
        {
            yield return new WaitForSeconds(1f);
            currentWaitTime--;
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
        localGoals = 0;
        visitantGoals = 0;
        currentMatchTime = maxTime;
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
        MatchState st = localGoals > visitantGoals ? MatchState.WON : MatchState.LOOSE;
        SetMatchState(st);

        Debug.Log("Match Ended!");
        Debug.Log($"Final Score: Local {localGoals} - {visitantGoals} Visitant");
    }

    private IEnumerator MatchTimer()
    {
        while (currentMatchTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentMatchTime--;
            //UpdateTimerUI();
        }

        EndMatch();
    }

    void UpdateMatchScore()
    {
        OnUpdateMatchScore.Invoke();
        Debug.Log("Match score updated!");
    }

    private string UpdateTimerAsText()
    {
        int minutes = Mathf.FloorToInt(currentMatchTime / 60f);
        int seconds = Mathf.FloorToInt(currentMatchTime % 60f);

        string timeText = $"{minutes:D2}:{seconds:D2}";

        return timeText;
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

    public float GetCurrentWaitTime() { return currentWaitTime; }
}
