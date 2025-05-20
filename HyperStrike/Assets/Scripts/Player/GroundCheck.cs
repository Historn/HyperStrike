using UnityEngine;

public class GroundCheck
{
    public static bool CheckGrounded(Transform transform, float objHeight = 1.0f)
    {
        if (transform == null) return false;

        float dist = objHeight * 0.5f + 0.1f;
        Vector3 endRayPos = new Vector3(transform.position.x, transform.position.y - dist, transform.position.z);
        Debug.DrawLine(transform.position, endRayPos, UnityEngine.Color.red);
        return Physics.Raycast(transform.position, Vector3.down, dist);
    }
}
