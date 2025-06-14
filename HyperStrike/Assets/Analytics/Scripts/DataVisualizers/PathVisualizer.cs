using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class PathVisualizer : MonoBehaviour
{
    public List<Vector3> playerPath = new List<Vector3>();

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (playerPath == null || playerPath.Count < 2)
            return;

        Handles.color = Color.green;
        for (int i = 0; i < playerPath.Count - 1; i++)
        {
            Handles.DrawLine(playerPath[i], playerPath[i + 1]);
        }
    }
#endif
}
