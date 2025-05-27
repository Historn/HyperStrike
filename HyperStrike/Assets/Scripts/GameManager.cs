using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HyperStrike
{
    public enum GameState
    {
        NONE = 0,
        TITLE,
        MENU,
        WAITING_ROOM,
        IN_GAME,
    }

    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        public NetworkVariable<bool> allowMovement = new NetworkVariable<bool>(true);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

#if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
        }

        [SerializeField] private GameState gameState = GameState.MENU;

        void GameStateBehavior()
        {
            switch (gameState)
            {
                case GameState.NONE:
                    gameState = GameState.TITLE;
                    break;
                case GameState.TITLE:
                    SceneManager.LoadScene("Title");
                    // Execute intro?
                    // After all finished->Press Enter or A in controller to go to main menu?
                    break;
                case GameState.MENU:
                    SceneManager.LoadScene("MainMenu");
                    break;
                case GameState.WAITING_ROOM:
                    SceneManager.LoadScene("WaitingRoom");
                    // Once loaded, invoke connection to server for all players
                    // Once connected go to the game?
                    break;
                case GameState.IN_GAME:
                    SceneManager.LoadScene("MatchRoom");
                    // Set smth still think about it
                    break;
                default:
                    break;
            }
        }

        public void SetGameState(GameState state)
        {
            gameState = state;
            GameStateBehavior();
        }

    }
}