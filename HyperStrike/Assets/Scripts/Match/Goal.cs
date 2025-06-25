using Unity.Netcode;
using UnityEngine;

public class Goal : NetworkBehaviour
{
    [SerializeField] private bool isLocalGoal;
    [SerializeField] private GameObject goalVfx;
    
    [SerializeField] private AudioClip goalSFX;
    [SerializeField] private AudioClip airHornSFX;
    private AudioSource goalAudioSource;

    private GoalEventSubscriber goalEventSubscriber;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            goalEventSubscriber = GetComponent<GoalEventSubscriber>();
            goalAudioSource = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other != null && other.CompareTag("Ball"))
        {
            BallController ballC = other.GetComponent<BallController>();
            //Debug.Log(goal);
            if (!ballC.IsGoal) return;

            if (isLocalGoal)
                MatchManager.Instance.visitantGoals.Value++;
            else
                MatchManager.Instance.localGoals.Value++;

            if (ballC.lastPlayerHitPosition != Vector3.zero) goalEventSubscriber.OnPlayerHitBallScored.Invoke(ballC.lastPlayerHitPosition);

            // Increase goals
            TriggerGoalVFX(other.transform.position);
            TriggerGoalSFX();

            MatchManager.Instance.SetMatchState(MatchState.GOAL);
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

    private void TriggerGoalSFX()
    {
        if (goalSFX != null && IsServer)
        {
            goalAudioSource.volume = Random.Range(0.8f, 1.0f);
            goalAudioSource.pitch = Random.Range(0.8f, 1.0f);
            
            // Play Goal SFX
            goalAudioSource.PlayOneShot(goalSFX);
            goalAudioSource.PlayOneShot(airHornSFX);
        }
    }
}
