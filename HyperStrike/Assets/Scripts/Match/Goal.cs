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
        }
        else
        {
            goalAudioSource = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;

        if (other != null && other.TryGetComponent<BallController>(out BallController ballC))
        {
            if (!ballC.IsGoal) return;

            if (isLocalGoal)
                MatchManager.Instance.visitantGoals.Value++;
            else
                MatchManager.Instance.localGoals.Value++;

            if (ballC.lastPlayerHitPosition != Vector3.zero) goalEventSubscriber.OnPlayerHitBallScored.Invoke(ballC.lastPlayerHitPosition);

            // Increase goals
            TriggerGoalVFXRpc(other.transform.position);
            TriggerGoalSFXRpc();

            MatchManager.Instance.SetMatchState(MatchState.GOAL);
        }
    }

    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
    private void TriggerGoalVFXRpc(Vector3 position)
    {
        if (goalVfx != null)
        {
            // Instantiate the VFX at the ball's current position
            GameObject vfxInstance = Instantiate(goalVfx, position, Quaternion.identity);
            Destroy(vfxInstance, 5f);
        }
    }

    [Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable    )]
    private void TriggerGoalSFXRpc()
    {
        if (goalSFX != null)
        {
            goalAudioSource.volume = Random.Range(0.8f, 1.0f);
            goalAudioSource.pitch = Random.Range(0.8f, 1.0f);
            
            // Play Goal SFX
            goalAudioSource.PlayOneShot(goalSFX);
            goalAudioSource.PlayOneShot(airHornSFX);
        }
    }
}
