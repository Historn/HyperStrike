using System.Collections;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Ability", menuName = "HyperStrike/Movement Ability")]
public class MovementAbility : Ability
{
    [Header("Movement Settings")]
    public float dashDistance;
    public float dashDuration;
    public float dashSpeed;
    public bool canDamageEnemies;
    public int damageAmount;
    public Vector3 dashDirection;
    public bool isForwardDirection;
    public LayerMask collisionLayers;
    public bool smoothTransition;

    protected bool isDashing = false;
    private float dashTimer = 0f;
    private Vector3 dashEndPosition;

    public override void ServerCast(ulong clientId, Vector3 initPos, Vector3 dir)
    {
        base.ServerCast(clientId, initPos, dir);

        if (isDashing) return;

        dashDirection = isForwardDirection ? dir : dashDirection.normalized;

        dashEndPosition = CalculateDashEndPosition(initPos, dashDirection, dashDistance);

        owner.StartCoroutine(PerformDash());

        PlayEffects(initPos);
    }

    private Vector3 CalculateDashEndPosition(Vector3 startPos, Vector3 direction, float distance)
    {
        RaycastHit hit;
        if (Physics.Raycast(startPos, direction, out hit, distance, collisionLayers))
        {
            return startPos + direction * (hit.distance - 0.5f);
        }
        Vector3 endPos = startPos + direction * distance;

        if (Physics.OverlapSphere(endPos, 0.5f, LayerMask.GetMask("Boundary")).Length > 0 || endPos.y < 0)
        {
            Vector3 returnPos = (startPos - endPos).normalized;
            endPos = returnPos;
        }

        return endPos;
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        dashTimer = 0f;
        Rigidbody rb = owner.GetComponent<Rigidbody>();
        Vector3 startPosition = owner.transform.position;

        rb.useGravity = false;
        Vector3 originalVelocity = rb.linearVelocity;
        rb.linearVelocity = Vector3.zero;

        while (dashTimer < dashDuration)
        {
            dashTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(dashTimer / dashDuration);

            float smoothedProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);

            Vector3 newPosition = Vector3.zero;
            if (smoothTransition) newPosition = Vector3.Lerp(startPosition, dashEndPosition, smoothedProgress);
            else newPosition = Vector3.Lerp(startPosition, dashEndPosition, 1);

            rb.MovePosition(newPosition);

            yield return null;
        }

        // Restore physics
        rb.useGravity = true;
        rb.linearVelocity = originalVelocity * 0.5f;
        isDashing = false;

        yield return new WaitForEndOfFrame();

        // Final boundary check
        if (!IsPositionInArena(owner.transform.position))
        {
            owner.transform.position = GetNearestArenaPoint(owner.transform.position);
        }
    }

    private bool IsPositionInArena(Vector3 pos)
    {
        return pos.x > -50 && pos.x < 50 && pos.z > -50 && pos.z < 50;
    }

    private Vector3 GetNearestArenaPoint(Vector3 outOfBoundsPosition)
    {
        Vector3 arenaCenter = Vector3.zero;

        Vector3 directionToCenter = (arenaCenter - outOfBoundsPosition).normalized;

        if (Physics.Raycast(outOfBoundsPosition,
                           directionToCenter,
                           out RaycastHit hit,
                           Mathf.Infinity,
                           LayerMask.GetMask("Boundary")))
        {
            return hit.point - (directionToCenter * 1f);
        }

        return arenaCenter;
    }

    public override void PlayEffects(Vector3 position)
    {
        // Play VFX/SFX on all clients
        PlayEffectsClientRpc(position);
    }

    [ClientRpc]
    private void PlayEffectsClientRpc(Vector3 position)
    {
        // Instantiate visual/audio effects
    }
}
