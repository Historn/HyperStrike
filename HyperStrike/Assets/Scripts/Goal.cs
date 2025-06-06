using Unity.Netcode;
using UnityEngine;

public class Goal : NetworkBehaviour
{
    [SerializeField] private GameObject goalVfx;
    [SerializeField] private bool isLocalGoal;
    private void OnTriggerStay(Collider other)
    {
        if (other != null && other.CompareTag("Ball"))
        {
            bool goal = other.GetComponent<BallController>().IsGoal;
            //Debug.Log(goal);
            if (goal)
            {
                if (isLocalGoal)
                    MatchManager.Instance.localGoals.Value++;
                else
                    MatchManager.Instance.visitantGoals.Value++;

                // Increase goals
                TriggerGoalVFX(other.transform.position);

                Destroy(other.gameObject);
            }
        }
    }

    private void TriggerGoalVFX(Vector3 position)
    {
        if (goalVfx != null && IsServer)
        {
            // Instantiate the VFX at the ball's current position
            GameObject vfxInstance = Instantiate(goalVfx, position, Quaternion.identity);
            vfxInstance.GetComponent<NetworkObject>().Spawn(true);
            Destroy(vfxInstance, 5f);
        }
    }
}
