using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LobbyState : byte
{
    NONE = 0,
    WAIT,
    COMPLETED
}

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public NetworkVariable<LobbyState> State { get; private set; } = new NetworkVariable<LobbyState>(LobbyState.NONE);


    public NetworkVariable<float> currentWaitTime = new NetworkVariable<float>(30.0f);

    private IEnumerator completedTimerCoroutine;

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

        completedTimerCoroutine = LobbyCompletedTimer();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LobbyStateBehaviour()
    {
        switch (State.Value)
        {
            case LobbyState.NONE:
                break;
            case LobbyState.WAIT:   
                {
                    // Once all 6 players are in the lobby, init Coroutine with timer, then change scene in server.
                    if (NetworkManager.Singleton?.ConnectedClientsList.Count > 0) StartCoroutine(completedTimerCoroutine);
                    else 
                    {
                        StopCoroutine(completedTimerCoroutine);
                        currentWaitTime.Value = 30.0f;
                    }
                    
                }
                break;
            case LobbyState.COMPLETED: 
                {
                    var status = NetworkManager.Singleton.SceneManager.LoadScene("Design/Levels/Blockouts/Pinball Testing/Pinball Blockout", LoadSceneMode.Single);
                    if (status != SceneEventProgressStatus.Started)
                    {
                        Debug.LogWarning($"Failed to load Match Scene" +
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
        if (IsServer)
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
}
