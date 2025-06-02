using System.Linq;
using UnityEngine;

public class HyperStrikeUtils
{
    public static bool CheckGrounded(Transform transform, float objHeight = 1.0f)
    {
        if (transform == null) return false;

        float dist = objHeight * 0.5f + 0.1f;
        Vector3 endRayPos = new Vector3(transform.position.x, transform.position.y - dist, transform.position.z);
        Debug.DrawLine(transform.position, endRayPos, UnityEngine.Color.red);
        return Physics.Raycast(transform.position, Vector3.down, dist);
    }

    public static bool CheckWalls(Transform transform, ref RaycastHit wallHit)
    {
        bool wall = false;
        Vector3[] directions = new Vector3[]
        {
            transform.right,
            transform.right + transform.forward,
            transform.forward,
            -transform.right + transform.forward,
            -transform.right
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Debug.DrawLine(transform.position, transform.position + directions[i], UnityEngine.Color.green);
            wall = Physics.Raycast(transform.position, directions[i], out wallHit, transform.localScale.x + 0.15f);
        }

        return wall;
    }

    public static bool CheckObjectInsideCollision(Transform transform)
    {
        bool[] completelyInside = new bool[]
        {
            false, false, false, false, false, false,
        };

        Vector3[] directions = new Vector3[]
        {
            transform.right,
            transform.forward,
            transform.up,
            -transform.right,
            -transform.forward,
            -transform.up
        };

        for (int i = 0; i < directions.Length && i < completelyInside.Length; i++)
        {
            Debug.DrawLine(transform.position, transform.position + (directions[i] * 3), UnityEngine.Color.green);
            completelyInside[i] = Physics.Raycast(transform.position, directions[i], transform.localScale.magnitude);
        }

        bool ret = completelyInside.Contains(false);

        return completelyInside.Contains(false) ? false : true;
    }
}
