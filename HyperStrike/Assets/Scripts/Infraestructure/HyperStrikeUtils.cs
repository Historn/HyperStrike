using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class HyperStrikeUtils
{
    public bool CheckGrounded(Transform transform, float objHeight = 1.0f)
    {
        if (transform == null) return false;

        float dist = objHeight * 0.5f + 0.1f;
        Vector3 og = new Vector3(transform.position.x, transform.position.y + (objHeight * 0.5f), transform.position.z);
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
            RaycastHit hit;
            Vector3 og = new Vector3(transform.position.x, transform.position.y + (transform.localScale.y), transform.position.z);
            Debug.DrawLine(og, og + (directions[i] * (transform.localScale.x * 0.5f + 0.01f)), UnityEngine.Color.green);
            wallChecked[i] = Physics.Raycast(og, directions[i], out hit, transform.localScale.x * 0.5f + 0.01f);
            if (wallChecked[i]) wallHit = hit;
        }

        return wallChecked.Contains(true);
    }

    public bool CheckWalls(Transform transform, ref RaycastHit wallHit, ref CameraTilt cameraTilt, LayerMask wallMask)
    {
        bool[] wallChecked = new bool[]
        {
            false, false, false, false, false
        };

        Vector3[] directions = new Vector3[]
        {
            transform.right,
            -transform.right
        };

        for (int i = 0; i < directions.Length; i++)
        {
            RaycastHit hit;
            Vector3 og = new Vector3(transform.position.x, transform.position.y + (transform.localScale.y), transform.position.z);
            Debug.DrawLine(og, og + (directions[i] * (transform.localScale.x * 0.5f + 0.01f)), UnityEngine.Color.green);
            wallChecked[i] = Physics.Raycast(og, directions[i], out hit, transform.localScale.x * 0.5f + 0.05f, wallMask);
            if (wallChecked[i])
            {
                wallHit = hit;

                if (directions[i] == transform.right || directions[i] == transform.right + transform.forward)
                {
                    cameraTilt = CameraTilt.RIGHT;
                }
                else if (directions[i] == -transform.right || directions[i] == -transform.right + transform.forward)
                {
                    cameraTilt = CameraTilt.LEFT;
                }
            }
        }
        return wallChecked.Contains(true);
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
            completelyInside[i] = Physics.Raycast(transform.position, transform.position + directions[i], transform.localScale.magnitude / 2);
        }

        return !completelyInside.Contains(false);
    }
}
