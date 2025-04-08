using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager instance;

    private void Awake()
    {
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
        instance = this;
    }

    


}
