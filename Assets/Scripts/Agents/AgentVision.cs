using UnityEngine;
using UnityEngine.AI;

public class AgentVision : MonoBehaviour
{
    public Transform player;
    public float viewAngle = 180f;

    public LayerMask obstacleLayer;
    public LayerMask playerLayer;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            UnityEngine.Debug.Log("Spieler gesehen!");

        }
        else
        {
            UnityEngine.Debug.Log("Kein Spieler in Sicht.");
        }
    }

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
        {
            if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToPlayer, 9999f, obstacleLayer))
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 9999f);

        Vector3 forward = transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * 9999f);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * 9999f);
    }

}
