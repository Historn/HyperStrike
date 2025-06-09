using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PathVisualizer : MonoBehaviour
{
    // Player positions
    public List<Vector3> playerPath = new List<Vector3>();

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
}
