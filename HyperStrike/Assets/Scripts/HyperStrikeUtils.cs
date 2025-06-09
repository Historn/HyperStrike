using System.Linq;
using UnityEngine;

public class HyperStrikeUtils
{
    public bool CheckGrounded(Transform transform, float objHeight = 1.0f)
    {
        if (transform == null) return false;

        float dist = objHeight * 0.5f + 0.1f;
        Vector3 og = new Vector3(transform.position.x, transform.position.y + (objHeight*0.5f), transform.position.z);
        Debug.DrawLine(og, transform.position, UnityEngine.Color.red);
        return Physics.Raycast(og, Vector3.down, dist);
    }

    public bool CheckWalls(Transform transform, ref RaycastHit wallHit)
    {
        bool[] wallChecked = new bool[]
        {
            false, false, false, false, false
        };

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
            Vector3 og = new Vector3(transform.position.x, transform.position.y + (transform.localScale.y), transform.position.z);
            Debug.DrawLine(og, og + directions[i], UnityEngine.Color.green);
            wallChecked[i] = Physics.Raycast(og, og + directions[i], out wallHit, 0.15f);
        }

        return wallChecked.Contains(true) ? true : false;
    }

    public bool CheckObjectInsideCollision(Transform transform)
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
            Debug.DrawLine(transform.position, transform.position + directions[i], UnityEngine.Color.green);
            completelyInside[i] = Physics.Raycast(transform.position, transform.position + directions[i], transform.localScale.magnitude/2);
        }

        return completelyInside.Contains(false) ? false : true;
    }
}
