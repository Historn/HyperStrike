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


    public NetworkVariable<float> currentWaitTime = new NetworkVariable<float>(5.0f);

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
        if (Input.GetKeyDown(KeyCode.P) && NetworkManager.Singleton?.ConnectedClientsList.Count > 1) SetLobbyState(LobbyState.WAIT);
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
                    StartCoroutine(completedTimerCoroutine);
                    
                }
                break;
            case LobbyState.COMPLETED: 
                {
                    var status = NetworkManager.Singleton.SceneManager.LoadScene("PinballTest", LoadSceneMode.Single);
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
