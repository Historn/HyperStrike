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
}
